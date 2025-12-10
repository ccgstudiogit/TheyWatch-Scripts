using System.Collections;
using UnityEngine;

public class ShakeAnimation : MonoBehaviour
{
    public bool shaking { get; private set; }

    [Header("Rect Transform")]
    [SerializeField] private RectTransform rectTransform;

    [Header("Scale Change")]
    [Tooltip("Add this amount to the scale of the rect transform ")]
    [SerializeField] private float scaleChange = 0.2f;

    [Tooltip("How long it will take for the scale to increase/decrease")]
    [SerializeField, Min(0)] private float scaleChangeTime = 0.15f;

    private Vector2 startingScale = new Vector2(1f, 1f);
    private Vector2 restingPosition = new Vector2(0f, 0f);

    [Header("Shaking")]
    [Tooltip("If enabled, the shake animation will include changing the x-axis")]
    [SerializeField] private bool includeX = false;
    [Tooltip("The min/max amount of shake on the x-axis")]
    [SerializeField] private float xShake = 0f;

    [Tooltip("If enabled, the shake animation will include changing the y-axis")]
    [SerializeField] private bool includeY = true;
    [Tooltip("The min/max amount of shake on the y-axis")]
    [SerializeField] private float yShake = 1.3f;

    [Header("Shake Intervals")]
    [Tooltip("The minimum amount of time in between movements")]
    [SerializeField, Min(0)] private float minInterval = 0.1f;
    [Tooltip("The maximum amount of time in between movements")]
    [SerializeField, Min(0.1f)] private float maxInterval = 0.15f;

    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (rectTransform == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s RectTransform reference null. Disabling {name}.");
#endif
            enabled = false;
            return;
        }

        startingScale = rectTransform.localScale;
        restingPosition = rectTransform.localPosition;

        if (minInterval > maxInterval)
        {
            (minInterval, maxInterval) = (maxInterval, minInterval);
        }

        shaking = false;
    }

    private void OnDisable()
    {
        shaking = false;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        rectTransform.localScale = startingScale;
        rectTransform.localPosition = restingPosition;
    }

    /// <summary>
    ///     Begin the shake animation.
    /// </summary>
    public void BeginShake()
    {
        if (shakeRoutine != null)
        {
            return;
        }

        shaking = true;
        shakeRoutine = StartCoroutine(ShakeRoutine(
            rectTransform,
            startingScale,
            new Vector2(startingScale.x + scaleChange, startingScale.y + scaleChange),
            scaleChangeTime,
            xShake,
            yShake,
            new Vector2(minInterval, maxInterval)
        ));
    }

    /// <summary>
    ///     End the shake animation.
    /// </summary>
    public void EndShake()
    {
        shaking = false;
    }

    private IEnumerator ShakeRoutine(RectTransform rect, Vector2 startingScale, Vector2 targetScale, float scaleDuration, float xShake, float yShake, Vector2 intervals)
    {
        float lerp = 0f;
        float interval;

        Vector2 lastPosition;
        Vector2 newPosition;

        bool movingDown = false; // Keeps track of cycling the rect transfrom between moving up/down

        // Change rect's size to the increased amount
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / scaleDuration);
            rect.localScale = Vector2.Lerp(startingScale, targetScale, lerp);

            yield return null;
        }

        // Handle the shaking animation
        while (shaking)
        {
            lerp = 0f;
            movingDown = !movingDown;

            lastPosition = rect.localPosition;
            newPosition = movingDown ?
                new Vector2(includeX ? restingPosition.x - xShake : restingPosition.x, includeY ? restingPosition.y - yShake : restingPosition.y) :
                new Vector2(includeX ? restingPosition.x + xShake : restingPosition.x, includeY ? restingPosition.y + yShake : restingPosition.y);

            interval = Random.Range(intervals.x, intervals.y);

            while (lerp < 1)
            {
                lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / interval);
                rect.localPosition = Vector2.Lerp(lastPosition, newPosition, lerp);

                yield return null;
            }
        }

        rect.localPosition = restingPosition;
        shakeRoutine = null;

        // Change rect's size back to it's starting amount
        lerp = 0f;
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / scaleDuration);
            rect.localScale = Vector2.Lerp(targetScale, startingScale, lerp);

            yield return null;
        }

        rect.localScale = startingScale;
    }
}
