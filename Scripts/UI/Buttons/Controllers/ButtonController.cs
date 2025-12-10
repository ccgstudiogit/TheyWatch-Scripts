using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif

public class ButtonController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool isInteractable = true;

    [Header("Text")]
    [Tooltip("If enabled, the below string will be used to override the normal/highlighted texts' text")]
    [SerializeField] private bool overrideTextWithString = true;

    /// <summary>
    ///     This variable is read-only. To set the text of this button, use the method SetText().
    /// </summary>
    [field: SerializeField] public string buttonText { get; private set; } = "Button";

    [Header("Content")]
    public CanvasGroup normalCG;
    public CanvasGroup highlightedCG;
    public TextMeshProUGUI normalText;
    public TextMeshProUGUI highlightedText;
    public Image normalImage;
    public Image highlightedImage;

    [HideInInspector] public bool stayHighlighted = false;

    [Header("Audio")]
    public bool enableSounds = true;
    public AudioSource audioSource;

    [SerializeField] private bool useHoverSound = true;
    public AudioClip hoverClip;

    [SerializeField] private bool useClickSound = true;
    public AudioClip clickClip;

    [Header("Events")]
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onHover = new UnityEvent();
    public UnityEvent onLeave = new UnityEvent();

    private bool isInitialized = false;

    private void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        UpdateUI();
    }

    private void OnDisable()
    {
        SetNormalActive();
    }

    /// <summary>
    ///     Handles initializing this button (does not execute in edit mode).
    /// </summary>
    private void Initialize()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }

        if (enableSounds && audioSource == null)
        {
            Debug.Log($"{gameObject.name} has sounds enabled but no audio source.");
        }
#endif

        isInitialized = true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateUI();
    }
#endif

    /// <summary>
    ///     Update this button's UI.
    /// </summary>
    public void UpdateUI()
    {
        SetButtonText(buttonText);
    }

    /// <summary>
    ///     Handles checking and doing stuff if the user's mouse has hovered over this button.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }

        SetHighlightedActive();
        PlayHoverSound();
        onHover?.Invoke();
    }

    /// <summary>
    ///     Handles checking and doing stuff if the user's mouse has clicked on this button.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }

        PlayClickSound();
        onClick?.Invoke();
    }

    /// <summary>
    ///     Handles checking and doing stuff if the user's mouse is no longer hovering over this button.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }

        if (!stayHighlighted)
        {
            SetNormalActive();
        }

        onLeave?.Invoke();
    }

    /// <summary>
    ///     Play the hover sound effect.
    /// </summary>
    public void PlayHoverSound()
    {
        if (useHoverSound)
        {
            PlayClip(hoverClip);
        }
    }

    /// <summary>
    ///     Play the click sound effect.
    /// </summary>
    public void PlayClickSound()
    {
        if (useClickSound)
        {
            PlayClip(clickClip);
        }
    }

    /// <summary>
    ///     Play a an audio clip using this script's audio source.
    /// </summary>
    /// <param name="clip">The clip to be played.</param>
    private void PlayClip(AudioClip clip)
    {
        if (enableSounds && audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    ///     Sets the normal button to be active.
    /// </summary>
    public void SetNormalActive()
    {
        SetButtonCanvasGroup(normalCG, 1f);
        SetButtonCanvasGroup(highlightedCG, 0f);
    }

    /// <summary>
    ///     Sets the highlighted button to be active.
    /// </summary>
    public void SetHighlightedActive()
    {
        SetButtonCanvasGroup(normalCG, 0f);
        SetButtonCanvasGroup(highlightedCG, 1f);
    }

    /// <summary>
    ///     Set a button's canvas group to a new alpha
    /// </summary>
    /// <param name="alpha">The new alpha of the canvas group.</param>
    private void SetButtonCanvasGroup(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"{gameObject.name} attempted to set a canvas group alpha but the canvas group was null!");
        }
#endif
    }

    /// <summary>
    ///     Set the text of this button.
    /// </summary>
    public void SetText(string newText)
    {
        buttonText = newText;
        UpdateUI();
    }

    /// <summary>
    ///     Set the text of this button. Note: This will only set the text if overrideTextWithString is enabled.
    /// </summary>
    private void SetButtonText(string text)
    {
        if (overrideTextWithString)
        {
            if (normalText != null)
            {
                normalText.text = text;
            }

            if (highlightedText != null)
            {
                highlightedText.text = text;
            }
        }
    }
}
