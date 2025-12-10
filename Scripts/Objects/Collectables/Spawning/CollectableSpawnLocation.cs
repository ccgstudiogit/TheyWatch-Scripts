using UnityEngine;

public class CollectableSpawnLocation : MonoBehaviour
{
    [Tooltip("The offset is applied to the spawn position (recommend to apply a y-offset so that collectables spawn above ground)")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.75f, 0);
    private Vector3 position;
    private Transform trans;

    private void Awake()
    {
        position = transform.position + offset;
        trans = transform;

        if (CollectableSpawnController.instance != null)
        {
            CollectableSpawnController.instance.RegisterSpawnLocation(this);
        }
    }

    /// <summary>
    ///     Get this spawn location's position.
    /// </summary>
    /// <returns>A Vector3 position.</returns>
    public Vector3 GetPosition()
    {
        return position;
    }

    /// <summary>
    ///     Get this spawn location's transform
    /// </summary>
    /// <returns>Transform.</returns>
    public Transform GetTransform()
    {
        return trans;
    }
}
