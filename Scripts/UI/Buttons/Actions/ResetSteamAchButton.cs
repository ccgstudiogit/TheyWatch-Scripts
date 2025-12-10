using UnityEngine;

public class ResetSteamAchButton : MonoBehaviour
{
    public void ResetSteamAchievements()
    {
        if (SteamManager.instance != null)
        {
            SteamManager.instance.ResetAllAchievements();
        }
    }
}
