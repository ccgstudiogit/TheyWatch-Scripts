using System.Collections;
using UnityEngine;
using TMPro;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade Settings")]
    [Tooltip("The time it takes for the black panel to fade in/out before changing scenes")]
    [SerializeField] private float time = 1f;
    [Tooltip("Delays the scene fade in after the scene has loaded")]
    [SerializeField] private float delayTime = 0.5f;
    [Tooltip("This delay time is only used when the game first boots up")]
    [SerializeField] private float gameStartFadeTime = 2.5f;

    [Header("Loading Text")]
    [SerializeField] private TextMeshProUGUI loadingText;

    // Makes sure once a scene begins the process of loading, another scene is not loaded
    private bool loadingScene = true; // Set to true to make sure that scenes (somehow) cannot be loaded before Start()

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (canvasGroup == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}'s canvasGroup reference null. Please assign a reference.");
#endif
        }
        else if (canvasGroup.alpha != 0)
        {
            StartCoroutine(InitialLoadRoutine());
        }

        loadingScene = false;
    }

    /// <summary>
    ///     The coroutine that is called when the game is first launched.
    /// </summary>
    private IEnumerator InitialLoadRoutine()
    {
        yield return new WaitForSeconds(gameStartFadeTime);
        BeginFadeIn();
    }

    /// <summary>
    ///     Load a scene with a fade out/in.
    /// </summary>
    /// <param name="sceneName">The desired scene to load.</param>
    public void LoadSceneWithFade(SceneName sceneName)
    {
        LoadSceneWithFade(sceneName, time, delayTime);
    }

    // This method can be used to override the default time and delayTime
    /// <summary>
    ///     Load a scene with a fade out/in.
    /// </summary>
    /// <param name="sceneName">The desired scene to load.</param>
    /// <param name="fadeTime">Override fade time (the time it takes to fade in/out).</param>
    /// <param name="delay">Override delay time (the delay before fading from black to scene view after loading the scene).</param>
    public void LoadSceneWithFade(SceneName sceneName, float fadeTime, float delay = 0.5f)
    {
        LoadSceneWithFade(sceneName.ToString(), fadeTime, delay);
    }

    /// <summary>
    ///     Load a scene with a fade out/in.
    /// </summary>
    /// <param name="sceneName">The desired scene to load.</param>
    public void LoadSceneWithFade(string sceneName)
    {
        LoadSceneWithFade(sceneName, time, delayTime);
    }

    /// <summary>
    ///     Load a scene with a fade out/in.
    /// </summary>
    /// <param name="sceneName">The desired scene to load.</param>
    /// <param name="fadeTime">Override fade time (the time it takes to fade in/out).</param>
    /// <param name="delay">Override delay time (the delay before fading from black to scene view after loading the scene).</param>
    public void LoadSceneWithFade(string sceneName, float fadeTime, float delay = 0.5f)
    {
        if (!loadingScene)
        {
            StartCoroutine(LoadSceneWithFadeOperation(sceneName, fadeTime, delay));
        }
    }

    /// <summary>
    ///     Begin the scene fade in (go from black to scene view).
    /// </summary>
    public void BeginFadeIn()
    {
        StartCoroutine(FadeInOutRoutine(time, 0f));
    }

    /// <summary>
    ///     Begin the scene fade out (go from scene view to black).
    /// </summary>
    public void BeginFadeOut()
    {
        StartCoroutine(FadeInOutRoutine(time, 1f));
    }

    private IEnumerator LoadSceneWithFadeOperation(string sN, float fadeTime, float delay)
    {
        loadingScene = true;

        StartCoroutine(FadeInOutRoutine(fadeTime, 1f)); // Fade to black
        yield return new WaitForSecondsRealtime(fadeTime);

        // Time scale check to make sure timeScale resets properly before every scene
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
        }

        yield return SceneHandler.LoadSceneCoroutine(sN);
#if UNITY_EDITOR
        Debug.Log("Finished loading scene");
#endif

        yield return new WaitForSecondsRealtime(delay);
#if UNITY_EDITOR
        Debug.Log("Delay time finished, fading in");
#endif

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        StartCoroutine(FadeInOutRoutine(fadeTime, 0f)); // Fade from black to whatever scene was loaded

        loadingScene = false;
    }

    // Set targetAlpha == 1f to fade to black, set targetAlpha == 0f to fade from black
    private IEnumerator FadeInOutRoutine(float fadeTime, float targetAlpha)
    {
        float lerp = 0f;
        float startAlpha = canvasGroup.alpha;

        if (fadeTime <= 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("fadeTime is less than 0, unable to fade properly.");
#endif
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.unscaledDeltaTime / fadeTime); // Use unscaledDeltaTime for time-independent fading
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
