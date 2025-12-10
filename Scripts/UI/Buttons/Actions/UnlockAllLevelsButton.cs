using UnityEngine;

public class UnlockAllLevelsButton : MonoBehaviour
{
    public void UnlockAllLevels()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.UnlockAllLevels();
        }
    }
}
