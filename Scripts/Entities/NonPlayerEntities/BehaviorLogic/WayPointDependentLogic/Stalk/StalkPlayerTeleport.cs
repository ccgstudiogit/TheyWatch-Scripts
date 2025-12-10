using System.Collections.Generic;
using UnityEngine;

public class StalkPlayerTeleport : StalkState
{
    [Header("Strategic WayPoint Movement Settings")]
    [Tooltip("When completing a strategic move towards player, consider this amount of the closest waypoints to the player")]
    [SerializeField, Min(0)] private int maxStrategicWayPoints = 8;
    [Tooltip("Only calculate a strategic waypoint movement if the player is within this distance to this entity. Otherwise, " +
        "perform a sequential movement towards the general direction of the player (Visualized by blue circle)")]
    [SerializeField, Min(0f)] private float maxStrategicDistance = 30f;
    [Tooltip("If a waypoint is visible (within the player camera's frustrum) and within this distance, do not consider this waypoint")]
    [SerializeField, Min(0f)] private float minVisibleWaypointDist = 21f;
# if UNITY_EDITOR
    public override float strategicWayPointRange => maxStrategicDistance;
#endif

    [Header("- Generic Movement/Behavior/Other Settings -")]
    [Header("Sequential WayPoint Movement Settings")]
    [Tooltip("When moving from waypoint to waypoint, this entity will get the closest maxGenericWayPoints and evaluate those." +
        "Note: If the entity cannot move towards the player with this amount of waypoints, more will be added for that movement")]
    [SerializeField, Min(0)] private int maxGenericWayPoints = 4;

    [Header("Generic Movement Settings")]
    [Tooltip("The min pause time and max pause time after performing a movement from one waypoint to another")]
    [SerializeField] private Vector2 pauseTimeRange = new Vector2(0.2f, 2f);
    private float pauseTime, pauseTimeElapsed;
    [Tooltip("This entity will stop moving towards the player if the player is within this range (Visualized by red circle)")]
    [SerializeField, Min(0f)] private float stopAtThisDistanceToPlayer = 10f;
#if UNITY_EDITOR
    public override float stopAtPlayerRange => stopAtThisDistanceToPlayer;
#endif
    [Tooltip("The maximum time in seconds this entity will be stopped while close enough to the player. Once this limit is reached, " +
            "this entity will move to a different strategic waypoint.")]
    [SerializeField, Min(0f)] private float timeStoppedNearPlayer = 8f;
    private float elapsedTimeStoppedNearPlayer;

    [Header("Looking At Player")]
    [Tooltip("This reference allows the entity to look at the player while not moving (otherwise the nav mesh agent component " +
        "prevents this entity from being able to change rotations")]
    [SerializeField] private Transform pivot;
    [Tooltip("The speed at which this entity will look at the player when sitting at a waypoint")]
    [SerializeField, Min(0f)] private float lookTrackingSpeed = 10f;

    [Header("Stalk Behavior")]
    [Tooltip("When entering Stalk State from any other state, this is the stalk behavior this entity will start off with")]
    [SerializeField] private Behavior startingBehavior = Behavior.Passive;
    [Tooltip("The time in seconds spent stalking before this entity's stalk behavior will become aggressive")]
    [SerializeField, Min(0f)] private float timeBeforeBecomingAggressive = 22f;
    private float timeElapsedStalking;

    private float distanceToPlayer; // This is cached so distance doesn't have to be calculated multiple times per frame for this entity

    private List<WayPoint> wayPointsBehindCover = new List<WayPoint>();

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (pivot == null)
        {
            Debug.LogWarning($"{gameObject.name}'s pivot reference null. Unable to look at player.");
        }
#endif
    }

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

        timeElapsedStalking += Time.deltaTime;

        // Stalk more aggressively after stalking without any success after a certain amount of time
        if (currentBehavior != Behavior.Aggressive && timeElapsedStalking > timeBeforeBecomingAggressive)
        {
            currentBehavior = Behavior.Aggressive;
#if UNITY_EDITOR
            Debug.Log($"{entity.gameObject.name}'s current behavior switched to " + currentBehavior.ToString());
#endif
        }

        distanceToPlayer = DistanceToTarget(player);

        // Make sure this entity stays looking at the player/in the player's direction if not moving
        if (pivot != null && entity.agent.velocity.magnitude < 0.5f)
        {
            LookAtTarget(player.transform, pivot, lookTrackingSpeed);
        }

        // Check if the distance is small enough to be completely stopped
        if (distanceToPlayer < stopAtThisDistanceToPlayer)
        {
            elapsedTimeStoppedNearPlayer += Time.deltaTime;

            // Check if this entity has spent too much time stopped in this spot. If so, move positions -- this keeps this
            // entity from being soft-locked out of doing anything if it cannot see the player and the player doesn't move
            if (elapsedTimeStoppedNearPlayer > timeStoppedNearPlayer && !WithinCameraFrustrum(gameObject))
            {
                TeleportToStrategicWayPoint(maxStrategicWayPoints);
                elapsedTimeStoppedNearPlayer = 0f;
            }
        }
        // If not within distance to player, pause for a bit in between movements
        else if (pauseTimeElapsed < pauseTime)
        {
            // Only increment pauseTimeElapsed while actually paused
            pauseTimeElapsed += Time.deltaTime;
            return;
        }
        // Check if the distance is small enough to perform a strategic movement
        else if (distanceToPlayer < maxStrategicDistance && BehindCover(gameObject, coverRaycastOffset, coverLayerMask))
        {
            TeleportToStrategicWayPoint(maxStrategicWayPoints);
        }
        // If distance is not small enough to stop at or perform a strategic movement, perform a sequential movement
        // towards the general direction of the player
        else if (distanceToPlayer > maxStrategicDistance)
        {
            TeleportToWayPoint(GetSequentialWayPointTowardsTarget(player, maxGenericWayPoints));
        }

        // Makes sure this entity pauses and waits until pauseTime is reached before making movements
        if (pauseTimeElapsed > pauseTime)
        {
            pauseTimeElapsed = 0f;
            pauseTime = GetRandomPauseTime();
        }
    }

    /// <summary>
    ///     Teleport to a strategic waypoint and make sure that the waypoint is not visible and behind cover.
    /// </summary>
    private void TeleportToStrategicWayPoint(int maxWayPoints, int attempts = 0)
    {
        const int maxRecursion = 50;
        wayPointsBehindCover.Clear();
        GetClosestWayPoints(maxWayPoints, excludeCurrentWayPoint: true);

        // For making sure the waypoint is not within the camera's frustrum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);

        // Add waypoints that are behind cover to the wayPointsBehindCover list and evaluate those waypoints to get a strategic one
        for (int i = 0; i < closestWayPoints.Count; i++)
        {
            float wayPointDistanceToPlayer = (closestWayPoints[i].gameObject.transform.position - playerCamera.transform.position).sqrMagnitude;
            if (BehindCover(closestWayPoints[i].gameObject, coverRaycastOffset, coverLayerMask) && (wayPointDistanceToPlayer > minVisibleWaypointDist * minVisibleWaypointDist || !WithinCameraFrustrum(closestWayPoints[i].gameObject, planes)))
            {
                wayPointsBehindCover.Add(closestWayPoints[i]);
            }
        }

        if (wayPointsBehindCover.Count > 0)
        {
            TeleportToWayPoint(GetStrategicWayPointAroundPlayer(wayPointsBehindCover));
        }
        else if (attempts < maxRecursion)
        {
            attempts++;
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} increasing maxWayPoints to {maxWayPoints++}, attempt #{attempts}.");
#endif
            TeleportToStrategicWayPoint(maxWayPoints++, attempts);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"{gameObject.name} could not find any close waypoints that were behind cover, selecting a random waypoint.");
#endif
            TeleportToWayPoint(LevelController.instance.wayPoints[Random.Range(0, LevelController.instance.wayPoints.Count)]);
        }
    }

    /// <summary>
    ///     Gets a random pause time.
    /// </summary>
    /// <returns>Returns a float between the serialized Vector2 pauseTimeRange.</returns>
    private float GetRandomPauseTime()
    {
        return Random.Range(pauseTimeRange.x, pauseTimeRange.y);
    }

    /// <summary>
    ///     Instead of having value resets in EnterState(), keep it here to make it look nicer.
    /// </summary>
    private void ResetValues()
    {
        currentBehavior = startingBehavior;

        distanceToPlayer = 0f;

        timeElapsedStalking = 0f;

        pauseTimeElapsed = 0f;
        pauseTime = GetRandomPauseTime();

        elapsedTimeStoppedNearPlayer = 0f;
    }
}
