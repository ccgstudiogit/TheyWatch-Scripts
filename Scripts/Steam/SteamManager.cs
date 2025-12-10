using System.Collections.Generic;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager instance { get; private set; }

    [Header("App ID")]
    [SerializeField] private uint steamAppID = 3879000;

#if UNITY_EDITOR
    [Header("Editor Settings")]
    [Tooltip("If enabled, SteamManager will connect to the steam client while in playmode")]
    [SerializeField] private bool connectToSteamInPlaymode = false;
#endif

    public bool connectedToSteam { get; private set; } = false;
    private int achievementCount => System.Enum.GetNames(typeof(SteamAchievement)).Length;

    /// <summary>
    ///     Maps each level to its own steam level completion achievement.
    /// </summary>
    public static readonly Dictionary<Level, SteamAchievement> LevelAchievementMap = new Dictionary<Level, SteamAchievement>
    {
        { Level.HedgeMaze, SteamAchievement.Gardener },
        { Level.SimonSays, SteamAchievement.ImListening },
        { Level.Backrooms, SteamAchievement.NoClippedNoProblem },
        { Level.Darkrooms, SteamAchievement.CameraShy },
        { Level.Dungeon, SteamAchievement.Inmate },
        { Level.Jailbreak, SteamAchievement.Jailbird },
        { Level.Factory, SteamAchievement.CogInTheMachine },
        { Level.NoSignal, SteamAchievement.Static }
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        // Makes sure the steam API doesn't activate when entering playmode within the editor
        if (Application.isPlaying && !connectToSteamInPlaymode)
        {
            return;
        }
#endif

        try
        {
            Steamworks.SteamClient.Init(steamAppID);
            connectedToSteam = true;
#if UNITY_EDITOR
            Debug.Log("Successfully connected to steam.");
#endif
        }
        catch (System.Exception e)
        {
            // Something went wrong!
            //
            //    Steam is closed?
            //    Can't find steam_api dll?
            //    Don't have permission to play app?

#if UNITY_EDITOR
            Debug.LogError($"Failed to initialize Steam: {e.Message}");
#endif
            connectedToSteam = false;
        }
    }

    private void OnDisable()
    {
        if (connectedToSteam)
        {
            Steamworks.SteamClient.Shutdown();
        }
    }

    private void Update()
    {
        if (connectedToSteam)
        {
            Steamworks.SteamClient.RunCallbacks();
        }
    }

    /// <summary>
    ///     Unlock a steam achievement.
    /// </summary>
    /// <param name="achievementToUnlock">The steam achievement to unlock.</param>
    public void UnlockAchievement(SteamAchievement achievementToUnlock)
    {
        if (connectedToSteam)
        {
            var achievement = new Steamworks.Data.Achievement("Achievement_" + (int)achievementToUnlock);
            achievement.Trigger();
#if UNITY_EDITOR
            Debug.Log("Unlocked achievement " + achievementToUnlock.ToString());
#endif
        }
#if UNITY_EDITOR
        else
        {
            Debug.Log($"Unlocking achievement {achievementToUnlock} but SteamManager is not currently connected to steam.");
        }
#endif
    }

    /// <summary>
    ///     Check if the player has unlocked a specific steam achievement.
    /// </summary>
    /// <param name="isThisAchievementUnlocked">Has this achievement unlocked?</param>
    /// <returns>True if connected to steam and the user has unlocked the achievement. False if not unlocked or if SteamManager
    ///     is not connected to steam.</returns>
    public bool HasAchievement(SteamAchievement isThisAchievementUnlocked)
    {
        if (connectedToSteam)
        {
            var achievement = new Steamworks.Data.Achievement("Achievement_" + (int)isThisAchievementUnlocked);
            return achievement.State;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("HasAchievement() was called but SteamManager is not connected to steam. Returning false instead.");
#endif
            return false;
        }
    }

    /// <summary>
    ///     Reset all steam achievements. Useful for testing achievements while play-testing, recommended to put this method in
    ///     Awake(), let it call once, and then remove it.
    /// </summary>
    public void ResetAllAchievements()
    {
        if (connectedToSteam)
        {
            for (int i = 0; i < achievementCount; i++)
            {
                var achievement = new Steamworks.Data.Achievement("Achievement_" + i);
                achievement.Clear();
            }
        }
    }
}
