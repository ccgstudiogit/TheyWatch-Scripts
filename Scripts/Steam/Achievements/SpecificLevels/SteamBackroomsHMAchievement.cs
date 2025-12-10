using UnityEngine;

public class SteamBackroomsHMAchievement : SteamLevelAchievement
{
    [Tooltip("The maximum amount of time (in seconds) the player can take to beat this map in order to unlock this achievement. " +
        "Recommended to keep at 5 minutes (300 seconds)")]
    [SerializeField] private float maxTimeToComplete = 300f;

    private float timeElapsed;

    private void Awake()
    {
        timeElapsed = 0f;
    }

    private void OnEnable()
    {
        LevelController.OnPlayerEscaped += CheckIfPlayerCompletedAchievement;
    }

    private void OnDisable()
    {
        LevelController.OnPlayerEscaped -= CheckIfPlayerCompletedAchievement;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
    }

    /// <summary>
    ///     Check if the player completed this achievement (timeElapsed should be less than maxTimeToComplete) on player escaping into the portal.
    /// </summary>
    private void CheckIfPlayerCompletedAchievement()
    {
        if (timeElapsed < maxTimeToComplete && SteamManager.instance != null)
        {
            SteamManager.instance.UnlockAchievement(achievement);
        }
    }
}
