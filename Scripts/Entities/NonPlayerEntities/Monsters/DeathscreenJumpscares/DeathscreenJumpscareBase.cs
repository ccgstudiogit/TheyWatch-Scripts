using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System;

/*
*** NOTE ***
The way this script works is that a specific monster's jumpscare script should inherit from this script and implement
the abstract method HandleDeathscreenJumpscare. Based on the specific monster's jumpscare needs, it can utilize any
of the protected methods below to customize the jumpscare.
*/

[RequireComponent(typeof(Monster))]
public abstract class DeathscreenJumpscareBase : MonoBehaviour
{
    // Currently used by Flashlight.cs to turn off the flashlight's light if needed
    public static event Action OnDeathscreenJumpscare;

    public bool caughtPlayer { get; protected set; }

    [Header("Monster Head Position")]
    [Tooltip("Optional reference that makes it so that the player looks at this game object when jumpscared")]
    [SerializeField] private GameObject _headPos;
    protected GameObject headPos => _headPos;

    [Header("Sound Effects")]
    [Tooltip("A sound effect that can be played when the player gets caught")]
    [SerializeField] private SoundEffectSO jumpscareSFX;
    [SerializeField] protected AudioSource jumpscareSFXAudioSource;

    [Header("Load Back To Main Menu Settings")]
    [Tooltip("If enabled, once the player get's jumpscared they will automatically be sent back to the main menu")]
    [SerializeField] private bool loadBackIntoMainMenuOnLoss = true;
    [SerializeField] private float fadeTime = 0.3f;

    protected Monster monster;
    protected Animator animator;

    protected virtual void Awake()
    {
        monster = GetComponent<Monster>();
        animator = GetComponent<Animator>();

        caughtPlayer = false;
    }

    protected virtual void OnEnable()
    {
        PlayerCollisions.OnPlayerCollidedWithMonster += HandleDeathscreenJumpscare;
    }

    protected virtual void OnDisable()
    {
        PlayerCollisions.OnPlayerCollidedWithMonster -= HandleDeathscreenJumpscare;
    }

    protected abstract void HandleDeathscreenJumpscare(PlayerReferences playerReferences, Monster monster);

    /// <summary>
    ///     Disables a script of type T on the target game object
    /// </summary>
    protected void DisableScript<T>(GameObject target) where T : MonoBehaviour
    {
        if (target.TryGetComponent(out T script))
        {
            script.enabled = false;
        }
    }

    /// <summary>
    ///     Immediately rotate this monster towards a target.
    /// </summary>
    protected void RotateMonsterTowardsTarget(Transform target, Monster monster)
    {
        GameObject monsterGO = monster.gameObject;

        Vector3 direction = (target.position - monsterGO.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        monsterGO.transform.rotation = targetRotation;
    }

    /// <summary>
    ///     Coroutine that smoothly rotates the player's camera to be looking at the monster.
    /// </summary>
    protected IEnumerator MakePlayerLookAtMonster(PlayerReferences playerReferences, Transform monsterTransform, float rotationSpeed)
    {
        if (playerReferences.cinemachineCam.TryGetComponent(out CinemachinePanTilt cinemachinePanTilt))
        {
            Transform looker = playerReferences.cinemachineCam.transform;
            cinemachinePanTilt.enabled = false; // Makes sure cinemachine does not override the rotation

            Vector3 direction = (monsterTransform.position - looker.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            while (Quaternion.Angle(looker.rotation, targetRotation) > 0.5f)
            {
                looker.rotation = Quaternion.RotateTowards(
                    looker.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

                yield return null;
            }

            looker.rotation = targetRotation;
        }
    }

    /// <summary>
    ///     Disables player movement by disabling PlayerMovement, PlayerLook, and UniversalPlayerInput
    /// </summary>
    // This method is here so that I don't have to constantly manually disable each of these scripts for each monster
    protected void DisablePlayerMovement(GameObject player)
    {
        DisableScript<UniversalPlayerInput>(player);
        DisableScript<PlayerLook>(player);
        DisableScript<PlayerMovement>(player);
    }

    protected void InvokeOnDeathscreenJumpscareAction()
    {
        OnDeathscreenJumpscare?.Invoke();
    }

    /// <summary>
    ///     Play the jumpscare sound effect.
    /// </summary>
    /// <returns>The audio source that was used to play the sound effect. Useful for getting the clip's length/pitch and
    ///     returning back to the main menu using those times.</returns>
    protected AudioSource PlayJumpscareSFX()
    {
        if (jumpscareSFX == null)
        {
            return null;
        }

        AudioSource source;

        // Get the source that was used to the play the sound effect and return it
        source = jumpscareSFXAudioSource != null ? jumpscareSFX.Play(jumpscareSFXAudioSource) : jumpscareSFX.Play();
        return source;
    }

    /// <summary>
    ///     Starts a coroutine that automatically sends the player back to the main menu after a specified delay set in the inspector.
    /// </summary>
    protected void LoadPlayerBackIntoMainMenu(float delay)
    {
        if (loadBackIntoMainMenuOnLoss)
        {
            StartCoroutine(LoadBackIntoMainMenuRoutine(delay));
        }
    }

    /// <summary>
    ///     Loads back into the main menu after a delay.
    /// </summary>
    private IEnumerator LoadBackIntoMainMenuRoutine(float d)
    {
        yield return new WaitForSeconds(d);
        SceneSwapManager.instance.LoadSceneWithFade(LevelController.instance.GetLevelFailScene(), fadeTime);
    }
}
