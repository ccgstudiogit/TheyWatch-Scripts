using UnityEngine;

public class FlashlightCollisions : MonoBehaviour
{
    [SerializeField] private Collider lightCollider;

    [Tooltip("The raycast for collision detection will only hit objects with these layers")]
    [SerializeField] private LayerMask layerMask;

    private void Awake()
    {
        if (lightCollider == null)
        {
            lightCollider = GetComponent<Collider>();
            if (lightCollider == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{gameObject.name} does not have collider component. Destroying {gameObject.name}.");
#endif
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    ///     Checks if the target's transform is considered visible in this flashlight by using a Raycast and checking colliders.
    ///     targetComponent is used to check if the target's collider's game object has this specific component (useful for checking
    ///     if the game object has a FlashlightDetector component for instance).
    /// </summary>
    /// <returns>True if no collisions were detected, false if otherwise.</returns>
    public bool IsVisible<T>(T targetComponent, Transform target) where T : MonoBehaviour
    {
        if (Physics.Raycast(transform.position, Vector3.Normalize(target.position - transform.position), out RaycastHit hit, 20f, layerMask))
        {
            if (hit.collider.TryGetComponent<T>(out _))
            {
#if UNITY_EDITOR
                Debug.DrawRay(transform.position, Vector3.Normalize(target.position - transform.position) * hit.distance, Color.green);
#endif
                return true;
            }
        }

#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Vector3.Normalize(target.position - transform.position) * 25f, Color.red);
#endif

        return false;
    }

    /// <summary>
    ///     Makes sure that this flashlight's lightCollider game object is active.
    /// </summary>
    /// <returns>True if the lightCollider game object is active, false if not.</returns>
    public bool IsColliderGameObjectActive()
    {
        return lightCollider.gameObject.activeSelf;
    }
}
