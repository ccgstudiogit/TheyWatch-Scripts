using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuitApplicationButton : MonoBehaviour
{
    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
