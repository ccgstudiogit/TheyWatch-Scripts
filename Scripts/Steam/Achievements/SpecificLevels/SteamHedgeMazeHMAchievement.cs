using UnityEngine;

public class SteamHedgeMazeHMAchievement : SteamLevelAchievement
{
    [Tooltip("Used to keep track of how many instructions the player has failed this run")]
    [SerializeField] private int failedInstructions = 0;

    // The maximum amount of times the player can fail instructions in order to unlock this achievement **SHOULD BE 0
    private const int maxFailedInstructions = 0;

    private void Awake()
    {
        failedInstructions = 0;
    }

    private void OnEnable()
    {
        HedgeMazeHMInstruction.OnPlayerFailed += HandlePlayerFailedInstruction;
        LevelController.OnPlayerEscaped += CheckIfPlayerCompletedAchievement;
    }

    private void OnDisable()
    {
        HedgeMazeHMInstruction.OnPlayerFailed -= HandlePlayerFailedInstruction;
        LevelController.OnPlayerEscaped -= CheckIfPlayerCompletedAchievement;
    }

    /// <summary>
    ///     If the player fails an instruction, failedInstructions is incremented by 1.
    /// </summary>
    private void HandlePlayerFailedInstruction()
    {
        failedInstructions++;
    }

    /// <summary>
    ///     Check if the player completed this achievement (failedInstructions should be 0 to unlock) on player escaping into the portal.
    /// </summary>
    private void CheckIfPlayerCompletedAchievement()
    {
        if (failedInstructions <= maxFailedInstructions && SteamManager.instance != null)
        {
            SteamManager.instance.UnlockAchievement(achievement);
        }
    }
}
