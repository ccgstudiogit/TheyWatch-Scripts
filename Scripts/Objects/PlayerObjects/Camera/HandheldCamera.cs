using System;
using System.Collections;
using UnityEngine;

public class HandheldCamera : MonoBehaviour
{
    // Lets DisplayInputActionCooldown.cs display the picture cooldown length via the slider (float is the cooldown length)
    public static event Action<float> OnPictureTaken;

    [Header("Flash Light")]
    [SerializeField] private Light flashLightSource;
    [Tooltip("The time it takes for the light source to fade to it's flash intensity to 0")]
    [SerializeField, Min(0.1f)] private float flashFadeTime = 1f;
    private float lightSourceStartingIntensity;

    [Header("Cooldown")]
    [SerializeField, Min(0f)] private float cooldownDuration = 4.5f;
    private bool onCooldown;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO shutterFlashSFX;

    [Header("Light Collisions")]
    [SerializeField] private FlashlightCollisions lightCollisions; // Re-using FlashlightCollisions for camera flash collider detection

    private void Awake()
    {
        if (flashLightSource != null)
        {
            lightSourceStartingIntensity = flashLightSource.intensity;
            flashLightSource.intensity = 0;
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"{gameObject.name}'s flashLightSource null!");
        }
#endif

        onCooldown = false;
        SetLightColliderActive(false);
    }

    private void OnEnable()
    {
        InputFlashlight.OnFlashlightToggle += TakePicture;
    }

    private void OnDisable()
    {
        InputFlashlight.OnFlashlightToggle -= TakePicture;
    }

    /// <summary>
    ///     Take a picture (flash the camera).
    /// </summary>
    public void TakePicture()
    {
        if (onCooldown)
        {
            return;
        }

        onCooldown = true;
        OnPictureTaken?.Invoke(cooldownDuration);
        this.Invoke(() => onCooldown = false, cooldownDuration);

        if (flashLightSource != null)
        {
            // Begin the flash and fade it away
            flashLightSource.intensity = lightSourceStartingIntensity;
            StartCoroutine(FadeFlash(flashLightSource, flashFadeTime));
        }

        SetLightColliderActive(true);

        if (shutterFlashSFX != null)
        {
            shutterFlashSFX.Play();
        }
    }

    /// <summary>
    ///     Fade a light source's intensity from its current intensity to 0 over a specified duration.
    /// </summary>
    /// <param name="lightSource">The light source that should have its intensity brought to 0.</param>
    /// <param name="fadeTime">The time it takes for the light's intensity to reach 0.</param>
    private IEnumerator FadeFlash(Light lightSource, float fadeTime)
    {
        float lerp = 0f;
        float startIntensity = lightSource.intensity;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / fadeTime);
            lightSource.intensity = Mathf.Lerp(startIntensity, 0, lerp);

            yield return null;
        }

        SetLightColliderActive(false);
        lightSource.intensity = 0;
    }

    /// <summary>
    ///     Set the light collider game object to be active/in-active.
    /// </summary>
    /// <param name="active">Whether or not the light collider game object should be active.</param>
    private void SetLightColliderActive(bool active)
    {
        if (lightCollisions != null)
        {
            lightCollisions.gameObject.SetActive(active);
        }
    }
}
