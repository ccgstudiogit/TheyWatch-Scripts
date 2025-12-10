using System.Collections;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController instance { get; private set; }

    [Header("Camera Flythrough Settings")]
    [Tooltip("A reference to the main camera is used to move the camera to/from background locations")]
    [SerializeField] private GameObject mainCamera;

    [Tooltip("The various backgrounds used as camera flythroughs")]
    [SerializeField] private MainMenuBackground[] backgrounds;

    [Tooltip("Shuffles the backgrounds in Start() so that the backgrounds will have a random order")]
    [SerializeField] private bool shuffleBackgrounds = true;
    [Tooltip("The minimum amount of backgrounds there needs to be in order to shuffle the array")]
    [SerializeField, Min(2)] private int minAmountToShuffle = 2;

    [Tooltip("The total duration of a single flythrough")]
    [field: SerializeField, Min(0f)] public float flythroughDuration { get; private set; } = 8f;

    [Header("Fade Between Flythroughs")]
    [Tooltip("The fade panel's canvas group component, used to control the panel's alpha")]
    [SerializeField] private CanvasGroup fadePanelCanvasGroup;

    [Tooltip("The time it takes for the fade panel to fade from opaque to transparent")]
    [field: SerializeField, Min(0f)] public float backgroundFadeInTime { get; private set; } = 0.3f;

    [Tooltip("The time it takes for the fade panel to fade from transparent to opaque")]
    [field: SerializeField, Min(0f)] public float backgroundFadeOutTime { get; private set; } = 1.5f;

    [Tooltip("The background will start fading out when the camera reaches this percentage of the way to the ending location")]
    [field: SerializeField, Range(0, 100)] public int backgroundFadeOutPercentage { get; private set; } = 80;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // Fallback to make sure the UI map is enabled incase InputManager.instance was null in 
        // OnEnable() (should really only be an edge case on game startup)
        if (InputManager.instance != null && !InputManager.instance.IsUIMapEnabled())
        {
            EnableUIMap();
            DisableBaseMap();
        }

        if (shuffleBackgrounds && backgrounds.Length >= minAmountToShuffle)
        {
            System.Random rng = new System.Random();
            rng.ShuffleArray(backgrounds);
        }

        StartCoroutine(Flythoughs(mainCamera));
    }

    private void OnEnable()
    {
        if (InputManager.instance != null)
        {
            EnableUIMap();
            DisableBaseMap();
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    ///     Calls InputManager.instance to enable the UI map.
    /// </summary>
    private void EnableUIMap()
    {
        InputManager.instance.EnableUIMap(true);
    }

    /// <summary>
    ///     Calls InputManager.instance to disable the base map.
    /// </summary>
    private void DisableBaseMap()
    {
        InputManager.instance.EnableBaseMap(false);
    }

    /// <summary>
    ///     Fade the panel in or out.
    /// </summary>
    /// <param name="targetAlpha">The target alpha of the fade panel's canvas group.</param>
    /// <param name="duration">The total duration of the fade in seconds.</param>
    public void FadePanel(float targetAlpha, float duration)
    {
        StartCoroutine(FadePanelRoutine(targetAlpha, duration));
    }

    /// <summary>
    ///     Handles fading the panel in/out depending on the target alpha.
    /// </summary>
    private IEnumerator FadePanelRoutine(float targetAlpha, float duration)
    {
        float lerp = 0f;
        float staringAlpha = fadePanelCanvasGroup.alpha;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            fadePanelCanvasGroup.alpha = Mathf.Lerp(staringAlpha, targetAlpha, lerp);

            yield return null;
        }

        fadePanelCanvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    ///     Handles flying through the various menu backgrounds.
    /// </summary>
    private IEnumerator Flythoughs(GameObject camera)
    {
        while (enabled)
        {
            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i].CanUseBackground())
                {
                    yield return backgrounds[i].FlythroughRoutine(camera);
                }
            }
        }
    }
}
