using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SearchForPlayer : SearchState
{
    [Header("Search Radius Settings")]
    [SerializeField, Min(0.1f)] private float startingRadius = 55f;
    private float radius;
    [SerializeField, Min(0.1f)] private float startingMinTravelDistance = 10f;
    private float minTravelDistance;

    private const float absoluteMinTravelDistance = 1f;

    [Header("Decrease Radius Over Time")]
    [SerializeField] private bool decreaseRadiusOverTime = true;
    [SerializeField, Min(5)] private float minRadius = 17.5f;
    [SerializeField, Min(0)] private float decreaseRadiusByX = 0.75f;
    [SerializeField, Min(0)] private float decreaseRadiusEveryXSeconds = 2f;
    private float timeSinceLastDecrease;

    [Header("Waypoint Precision")]
    [Tooltip("The entity will go within this distance of the waypoint it's moving to (makes it so the entity does not land exactly on " + 
        "a waypoint's position every time, provides some variation)")]
    [SerializeField, Min(0)] private float waypointRadius = 6f;

    [Header("Pause Between Movements")]
    [SerializeField] private bool includePause = true;
    [Tooltip("The min/max time the entity will wait before moving on to the next waypoint")]
    [SerializeField] private Vector2 pauseTimeRange = new Vector2(1f, 4.5f);
    private float pauseTime;
    private float pauseTimeElapsed;

#if UNITY_EDITOR
    [Header("Editor Only")]
    [SerializeField] private Color radiusArcColor = Color.blue;
    [SerializeField] private Color minTravelArcColor = Color.red;
#endif

    public override void EnterState()
    {
        base.EnterState();
        ResetValues();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (player == null)
        {
            player = GetPlayerReference();
            return;
        }

        if (decreaseRadiusOverTime && radius > minRadius)
        {
            timeSinceLastDecrease += Time.deltaTime;

            // Slowly decrease the radius' size down to the minimum
            if (timeSinceLastDecrease > decreaseRadiusEveryXSeconds)
            {
                radius -= decreaseRadiusByX;
                timeSinceLastDecrease = 0f;

                // Make sure to scale minTravelDistance down with the radius
                if (minTravelDistance > absoluteMinTravelDistance)
                {
                    minTravelDistance -= decreaseRadiusByX / minTravelDistance;
                }
            }
        }

        // If finished moving, wait for a random time before continuing
        if (includePause && !IsEntityMovingToPos() && pauseTimeElapsed < pauseTime)
        {
            pauseTimeElapsed += Time.deltaTime;
            return;
        }
        // Move to a random waypoint with the player as the center and the radius being the max distance from a waypoint to the player
        else if (!IsEntityMovingToPos())
        {
            Vector3 posToMoveTo = GetRandomPositionAroundWayPoint(GetRandomWayPointInRadius(player.transform, radius, minTravelDistance), waypointRadius);
            MoveToPosition(posToMoveTo);
        }

        // Makes sure this entity pauses and waits until pauseTime is reached before making movements
        if (pauseTimeElapsed > pauseTime)
        {
            pauseTimeElapsed = 0f;
            pauseTime = GetRandomPauseTime();
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    /// <summary>
    ///     Gets a random pause time.
    /// </summary>
    /// <returns>Returns a float between the serialized Vector2 pauseTimeRange.</returns>
    private float GetRandomPauseTime()
    {
        return Random.Range(pauseTimeRange.x, pauseTimeRange.y);
    }

    private void ResetValues()
    {
        radius = startingRadius;
        minTravelDistance = startingMinTravelDistance;
        timeSinceLastDecrease = 0f;

        pauseTimeElapsed = 0f;
        pauseTime = GetRandomPauseTime();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawArc(player != null ? player.transform.position : transform.position, Application.isPlaying ? radius : startingRadius, radiusArcColor);
        DrawArc(transform.position, Application.isPlaying ? minTravelDistance : startingMinTravelDistance, minTravelArcColor);
    }

    private void DrawArc(Vector3 center, float radius, Color color)
    {
        float angle = 360;
        Handles.color = color;
        Handles.DrawWireArc(center, Vector3.up, Vector3.forward, angle, radius);
    }
#endif
}
