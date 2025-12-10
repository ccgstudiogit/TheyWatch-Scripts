using UnityEngine;

public class LevelCompleteStar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The star that is shown when neither the level nor hardmode has been beaten")]
    [SerializeField] private GameObject emptyStar;
    [Tooltip("The star that is shown when the classic level has been beaten (in this case, that should just be \"level\"")]
    [SerializeField] private GameObject basicStar;
    [Tooltip("The glow that is shown when both the classic and hardmode variant have been beat")]
    [SerializeField] private GameObject glowStar;

    [Header("Level")]
    [Tooltip("The level this star pertains to. If this level is completed, this star is shown")]
    [SerializeField] private Level level;

    [Tooltip("The hardmode variant of this level (if the HM is beaten, the glow will unlock)")]
    [SerializeField] private Level hardmodeLevel;

    private void Start()
    {
        ShowStar(IsLevelCompleted(level), IsLevelCompleted(hardmodeLevel));
    }

    /// <summary>
    ///     Check if a level has been completed by the player.
    /// </summary>
    /// <param name="level">Was this level completed?</param>
    /// <returns>True if the level has been completed, false if otherwise. Also returns false if LevelManager.instance is null.</returns>
    private bool IsLevelCompleted(Level level)
    {
        return LevelManager.instance != null && LevelManager.instance.completedLevels.Contains(level);
    }

    /// <summary>
    ///     Show or hide the star.
    /// </summary>
    /// <param name="showStar">Whether or not the star should be shown.</param>
    /// <param name="includeHMGlow">If the glow should also be shown.</param>
    private void ShowStar(bool showStar, bool includeHMGlow)
    {
        emptyStar.SetActive(!showStar);
        basicStar.SetActive(showStar);

        // Only show the glow if the basic star is shown (meaning the player has completed classic). This prevents an edge-case scenario where
        // the player unlocks all levels and completes a HM before classic, resulting in the glow being shown but not the actual star
        glowStar.SetActive(showStar && includeHMGlow);
    }
}
