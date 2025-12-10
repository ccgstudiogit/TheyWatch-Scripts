using UnityEngine;

// This script belongs on the Main Camera and controls the behavior associated with the Collectable Sight ability
[RequireComponent(typeof(Camera))]
public class CollectableSightCameraController : MonoBehaviour
{
    [SerializeField] private LayerMask collectableRendererLayer;
    [SerializeField] private Camera collectableRenderer;
    [SerializeField] private LayerMask layersToHideWhileInCollectableSight;
    private Camera mainCam;

    private void Awake()
    {
        if (collectableRenderer == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}'s collectableRenderer null. Collectable Sight mechanic is unable to work.");
#endif
            enabled = false;
        }

        mainCam = GetComponent<Camera>();
    }

    private void Start()
    {
        collectableRenderer.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InputCollectableSight.OnCollectableSight += HandleCollectableSight;
    }

    private void OnDisable()
    {
        InputCollectableSight.OnCollectableSight -= HandleCollectableSight;
    }

    private void HandleCollectableSight(bool isActive)
    {
        if (isActive)
        {
            mainCam.cullingMask &= ~layersToHideWhileInCollectableSight.value;
            mainCam.cullingMask &= ~(1 << collectableRendererLayer); // Removes that layer from the culling mask
            collectableRenderer.gameObject.SetActive(true);
        }
        else
        {
            mainCam.cullingMask |= layersToHideWhileInCollectableSight.value;
            mainCam.cullingMask |= 1 << collectableRendererLayer; // Adds that layer back into the culling mask
            collectableRenderer.gameObject.SetActive(false);
        }
    }
}
