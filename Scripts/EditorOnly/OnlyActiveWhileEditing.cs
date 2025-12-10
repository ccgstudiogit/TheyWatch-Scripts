using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class OnlyActiveWhileEditing : MonoBehaviour
{
#if UNITY_EDITOR
    [Tooltip("If this setting is enabled, once the application starts playing this game object will be turned off")]
    [SerializeField] private bool makeOnlyActiveWhileEditing = true;

    private void Awake()
    {
        if (Application.isPlaying && makeOnlyActiveWhileEditing)
        {
            gameObject.SetActive(false);
        }
    }
#endif
}