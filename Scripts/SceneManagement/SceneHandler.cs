using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHandler
{
    /// <summary>
    ///     A coroutine that can be used to load scenes.
    /// </summary>
    /// <param name="sceneName">The desired scene to load.</param>
    /// <param name="additive">Should this scene be loaded additively?</param>
    /// <param name="setActive">Should this scene be set as the active scene?</param>
    public static IEnumerator LoadSceneCoroutine(string sceneName, bool additive = false, bool setActive = false)
    {
        // Reloads the current scene if sceneName is empty/null
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

        yield return new WaitUntil(() => loadOperation.isDone);

        if (setActive)
        {
            SetSceneAsActiveScene(sceneName);
        }
    }

    /// <summary>
    ///     Immediately loads a target scene.
    /// </summary>
    /// <param name="sceneName">The desired scene to load.</param>
    /// <param name="additive">Should this scene be loaded additively?</param>
    /// <param name="setActive">Should this scene be set as the active scene?</param>
    public static void LoadSceneImmediate(string sceneName, bool additive = false, bool setActive = false)
    {
        // Reloads the current scene if sceneName is empty/null
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

        if (setActive)
        {
            loadOperation.completed += _ => SetSceneAsActiveScene(sceneName);
        }
    }

    /// <summary>
    ///     Unloads a scene asynchronously.
    /// </summary>
    /// <param name="sceneName">The desired scene to unload.</param>
    public static void UnloadSceneAsync(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
    }

    /// <summary>
    ///     Check what the active scene is.
    /// </summary>
    /// <returns>Scene.</returns>
    public static Scene GetActiveScene()
    {
        return SceneManager.GetActiveScene();
    }

    /// <summary>
    ///     Checks if a particular scene is loaded.
    /// </summary>
    /// <param name="sceneName">The desired scene to check if it's loaded.</param>
    /// <returns>True if the scene is loaded, false if not.</returns>
    public static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.IsValid() && scene.isLoaded;
    }

    private static void SetSceneAsActiveScene(string s)
    {
        Scene loadedScene = SceneManager.GetSceneByName(s);

        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"Failed to set the active scene. Scene '{s}' is invalid.");
#endif
        }
    }   
}
