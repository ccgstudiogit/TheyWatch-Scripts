using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class MakeAllChildrenEditorOnly : MonoBehaviour
{
#if UNITY_EDITOR
    private int previousHierarchyHash;

    private void Start()
    {
        previousHierarchyHash = CalculateHierarchyHash();
        SetEditorOnlyRecursive(gameObject);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            return;
        }

        int currentHierarchyHash = CalculateHierarchyHash();
        if (currentHierarchyHash != previousHierarchyHash)
        {
            SetEditorOnlyRecursive(gameObject);
            previousHierarchyHash = currentHierarchyHash;
        }
    }

    private void SetEditorOnlyRecursive(GameObject obj)
    {
        obj.tag = "EditorOnly";

        foreach (Transform child in obj.transform)
        {
            SetEditorOnlyRecursive(child.gameObject);
        }
    }

    private int CalculateHierarchyHash()
    {
        // Generate a simple hash based on the hierarchy structure
        int hash = transform.childCount;
        
        foreach (Transform child in transform)
        {
            hash ^= child.GetInstanceID(); // Use unique instance IDs of child objects
            hash ^= child.childCount;     // Include the count of grandchildren
        }
        
        return hash;
    }
#endif
}
