using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Keeps track of levels progress: if they have been completed, how many attempts the player has on that level, etc.
/// </summary>
public class LevelProgressData : SerializableClass
{
    /// <summary>
    ///     Keeps track of all of the completed levels (a level is considered complete when the player exits through the portal).
    /// </summary>
    public SerializableHashSet<Level> completedLevels;

    /// <summary>
    ///     Keeps track of individual levels and the number of attempts the player has on that level.
    /// </summary>
    public SerializableDictionary<Level, int> levelAttempts;
    private const int defaultAttempts = 0; // When creating a new file, this int is used as the starting int in levelAttempts

    private bool initialized = false;

    /// <summary>
    ///     Create a LevelProgressData object. After creation, Initialize() should be called.
    /// </summary>
    /// <param name="allLevels">All of the levels. This is used to populate levelAttempts.</param>
    /// <param name="fileName">The name of the level progress file.</param>
    /// <param name="useEncryption">Whether the file should be encrypted or not.</param>
    public LevelProgressData(Level[] allLevels, string fileName, bool useEncryption = true) : base(fileName, useEncryption)
    {
        completedLevels = new SerializableHashSet<Level>();
        levelAttempts = new SerializableDictionary<Level, int>();

        // Populate levelAttempts. If a file exists, this will be overridden, but if a file does not exist, the variables populated here will
        // become the default (should be that each level has 0 attempts)
        for (int i = 0; i < allLevels.Length; i++)
        {
            levelAttempts.TryAdd(allLevels[i], defaultAttempts);
        }
    }

    /// <summary>
    ///     Initialize this object by attempting to load an existing LevelProgressData from the Json file. If no data is found, a new file is
    ///     created (if connected to steam, this will check which steam achievements have been unlocked and make each level be considered
    ///     complete as long as the necessary steam achievement has been unlocked).
    /// </summary>
    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        LevelProgressData data = Load<LevelProgressData>();

        if (data == null)
        {
#if UNITY_EDITOR
            Debug.Log($"Could not find existing LevelProgressData from Json, creating a new Json file {fileName}.");
#endif
            // Check steam achievements and auto-mark completed levels based on unlocked achievements
            if (SteamManager.instance != null && SteamManager.instance.connectedToSteam)
            {
                foreach (KeyValuePair<Level, SteamAchievement> kvp in SteamManager.LevelAchievementMap)
                {
                    if (SteamManager.instance.HasAchievement(kvp.Value))
                    {
                        completedLevels.Add(kvp.Key);
                    }
                }
            }

            // Save is called here to save the empty completedLevels and levelAttempts to the Json
            Save();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"Level progress data successfully loaded from {fileName}.");
#endif
            completedLevels = data.completedLevels;
            levelAttempts = data.levelAttempts;
        }

        initialized = true;
    }

    /// <summary>
    ///     Make a level be considered complete by adding it to the completedLevels hashset.
    /// </summary>
    /// <param name="level">The level that was completed.</param>
    public void CompleteLevel(Level level)
    {
        if (!initialized)
        {
            Initialize();
        }

        completedLevels.Add(level);
        Save();
    }

    /// <summary>
    ///     Increment a level's total attempts by 1.
    /// </summary>
    public void IncrementLevelAttempts(Level level)
    {
        if (!initialized)
        {
            Initialize();
        }

        if (levelAttempts.ContainsKey(level))
        {
            levelAttempts[level]++;
            Save();
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogError($"Attempted to increment a level's attempt by 1 but the dictionary does not have the key {level}");
        }
#endif
    }
}
