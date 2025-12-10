using UnityEngine;

// **Note: A Rigidbody is required for radar to find the collectable
[RequireComponent(typeof(Collectable), typeof(Rigidbody))]
public class CollectableCollisions : MonoBehaviour
{
    private RadarHitbox radarHitbox = null;
    private Collectable collectable;

    private void Awake()
    {
        collectable = GetComponent<Collectable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out RadarHitbox radarHitbox))
        {
            this.radarHitbox = radarHitbox;
            this.radarHitbox.AddCollectable(collectable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out RadarHitbox radarHitbox))
        {
            radarHitbox.RemoveCollectable(collectable);
            this.radarHitbox = null;
        }
    }

    private void OnDestroy()
    {
        if (radarHitbox != null)
        {
            radarHitbox.RemoveCollectable(collectable);
        }
    }
}
