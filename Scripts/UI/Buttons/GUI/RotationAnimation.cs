using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RotationAnimation : MonoBehaviour
{
    [Header("- Rotation Settings -")]
    [Header("Scaling")]
    [SerializeField] private float scaleIncrease = 0.2f;
    [SerializeField, Min(0)] private float scaleChangetime = 0.15f;
    private Vector2 startingScale = new Vector2(1, 1);

    [Header("Rotating")]
    [SerializeField, Range(0, 360)] private float rotationChange = 2.25f;
    private Quaternion startingRotation;

    [SerializeField, Min(0)] private float minInterval = 0.1f;
    [SerializeField, Min(0.1f)] private float maxInterval = 0.2f;

    private RectTransform rect;

    private Coroutine shakeRoutine = null;
    private bool shaking;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        startingScale = rect.localScale;
        shaking = false;

        startingRotation = rect.localRotation;
    }

    private void OnDisable()
    {
        shaking = false;

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        rect.localScale = startingScale;
        rect.localRotation = startingRotation;
    }

    public void BeginShake()
    {
        if (shakeRoutine != null)
        {
            return;
        }

        shaking = true;
        shakeRoutine = StartCoroutine(ShakeRoutine(
            rect,
            startingScale,
            new Vector2(startingScale.x + scaleIncrease, startingScale.y + scaleIncrease),
            scaleChangetime,
            rotationChange,
            minInterval,
            maxInterval
        ));
    }

    public void EndShake()
    {
        shaking = false;
    }

    private IEnumerator ShakeRoutine(RectTransform rect, Vector2 startingScale, Vector2 targetScale, float scaleDuration, float rotation, float minInterval, float maxInterval)
    {
        float lerp = 0f;
        float interval;

        Quaternion lastRotation;
        Quaternion newRotation;

        bool rotatingDown = false;

        // Change rect's size to the increased amount
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / scaleDuration);
            rect.localScale = Vector2.Lerp(startingScale, targetScale, lerp);

            yield return null;
        }

        // Start "shaking" the rect transform by rotating it around the z-axis
        while (shaking)
        {
            rotatingDown = !rotatingDown;
            lerp = 0f;

            lastRotation = rect.localRotation;
            newRotation = rotatingDown ?
                Quaternion.Euler(startingRotation.x, startingRotation.y, startingRotation.z - rotation) :
                Quaternion.Euler(startingRotation.x, startingRotation.y, startingRotation.z + rotation);

            interval = Random.Range(minInterval, maxInterval);

            while (lerp < 1)
            {
                lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / interval);
                rect.localRotation = Quaternion.Lerp(lastRotation, newRotation, lerp);

                yield return null;
            }
        }

        rect.localRotation = startingRotation;

        // Change rect's size back to it's starting amount
        lerp = 0f;
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / scaleDuration);
            rect.localScale = Vector2.Lerp(targetScale, startingScale, lerp);

            yield return null;
        }

        rect.localScale = startingScale;

        shakeRoutine = null;
    }
}
