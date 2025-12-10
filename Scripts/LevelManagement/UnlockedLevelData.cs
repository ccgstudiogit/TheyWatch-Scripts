using System;
using UnityEngine;

/// <summary>
///     Keeps a serializable HashSet of all of the unlocked levels and unlocked guides.
/// </summary>
public class UnlockedLevelData : SerializableClass
{
    /// <summary>
    ///     Keeps track of the levels that have been unlocked.
    /// </summary>
    public SerializableHashSet<Level> unlockedLevels;

    /// <summary>
    ///     Keeps track of what levels have their guides unlocked. If a level is in this hashset, its guide has been unlocked.
    /// </summary>
    public SerializableHashSet<Level> unlockedGuides;

    private bool initialized = false;

    /// <summary>
    ///     Create an UnlockedLevelData object. After creation, Initialize() should be called.
    /// </summary>
    /// <param name="defaultUnlockedLevels">The levels that are unlocked when the game is loaded-into for the very first time or when
    ///     the Json file is not found.</param>
    /// <param name="fileName">The name of the Json file.</param>
    /// <param name="useEncryption">Whether the Json file should be encrypted or not.</param>
    public UnlockedLevelData(Level[] defaultUnlockedLevels, string fileName, bool useEncryption = false) : base(fileName, useEncryption)
    {
        unlockedGuides = new SerializableHashSet<Level>(); // No guides are automatically unlocked if a new game is started
        unlockedLevels = new SerializableHashSet<Level>();

        for (int i = 0; i < defaultUnlockedLevels.Length; i++)
        {
            unlockedLevels.Add(defaultUnlockedLevels[i]);
        }
    }

    /// <summary>
    ///     Initialize this object by attempting to load an existing UnlockedLevelData from the Json file. If no data is found, the levels
    ///     from DefaultUnlockedLevels passed into this object from the constructor are used and a new save file is created using those levels.
    /// </summary>
    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        UnlockedLevelData data = Load<UnlockedLevelData>();

        if (data == null)
        {
#if UNITY_EDITOR
            Debug.Log($"Could not find existing UnlockedLevelData from Json, creating a new Json file {fileName} with the default levels.");
#endif
            // Since unlockedLevels was set to the defaultUnlockedLevels in the constructor, Save() can just be called here to create a new
            // Json file using those default levels
            Save();
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"Unlocked level data successfully loaded from {fileName}.");
#endif
            unlockedGuides = data.unlockedGuides;
            unlockedLevels = data.unlockedLevels;
        }

        initialized = true;
    }

    /// <summary>
    ///     Add levels to the unlockedLevels hashset.
    /// </summary>
    /// <param name="levelsToUnlock">An array of levels to unlock.</param>
    public void AddLevels(Level[] levelsToUnlock)
    {
        if (!initialized)
        {
            Initialize();
        }

        for (int i = 0; i < levelsToUnlock.Length; i++)
        {
            unlockedLevels.Add(levelsToUnlock[i]);
        }

        Save();
    }

    /// <summary>
    ///     Adds all levels found in the Level enum to the unlockedLevel hashset.
    /// </summary>
    public void AddAllLevels()
    {
        AddLevels((Level[])Enum.GetValues(typeof(Level)));
    }

    /// <summary>
    ///     Add a level's guide to the unlockedGuides hashset.
    /// </summary>
    public void AddGuide(Level level)
    {
        if (!initialized)
        {
            Initialize();
        }

        unlockedGuides.Add(level);
        Save();
    }
}
