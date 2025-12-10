using UnityEngine;

public class MonsterSpawnLocation : MonoBehaviour
{
    private Vector3 position;
    private Transform trans;

    private void Awake()
    {
        position = transform.position;
        trans = transform;

        if (MonsterSpawnController.instance != null)
        {
            MonsterSpawnController.instance.RegisterSpawnLocation(this);
        }
    }

    /// <summary>
    ///     Get this spawn location's position.
    /// </summary>
    public Vector3 GetPosition()
    {
        return position;
    }

    /// <summary>
    ///     Get this spawn location's transform.
    /// </summary>
    public Transform GetTransform()
    {
        return trans;
    }
}
