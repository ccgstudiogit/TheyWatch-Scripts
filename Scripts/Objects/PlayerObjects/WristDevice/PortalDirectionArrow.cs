using UnityEngine;

public class PortalDirectionArrow : MonoBehaviour
{
    [Tooltip("This reference is necessary to calculate the angle towards the portal (Recommend to use the arms root object)")]
    [SerializeField] private GameObject root;

    [SerializeField] private RectTransform arrowTransform;

    private Portal portal;
    private Transform portalTransform;

    private void Start()
    {
        if (arrowTransform == null || root == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s arrow || root reference null. Disabling PortalDirectionArrow.cs");
#endif
            enabled = false;
            return;
        }

        if (Portal.instance != null)
        {
            portal = Portal.instance;
            portalTransform = portal.gameObject.transform;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Portal.instance null. Disabling PortalDirectionArrow.cs");
#endif
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        Vector2 targetDir = new Vector2(
            portalTransform.position.x - root.transform.position.x,
            portalTransform.position.z - root.transform.position.z
        );

        Vector2 forwardTransform = new Vector2(
            root.transform.forward.x,
            root.transform.forward.z
        );

        float angle = Vector2.SignedAngle(targetDir, forwardTransform);

        Quaternion newRotation = Quaternion.Euler(0, 0, -angle);
        arrowTransform.localRotation = newRotation;
    }
}
