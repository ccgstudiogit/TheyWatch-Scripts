using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StarGlow : MonoBehaviour
{
    [Header("Alpha Change Settings")]
    [SerializeField, Range(0f, 1f)] private float maxAlphaChange = 0.375f;
    private float minAlpha;

    [SerializeField, Min(0f)] private float minInterval = 0.25f;
    [SerializeField, Min(0f)] private float maxInterval = 0.65f;

    private float baseAlpha;
    private float targetAlpha;
    private float lastAlpha;

    private float interval = -1f;
    private float timer = 0f;

    private Image image;
    private Color baseColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        baseColor = image.color;
        baseAlpha = image.color.a;
        minAlpha = baseAlpha - maxAlphaChange;

        if (minInterval > maxInterval)
        {
            (minInterval, maxInterval) = (maxInterval, minInterval);
        }
    }

    private void OnDisable()
    {
        image.color = baseColor;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > interval)
        {
            timer = 0f;

            // Get a random new alpha
            lastAlpha = image.color.a;
            targetAlpha = Random.Range(minAlpha, baseAlpha);

            interval = Random.Range(minInterval, maxInterval);
        }

        // Lerp the color's alpha to create a pulsating glow effect
        float alpha = Mathf.Lerp(lastAlpha, targetAlpha, timer / interval);
        Color color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        image.color = color;
    }
}
