using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Each of these required components are features that should be found in every single level/map
[RequireComponent(typeof(AdditivelyLoadScenesHandler), typeof(CollectablesTrackingHandler), typeof(PauseHandler))]
[RequireComponent(typeof(SpawnPlayerHandler), typeof(LevelDataHandler), typeof(DeathscreenDistortionHandler))]
public abstract class LevelController : MonoBehaviour
{
    public static LevelController instance { get; private set; }

    // Currently used by SteamAchievementController to unlock level completion achievements
    public static event Action OnPlayerEscaped;

    // Lets ScreenCoverPanel.cs know what to set it's canvasgroup.alpha to as well as a fade time to transition from its current alpha
    // to the new alpha. Useful for forcing a black screen without starting a new scene load with SceneSwapManager
    public static event Action<float, float> OnSetScreenCoverPanel;

    // This event lets any listeners (like AudioAmbientController) know when to get rid of/restore ambient sound effects (for instance,
    // when Shade monster becomes berserk, get rid of insects/wind ambience). If bool is true, get rid of the ambient SFX. If bool is false,
    // restore the ambient SFX
    public static event Action<bool> OnReduceAmbientSFX;

    // Event actions that influence the player's wrist device
    public static event Action<string, float> OnSendMessageToWristDevice; // Send a message to be displayed on the device, float is a duration override
    public static event Action<string> OnChangeWristDeviceMessage; // Can be used to change the device's current message
    public static event Action<Color> OnChangeWristDeviceMessageColor; // Can be used to change the device's current message color
    public static event Action<bool> OnDisableWristDevice; // Disable functionality of the device (sets a staticy screen effect)

    // Used to flicker then disable the player's flashlight
    // float 1: flicker time
    // float 2: new base intensity
    // Vector2 1: intensity displacement (min, max)
    // Vector2 2: interval displacement (min, max)
    // float 3: final fade time after flicker time has finished
    public static event Action<float, float, Vector2, Vector2, float> OnDisablePlayerFlashlight;

    // Used to flicker the player's flashlight (same variables as above without the final fade time)
    public static event Action<float, float, Vector2, Vector2> OnFlickerPlayerFlashlight;

    // GameplayUI will always be loaded no matter what level
    [Header("Additively Load Scenes")]
    [SerializeField] private SceneName[] scenesToLoad = { SceneName.GameplayUI };

    [Header("Level Complete Scene")]
    [Tooltip("Optional final scene to load once the player opens and enters the portal (default is load back into Main Menu)")]
    [SerializeField] protected SceneName levelCompleteScene = SceneName.MainMenu;
    [SerializeField, Min(0)] private float fadeTime = 0.25f;
    [SerializeField, Min(0)] private float delayTime = 0.85f;

    [Header("Level Fail Scene")]
    [Tooltip("The scene that will load if the player fails a level (a monster catches the player). E.G. DeathscreenJumpscareBase loads " + 
        "this scene")]
    [SerializeField] protected SceneName levelFailScene = SceneName.MainMenu;

    [Header("Special Input Action")]
    [SerializeField] private string _inputMessage = "Check Device";
    public string inputMessage => _inputMessage;
    [SerializeField] private InputActionReference _inputActionReference;
    public InputActionReference inputActionReference => _inputActionReference;

    [Header("Load In SFX")]
    [SerializeField] private AudioSource loadInSFXAudioSource;
    [Tooltip("Optional SFX to play on level load-up when the Player scene is loaded and the player is configured")]
    [SerializeField] private SoundEffectSO loadInSFX;
    [Tooltip("The delay in which the loadInSFX will play after the Player scene is loaded")]
    [SerializeField, Min(0)] private float loadInSFXDelay = 1.25f;

    [Header("Monster Spawn Settings")]
    [Tooltip("If enabled, MonsterSpawnController will spawn the monster(s) after the monsterSpawnDelay")]
    [SerializeField] private bool _includeMonsterSpawnDelay;
    public bool includeMonsterSpawnDelay => _includeMonsterSpawnDelay;
    [SerializeField, Min(0)] private float _monsterSpawnDelay = 8.5f;
    public float monsterSpawnDelay => _monsterSpawnDelay;

    protected AdditivelyLoadScenesHandler additivelyLoadScenesHandler;
    protected CollectablesTrackingHandler collectablesTrackingHandler;
    protected PauseHandler pauseHandler;
    protected SpawnPlayerHandler spawnPlayerHandler;
    protected LevelDataHandler levelDataHandler;
    protected DeathscreenDistortionHandler deathscreenDistortionHandler;

    // The level does not have to have waypoints, but if it does each waypoint will individually register to
    // this controller and any EntityWayPointState can get the level's waypoints
    public List<WayPoint> wayPoints { get; private set; } = new List<WayPoint>();

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;

            additivelyLoadScenesHandler = GetComponent<AdditivelyLoadScenesHandler>();
            collectablesTrackingHandler = GetComponent<CollectablesTrackingHandler>();
            pauseHandler = GetComponent<PauseHandler>();
            spawnPlayerHandler = GetComponent<SpawnPlayerHandler>();
            levelDataHandler = GetComponent<LevelDataHandler>();
            deathscreenDistortionHandler = GetComponent<DeathscreenDistortionHandler>();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    protected virtual void Start()
    {
        additivelyLoadScenesHandler.AdditivelyLoadScenes(scenesToLoad);
        collectablesTrackingHandler.SetCollectablesRemaining(levelDataHandler.GetLevelData().collectableCount);

        InputManager.instance.EnableBaseMap(true);
        InputManager.instance.EnableUIMap(false);
    }

    protected virtual void OnEnable()
    {
        UniversalPlayerInput.OnEnterPauseMenu += pauseHandler.Pause;
        PauseMenuController.OnExitPauseMenu += pauseHandler.Resume;

        Collectable.OnCollected += collectablesTrackingHandler.HandleCollectableCollected;

        SpawnPlayerHandler.OnPlayerLoaded += PlayLoadInSFX;

        InputSystem.onDeviceChange += HandleGamepadDisconnected;
    }

    protected virtual void OnDisable()
    {
        UniversalPlayerInput.OnEnterPauseMenu -= pauseHandler.Pause;
        PauseMenuController.OnExitPauseMenu -= pauseHandler.Resume;

        Collectable.OnCollected -= collectablesTrackingHandler.HandleCollectableCollected;

        SpawnPlayerHandler.OnPlayerLoaded -= PlayLoadInSFX;

        InputSystem.onDeviceChange -= HandleGamepadDisconnected;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    ///     Spawn the player at the given transform.
    /// </summary>
    public void SpawnPlayer(Transform spawnPlayerTransform)
    {
        spawnPlayerHandler.SpawnPlayer(spawnPlayerTransform, levelDataHandler.GetLevelData().playerConfig);
    }

    /// <summary>
    ///     Get a reference to the player. Note: only works once the player scene is loaded in and the player is ready to go.
    /// </summary>
    /// <returns>The player game object (or null if there currently is no player game object).</returns>
    public GameObject GetPlayer()
    {
        return spawnPlayerHandler.player != null ? spawnPlayerHandler.player : null;
    }

    /// <summary>
    ///     Begin the fullscreen distortion effect (should only be called once a monster catches the player and the player loses).
    /// </summary>
    public void BeginFullscreenDistortionOnPlayerDeath()
    {
        deathscreenDistortionHandler.BeginFullscreenDistortion();
    }

    /// <summary>
    ///     Sends a message to the player's wrist device.
    /// </summary>
    /// <param name="message">The message to be sent to the wrist device.</param>
    /// <param name="durationOverride">Optional duration override that overrides the wrist device's default message length.</param>
    public void SendMessageToWristDevice(string message, float durationOverride = -1)
    {
        OnSendMessageToWristDevice?.Invoke(message, durationOverride);
    }

    /// <summary>
    ///     Can be used to override the device's current message to a new message.
    /// </summary>
    /// <param name="message">The updated message.</param>
    public void ChangeWristDeviceMessage(string message)
    {
        OnChangeWristDeviceMessage?.Invoke(message);
    }

    /// <summary>
    ///     Can be used to override the device's message color to a new color.
    /// </summary>
    /// <param name="messageTextColor">The new color of the message text.</param>
    public void ChangeWristDeviceMessageColor(Color messageTextColor)
    {
        OnChangeWristDeviceMessageColor?.Invoke(messageTextColor);
    }

    /// <summary>
    ///     Can be used to disable or re-enable the player's wrist device.
    /// </summary>
    /// <param name="disabled">Whether or not the wrist device should be disabled.</param>
    public void DisableWristDevice(bool disabled)
    {
        OnDisableWristDevice?.Invoke(disabled);
    }

    /// <summary>
    ///     Get this level's data.
    /// </summary>
    /// <returns>LevelDataSO</returns>
    public LevelDataSO GetLevelData()
    {
        return levelDataHandler.GetLevelData();
    }

    /// <summary>
    ///     Check if the game is paused.
    /// </summary>
    /// <returns>True if paused, false if not.</returns>
    public bool IsPaused()
    {
        return pauseHandler.IsPaused();
    }

    /// <summary>
    ///     Resume the game.
    /// </summary>
    public void ResumeGame()
    {
        pauseHandler.Resume();
    }

    /// <summary>
    ///     If the gamepad is disconnected while playing, pause the game.
    /// </summary>
    private void HandleGamepadDisconnected(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad && change == InputDeviceChange.Removed)
        {
            pauseHandler.Pause();
        }
    }

    /// <summary>
    ///     Register a WayPoint with LevelController and add it the wayPoints list.
    /// </summary>
    public void RegisterWayPoint(WayPoint newWayPoint)
    {
        wayPoints.Add(newWayPoint);
    }

    /// <summary>
    ///     Enter's the scene that should be loaded when the player opens and enters the portal.
    /// </summary>
    public void EnterLevelCompleteScene()
    {
        OnPlayerEscaped?.Invoke();

        if (SceneSwapManager.instance != null)
        {
            SceneSwapManager.instance.LoadSceneWithFade(levelCompleteScene, fadeTime, delayTime);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"SceneSwapManager.instance is null, unable to fade in to levelCompleteScene.");
#endif
            SceneHandler.LoadSceneImmediate(levelCompleteScene.ToString(), additive: false, setActive: true);
        }
    }

    /// <summary>
    ///     Get the scene to load to when the player fails this level.
    /// </summary>
    /// <returns>The SceneName of the scene that should be loaded.</returns>
    public SceneName GetLevelFailScene()
    {
        return levelFailScene;
    }

    /// <summary>
    ///     Can be used to turn ambient audio sources off/on by either reducing their volumes to 0 or back to their original volumes.
    /// </summary>
    /// <param name="enabled">Should the audio sources' volumes be turned off or on?</param>
    public void EnableAmbientAudio(bool enabled)
    {
        // enabled is reversed because listeners expect the bool to be true if the volume should be disabled, not enabled
        OnReduceAmbientSFX?.Invoke(!enabled);
    }

    /// <summary>
    ///     Disable the player's flashlight with some flickering.
    /// </summary>
    /// <param name="flickerTime">The total time to flicker before turning completely off.</param>
    /// <param name="baseIntensity">The base intensity that intensityDisplacement will base itself around.</param>
    /// <param name="intensityDisplacement">The min/max intensity that the flashlight should randomly move to.</param>
    /// <param name="intervalDisplacement">The min/max time interval between intensity displacements.</param>
    /// <param name="finalFadeTime">Once the flickering is finished, this is the time it takes to fade to completely off.</param>
    public void DisablePlayerFlashlight(float flickerTime, float baseIntensity, Vector2 intensityDisplacement, Vector2 intervalDisplacement, float finalFadeTime)
    {
        OnDisablePlayerFlashlight?.Invoke(flickerTime, baseIntensity, intensityDisplacement, intervalDisplacement, finalFadeTime);
    }

    /// <summary>
    ///     Flicker the player's flashlight.
    /// </summary>
    /// <param name="flickerTime">The total time to flicker.</param>
    /// <param name="baseIntensity">The base intensity that intensityDisplacement will base itself around.</param>
    /// <param name="intensityDisplacement">The min/max intensity that the flashlight should randomly move to.</param>
    /// <param name="intervalDisplacement">The min/max time interval between intensity displacements.</param>
    public void FlickerPlayerFlashlight(float flickerTime, float baseIntensity, Vector2 intensityDisplacement, Vector2 intervalDisplacement)
    {
        OnFlickerPlayerFlashlight?.Invoke(flickerTime, baseIntensity, intensityDisplacement, intervalDisplacement);
    }

    /// <summary>
    ///     Fires off an event for ScreenCoverPanel to set an alpha
    /// </summary>
    /// <param name="alpha">The target alpha for panel.</param>
    /// <param name="fadeTime">The time it takes to fade from its current alpha to the target alpha.</param>
    public void SetScreenCoverPanelAlpha(float alpha, float fadeTime = 0.1f)
    {
        alpha = Mathf.Clamp01(alpha);
        OnSetScreenCoverPanel?.Invoke(alpha, fadeTime);
    }

    /// <summary>
    ///     Plays the load in sound effect using loadInSFXDelay.
    /// </summary>
    private void PlayLoadInSFX(GameObject player)
    {
        if (loadInSFXAudioSource != null && loadInSFX != null)
        {
            this.Invoke(() => loadInSFX.Play(loadInSFXAudioSource), loadInSFXDelay);
        }
    }
}
