using System.Collections.Generic;
using UnityEngine;

public abstract class SleepState : EntityState
{
    private ISleepPointLevelController sleepPointLevelController;

    [Tooltip("Any object with this layer will be considered as cover. For a SleepPoint to be considered valid, it must be behind cover")]
    [SerializeField] private LayerMask coverLayerMask;
    private Vector3 coverRaycastOffset = Vector3.up; // This is used as an offset for raycasting so that raycasts don't hit ground and return true

    protected List<SleepPoint> validSleepPoints = new List<SleepPoint>();
    protected SleepPoint currentSleepPoint = null;

    protected override void Start()
    {
        base.Start();

        sleepPointLevelController = LevelController.instance as ISleepPointLevelController;

        // Makes sure that if coverLayerMask is accidentally not set to anything that at the very least it will have Default layer
        HelperMethods.AddLayerToLayerMask(ref coverLayerMask, "Default");
    }

    /// <summary>
    ///     Move to a sleep point (this entity is teleported to the sleep point's location).
    /// </summary>
    /// <param name="sleepPoint">The SleepPoint to move to.</param>
    protected void MoveToSleepPoint(SleepPoint sleepPoint)
    {
        if (currentSleepPoint != null)
        {
            currentSleepPoint.SetOccupied(false);
        }

        currentSleepPoint = sleepPoint;
        entity.Teleport(currentSleepPoint.gameObject.transform.position);

        currentSleepPoint.SetOccupied(true);
    }

    /// <summary>
    ///     Get an un-occupied sleep point within a specified distance to the player. Sleep points that are currently occupied or
    ///     are within the player's view and not obstructed by an obstacle or not considered as valid.
    /// </summary>
    /// <param name="maxDistanceToPlayer">The maximum distance the sleep point should be to the player in order to be considered.</param>
    /// <param name="minDistanceToPlayer">Optional minimum distance the sleep point should be to the player in order to be considered.</param>
    /// <returns>A valid SleepPoint.</returns>
    protected SleepPoint GetSleepPointWithinDistance(float maxDistanceToPlayer, float minDistanceToPlayer = 0)
    {
        GetValidSleepPoints(maxDistanceToPlayer, minDistanceToPlayer);

        if (validSleepPoints.Count > 0)
        {
            return validSleepPoints[Random.Range(0, validSleepPoints.Count)];
        }
        // All of this happens if there are no valid sleep points found
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Not enough valid sleep points. Picking a random one without taking distance to player into consideration.");
#endif
            const int maxAttempts = 100;
            int attempts = 0;

            while (attempts <= maxAttempts)
            {
                attempts++;
                SleepPoint sleepPoint = sleepPointLevelController.sleepPoints[Random.Range(0, sleepPointLevelController.sleepPoints.Count)];

                if (!sleepPoint.IsOccupied() && BehindCover(sleepPoint.gameObject, coverRaycastOffset, coverLayerMask))
                {
                    return sleepPoint;
                }
            }

            // Final fallback. Just a pick a random sleep point, doesn't matter if it's occupied or not
            return sleepPointLevelController.sleepPoints[Random.Range(0, sleepPointLevelController.sleepPoints.Count)];
        }
    }

    /// <summary>
    ///     Fills the validSleepPoints list with valid sleep points this entity can teleport to.
    /// </summary>
    private void GetValidSleepPoints(float maxDistanceToPlayer, float minDistanceToPlayer)
    {
        validSleepPoints.Clear();

        for (int i = 0; i < sleepPointLevelController.sleepPoints.Count; i++)
        {
            if (IsSleepPointValid(sleepPointLevelController.sleepPoints[i], maxDistanceToPlayer, minDistanceToPlayer))
            {
                validSleepPoints.Add(sleepPointLevelController.sleepPoints[i]);
            }
        }
    }

    /// <summary>
    ///     Check if a sleep point is a valid one to move to or not. If it is valid, that means it is not currently occupied or visible
    ///     by the player.
    /// </summary>
    /// <returns>True if valid and the entity should be able to move to it, false if otherwise.</returns>
    private bool IsSleepPointValid(SleepPoint sleepPoint, float maxDistance, float minDistance)
    {
        // Make sure the sleep point is not occupied
        if (sleepPoint.IsOccupied())
        {
            return false;
        }

        // Make sure the sleep point is not too far and not too close to the player
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, sleepPoint.gameObject.transform.position);

            if (distanceToPlayer > maxDistance || distanceToPlayer < minDistance)
            {
                return false;
            }
        }

        // Make sure the sleep is not behind cover
        if (!BehindCover(sleepPoint.gameObject, coverRaycastOffset, coverLayerMask))
        {
            return false;
        }

        return true;
    }
}
