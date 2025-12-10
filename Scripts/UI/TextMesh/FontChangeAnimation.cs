using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FontChangeAnimation : MonoBehaviour
{
    [Tooltip("This script will change the font of the text by + and - this amount through lerping")]
    [SerializeField] private float fontChange = 10f;

    private float minFontSize;
    private float maxFontSize;

    [Tooltip("The time it takes for the font to move from the min/max font size to the max/min font size")]
    [SerializeField, Min(0.1f)] private float time = 0.5f;

    private TextMeshProUGUI text;
    private Coroutine animationRoutine = null;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        minFontSize = text.fontSize - fontChange;
        maxFontSize = text.fontSize + fontChange;
    }

    private void OnEnable()
    {
        if (animationRoutine == null)
        {
            animationRoutine = StartCoroutine(BackAndForth(time));
        }
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

    private IEnumerator BackAndForth(float duration)
    {
        float startSize;
        float targetSize;
        bool increasing = true;

        while (true)
        {
            float lerp = 0f;
            startSize = text.fontSize;
            targetSize = increasing ? maxFontSize : minFontSize;

            while (lerp < 1)
            {
                lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
                text.fontSize = Mathf.Lerp(startSize, targetSize, Mathf.SmoothStep(0, 1, lerp)); // Smoothstep for smoother in/out

                yield return null;
            }

            increasing = !increasing;
        }
    }
}
