using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SleepAroundPlayer : SleepState
{
    [Header("Movement Settings")]
    [Tooltip("If this entity's distance to the player exceeds this amount, the entity is moved to another sleep point within this " +
        "distance to the player")]
    [SerializeField, Min(0f)] private float maxDistanceToPlayer = 25f;
    [Tooltip("The entity will not move to a sleep point within this distance to the player")]
    [SerializeField, Min(0f)] private float minDistanceToPlayer = 10f;
    [Tooltip("Once this entity has moved, it will remain at the current SleepPoint for at least this many seconds before moving on")]
    [SerializeField, Min(0f)] private float minTimeBetweenMovements = 10f;
    private float timeElapsedAfterMovement;

#if UNITY_EDITOR
    [Header("Visualize Distance To Player")]
    [SerializeField] private bool visualizeDistance = true;
    [Tooltip("If the player is no longer within this range, move to a new sleep point")]
    [SerializeField] private Color moveDistanceColor = Color.green;
    [Tooltip("This entity will not move to a sleep point if the sleep point is within this distance to the player")]
    [SerializeField] private Color minDistanceColor = Color.red;
#endif

    public override void EnterState()
    {
        base.EnterState();

        timeElapsedAfterMovement = 0f;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (player == null)
        {
            player = GetPlayerReference();
            return;
        }

        timeElapsedAfterMovement += Time.deltaTime;
        float distanceToPlayer = (transform.position - player.transform.position).magnitude;

        if (distanceToPlayer > maxDistanceToPlayer && timeElapsedAfterMovement > minTimeBetweenMovements)
        {
            MoveToRandomSleepPoint();
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    /// <summary>
    ///     Move to a random sleep point within the maxDistanceToPlayer.
    /// </summary>
    private void MoveToRandomSleepPoint()
    {
        SleepPoint newSleepPoint = GetSleepPointWithinDistance(maxDistanceToPlayer, minDistanceToPlayer);
        MoveToSleepPoint(newSleepPoint);
        timeElapsedAfterMovement = 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (visualizeDistance)
        {
            DrawArc(transform.position, maxDistanceToPlayer, moveDistanceColor);
            DrawArc(player != null ? player.transform.position : transform.position, minDistanceToPlayer, minDistanceColor);
        }
    }

    private void DrawArc(Vector3 center, float radius, Color color)
    {
        float angle = 360;
        Handles.color = color;
        Handles.DrawWireArc(center, Vector3.up, Vector3.forward, angle, radius);
    }
#endif
}
