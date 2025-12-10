using UnityEngine;

/// <summary>
///     GuidesController does not manage unlocking guides (that role belongs to GuideUnlockController). GuidesController only manages the guide
///     buttons and enables or disables them depending on if that guide button's level has unlocked that guide.
/// </summary>
public class GuidesController : MonoBehaviour
{
    [Header("Guide Buttons")]
    [SerializeField] private GuideButton[] guideButtons;

    [Header("Extra Settings")]
    [Tooltip("Ignore whether or not the guide is unlocked and just set all of them active")]
    [SerializeField] private bool setAllActive;

    private void OnEnable()
    {
        UnlockGuides();
    }

    /// <summary>
    ///     Loop through all of the buttons in the guideButtons array and check to see if that guide button's level has unlocked the guide.
    /// </summary>
    private void UnlockGuides()
    {
        if (LevelManager.instance == null)
        {
            return;
        }

        for (int i = 0; i < guideButtons.Length; i++)
        {
            if (guideButtons[i] == null)
            {
                continue;
            }

            guideButtons[i].gameObject.SetActive(setAllActive || LevelManager.instance.unlockedGuides.Contains(guideButtons[i].level));
        }
    }
}
