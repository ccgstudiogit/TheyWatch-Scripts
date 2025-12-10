using System.Collections;
using UnityEngine;

public class TrailerLightIntensity : MonoBehaviour
{
    [Header("Intensity Change")]
    [SerializeField] private AnimationCurve curve;

    [Header("Optional Emissive Material")]
    [SerializeField] private Material material;
    [SerializeField] private float targetIntensity = 2.5f;
    private Color baseColor;

    [Header("Duration")]
    [Tooltip("The delay before starting the intensity changes")]
    [SerializeField, Min(0f)] private float delay = 5f;
    [Tooltip("The duration, in seconds, for how long it will take to for the light's intensity to move through the curve")]
    [SerializeField, Min(0f)] private float duration = 5f;

    private Light thisLight;

    private void Awake()
    {
        thisLight = GetComponent<Light>();
    }

    private void Start()
    {
        if (material != null)
        {
            baseColor = material.GetColor("_EmissionColor");
            material.SetColor("_EmissionColor", new Color(baseColor.r, baseColor.g, baseColor.b) * 0);
        }

        thisLight.intensity = 0;
        this.Invoke(() => StartCoroutine(LightIntensity()), delay);
    }

    private IEnumerator LightIntensity()
    {
        float lerp = 0f, smoothLerp;

        // Get the animation curve's start and end points (x-axis)
        float curveStart = curve.keys[0].time;
        float curveEnd = curve.keys[curve.length - 1].time;

        float materialStrength;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            smoothLerp = Mathf.SmoothStep(0, 1, lerp);

            float curveValue = curve.Evaluate(Mathf.Lerp(curveStart, curveEnd, smoothLerp));
            thisLight.intensity = curveValue;

            // Update the material's emissive intensity
            if (material != null)
            {
                // Remap the intensity from the curve's value to between 0 and the target intensity (0 == black)
                materialStrength = HelperMethods.Remap(curveValue, 0, curve.keys[curve.length - 1].value, 0, targetIntensity);
                material.SetColor("_EmissionColor", new Color(baseColor.r, baseColor.g, baseColor.b) * materialStrength);
            }

            yield return null;
        }

        thisLight.intensity = curve.keys[curve.length - 1].value;
    }
}
