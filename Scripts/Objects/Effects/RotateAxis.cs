using UnityEngine;

public abstract class RotateAxis : MonoBehaviour
{
    protected abstract Vector3 axis { get; }

    [Header("Rotation Settings")]
    [SerializeField] private Space coordinateSpace = Space.World;
    [SerializeField] private float speed = 25f;

    private void Update()
    {
        transform.Rotate(axis, speed * Time.deltaTime, coordinateSpace);
    }

    /// <summary>
    ///     Set the rotation speed to a new value.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
