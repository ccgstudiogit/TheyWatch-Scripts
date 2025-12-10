using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    [SerializeField] private Transform targetObj;

    private void Awake()
    {
        if (targetObj == null)
        {
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        transform.rotation = targetObj.rotation;
    }
}
