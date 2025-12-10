using UnityEngine;

public class UnlockLevelsOnCompletion : MonoBehaviour
{
    [Tooltip("When the player escapes through the portal and completes this current level, these levels will be unlocked")]
    [SerializeField] private Level[] levelsToUnlock;

    private void OnEnable()
    {
        LevelController.OnPlayerEscaped += UnlockLevels;
    }

    private void OnDisable()
    {
        LevelController.OnPlayerEscaped -= UnlockLevels;
    }

    /// <summary>
    ///     Unlock the levels specified in the serialized array levelsToUnlock.
    /// </summary>
    private void UnlockLevels()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.UnlockLevels(levelsToUnlock);
        }
    }
}
