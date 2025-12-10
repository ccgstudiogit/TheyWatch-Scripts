using UnityEngine;

public class AdditivelyLoadScenesHandler : MonoBehaviour
{   
    /// <summary>
    ///     Additively load in the desired scenes.
    /// </summary>
    public void AdditivelyLoadScenes(SceneName[] scenes)
    {
        if (scenes.Length < 1)
        {
            return;
        }

        foreach (SceneName scene in scenes)
        {
            string sN = scene.ToString();

            if (SceneHandler.IsSceneLoaded(sN))
            {
                continue;
            }

            SceneHandler.LoadSceneImmediate(sN, additive: true);
        }
    }
}
