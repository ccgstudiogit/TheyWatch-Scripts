using UnityEngine;

public class SteamLevelCompleteAchievement : SteamLevelAchievement
{
    private void OnEnable()
    {
        LevelController.OnPlayerEscaped += UnlockLevelCompleteAchievement;
    }

    private void OnDisable()
    {
        LevelController.OnPlayerEscaped -= UnlockLevelCompleteAchievement;
    }

    /// <summary>
    ///     Unlocks this level's level complete achievement via SteamManager.
    /// </summary>
    private void UnlockLevelCompleteAchievement()
    {
        if (SteamManager.instance != null)
        {
            SteamManager.instance.UnlockAchievement(achievement);
        }
    }
}
