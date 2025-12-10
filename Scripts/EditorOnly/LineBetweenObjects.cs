using UnityEngine;

/// <summary>
///     Visualize a line between 2 game objects via OnDrawGizmos().
/// </summary>
public class LineBetweenObjects : MonoBehaviour
{
    [SerializeField] private Transform transformOne;
    [SerializeField] private Transform transformTwo;

    private void OnDrawGizmos()
    {
        if (transformOne != null && transformTwo != null)
        {
            Gizmos.DrawLine(transformOne.position, transformTwo.position);
        }
    }
}
