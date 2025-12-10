using System;
using UnityEngine;

// This script belongs on a game object within the specific scene and will fire an event when this game object is loaded,
// meaning that the scene has been loaded.
// Currently this is only used to detect if the main menu was loaded in order to destroy DontDestroyOnLoad game objects that
// were created for in-level use (such as LevelController).
public class SceneLoadedDetector : MonoBehaviour
{
    public static event Action<SceneName> OnSceneLoaded;

    [SerializeField] private SceneName sceneName;

    private void Awake()
    {
        OnSceneLoaded?.Invoke(sceneName);
    }
}
