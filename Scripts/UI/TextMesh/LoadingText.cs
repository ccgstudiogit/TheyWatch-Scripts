using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI), typeof(CanvasGroup))]
public class LoadingText : MonoBehaviour
{
    [Header("Period Animation")]
    [Tooltip("The maximum amount of periods following \"Loading\"")]
    [SerializeField] private int maxPeriodCount = 3;
    [Tooltip("A period will appear every X seconds. If the max is reached, all periods are cleared")]
    [SerializeField] private float periodEveryXSeconds = 1f;

    [Header("Fade Animation")]
    [Tooltip("The amount of time it will take for the loading text to fade in")]
    [SerializeField] private float fadeTime = 0.5f;

    private TextMeshProUGUI loadingText;
    private CanvasGroup canvasGroup;

    private string startingText;

    private Coroutine animationRoutine = null;

    private void Awake()
    {
        loadingText = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        startingText = loadingText.text;
    }

    private void OnEnable()
    {
        loadingText.text = startingText; // Make sure to reset the text every time this game object is turned on
        animationRoutine = StartCoroutine(Animation());
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        // Turn the text invisible so the next time this is activated, it's already invisible
        canvasGroup.alpha = 0f;
    }

    private IEnumerator Animation()
    {
        int currentPeriodCount = 0;

        // Fade In
        float lerp = 0f;
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / fadeTime);
            canvasGroup.alpha = Mathf.Lerp(0, 1, lerp);

            yield return null;
        }

        // Do the loading. . . animation
        while (enabled)
        {
            yield return new WaitForSeconds(periodEveryXSeconds);

            currentPeriodCount++;

            if (currentPeriodCount > maxPeriodCount)
            {
                currentPeriodCount = 0;
                loadingText.text = startingText;
            }
            else
            {
                loadingText.text = startingText + new string('.', currentPeriodCount);
            }
        }
    }
}
