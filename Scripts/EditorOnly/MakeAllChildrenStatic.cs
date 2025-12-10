using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// When attached to a game object, this editor-only script will automatically set any children that are added to it as static
[ExecuteInEditMode]
public class MakeAllChildrenStatic : MonoBehaviour
{
#if UNITY_EDITOR
    private int previousHierarchyHash;

    private void Start()
    {
        previousHierarchyHash = CalculateHierarchyHash();
        SetStaticRecursive(gameObject);
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
            SetStaticRecursive(gameObject);
            previousHierarchyHash = currentHierarchyHash;
        }
    }

    private void SetStaticRecursive(GameObject obj)
    {
        obj.isStatic = true;

        foreach (Transform child in obj.transform)
        {
            SetStaticRecursive(child.gameObject);
        }
    }

    private int CalculateHierarchyHash()
    {
        return HashHierarchyRecursive(transform);
    }

    // Generates a simple hash based on the hierarchy structure
    private int HashHierarchyRecursive(Transform current)
    {
        int hash = current.GetInstanceID();

        foreach (Transform child in current)
        {
            hash ^= HashHierarchyRecursive(child);
        }

        return hash;
    }
#endif
}
