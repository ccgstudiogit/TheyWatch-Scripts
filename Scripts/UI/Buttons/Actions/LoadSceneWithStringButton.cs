using UnityEngine;

public class LoadSceneWithStringButton : MonoBehaviour
{
    // Makes sure if this button is pressed more than once, LoadScene isn't called more than once.
    // Also prevents this button from *somehow* being used before the first frame update
    private bool alreadyLoadingScene = true;

    private void Start()
    {
        alreadyLoadingScene = false;
    }

    public void LoadScene(string sceneName)
    {
        if (SceneSwapManager.instance == null || alreadyLoadingScene)
        {
            return;
        }

        alreadyLoadingScene = true;
        SceneSwapManager.instance.LoadSceneWithFade(sceneName);
    }
}
