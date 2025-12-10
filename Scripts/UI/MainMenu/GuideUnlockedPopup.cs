using System.Collections;
using UnityEngine;
using TMPro;

public class GuideUnlockedPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI guideUnlockedText;
    private Color originalTextColor;

    [SerializeField] private ShakeAnimation shakeAnimation;

    [Header("Settings")]
    [Tooltip("How many seconds the popup will remain visible for")]
    [SerializeField, Min(0f)] private float popupDuration = 6f;
    [Tooltip("How long it will take for the popup's alpha to fade to 0")]
    [SerializeField] private float fadeTime = 1f;

    private void Awake()
    {
        if (guideUnlockedText == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s guideUnlockedText null! Disabling {name}.");
#endif
            enabled = false;
            return;
        }

        originalTextColor = guideUnlockedText.color;

        // Set the text's alpha to 0 immediately
        SetAlpha(0f);
    }

    private void OnDisable()
    {
        // Prevents an issue where if the player opens the guides before the coroutine finishes, the popup would stay visible due to
        // the coroutine being interrupted
        SetAlpha(0f);
    }

    /// <summary>
    ///     Starts the popup coroutine.
    /// </summary>
    public void ActivatePopup()
    {
        StartCoroutine(PopupRoutine(guideUnlockedText, originalTextColor, popupDuration, fadeTime, 1f, 0f));
    }

    /// <summary>
    ///     Handles keeping the popup message up for the specified duration before fading away.
    /// </summary>
    /// <param name="text">The popup message's text component.</param>
    /// <param name="textColor">The text's original color.</param>
    /// <param name="duration">The duration of the popup message (how long it will be visible).</param>
    /// <param name="fadeTime">The time it takes for the text's alpha to fade to 0 after the duration is up.</param>
    /// <param name="startAlpha">The starting alpha of the popup message.</param>
    /// <param name="targetAlpha">The target alpha of the popup message.</param>
    private IEnumerator PopupRoutine(TextMeshProUGUI text, Color textColor, float duration, float fadeTime, float startAlpha, float targetAlpha)
    {
        if (shakeAnimation != null)
        {
            shakeAnimation.BeginShake();
        }

        // Make the text visible
        text.color = new Color(textColor.r, textColor.g, textColor.b, startAlpha);

        // Wait the specified duration
        yield return new WaitForSeconds(duration);

        // Fade the text out
        float lerp = 0f;
        while (lerp < 1f)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / fadeTime);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);
            text.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

            yield return null;
        }

        text.color = new Color(textColor.r, textColor.g, textColor.b, targetAlpha);

        if (shakeAnimation != null)
        {
            shakeAnimation.EndShake();
        }
    }

    /// <summary>
    ///     Set the alpha of the text's color.
    /// </summary>
    private void SetAlpha(float alpha)
    {
        guideUnlockedText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
    }
}
