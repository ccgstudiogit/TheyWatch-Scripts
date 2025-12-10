using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenCoverPanel : MonoBehaviour
{
    [SerializeField] private bool hideOnStart = true;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (hideOnStart)
        {
            canvasGroup.alpha = 0;
        }
    }

    private void OnEnable()
    {
        LevelController.OnSetScreenCoverPanel += SetAlpha;
    }

    private void OnDisable()
    {
        LevelController.OnSetScreenCoverPanel -= SetAlpha;
    }

    private void SetAlpha(float targetAlpha, float time)
    {
        StartCoroutine(LerpAlpha(targetAlpha, time));
    }

    private IEnumerator LerpAlpha(float targetAlpha, float duration)
    {
        if (duration <= 0)
        {
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        float lerp = 0f;
        float startingAlpha = canvasGroup.alpha;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / duration);
            canvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerp);

            yield return null;
        }
    }
}
