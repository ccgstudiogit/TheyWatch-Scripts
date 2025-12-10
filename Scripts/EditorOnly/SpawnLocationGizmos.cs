using UnityEngine;

public class SpawnLocationGizmos : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private float radius = 2f;
    [SerializeField] private Color color = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
