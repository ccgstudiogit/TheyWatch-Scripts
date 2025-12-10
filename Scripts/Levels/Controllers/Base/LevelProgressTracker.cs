using UnityEngine;

public class LevelProgressTracker : MonoBehaviour
{
    [Tooltip("When the player escapes through the portal, this level will be considered complete")]
    [SerializeField] private Level level;

    [Header("For Debug - Do Not Edit -")]
    [Tooltip("View the player's current number of attempts for this level")]
    [SerializeField] private int currentAttempt = 0;

    private void Start()
    {
        IncrementThisLevelsAttempts();
    }

    private void OnEnable()
    {
        LevelController.OnPlayerEscaped += LevelComplete;
    }

    private void OnDisable()
    {
        LevelController.OnPlayerEscaped -= LevelComplete;
    }

    /// <summary>
    ///     Increment the number of attempts the player has on this level by 1.
    /// </summary>
    private void IncrementThisLevelsAttempts()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.RecordAttempt(level);
            currentAttempt = LevelManager.instance.levelAttempts[level];
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogError($"{gameObject.name} attempted to record this level's attempt but LevelManager.instance was null.");
        }
#endif
    }

    /// <summary>
    ///     Mark this level as complete.
    /// </summary>
    private void LevelComplete()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.CompleteLevel(level);
        }
    }
}
