using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PingIcon : MonoBehaviour
{
    public bool activelyPinged { get; private set; }

    [Header("Fade Settings")]
    [Tooltip("The time it takes for this icon to fade away once revealed by the radar sweep")]
    [SerializeField, Min(0f)] private float fadeTime = 0.45f;

    private RectTransform rectTransform;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    ///     Activate this ping icon by revealing it and having it slowly fade away.
    /// </summary>
    /// <param name="pingPosition">The location where this icon should be when the ping starts.</param>
    public void Ping(Vector2 pingPosition)
    {
        rectTransform.localPosition = pingPosition;
        activelyPinged = true;
        Reveal();
        FadeAway();
    }

    /// <summary>
    ///     Reveal this ping icon by setting its alpha to 1.
    /// </summary>
    public void Reveal()
    {
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    ///     Fade this icon away until its alpha reaches 0.
    /// </summary>
    public void FadeAway()
    {
        if (fadeRoutine == null)
        {
            fadeRoutine = StartCoroutine(FadeRoutine());
        }
    }

    /// <summary>
    ///     Hide this icon by setting its alpha to 0.
    /// </summary>
    public void Hide()
    {
        activelyPinged = false;
        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeRoutine(float targetAlpha = 0f)
    {
        float lerp = 0f;
        float startingAlpha = canvasGroup.alpha;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / fadeTime);
            canvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerp);

            yield return null;
        }

        fadeRoutine = null;
        Hide();
    }
}
