using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
///     This script should be in the Main Menu, as guides should only be unlocked within the Main Menu.
/// </summary>
public class GuideUnlockController : MonoBehaviour
{
    [Header("Guide Popups")]
    [SerializeField] private RectTransform guideButtonRect;
    [SerializeField] private SoundEffectSO popupSFX;
    [SerializeField, Min(0f)] private float popupDelay = 1.15f;
    [SerializeField, Min(0f)] private float scaleIncrease = 0.33f;
    [SerializeField, Min(0f)] private float animationLength = 0.35f;

    public UnityEvent OnShowGuideUnlockedPopup;

    [Header("Check Out Welcome Guide Popup")]
    [Tooltip("The welcome guide popup can be enabled/disabled (useful for creating builds, such as demo builds, without displaying the welcome page)")]
    [SerializeField] private bool showWelcomeGuidePopup = true;

    public UnityEvent OnShowWelcomePopup;

    private const string welcomePopupStr = "welcomeGuidePopupShown";
    private int defaultWelcomeGuidePopupShown = 0; // 0 == welcome guide popup has NOT been shown, 1 == welcome guide popup has been shown

#if UNITY_EDITOR
    [Tooltip("Can be used to reset and show the welcome guide popup when loading in")]
    [SerializeField] private bool resetWelcomeGuidePopup;
#endif

    private void Start()
    {
#if UNITY_EDITOR
        if (resetWelcomeGuidePopup)
        {
            ResetWelcomeGuidePopup();
        }
#endif

        // If this is the first time the player has loaded into the game, show the welcome page
        if (showWelcomeGuidePopup && ShouldShowWelcomePopup())
        {
            ShowWelcomePopup();
        }

        CheckIfShouldUnlockAGuide();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SendMessage();
        }
    }
#endif

    /// <summary>
    ///     Check if a guide should be unlocked. A message is only sent if the guide was unlocked via attempts and not unlocked if
    ///     the player beat the level.
    /// </summary>
    private void CheckIfShouldUnlockAGuide()
    {
        if (LevelManager.instance == null)
        {
            return;
        }

        Level[] levels = LevelManager.instance.GetAllLevels();

#if UNITY_EDITOR
        foreach (var kvp in LevelManager.instance.levelAttempts)
        {
            Debug.Log($"Current attempts for {kvp.Key} = {kvp.Value}");
        }
#endif

        for (int i = 0; i < levels.Length; i++)
        {
            if (!LevelManager.instance.unlockedGuides.Contains(levels[i]))
            {
                // 
                // A guide should unlock if these conditions are met:
                // 1) The minimum attempts have been met
                // 2) The level has already been completed
                // 

                // Unlock the guide via attempts and the level has not been beaten yet (if that's the case, send a message to let the 
                // player they've unlocked a guide)
                bool shouldUnlockWithMessage = (
                    LevelManager.instance.levelAttempts.TryGetValue(levels[i], out int attempts) &&
                    attempts >= LevelManager.instance.minAttemptsForGuide &&
                    !LevelManager.instance.completedLevels.Contains(levels[i])
                );

                if (shouldUnlockWithMessage)
                {
                    UnlockGuide(levels[i]);
                    SendMessage();
                }
                // Silently unlock a guide if a message is not necessary (for example, if the player beats a level on their first attempt)
                else if (LevelManager.instance.completedLevels.Contains(levels[i]))
                {
                    UnlockGuide(levels[i]);
                }
            }
        }
    }

    /// <summary>
    ///     Unlock a level's guide (this will not send a message, only unlock the guide).
    /// </summary>
    /// <param name="level"></param>
    private void UnlockGuide(Level level)
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.UnlockGuide(level);
        }
    }

    /// <summary>
    ///     Send a message to the player, letting them know a new guide has been unlocked.
    /// </summary>
    public void SendMessage()
    {
        this.Invoke(() =>
        {
            OnShowGuideUnlockedPopup?.Invoke();
            GuideButtonAnimation();

            if (popupSFX != null)
            {
                popupSFX.Play();
            }

        }, popupDelay);
    }

    /// <summary>
    ///     Start the guide button animation.
    /// </summary>
    public void GuideButtonAnimation()
    {
        StartCoroutine(SizeAnimation(guideButtonRect, scaleIncrease, animationLength));
    }

    private IEnumerator SizeAnimation(RectTransform rect, float scaleIncrease, float duration)
    {
        duration /= 2f; // Duration is cut in half to make sure the increase/decrease stays within the original duration length
        float lerp = 0f;
        Vector2 startingRect = rect.localScale;
        Vector2 targetRect = new Vector2(startingRect.x + scaleIncrease, startingRect.y + scaleIncrease);

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1f, Time.deltaTime / duration);
            rect.localScale = Vector2.Lerp(startingRect, targetRect, lerp);

            yield return null;
        }

        lerp = 0f;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1f, Time.deltaTime / duration);
            rect.localScale = Vector2.Lerp(targetRect, startingRect, lerp);

            yield return null;
        }

        rect.localScale = startingRect;
    }

#if UNITY_EDITOR
    /// <summary>
    ///     Can be used to reset PlayerPrefs welcome popup flag back to 0 (meaning the welcome popup will be shown).
    /// </summary>
    private void ResetWelcomeGuidePopup()
    {
        PlayerPrefs.SetInt(welcomePopupStr, 0);
    }
#endif

    /// <summary>
    ///     Check if this is the first time the player has opened the game. If so, the welcome popup should be shown.
    /// </summary>
    /// <returns>True if this is the first time the player has loaded in, false if otherwise.</returns>
    private bool ShouldShowWelcomePopup()
    {
        // Only show the welcome popup if it has not been shown previously
        return PlayerPrefs.GetInt(welcomePopupStr, defaultWelcomeGuidePopupShown) == 0;
    }

    /// <summary>
    ///     Show the welcome popup to the user.
    /// </summary>
    private void ShowWelcomePopup()
    {
        PlayerPrefs.SetInt(welcomePopupStr, 1);

        this.Invoke(() =>
        {
            OnShowWelcomePopup?.Invoke();
            GuideButtonAnimation();

            if (popupSFX != null)
            {
                popupSFX.Play();
            }

        }, popupDelay);
    }
}
