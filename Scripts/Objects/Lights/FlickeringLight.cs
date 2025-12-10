using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Flicker Settings")]
    
    [Tooltip("The intensity will increase randomly up by this amount")]
    [SerializeField] private float positiveIntensityDisplacement = 1f;

    [Tooltip("The intensity will decrease randomly down by this amount")]
    [SerializeField] private float negativeIntensityDisplacement = 1f;

    [Tooltip("The min interval between flickers in seconds")]
    [SerializeField] private float minInterval = 0.5f;

    [Tooltip("The max interval between flickers in seconds")]
    [SerializeField] private float maxInterval = 1.5f;

    [Tooltip("If this is higher than 0, the light will be displaced up this amount randomly (creates dancing shadows)")]
    [SerializeField] private float maxDisplacement = 0.25f;

    [Tooltip("Can control the displacement movement (e.g. (0, 1, 0) will only move the light in the y-direction)")]
    [SerializeField] private Vector3 displacement = new Vector3(1, 1, 1);

    private Light lightToFlicker;

    // Intensity
    private float baseIntensity;
    private float targetIntensity;
    private float lastIntensity;

    // Helpers
    private float interval = 0f;
    private float timer = 0f;
    private bool baseIntensityOverridden = false;

    // Position
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private Vector3 origin;

    private void Awake()
    {
        lightToFlicker = GetComponent<Light>();

        // Make sure the minimum is not accidentally greater than the maximum
        if (minInterval >= maxInterval)
        {
            (minInterval, maxInterval) = (maxInterval, minInterval);
        }
    }

    private void Start()
    {
        origin = transform.position;
        lastPosition = origin;

        if (lightToFlicker != null && !baseIntensityOverridden)
        {
            baseIntensity = lightToFlicker.intensity;
        }
    }

    private void Update()
    {
        if (lightToFlicker == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer > interval)
        {
            timer = 0f;

            // Get a random new intensity
            lastIntensity = lightToFlicker.intensity;
            targetIntensity = Random.Range(baseIntensity - negativeIntensityDisplacement, baseIntensity + positiveIntensityDisplacement);

            // Get a random new interval
            interval = Random.Range(minInterval, maxInterval);

            // Get a random new displacement
            if (maxDisplacement > 0.01f)
            {
                targetPosition = origin + new Vector3(Random.Range(0, displacement.x), Random.Range(0, displacement.y), Random.Range(0, displacement.z)) * maxDisplacement;
                lastPosition = lightToFlicker.transform.position;
            }
        }

        // Lerp the intensity and position
        lightToFlicker.intensity = Mathf.Lerp(lastIntensity, targetIntensity, timer / interval);

        if (maxDisplacement > 0.01f)
        {
            lightToFlicker.transform.position = Vector3.Lerp(lastPosition, targetPosition, timer / interval);
        }
    }

    /// <summary>
    ///     Set the light source of FlickeringLight.
    /// </summary>
    /// <param name="lightSource">The light to flicker.</param>
    public void SetLight(Light lightSource)
    {
        lightToFlicker = lightSource;
    }

    /// <summary>
    ///     Set the positive and negative intensity displacement of the light source.
    /// </summary>
    /// <param name="negative">How much the light's intensity can increase in the negative direction.</param>
    /// <param name="positive">How much the light's intensity can increase in the positive direction.</param>
    public void SetIntensityDisplacement(float negative, float positive)
    {
        if (negative > positive)
        {
            (negative, positive) = (positive, negative);
        }

        negativeIntensityDisplacement = negative;
        positiveIntensityDisplacement = positive;
    }

    /// <summary>
    ///     Set the minimum and maximum intervals in seconds for the light flicker.
    /// </summary>
    /// <param name="min">The minimum interval in seconds.</param>
    /// <param name="max">The maximum interval in seconds.</param>
    public void SetIntervalTiming(float min, float max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        minInterval = min;
        maxInterval = max;
    }

    /// <summary>
    ///     Set the maximum displacement this light should move whilst flickering.
    /// </summary>
    /// <param name="max">The max displacement.</param>
    public void SetDisplacement(float max)
    {
        maxDisplacement = max;
    }

    /// <summary>
    ///     Set the maximum displacement this light should move whilst flickering.
    /// </summary>
    /// <param name="max">The max displacement.</param>
    /// <param name="displacement">Controls the displacement direction.</param>
    public void SetDisplacement(float max, Vector3 displacement)
    {
        maxDisplacement = max;
        this.displacement = displacement;
    }

    /// <summary>
    ///     Can be used to override the base intensity, making the intensity displacement displace around a new base instead
    ///     of the intensity being displaced around the light's starting intensity.
    /// </summary>
    /// <param name="newBaseIntensity">The new base intensity that the intensity displacement's will be centered around.</param>
    public void OverrideBaseIntensity(float newBaseIntensity)
    {
        baseIntensity = newBaseIntensity;
        baseIntensityOverridden = true;
    }
}
