using UnityEngine;

public class SteamProdigyAchievement : SteamLevelAchievement
{
    [Tooltip("In order for the player to unlock this achievement, this must be 0 when the player enters the portal " +
        "and escapes the level")]
    [SerializeField] private int timesCheckedDevice = 0;

    // The maximum amount of times the player can check their device to unlock this achievement **SHOULD BE 0
    private const int maxTimesToCheckDevice = 0;

    private void Awake()
    {
        timesCheckedDevice = 0;
    }

    private void OnEnable()
    {
        InputCheckDevice.OnCheckDevice += HandlePlayerCheckedDevice;
        LevelController.OnPlayerEscaped += CheckIfPlayerCompletedAchievement;
    }

    private void OnDisable()
    {
        InputCheckDevice.OnCheckDevice -= HandlePlayerCheckedDevice;
        LevelController.OnPlayerEscaped -= CheckIfPlayerCompletedAchievement;
    }

    /// <summary>
    ///     Keeps track of how many times the player has checked their device this level.
    /// </summary>
    /// <param name="checkingDevice">Whether or not the player is currently checking their device.</param>
    private void HandlePlayerCheckedDevice(bool checkingDevice)
    {
        if (checkingDevice)
        {
            timesCheckedDevice++;
        }
    }

    /// <summary>
    ///     Handles checking if the player has successfully completed this achievement when the player escapes via the portal.
    /// </summary>
    private void CheckIfPlayerCompletedAchievement()
    {
        if (timesCheckedDevice <= maxTimesToCheckDevice && SteamManager.instance != null)
        {
            SteamManager.instance.UnlockAchievement(achievement);
        }
    }
}
