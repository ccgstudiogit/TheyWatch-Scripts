using System.Collections.Generic;
using UnityEngine;

public abstract class SearchState : EntityWayPointState
{
    // Keeps track of waypoints that are within the search radius as well as their distance to this entity (float)
    private List<WayPoint> wayPointsInRange = new List<WayPoint>();

    /// <summary>
    ///     Get a random waypoint within the radius and also the minimum distance. For example, if a waypoint is within the radius but the
    ///     distance of the waypoint to the centerTransform is less than the minimum distance, that waypoint will not be considered.
    /// </summary>
    /// <param name="centerTransform">The transform that should be used as the center (or anchor) of the search.</param>
    /// <param name="radius">The search radius--any waypoints beyond this radius will not be considered.</param>
    /// <param name="minDistance">The minimum distance to move--any waypoints within the search radius but with a distance less than this
    ///     amount will also not be considered.</param>
    /// <returns>A random waypoint using the above parameters.</returns>
    protected WayPoint GetRandomWayPointInRadius(Transform centerTransform, float radius, float minDistance = 0)
    {
        // Makes sure infinite recursion does not occur in the event that if, for whatever reason, this method is unable to find a waypoint
        // within the specified radius
        const int maxRadius = 300;

        wayPointsInRange.Clear();

        // Loop through all of the waypoints on the map and check its distance to this entity. If the distance is within the search radius
        // and minimum distance, add it to the wayPointsInRange list
        for (int i = 0; i < LevelController.instance.wayPoints.Count; i++)
        {
            WayPoint wayPoint = LevelController.instance.wayPoints[i];
            float distanceToWayPoint = Vector3.Distance(wayPoint.transform.position, centerTransform.position); // Distance for centerTransform (most likely player)
            float travelDistance = Vector3.Distance(wayPoint.transform.position, transform.position); // Distance for this entity, used for minDistance

            if (distanceToWayPoint < radius && travelDistance > minDistance)
            {
                wayPointsInRange.Add(wayPoint);
            }
        }

        // Fallback to make sure if a waypoint is not found because the radius and minDistance are too restrictive, the method is called again
        // with double the radius size. The maxRadius check also makes sure infinite recursion does not happen given too many attempts
        if (wayPointsInRange.Count < 1 && radius < maxRadius)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} found no waypoints within radius {radius}. Doubling radius to {radius * 2} and running again.");
#endif
            return GetRandomWayPointInRadius(centerTransform, radius * 2, minDistance);
        }

        WayPoint randomWP = wayPointsInRange[Random.Range(0, wayPointsInRange.Count)];
        return randomWP;
    }
}
