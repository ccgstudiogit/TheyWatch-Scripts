using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private Level level;

    [Header("Active Button (Level Unlocked)")]
    [Tooltip("This is the button that will show if the desired level is unlocked")]
    [SerializeField] private GameObject levelUnlocked;

    [Header("Inactive Button (Level Locked)")]
    [Tooltip("This is the button that will show if the desired level is locked")]
    [SerializeField] private GameObject levelLocked;

    public bool unlocked { get; private set; } = false;

    private void OnEnable()
    {
        if (LevelManager.instance != null)
        {
            UnlockButton(LevelManager.instance.unlockedLevels.Contains(level));
        }
    }

    /// <summary>
    ///     Unlock or lock this level select button.
    /// </summary>
    /// <param name="unlock">Whether or not this button should display the locked button or the unlocked button.</param>
    private void UnlockButton(bool unlock)
    {
        if (levelUnlocked == null || levelLocked == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s levelUnlocked || levelLocked null. Unable to continue.");
#endif
            return;
        }

        levelLocked.SetActive(!unlock);
        levelUnlocked.SetActive(unlock);
    }
}
