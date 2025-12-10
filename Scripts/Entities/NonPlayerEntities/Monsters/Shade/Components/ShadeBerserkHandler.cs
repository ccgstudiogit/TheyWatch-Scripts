using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Shade), typeof(ShadeAudioHandler), typeof(ShadeRendererHandler))]
public class ShadeBerserkHandler : MonoBehaviour
{
    public event Action OnBerserkReady; // Lets Shade.cs know when to change states (and do other berserk-related things)

    public bool currentlyBerserk { get; private set; }

    [Header("References")]
    [SerializeField] private Animator animator;
    [Tooltip("Objects that will be set active once Shade enters berserk mode and turned off once berserk mode ends")]
    [SerializeField] private GameObject[] berserkObjects;
    [Tooltip("Shade will change to this material once Shade enters berserk mode")]
    [SerializeField] private Material bodyBerserkMat;
    private Material regularBodyMat; // Holds a reference to the starting material to swap back to once berserk mode ends

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO berserkFootstepSFX;
    [SerializeField] private SoundEffectSO berserkWhisperBuildUpSFX;
    [SerializeField] private SoundEffectSO berserkExplosionSFX;
    [SerializeField] private SoundEffectSO berserkEndLaughterSFX;

    [Header("Other Audio Settings")]
    [Tooltip("If enabled, this script will reach out to LevelController.instance, and LevelController will reach out to AmbientAudioController " +
        "and set the ambient audio sources' volume to 0. After berserk chase ends, the ambient audio sources' volume will be set back to normal")]
    [SerializeField] private bool turnOffAmbientSources;

    [Header("Entering Berserk Settings")]
    [Tooltip("The time Shade will be in the stun animation before officially becoming berserk")]
    [SerializeField] private float timeStunnedBeforeBerserk = 1.5f;
    [Tooltip("This must be the same as the animation clip's speed in Shade's animator in order to get the berserk " +
        "animation timings right when entering berserk mode")]
    [SerializeField] private float shadeEnterBerserkClipSpeed = 3.15f;
    private float shadeEnterBerserkDelay; // Used for the enter berserk animation clip (to make sure it doesn't start until after the clip is done)

    [Header("VFX")]
    [Tooltip("Optional reference to play VFX when entering berserk")]
    [SerializeField] private ShadeBerserkVFX berserkVFX;

    private ShadeAudioHandler audioHandler;
    private ShadeRendererHandler rendererHandler;

    // For doing a small screen shake whenever Shade takes a berserk footstep
    private CinemachineImpulseSource cameraShakeSource;

    private void Awake()
    {
        audioHandler = GetComponent<ShadeAudioHandler>();
        rendererHandler = GetComponent<ShadeRendererHandler>();
        cameraShakeSource = GetComponent<CinemachineImpulseSource>();

        currentlyBerserk = false;
    }

    private void Start()
    {
        // Make sure all of the berserk-related game objects are not active on startup
        MonsterHelper.SetGameObjectsActive(berserkObjects, false);

        shadeEnterBerserkDelay = GetEnterBerserkDelay(shadeEnterBerserkClipSpeed);

        // Get a reference to Shade's regular body material so it can be re-applied after berserk mode ends
        regularBodyMat = rendererHandler.bodyMeshRenderer.material;
    }

    public void BerserkFootstepEvent()
    {
        if (berserkFootstepSFX != null)
        {
            berserkFootstepSFX.Play(audioHandler.berserkFootstepAudioSource);
        }

        if (cameraShakeSource != null && SettingsManager.instance.IsCameraShakeEnabled())
        {
            cameraShakeSource.GenerateImpulse();
        }
    }

    /// <summary>
    ///     Starts the process of becoming berserk.
    /// </summary>
    public void BecomeBerserk()
    {
        if (currentlyBerserk)
        {
#if UNITY_EDITOR
            Debug.LogWarning("BecomeBerserk() called when Shade is already berserk!");
#endif
            return;
        }

        StartCoroutine(BecomeBerserkRoutine());
    }

    private IEnumerator BecomeBerserkRoutine()
    {
        currentlyBerserk = true;
        float timeStunned = 0f;

        // Start the dark mist VFX
        if (berserkVFX != null)
        {
            // Make sure the VFX duration stays in sync with the time stunned
            berserkVFX.PlayDarkMist(timeStunnedBeforeBerserk);
        }

        // Begin the stun animation
        animator.SetTrigger("stun");

        // Play the build-up SFX
        if (berserkWhisperBuildUpSFX != null)
        {
            berserkWhisperBuildUpSFX.PlayOneShot(audioHandler.sfxAudioSource);
        }

        // Wait for a specified amount of time before exiting stun animation
        while (timeStunned < timeStunnedBeforeBerserk)
        {
            timeStunned += Time.deltaTime;
            yield return null;
        }

        // If required, turn off ambient audio sources
        if (turnOffAmbientSources && LevelController.instance != null)
        {
            LevelController.instance.EnableAmbientAudio(false);
        }

        // Begin the enter berserk animation and wait for it to finish until starting to move
        animator.SetTrigger("berserk");
        yield return new WaitForSeconds(shadeEnterBerserkDelay); // This delay is gotten from GetEnterBerserkDelay() in Start()

        // Play the red mist explosion VFX
        if (berserkVFX != null)
        {
            berserkVFX.PlayRedMist();
        }

        // Play the berserk explosion SFX
        if (berserkExplosionSFX != null)
        {
            berserkExplosionSFX.PlayOneShot(audioHandler.sfxAudioSource);
        }

        // Start the berserk chase looping audio
        audioHandler.whisperChaseAudioSource.Play();

        // Change Shade's material
        rendererHandler.SetBodyMaterial(bodyBerserkMat);

        // Set the berserk-related game objects active (such as the spot light)
        MonsterHelper.SetGameObjectsActive(berserkObjects, true);

        // Let Shade.cs know berserk mode is ready
        OnBerserkReady?.Invoke();
    }

    /// <summary>
    ///     Cleans up all of the changes made while entering berserk.
    /// </summary>
    public void ExitBerserk()
    {
        if (!currentlyBerserk)
        {
#if UNITY_EDITOR
            Debug.LogWarning("ExitBerserk() called when Shade is not currently berserk!");
#endif
            return;
        }

        // Create and play the laughter sfx
        MonsterHelper.CreateAudioSourceAndPlaySFX(audioHandler.audioSourcePrefab, transform.position, berserkEndLaughterSFX);

        // Turn off berserk-related objects
        MonsterHelper.SetGameObjectsActive(berserkObjects, false);

        // Put Shade's regular body material back on
        rendererHandler.SetBodyMaterial(regularBodyMat);

        // If the ambient audio sources were turned off in BecomeBerserkRoutine(), turn them back on now that berserk chase has ended
        if (turnOffAmbientSources && LevelController.instance != null)
        {
            LevelController.instance.EnableAmbientAudio(true);
        }

        // Stop berserk chase looping audio
        audioHandler.whisperChaseAudioSource.Stop();

        // Let the animator know that berserk mode is over
        animator.SetTrigger("endBerserk");

        currentlyBerserk = false;
    }

    /// <summary>
    ///     If Shade catches the player while berserk, this method turns off berserk-related audio and gameobjects but keeps the berserk
    ///     body material active.
    /// </summary>
    public void CaughtPlayerWhileBerserk()
    {
        // Turn off berserk-related objects
        MonsterHelper.SetGameObjectsActive(berserkObjects, false);

        // Stop berserk chase looping audio
        audioHandler.whisperChaseAudioSource.Stop();
    }

    /// <summary>
    ///     Loops through the animators runtime animator controller clips and gets the length of the clip "ShadeEnterBerserk". Note:
    ///     the current speed of the clip in the animator is 3, and if that changes clipSpeed also needs to be updated as well.
    /// </summary>
    /// <returns>The length of the animation clip ShadeEnterBerserk.</returns>
    private float GetEnterBerserkDelay(float clipSpeed)
    {
        for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            if (animator.runtimeAnimatorController.animationClips[i].name == "ShadeEnterBerserk")
            {
                return animator.runtimeAnimatorController.animationClips[i].length / clipSpeed;
            }
        }

#if UNITY_EDITOR
        Debug.LogWarning($"{name} could not find an animation clip named \"ShadeEnterBerserk\".");
#endif

        // If the clip is not found, just return 1/3 of a second as a defualt
        return 0.33f;
    }
}
