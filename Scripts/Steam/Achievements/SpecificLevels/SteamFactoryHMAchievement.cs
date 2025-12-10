using UnityEngine;

public class SteamFactoryHMAchievement : SteamLevelAchievement
{
    [Tooltip("In order for the player to unlock this achievement, this must be 0 when the player enters the portal " +
        "and escapes the level")]
    [SerializeField] private int timesUsedCS = 0;

    // The maximum amount of times the player can use collectable sight to unlock this achievement **SHOULD BE 0
    private const int maxTimesToUseCS = 0;
    
    private void Awake()
    {
        timesUsedCS = 0;
    }

    private void OnEnable()
    {
        InputCollectableSight.OnCollectableSight += HandlePlayerUsedCS;
        LevelController.OnPlayerEscaped += CheckIfPlayerCompletedAchievement;
    }

    private void OnDisable()
    {
        InputCollectableSight.OnCollectableSight -= HandlePlayerUsedCS;
        LevelController.OnPlayerEscaped -= CheckIfPlayerCompletedAchievement;
    }

    /// <summary>
    ///     Keeps track of how many times the player has used collectable sight this level.
    /// </summary>
    private void HandlePlayerUsedCS(bool usingCS)
    {
        if (usingCS)
        {
            timesUsedCS++;
        }
    }
    
    /// <summary>
    ///     Handles checking if the player has successfully completed this achievement when the player escapes via the portal.
    /// </summary>
    private void CheckIfPlayerCompletedAchievement()
    {
        if (timesUsedCS <= maxTimesToUseCS && SteamManager.instance != null)
        {
            SteamManager.instance.UnlockAchievement(achievement);
        }
    }
}
