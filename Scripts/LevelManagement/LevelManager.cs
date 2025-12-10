using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance { get; private set; }

    [Header("Unlocked Levels File Config")]
    [SerializeField] private string unlockedLevelsFileName = "UnlockedLevels.json";
    [SerializeField] private bool encryptUnlockedLevelsFile;

    [Header("Default Levels Unlocked")]
    [Tooltip("The levels that are automatically unlocked in a new game (first load-in on the user's PC) or when the existing Json file is deleted")]
    [SerializeField] Level[] defaultLevels;
    [SerializeField] private bool unlockAllLevels;

    [Header("Level Progress File Config")]
    [SerializeField] private string levelProgressFileName = "LevelProgress.json";
    [SerializeField] private bool encryptLevelProgressFile = true;

    [Header("Level Guide Settings")]
    [Tooltip("The minimum amount of attempts that are needed to unlock a level's particular guide")]
    [SerializeField, Min(0)] private int _minAttemptsForGuide = 2;
    public int minAttemptsForGuide => _minAttemptsForGuide;

    public int levelCount => Enum.GetNames(typeof(Level)).Length;

    // Keeps track of each level that has been unlocked as well as the guides for the levels. HashSets are used to prevent duplicate listings
    private UnlockedLevelData unlockedLevelData;
    public SerializableHashSet<Level> unlockedLevels
    {
        get
        {
            if (unlockedLevelData != null)
            {
                return unlockedLevelData.unlockedLevels;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("unlockedLevels was accessed but unlockedLevelData was null. A new unlockedLevelData object is being created.");
#endif
                CreateUnlockedLevelData();
                return unlockedLevelData.unlockedLevels;
            }
        }
    }

    public SerializableHashSet<Level> unlockedGuides
    {
        get
        {
            if (unlockedLevelData != null)
            {
                return unlockedLevelData.unlockedGuides;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("unlockedLevels was accessed but unlockedLevelData was null. A new unlockedLevelData object is being created.");
#endif
                CreateUnlockedLevelData();
                return unlockedLevelData.unlockedGuides;
            }
        }
    }

    // Keeps track of which levels have been completed as well as the number of attempts on any given level
    private LevelProgressData levelProgressData;
    public SerializableHashSet<Level> completedLevels
    {
        get
        {
            if (levelProgressData != null)
            {
                return levelProgressData.completedLevels;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("completedLevels was accessed but levelProgressData was null. A new levelProgressData object is being created.");
#endif
                CreateLevelProgressData();
                return levelProgressData.completedLevels;
            }
        }
    }

    public SerializableDictionary<Level, int> levelAttempts
    {
        get
        {
            if (levelProgressData != null)
            {
                return levelProgressData.levelAttempts;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("levelAttempts was accessed but levelProgressData was null. A new levelProgressData object is being created.");
#endif
                CreateLevelProgressData();
                return levelProgressData.levelAttempts;
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            CreateUnlockedLevelData();
            CreateLevelProgressData();

            if (unlockAllLevels)
            {
                UnlockAllLevels();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    ///     Unlock levels to be able to be made playable.
    /// </summary>
    /// <param name="levelsToUnlock">The levels to unlock.</param>
    public void UnlockLevels(Level[] levelsToUnlock)
    {
        if (unlockedLevelData == null)
        {
            CreateUnlockedLevelData();
        }

        unlockedLevelData.AddLevels(levelsToUnlock);
    }

    /// <summary>
    ///     Unlock all levels to be able to be played.
    /// </summary>
    public void UnlockAllLevels()
    {
        if (unlockedLevelData == null)
        {
            CreateUnlockedLevelData();
        }

#if UNITY_EDITOR
        Debug.Log("Unlocking all levels!");
#endif
        unlockedLevelData.AddAllLevels();
    }

    /// <summary>
    ///     Create an UnlockedLevelData object and initialize it.
    /// </summary>
    private void CreateUnlockedLevelData()
    {
        unlockedLevelData = new UnlockedLevelData(defaultLevels, unlockedLevelsFileName, encryptUnlockedLevelsFile);
        unlockedLevelData.Initialize();
    }

    /// <summary>
    ///     Create a LevelProgressData object and initialize it.
    /// </summary>
    private void CreateLevelProgressData()
    {
        levelProgressData = new LevelProgressData(GetAllLevels(), levelProgressFileName, encryptLevelProgressFile);
        levelProgressData.Initialize();
    }

    /// <summary>
    ///     Get all of the levels within the Level enum.
    /// </summary>
    public Level[] GetAllLevels()
    {
        return (Level[])Enum.GetValues(typeof(Level));
    }

    /// <summary>
    ///     Record and save an attempt at a level.
    /// </summary>
    /// <param name="level">The level that was attempted.</param>
    public void RecordAttempt(Level level)
    {
        if (levelProgressData == null)
        {
            CreateLevelProgressData();
        }

        levelProgressData.IncrementLevelAttempts(level);
    }

    /// <summary>
    ///     Make a level be considered complete.
    /// </summary>
    /// <param name="level">The that was completed.</param>
    public void CompleteLevel(Level level)
    {
        if (levelProgressData == null)
        {
            CreateLevelProgressData();
        }

        levelProgressData.CompleteLevel(level);
    }

    /// <summary>
    ///     Unlock a level's guide (this will just make it visible within the guide menus).
    /// </summary>
    public void UnlockGuide(Level level)
    {
        if (unlockedLevelData == null)
        {
            CreateUnlockedLevelData();
        }

#if UNITY_EDITOR
        Debug.Log("Unlocking guide for " + level.ToString());
#endif

        unlockedLevelData.AddGuide(level);
    }
}
