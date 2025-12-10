using UnityEngine;

// ***THIS BUTTON BELONGS IN-GAME WHERE A PAUSE FUNCTION IS UTILIZED***
public class ResumeGameButton : MonoBehaviour
{ 
    public void ResumeGame()
    {
        if (LevelController.instance != null && LevelController.instance.IsPaused())
        {
            LevelController.instance.ResumeGame();
        }
    }
}
