using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EntityWayPointState : EntityState
{
    protected List<WayPoint> closestWayPoints = new List<WayPoint>();
    protected List<WayPoint> farthestWayPoints = new List<WayPoint>();

    // This is used to keep track of which way point this entity is currently at
    protected WayPoint currentWayPoint;

    // This is used as the minimum for the dot product when checking to see if this entity can move forward towards
    // a target in the target's general direction. If the dot product returns a 1, then the waypoint is directly ahead
    // of the entity. If the dot product returns a 0, then the waypoint is perpendicular to this entity. If the dot product
    // returns a -1, then the way point is directly behind this entity. Recommended to keep dotProductMin around 0.2-0.4
    // so that this entity searches for waypoints to move forward to that are relatively in front of the dir entity is facing
    private float dotProductMin = 0.3f;

    public override void EnterState()
    {
        base.EnterState();

        if (LevelController.instance == null)
        {
#if UNITY_EDITOR
            Debug.LogError("LevelController.instance null. EntityWayPointState unable to function.");
#endif
            enabled = false;
            return;
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    /// <summary>
    ///     Set this entity's agent destination to this waypoint's gameObject's transform. If the WayPoint does not have a valid
    ///     path, a new random WayPoint will be selected.
    /// </summary>
    protected void MoveToWayPoint(WayPoint wayPoint, bool makeSureWayPointHasValidPath = true)
    {
        // If doing a valid path check isn't necessary/needed, just start moving the entity to this waypoint
        if (!makeSureWayPointHasValidPath)
        {
            currentWayPoint = wayPoint;
            MoveToTransform(currentWayPoint.gameObject.transform);
            return;
        }

        if (IsPathValid(wayPoint.transform.position))
        {
            currentWayPoint = wayPoint;
            MoveToTransform(currentWayPoint.gameObject.transform);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Invalid path while moving to {wayPoint.gameObject.name}. Moving to a different random waypoint.");
#endif
            // Loop through and attempt to find a random valid waypoint
            int maxAttempts = 35;
            WayPoint newWayPoint;

            for (int i = 0; i < maxAttempts; i++)
            {
                newWayPoint = GetRandomWayPoint(LevelController.instance.wayPoints);

                if (IsPathValid(newWayPoint.transform.position))
                {
                    // Since the path is already verified, no need to check again
                    MoveToWayPoint(newWayPoint, makeSureWayPointHasValidPath: false);
                    return;
                }
            }
#if UNITY_EDITOR
            Debug.LogError("Max attempts reached trying to go to a random waypoint with a valid path. Unable to move.");
#endif
        }
    }

    /// <summary>
    ///     Instantly move the entity to this waypoint's transform.position.
    /// </summary>
    protected void TeleportToWayPoint(WayPoint wayPoint)
    {
        currentWayPoint = wayPoint;
        entity.Teleport(wayPoint.gameObject.transform.position);
    }

    /// <summary>
    ///     Moves to the next sequential waypoint heading towards a target game object.
    /// </summary>
    /// <param name="target">The game object this entity should move towards.</param>
    /// <param name="maxWayPoints">The number of waypoints this entity will consider when moving.</param>
    /// <returns>A WayPoint to move towards.</returns>
    protected WayPoint GetSequentialWayPointTowardsTarget(GameObject target, int maxWayPoints = 4)
    {
        GetClosestWayPoints(maxWayPoints, excludeCurrentWayPoint: true);

        // Make sure this entity can move towards the object. If not, increase maxWayPoints by 1 and run this method again up to
        // a maximum of 10 waypoints (after the 10 closest waypoints have been evaluated, it's safe to say that the entity is looking
        // in a direction that has no waypoints as CannotMoveForward() checks to make sure the waypoints are in front of this entity's
        // direciton). If the maximum of 10 is reached, just move this entity along
        if (CannotMoveForward() && maxWayPoints <= 10)
        {
#if UNITY_EDITOR
            if (maxWayPoints >= 10)
            {
                Debug.Log("maxWayPoints attempts reached with CannotMoveForward() check, skipping CannotMoveForward() check.");
            }
#endif
            return GetSequentialWayPointTowardsTarget(target, ++maxWayPoints);
        }

        WayPoint bestWayPoint = null;
        Vector3 toTarget = (target.transform.position - transform.position).normalized;

        float bestScore = Mathf.Infinity;
        float angleWeight = 0.4f; // The weight the angle has on the score (higher == more influence on score)
        float distanceWeight = 2.75f; // The weight the distance has on the score (higher == more influence on score)

        for (int i = 0; i < closestWayPoints.Count; i++)
        {
            Vector3 toWayPoint = (closestWayPoints[i].transform.position - transform.position).normalized;

            float angle = Vector3.Angle(toTarget, toWayPoint);
            float distance = Vector3.Distance(closestWayPoints[i].transform.position, target.transform.position);

            // Lower angle and lower distance = better score (lower score means closer to target)
            float score = angle * angleWeight + distance * distanceWeight;

            if (score < bestScore)
            {
                bestScore = score;
                bestWayPoint = closestWayPoints[i];
            }
        }

        return bestWayPoint;
    }

    /// <summary>
    ///     Checks if the entity is soft-locked out of being able to move foward based on the direction it's currently facing. If all of
    ///     the closest waypoints are behind this entity and the entity is forced to move backward, this returns true. Note: in order for
    ///     this method to function correctly, GetClosestWayPoints() should be called before running this method.
    /// </summary>
    /// <returns>True if the entity cannot move forward in the direction its facing with the current amount of waypoints, false if it can
    ///     move forward.</returns>
    protected bool CannotMoveForward()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        for (int i = 0; i < closestWayPoints.Count; i++)
        {
            if (Vector3.Dot(forward, Vector3.Normalize(closestWayPoints[i].gameObject.transform.position - transform.position)) > dotProductMin)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Gets a random WayPoint from a list of WayPoints.
    /// </summary>
    /// <returns>A randomly selected WayPoint from the list.</returns>
    protected WayPoint GetRandomWayPoint(List<WayPoint> wayPoints, bool excludeCurrentWayPoint = true)
    {
        // Makes sure that this method does not end up in infinite recursion if there is only one waypoint
        if (wayPoints.Count == 1)
        {
            return wayPoints[0];
        }

        if (wayPoints.Count < 1)
        {
            return null;
        }

        WayPoint randomWP = wayPoints[Random.Range(0, wayPoints.Count)];

        if (excludeCurrentWayPoint && randomWP == currentWayPoint)
        {
            return GetRandomWayPoint(wayPoints);
        }

        return randomWP;
    }

    /// <summary>
    ///     Finds a random distant waypoint from this entity's current position.
    /// </summary>
    /// <param name="maxWayPoints">The number of waypoints to consider.</param>
    /// <returns>A random distant WayPoint.</returns>
    protected WayPoint GetRandomDistantWayPoint(int maxWayPoints = 4)
    {
        GetFarthestWayPoints(maxWayPoints);
        return GetRandomWayPoint(farthestWayPoints);
    }

    /// <summary>
    ///     Moves to a random waypoint that is close to the player's current location.
    /// </summary>
    /// <param name="maxWayPoints">The number of waypoints to consider.</param>
    protected void MoveToWayPointCloseToPlayer(int maxWayPoints = 4)
    {
        if (player == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} is attempting to move to a way point close to the player but player is null.");
#endif
            return;
        }

        GetClosestWayPoints(maxWayPoints, excludeCurrentWayPoint: true, player);
        MoveToTransform(closestWayPoints[Random.Range(0, closestWayPoints.Count)].gameObject.transform);
    }

    // GameObject aroundThisObj can be used to find the closest way points to that particular game object's position
    /// <summary>
    ///     Clears the closestWayPoints list and searches for the closest waypoints, then adds the closest waypoints to the list.
    /// </summary>
    /// <param name="maxWayPoints">The number of waypoints that should be added to the closestWayPoints list.</param>
    /// <param name="excludeCurrentWayPoint">If true, exclude this entity's current waypoint from being added to the list.</param>
    /// <param name="aroundThisObj">If set, get the closest waypoints around aroundThisObj instead of this entity.</param>
    protected void GetClosestWayPoints(int maxWayPoints = 4, bool excludeCurrentWayPoint = true, GameObject aroundThisObj = null)
    {
        closestWayPoints.Clear();

        Vector3 position = aroundThisObj != null ? aroundThisObj.transform.position : transform.position;

        // Create a new list of candidate waypoints that will be sorted based on distance
        List<WayPoint> candidates = new List<WayPoint>();

        for (int i = 0; i < LevelController.instance.wayPoints.Count; i++)
        {
            // Skip this entity's current waypoint if needed
            if (excludeCurrentWayPoint && LevelController.instance.wayPoints[i] == currentWayPoint)
            {
                continue;
            }

            candidates.Add(LevelController.instance.wayPoints[i]);
        }

        // Sort the candidate waypoints by distance
        candidates.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.position, position);
            float distB = Vector3.Distance(b.transform.position, position);
            return distA.CompareTo(distB);
        });

        // Add the top N closest waypoints to closestWayPoints
        int count = Mathf.Min(maxWayPoints, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            closestWayPoints.Add(candidates[i]);
        }

#if UNITY_EDITOR
        foreach (WayPoint point in closestWayPoints)
        {
            Debug.DrawLine(transform.position + Vector3.up, point.transform.position + Vector3.up, Color.blue, 2f);
        }
#endif
    }

    /// <summary>
    ///     Clears the farthestWayPoints list and searches for the farthest waypoints, then adds the farthest waypoints to the list.
    /// </summary>
    /// <param name="maxWayPoints">The number of waypoints to consider.</param>
    protected void GetFarthestWayPoints(int maxWayPoints = 4)
    {
        farthestWayPoints.Clear();

        // Create a new list of candidate waypoints that will be sorted based on distance
        List<WayPoint> candidates = new List<WayPoint>();

        for (int i = 0; i < LevelController.instance.wayPoints.Count; i++)
        {
            candidates.Add(LevelController.instance.wayPoints[i]);
        }

        // Sort the candidate waypoints by distance
        candidates.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.position, transform.position);
            float distB = Vector3.Distance(b.transform.position, transform.position);
            return distB.CompareTo(distA);
        });

        // Add the top N farthest waypoints to closestWayPoints
        int count = Mathf.Min(maxWayPoints, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            farthestWayPoints.Add(candidates[i]);
        }

#if UNITY_EDITOR
        foreach (WayPoint point in farthestWayPoints)
        {
            Debug.DrawLine(transform.position + Vector3.up, point.transform.position + Vector3.up, Color.blue, 2f);
        }
#endif
    }

    /// <summary>
    ///     Calculates the distance from this entity to a target game object.
    /// </summary>
    /// <param name="target">The target game object that the distance should be calculated with.</param>
    /// <returns>A float distance between this entity and target game object.</returns>
    protected float DistanceToTarget(GameObject target)
    {
        return Vector3.Distance(target.transform.position, transform.position);
    }

    /// <summary>
    ///     Get a random position on the navmesh around a waypoint within a specified radius.
    /// </summary>
    /// <param name="wayPoint">The waypoint that should be used as the center position within the radius.</param>
    /// <param name="radius">The radius (or range) around the waypoint.</param>
    /// <param name="includeObjectDetection">Whether or not objects around the waypoint should block a position near the waypoint
    ///     (for example, if the entity should NOT be able to move to on the opposite side of the wall compared to the waypoint).</param>
    /// <returns>A random Vector3 position within the radius around the waypoint.</returns>
    protected Vector3 GetRandomPositionAroundWayPoint(WayPoint wayPoint, float radius, bool includeObjectDetection = true)
    {
        return FindNewRandomPositionInRange(radius, includeObjectDetection, centerOfRange: wayPoint.gameObject);
    }

#if UNITY_EDITOR
    protected void DebugClosestWayPoints(float lineDuration = 0.1f)
    {
        foreach (WayPoint closePoint in closestWayPoints)
        {
            Debug.DrawLine(transform.position, closePoint.transform.position, Color.green, lineDuration);
        }
    }
#endif
}
