using UnityEngine;

public class SteamDungeonHMAchievement : SteamLevelAchievement
{
    [Tooltip("Used to keep track of how many times the Warden has spotted the player")]
    [SerializeField] private int timesPlayerSpottedByWarden = 0;

    // The maximum amount of times the player can be spotted by the Warden in order to unlock this achievement **SHOULD BE 0
    private const int maxTimesSpottedByWarden = 0;

    private void OnEnable()
    {
        Warden.OnWardenSpottedPlayer += HandleWardenSpottedPlayer;
        LevelController.OnPlayerEscaped += CheckIfPlayerCompletedAchievement;
    }

    private void OnDisable()
    {
        Warden.OnWardenSpottedPlayer -= HandleWardenSpottedPlayer;
        LevelController.OnPlayerEscaped -= CheckIfPlayerCompletedAchievement;
    }

    /// <summary>
    ///     Increments timesPlayerSpottedByWarden by 1 every time the Warden spots the player and begins chasing.
    /// </summary>
    private void HandleWardenSpottedPlayer()
    {
        timesPlayerSpottedByWarden++;
    }

    /// <summary>
    ///     Check if the player completed this achievement (timesPlayerSpottedByWarden should be 0 to unlock) on player escaping into the portal.
    /// </summary>
    private void CheckIfPlayerCompletedAchievement()
    {
        if (timesPlayerSpottedByWarden <= maxTimesSpottedByWarden && SteamManager.instance != null)
        {
            SteamManager.instance.UnlockAchievement(achievement);
        }
    }
}
