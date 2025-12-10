using System.Collections;
using UnityEngine;

public class MainMenuBackground : MonoBehaviour
{
    [Header("Environment")]
    [Tooltip("These objects are not particular to single MainMenuBackgroundCamMovement but to an entire background. These objects " +
        "will be turned on once this background is selected and turned back off once all of the flythroughs of this background are done")]
    [SerializeField] private GameObject[] environmentsObjects;

    [Header("MainMenuBackgroundCamMovement References")]
    [Tooltip("This background will go through all of the camMovements before being considered complete")]
    [SerializeField] private MainMenuFlythrough[] camFlythroughs;

    [Tooltip("If enabled, the order of the cameraMovements array will be shuffled in OnEnable(), creating extra variation")]
    [SerializeField] private bool shuffleCamMovementsOrder = true;
    [Tooltip("The minimum amount of camFlythroughs there needs to be in order to shuffle the array. Recommended to keep at least " +
        "3 since there is a chance if there is 2, the same movements will be shown back-to-back")]
    [SerializeField, Min(2)] private int minAmountToShuffle = 3;

    [Header("Level Unlocked Settings")]
    [Tooltip("If enabled, this background will only be used if the pertaining level has been unlocked")]
    [SerializeField] private bool onlyShowIfLevelUnlocked = true;

    [Tooltip("The level this background is based upon")]
    [field: SerializeField] public Level level { get; private set; }

    private void Awake()
    {
        if (shuffleCamMovementsOrder && camFlythroughs.Length >= minAmountToShuffle)
        {
            System.Random rng = new System.Random();
            rng.ShuffleArray(camFlythroughs);
        }

        SetEnvironmentActive(false);
    }

    /// <summary>
    ///     Check if this background should be shown whilst in the Main Menu screen.
    /// </summary>
    /// <returns>True if this background can be shown, false if otherwise.</returns>
    public bool CanUseBackground()
    {
        // If onlyShowIfLevelUnlocked is NOT enabled (false), just return true since the level does not need to be unlocked to be shown
        return !onlyShowIfLevelUnlocked || (LevelManager.instance != null && LevelManager.instance.unlockedLevels.Contains(level));
    }

    /// <summary>
    ///     Handles the process of the camera flying through this background, using all of the areas of the camFlythroughs.
    /// </summary>
    public IEnumerator FlythroughRoutine(GameObject camera)
    {
        SetEnvironmentActive(true);

        for (int i = 0; i < camFlythroughs.Length; i++)
        {
            if (camFlythroughs[i] != null)
            {
                yield return camFlythroughs[i].FlythroughRoutine(camera.transform);
            }
        }

        SetEnvironmentActive(false);
    }

    /// <summary>
    ///     Set this background's environment to be active or in-active.
    /// </summary>
    /// <param name="setActive">Whether the game objects of this flythrough's environment should be active or not.</param>
    public void SetEnvironmentActive(bool setActive)
    {
        HelperMethods.SetGameObjectsActive(environmentsObjects, setActive);
    }
}
