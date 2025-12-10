using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DeathscreenDistortionHandler : MonoBehaviour
{
    [Header("Deathscreen Distortion Effect")]

    [Tooltip("If enabled, once a monster catches the player, a fullscreen distortion effect will take place")]
    [SerializeField] private bool enableFullscreenDistortion = true;

    [SerializeField] private ScriptableRendererFeature fullscreenDistortionFeature;

    [SerializeField] private Material fullscreenDistortionMat;
    private string intensityProperty = "_NoiseIntensity"; // ***Only change if the material's property name changes
    private string saturationProperty = "_Saturation"; // ***Only change if the material's property name changes

    [Tooltip("The intensity of the distortion effect")]
    [SerializeField, Range(0, 1)] private float distortionIntensity = 1f;

    [Tooltip("Reduce the saturation to this amount for the effect")]
    [SerializeField, Range(0, 1)] private float saturation = 0f;

    [Tooltip("The time it takes to transition from the normal screen to the distorted screen")]
    [SerializeField, Min(0)] private float transitionTime = 1.1f;

    [Tooltip("The delay before the effect will transition")]
    [SerializeField, Min(0)] private float effectDelayTime = 0.5f;

    private Coroutine fullscreenDistortionRoutine; // Makes sure multiple routines are not accidentally run

    private void Awake()
    {
        if (fullscreenDistortionFeature != null)
        {
            fullscreenDistortionFeature.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Reset the fullscreen distortion material on end of game
        if (fullscreenDistortionFeature != null && fullscreenDistortionMat != null)
        {
            fullscreenDistortionMat.SetFloat(intensityProperty, 0);
            fullscreenDistortionMat.SetFloat(saturationProperty, 1);
        }
    }

    /// <summary>
    ///     Begin the fullscreen distortion effect
    /// </summary>
    public void BeginFullscreenDistortion()
    {
        if (!enableFullscreenDistortion || fullscreenDistortionFeature == null || fullscreenDistortionMat == null || fullscreenDistortionRoutine != null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} attempted to begin the fullscreen distortion effect on player death but something went wrong.");
#endif
            return;
        }

        // Set the render feature to become and active and start the coroutine
        fullscreenDistortionFeature.SetActive(true);
        fullscreenDistortionRoutine = StartCoroutine(FullscreenDistortionRoutine(
            fullscreenDistortionMat,
            distortionIntensity,
            saturation,
            transitionTime
        ));
    }

    /// <summary>
    ///     Handles transitioning from the regular screen to a distorted screen.
    /// </summary>
    /// <param name="feature">The scriptable renderer feature that should be used.</param>
    /// <param name="mat">The fullscreen material that will create the effect.</param>
    /// <param name="intensity">The intensity of the noise on the material.</param>
    /// <param name="saturation">The saturation effect of the material (1 == full color, 0 == grayscale).</param>
    /// <param name="duration">The time it takes to transition from the regular screen to the distorted screen.</param>
    private IEnumerator FullscreenDistortionRoutine(Material mat, float intensity, float saturation, float duration)
    {
        float lerp = 0f; // Lerp from 0-1 over the duration (used for intensity)
        float inverseLerp = 1f; // Lerp from 1-0 over the duration (used for saturation)

        if (duration <= 0)
        {
            mat.SetFloat(intensityProperty, distortionIntensity);
            mat.SetFloat(saturationProperty, saturation);
            yield break;
        }

        // Slight delay before transitioning to the distortion fullscreen effect
        yield return new WaitForSeconds(effectDelayTime);

        while (lerp < 1 && inverseLerp > 0)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            inverseLerp = Mathf.MoveTowards(inverseLerp, 0, Time.deltaTime / duration);

            mat.SetFloat(intensityProperty, Mathf.Lerp(0, intensity, lerp));
            mat.SetFloat(saturationProperty, Mathf.Lerp(saturation, 1, inverseLerp));

            yield return null;
        }

        mat.SetFloat(intensityProperty, intensity);
        mat.SetFloat(saturationProperty, saturation);

        fullscreenDistortionRoutine = null;
    }
}
