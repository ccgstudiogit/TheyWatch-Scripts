using UnityEngine;

public class DiscordManager : MonoBehaviour
{
    [SerializeField] private long clientId = 1440490202630983823;

    private Discord.Discord discord;

    private void Start()
    {
        discord = new Discord.Discord(clientId, (ulong)Discord.CreateFlags.NoRequireDiscord);
        ChangeActivity();
    }

    private void OnApplicationQuit()
    {
        if (discord != null)
        {
            discord.Dispose();
        }
    }

    private void Update()
    {
        if (discord != null)
        {
            discord.RunCallbacks();
        }
    }

    public void ChangeActivity()
    {
        if (discord == null)
        {
            return;
        }

        Discord.ActivityManager activityManager = discord.GetActivityManager();
        Discord.Activity activity = new Discord.Activity
        {
            State = "Playing"
        };

        activityManager.UpdateActivity(activity, (res) =>
        {
#if UNITY_EDITOR
            Debug.Log("Activity updated");
#endif
        });
    }
}
