using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private SoundEffectSO flashlightClickSFX;
    [SerializeField] private SoundEffectSO flashlightBreakSFX;

    [Header("Light Collisions")]
    [SerializeField] private FlashlightCollisions flashlightCollisions;

    [Header("Stun Flicker")]
    [SerializeField, Min(0f)] private float stunDuration = 4f;
    [SerializeField, Min(0f)] private float baseIntensity = 15f;
    [SerializeField] private Vector2 intensity = new Vector2(8f, 5f);
    [SerializeField] private Vector2 interval = new Vector2(0.1f, 0.25f);

    private Light lightSource;
    private AudioSource audioSource;
    private FlickeringLight flickeringLight;
    private Stunnable stunnable;

    private float startingIntensity; // Helpful to cache the starting intensity so when Flicker() is called the light returns back to normal when finished
    private bool disabled;

    private void Awake()
    {
        stunnable = GetComponent<Stunnable>();

        lightSource = GetComponentInChildren<Light>();
        if (lightSource == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} does not have a lightsource. Disabling Flashlight script.");
#endif
            enabled = false;
            return;
        }
        else
        {
            startingIntensity = lightSource.intensity;
        }

        audioSource = GetComponent<AudioSource>();
#if UNITY_EDITOR
        if (audioSource == null)
        {
            Debug.LogWarning($"{gameObject.name}'s Flashlight script was unable to find an audiosource. Unable to play click sfx.");
        }

        if (flashlightCollisions == null)
        {
            Debug.LogWarning($"{gameObject.name}'s lightDetectionCollider null. Unable for other scripts to detect if being lit by {gameObject.name}.");
        }
#endif

        disabled = false;
    }

    private void OnEnable()
    {
        InputFlashlight.OnFlashlightToggle += HandleFlashlightToggle;

        DeathscreenJumpscareBase.OnDeathscreenJumpscare += TurnOff;

        LevelController.OnDisablePlayerFlashlight += FlickerAndDisable;
        LevelController.OnFlickerPlayerFlashlight += Flicker;

        if (stunnable != null)
        {
            stunnable.OnStunned += Stun;
            stunnable.OnStunEnd += StopStun;
        }
    }

    private void OnDisable()
    {
        InputFlashlight.OnFlashlightToggle -= HandleFlashlightToggle;

        DeathscreenJumpscareBase.OnDeathscreenJumpscare -= TurnOff;

        LevelController.OnDisablePlayerFlashlight -= FlickerAndDisable;
        LevelController.OnFlickerPlayerFlashlight -= Flicker;

        if (stunnable != null)
        {
            stunnable.OnStunned -= Stun;
            stunnable.OnStunEnd -= StopStun;
        }
    }

    /// <summary>
    ///     Automatically turns the flashlight on/off depending on if the flashlight is off/on and also plays click SFX.
    /// </summary>
    private void HandleFlashlightToggle()
    {
        // Play click sfx regardless of whether or not the flashlight is disabled
        PlayClickSFX();

        if (disabled)
        {
            return;
        }

        (IsOn() ? (Action)TurnOff : TurnOn)();
    }

    /// <summary>
    ///     Check if the flashlight is currently on.
    /// </summary>
    /// <returns>True if the flashlight is on, false if otherwise.</returns>
    public bool IsOn()
    {
        return lightSource.isActiveAndEnabled;
    }

    /// <summary>
    ///     Turns the light source and collider on.
    /// </summary>
    private void TurnOn()
    {
        lightSource.enabled = true;

        if (flashlightCollisions != null && !flashlightCollisions.gameObject.activeSelf)
        {
            flashlightCollisions.gameObject.SetActive(true);
        }
    }

    /// <summary>
    ///     Turns the light source and collider off.
    /// </summary>
    private void TurnOff()
    {
        lightSource.enabled = false;

        if (flashlightCollisions != null && flashlightCollisions.gameObject.activeSelf)
        {
            flashlightCollisions.gameObject.SetActive(false);
        }
    }

    private void PlayClickSFX()
    {
        if (flashlightClickSFX != null && audioSource != null)
        {
            flashlightClickSFX.Play(audioSource);
        }
    }

    /// <summary>
    ///     Flickers the light for a set amount of time.
    /// </summary>
    private void Flicker(float timeToFlicker, float newBaseIntensity, Vector2 intensity, Vector2 interval)
    {
        StartCoroutine(FlickerRoutine(timeToFlicker, newBaseIntensity, intensity, interval));
    }

    /// <summary>
    ///     Starts the flicker routine in which the light will flicker for a specified amount of time and then become
    ///     disabled after the flickering is finished.
    /// </summary>
    private void FlickerAndDisable(float timeToFlicker, float newBaseIntensity, Vector2 intensity, Vector2 interval, float finalFade)
    {
        // Turn the flashlight on if it is off to make it extra clear it is going out
        if (!IsOn())
        {
            TurnOn();
        }

        // Disable before starting the routine so the player cannot turn the flashlight on/off while the
        // flickering routine is going on
        disabled = true;
        StartCoroutine(FlickerAndDisableRoutine(timeToFlicker, newBaseIntensity, intensity, interval, finalFade));
    }

    /// <summary>
    ///     Starts flickering the light, and then after the flicker duration is complete the light is turned off and the
    ///     flashlight becomes disabled.
    /// </summary>
    /// <param name="duration">The total duration of the flickering.</param>
    /// <param name="baseIntensity">The new base intensity of the light.</param>
    /// <param name="intensity">The min/max the intensity will randomly go down/up to.</param>
    /// <param name="interval">The min/max interval in seconds between intensity displacements.</param>
    /// <param name="finalFadeTime">After the flicker duration is up, this is the final time it will take to fade the light's intensity to 0.</param>
    private IEnumerator FlickerAndDisableRoutine(float duration, float baseIntensity, Vector2 intensity, Vector2 interval, float finalFadeTime)
    {
        // Wait until the flickering is finished before disabling and turning the light off
        yield return StartCoroutine(FlickerRoutine(duration, baseIntensity, intensity, interval));

        if (audioSource != null && flashlightBreakSFX != null)
        {
            flashlightBreakSFX.PlayOneShot(audioSource);
        }

        // Fade to nothing
        float lerp = 0f;
        float startingIntensity = lightSource.intensity;
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / finalFadeTime);
            lightSource.intensity = Mathf.Lerp(startingIntensity, 0, lerp);

            yield return null;
        }

        // Make sure light source and collider are not enabled
        TurnOff();
    }

    /// <summary>
    ///     Flickers the light by randomly changing the baseIntensity +- intensity with a randomly chosen interval.
    /// </summary>
    /// <param name="duration">The total duration of the flickering.</param>
    /// <param name="baseIntensity">The new base intensity of the light.</param>
    /// <param name="intensity">The min/max intensity variation.</param>
    /// <param name="interval">The min/max intervals between intensity changes.</param>
    /// <returns></returns>
    private IEnumerator FlickerRoutine(float duration, float baseIntensity, Vector2 intensity, Vector2 interval)
    {
        float elapsedTime = 0f;

        // If the flashlight does not have a FlickeringLight component, add one
        if (flickeringLight == null)
        {
            flickeringLight = GetComponent<FlickeringLight>();

            if (flickeringLight == null)
            {
                flickeringLight = gameObject.AddComponent<FlickeringLight>();
            }
        }

        // Set the flickeringLight's variables
        flickeringLight.SetLight(lightSource);
        flickeringLight.SetIntensityDisplacement(intensity.x, intensity.y);
        flickeringLight.SetIntervalTiming(interval.x, interval.y);
        flickeringLight.SetDisplacement(0); // Make sure the flashlight's position doesn't get overridden
        flickeringLight.OverrideBaseIntensity(baseIntensity);

        flickeringLight.enabled = true;

        // Flicker
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Turn off the flickering light script
        flickeringLight.enabled = false;

        // Make sure the light's intensity goes back to what it was previously
        lightSource.intensity = startingIntensity;
    }

    /// <summary>
    ///     Disable the flashlight and turn it off.
    /// </summary>
    private void Stun()
    {
        disabled = true;
        StartCoroutine(FlickerRoutine(stunDuration, baseIntensity, intensity, interval));
    }
    
    /// <summary>
    ///     Enable the flashlight and turn it on.
    /// </summary>
    private void StopStun()
    {
        disabled = false;
        TurnOn();
    }
}
