using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WayPoint))]
public class DebugWayPointStalkScore : MonoBehaviour
{
#if UNITY_EDITOR
    private WayPoint wayPoint;

    private void OnDrawGizmos()
    {
        if (wayPoint == null)
        {
            wayPoint = GetComponent<WayPoint>();
        }

        string message = $"Score: {wayPoint.GetScore()}";
        Handles.Label(transform.position + Vector3.up, message);
    }
#endif
}
