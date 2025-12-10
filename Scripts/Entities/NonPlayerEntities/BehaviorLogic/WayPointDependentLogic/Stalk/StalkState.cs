using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class StalkState : EntityWayPointState
{
    [Header("- Strategic WayPoint Navigation -")]
    [Header("Visibility Score Settings")]
    [Tooltip("If an object is considered visible, that is a bad thing for visibility score as waypoints should be " +
     "hidden out of the camera's view frustrum to promote stealth")]
    [SerializeField] private float visibleScore = 0f;
    [SerializeField] private float notVisibleScore = 1f;
    [Tooltip("The weight of this score in comparison to the scores")]
    [SerializeField] private float visibilityWeight = 0.25f;

    [Header("Cover Score Settings")]
    [SerializeField] private float behindCoverScore = 1f;
    [SerializeField] private float notBehindCoverScore = 0f;
    [Tooltip("The weight of this score in comparison to the scores")]
    [SerializeField] private float coverWeight = 0.25f;
    [Tooltip("Any object with this layer will be considered as cover")]
    [SerializeField] protected LayerMask coverLayerMask;
    protected Vector3 coverRaycastOffset = Vector3.up; // This is used as an offset for raycasting so that raycasts don't hit ground and return true

    [Header("Proximity Score Settings")]
    [SerializeField] private float maxDistanceToPlayer = 15f;
    [Tooltip("The weight of this score in comparison to the scores")]
    [SerializeField] private float proximityWeight = 0.25f;

    [Header("Ambush Potential Score Settings")]
    [Tooltip("Ambush Potential seeks waypoints that are in front of the player and gives weight to waypoints that may " +
        "act as a position this entity can use to ambush the player. Recommended to keep this threshold anywhere from " +
        "0-0.4, as -1 is when the player is running away from the waypoint and 1 is the player running towards (0 being parallel)")]
    [SerializeField, Range(-1f, 1f)] private float apDotProductThreshold = 0.1f;
    [Tooltip("The weight of this score in comparison to the scores")]
    [SerializeField] private float ambushWeight = 0.25f;
    private PlayerMovement playerMovement; // This is here to get the Vector3 direction of the player's movement

    [Header("Low Exposure Risk Score Settings")]
    [Tooltip("Low Exposure Risk seeks waypoints that are on the same relative side of the player as this entity is. If a waypoint " +
        "is on the other side of the player, that waypoint will not have any Low Exposure Score added to its collective score")]
    [SerializeField, Range(-1f, 1f)] private float lerDotProductThreshold = 0.05f;
    [Tooltip("The weight of this score in comparison to the scores")]
    [SerializeField] private float lowExposureRiskWeight = 0.25f;

    [Header("Bonus Score Settings")]
    [Tooltip("A flat bonus that is applied to the score of the waypoint that has the shortest path to the player")]
    [SerializeField] private float shortestPathToPlayerBonus = 0.25f;
    private NavMeshPath navMeshPath; // This should only be used for calculating the shortest path to the player

    [Header("Strategic WayPoint Selection")]
    [Tooltip("The chance that the waypoint with the highest score will be selected to go to. If this chance is not met, " +
        "the next highest scoring waypoint will be selected instead (to gaurentee the highest being selected, keep at 1)")]
    [SerializeField, Range(0f, 1f)] private float highestWayPointScoreSelectionWeight = 0.8f;
    private Vector2 wayPointSelectionRange = new Vector2(0f, 1f); // ^ This should match the serialized range of the above float

    // This is used as a standard range for proximity score, ambush potential and low exposure score calculations (recommended
    // to leave as 0-1 so that each score will have similar weights)
    private Vector2 scoreRange = new Vector2(0f, 1f);

    [Header("Behavior Bonus Score Settings")]
    [Tooltip("This multiplier influences Visibility, Cover, and Low Exposure Risk weights if this entity's currentBehavior == Passive")]
    [SerializeField] private float passiveMultiplier = 2f;
    [Tooltip("This multiplier influences Proximity and Ambush Potential weights if this entity's currentBehavior == Aggressive")]
    [SerializeField] private float aggressiveMultiplier = 2f;

#if UNITY_EDITOR
    [Header("Debug")]
    [Tooltip("When this entity gets the cover score for a waypoint, a ray is created. Red means it intersected with a collider, green " +
        "means there were no intersections detected and the object is not considered to be behind cover")]
    [SerializeField] private float coverRaycastCheckTime = 3f;
#endif

    // Keeps track of waypoints and their scores (float)
    private Dictionary<WayPoint, float> strategicWayPoints = new Dictionary<WayPoint, float>();

#if UNITY_EDITOR
    // Used to visualize ranges in the editor via StalkStateEditor.cs
    public abstract float stopAtPlayerRange { get; }
    public abstract float strategicWayPointRange { get; }
#endif

    protected enum Behavior
    {
        Passive, // Passive gives extra weight to a waypoint's Visibility, Cover, and Low Exposure Risk scores
        Aggressive // Aggressive gives extra weight to a waypoint's AmbushPotential and Proximity (to player) scores
    }

    protected Behavior currentBehavior = Behavior.Passive; // Passive is starting behavior

    protected override void Awake()
    {
        base.Awake();

        navMeshPath = new NavMeshPath();
    }

    protected override void Start()
    {
        base.Start();

        // Makes sure that if coverLayerMask is accidentally not set to anything that at the very least it will have Default layer
        HelperMethods.AddLayerToLayerMask(ref coverLayerMask, "Default");
    }

    /// <summary>
    ///     Selects a strategically optimal waypoint around the player based on a combination of factors: Visibility, Cover, Proximity
    ///     (waypoint proximity to player), Ambush Potential, and Low Exposure Risk scores as well as any relevant bonuses (such as
    ///     shortest path to player bonus).
    /// </summary>
    /// <returns>The selected WayPoint with the highest composite score (with a random chance to select the 2nd-highest scoring WayPoint).</returns>
    protected WayPoint GetStrategicWayPointAroundPlayer(int wayPointsToConsiderAroundTarget = 6)
    {
        // If player is null or, for some reason, wayPointsToConsiderAroundTarget is less than 1, just get a random waypoint
        if (wayPointsToConsiderAroundTarget < 1 || player == null)
        {
#if UNITY_EDITOR
            Debug.LogError("wayPointsToConsiderAroundTarget was less than 1 || player null. Selecting a random way point.");
#endif
            return GetRandomWayPoint(LevelController.instance.wayPoints);
        }

        GetClosestWayPoints(wayPointsToConsiderAroundTarget, excludeCurrentWayPoint: true, player);
        FillStrategicWayPointsDict(closestWayPoints);

        // Uses a helper method to get a somewhat randomly chosen waypoint from strategicWayPoints dictionary
        return GetTopScoringWayPoint(strategicWayPoints, wayPointSelectionRange);
    }

    /// <summary>
    ///     Selects a strategically optimal waypoint around the player based on a combination of factors: Visibility, Cover, Proximity
    ///     (waypoint proximity to player), Ambush Potential, and Low Exposure Risk scores as well as any relevant bonuses (such as
    ///     shortest path to player bonus). Can use a list of waypoints instead of just using the closest waypoints around the player.
    /// </summary>
    /// <param name="wayPoints">The list of waypoints that should be used for calculating their scores.</param>
    /// <returns>The selected WayPoint with the highest composite score (with a random chance to select the 2nd-highest scoring WayPoint).</returns>
    protected WayPoint GetStrategicWayPointAroundPlayer(List<WayPoint> wayPoints, int wayPointsToConsiderAroundTarget = 6)
    {
        // If player is null or, for some reason, wayPointsToConsiderAroundTarget is less than 1, just get a random waypoint
        if (wayPointsToConsiderAroundTarget < 1 || player == null)
        {
#if UNITY_EDITOR
            Debug.LogError("wayPointsToConsiderAroundTarget was less than 1 || player null. Selecting a random way point.");
#endif
            return GetRandomWayPoint(LevelController.instance.wayPoints);
        }

        FillStrategicWayPointsDict(wayPoints);
        return GetTopScoringWayPoint(strategicWayPoints, wayPointSelectionRange);
    }

    /// <summary>
    ///     Fills the strategicWayPoints dictionary with waypoints from the list parameter and their calculated weight scores.
    /// </summary>
    /// <param name="wayPoints">The waypoints that should be have their scores calculated and added to the dictionary.</param>
    private void FillStrategicWayPointsDict(List<WayPoint> wayPoints)
    {
        strategicWayPoints.Clear();

        // Find the waypoint with the shortest path to the player and use it to later add a bonus to this waypoint
        WayPoint wpWithShortestPath = GetWayPointWithShortestPathToPlayer(wayPoints);

        // Calculate the weighted scores with Visibility, Cover, Proximity, and Ambush Potential and apply bonuses to any relevant waypoint
        for (int i = 0; i < wayPoints.Count; i++)
        {
            float bonus = 0f;
            WayPoint wp = wayPoints[i];

            // If this waypoint has the shortest path, apply the shortestPathBonus to it
            if (wp == wpWithShortestPath)
            {
                bonus += shortestPathToPlayerBonus;
            }

            float score = GetWeightedWayPointScore(wp, bonus);
#if UNITY_EDITOR
            wp.SetScore(score);
#endif
            // Using TryAdd() instead of Add() is safer because it checks if the waypoint is already in the dictionary
            strategicWayPoints.TryAdd(wp, score);
        }
    }

    // The higher the composite score of this waypoint that is being evaluated, the more likely the entity will move to this
    // waypoint. Scores are calculated with 5 weights: Visibility, Cover, Proximity, AmbushPotential, and Low Exposure Risk
    private float GetWeightedWayPointScore(WayPoint wayPoint, float bonuses = 0f)
    {
        // Get each of the scores and influence them by weights and passive multipliers
        // Passive Multipliers
        float visibility = GetVisibilityScore(wayPoint) * visibilityWeight * (currentBehavior == Behavior.Passive ? passiveMultiplier : 1f);
        float cover = GetCoverScore(wayPoint, out _) * coverWeight * (currentBehavior == Behavior.Passive ? passiveMultiplier : 1f);
        float lowExposureRisk = GetLowExposureRiskScore(wayPoint) * lowExposureRiskWeight * (currentBehavior == Behavior.Passive ? passiveMultiplier : 1f);

        // Aggressive Multipliers
        float proximity = GetProximityScore(wayPoint) * proximityWeight * (currentBehavior == Behavior.Aggressive ? aggressiveMultiplier : 1f);
        float ambushPotential = GetAmbushPotentialScore(wayPoint) * ambushWeight * (currentBehavior == Behavior.Aggressive ? aggressiveMultiplier : 1f);

        return visibility + cover + lowExposureRisk + proximity + ambushPotential + bonuses;
    }

    // Visibility score checks to see if the waypoint is currently within the player's camera's view frustrum and returns
    // a 0 if the waypoint is within the camera's view and a 1 if not. This to give the waypoint's score an increase if it
    // is out of the player's camera view, which promotes the entity staying hidden
    private float GetVisibilityScore(WayPoint wayPoint)
    {
        // The below checks make sure player is not null and playerCamera are not null. If they are, just return visibleScore
        if (IsPlayerOrPlayerCameraNull())
        {
            return visibleScore;
        }

        return HelperMethods.IsVisible(playerCamera, wayPoint.gameObject) ? visibleScore : notVisibleScore;
    }

    // Cover score checks to see if the waypoint is behind another object from the player. If there is a direct path from this waypoint
    // to the player, the waypoint is not considered to be behind cover. This method also puts out a bool: BehindCover. This can
    // be saved in order to not have to calculate whether or not the waypoint is behind cover in a different method on the same frame
    private float GetCoverScore(WayPoint wayPoint, out bool behindCover)
    {
        if (IsPlayerOrPlayerCameraNull())
        {
            behindCover = false;
            return notBehindCoverScore;
        }

        if (BehindCover(wayPoint.gameObject, coverRaycastOffset, coverLayerMask))
        {
            behindCover = true;
            return behindCoverScore;
        }
        else
        {
            behindCover = false;
            return notBehindCoverScore;
        }
    }

    // Proximity score returns a score based upon how close this waypoint is to the player. The closer the waypoint is,
    // the higher the returned score will be (score is remapped to fit within proximityScoreRange)
    private float GetProximityScore(WayPoint wayPoint)
    {
        if (IsPlayerOrPlayerCameraNull())
        {
            return scoreRange.x;
        }

        float distance = Vector3.Distance(wayPoint.gameObject.transform.position, player.transform.position);
        return distance < maxDistanceToPlayer ? HelperMethods.Remap(distance, maxDistanceToPlayer, 0, scoreRange.x, scoreRange.y) : 0f;
    }

    // Ambush Potential score is calculated by getting the dot product of the direction to the target waypoint and the player's
    // movement direction and camera direction. If the player is heading directly towards the waypoint, the dot product is 1, 
    // whilst if the player is running directly away from the waypoint, the dot product is -1. The player's view direction is also
    // taken into account to prevent the issue of this method returning 0 if the player was standing still (but still looking in
    // the target waypoint's general direction)
    private float GetAmbushPotentialScore(WayPoint wayPoint)
    {
        if (IsPlayerOrPlayerCameraNull())
        {
            return scoreRange.x;
        }

        if (playerMovement == null)
        {
            if (player.TryGetComponent(out PlayerMovement pM))
            {
                playerMovement = pM;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{name} could not find PlayerMovement.cs on Player.");
#endif
                return scoreRange.x;
            }
        }

        // directionalIntent makes sure that the player's look direction and movement direction are factored into the dot product calculations
        Vector3 playerDirectionalIntent = Vector3.Normalize(playerMovement.moveDir + playerCamera.transform.forward);
        Vector3 playerToWayPointDirection = Vector3.Normalize(wayPoint.gameObject.transform.position - player.transform.position);
        float dot = Vector3.Dot(playerDirectionalIntent, playerToWayPointDirection);

        // **Potential: May want to change Remap's fromMin from -1f to apDotProductThreshold for smoother scoring, but leaving it as is
        // may be beneficial to give a sharper jump in score increase to waypoint's that meet ambush potential score criteria
        return dot >= apDotProductThreshold ? HelperMethods.Remap(dot, -1f, 1f, scoreRange.x, scoreRange.y) : scoreRange.x;
    }

    // Low Exposure Risk is calculated using the dot product of the player's direction to the entity and the player's direction to
    // the targeted waypoint. It essentially creates a line that is perpendicular to the player's direction to the entity, and any
    // waypoint that is on the same side of that line as the entity has gets a low exposure risk whilst any waypoiny that is on the
    // opposite side will not get any score, since that waypoint is considered to have a higher risk exposure (because if the entity
    // has to cross the player's position to reach a waypoint, moving to that waypoint increases the chance of being seen -- see below)
    //
    // L.E.W: Low Exposure WayPoint, H.E.W. High Exposure WayPoint
    //                 \     x <- L.E.W.
    //                  \                @ <- entity position
    //     x <- H.E.W    \
    //                    * <- player position
    //                     \
    //                      \       x <- L.E.W
    //       x <- H.E.W      \
    //
    private float GetLowExposureRiskScore(WayPoint wayPoint)
    {
        if (IsPlayerOrPlayerCameraNull())
        {
            return scoreRange.x;
        }

        Vector3 playerToEntityDirection = Vector3.Normalize(transform.position - player.transform.position);
        Vector3 playerToWayPointDirection = Vector3.Normalize(wayPoint.gameObject.transform.position - player.transform.position);
        float dot = Vector3.Dot(playerToEntityDirection, playerToWayPointDirection);

        return dot >= lerDotProductThreshold ? HelperMethods.Remap(dot, lerDotProductThreshold, 1f, scoreRange.x, scoreRange.y) : scoreRange.x;
    }

    // Accepts a list of waypoints and loops through them, calculating the overall distance of a particular waypoint
    // and returns the waypoint that has the shortest overall path to the player. For example, if there are 2 waypoints
    // extremely close to the player in proximity but one of them is behind cover and would take a lot longer to get around
    // objects to reach the player, this method can give bonus score to the waypoint with the shortest path to the player
    private WayPoint GetWayPointWithShortestPathToPlayer(IReadOnlyList<WayPoint> wayPoints)
    {
        if (IsPlayerOrPlayerCameraNull())
        {
            return null;
        }

        WayPoint wayPointWithShortestPath = null;
        float currentShortestDistance = Mathf.Infinity;
        float distanceFromWPToPlayer;

        for (int i = 0; i < wayPoints.Count; i++)
        {
            if (wayPoints[i] == null)
            {
                continue;
            }

            // Check to see if a path from this waypoint to the player is possible
            if (NavMesh.CalculatePath(wayPoints[i].gameObject.transform.position, player.transform.position, entity.GetNavMeshFilter(), navMeshPath))
            {
                // Check to see if the generated path is a complete path and not partial or invalid. If not complete, skip this waypoint
                if (navMeshPath.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                // If a path is found, calculate the overall distance using the corners of the path. Note: since navMeshPath.corners[0]
                // is the starting point, the distance is always 0 (or near 0), so instead of calculating the distance from
                // navMeshPath.corners[0] I just set distanceFromWPToPlayer to 0
                distanceFromWPToPlayer = 0f;

                for (int j = 1; j < navMeshPath.corners.Length; j++)
                {
                    distanceFromWPToPlayer += Vector3.Distance(navMeshPath.corners[j - 1], navMeshPath.corners[j]);
                }

                if (distanceFromWPToPlayer < currentShortestDistance)
                {
                    currentShortestDistance = distanceFromWPToPlayer;
                    wayPointWithShortestPath = wayPoints[i];
                }
            }
        }

        return wayPointWithShortestPath;
    }

    // I did not use any Linq sorting methods in this case to avoid garbage collection -- even though this method won't be called all
    // that often, I would still like to avoid GC as much as possible wherever possible
    private WayPoint GetTopScoringWayPoint(IReadOnlyDictionary<WayPoint, float> wayPoints, Vector2 range)
    {
        // Safety checks because why not
        if (wayPoints.Count < 1)
        {
            return null;
        }

        if (wayPoints.Count == 1)
        {
            return wayPoints.First().Key;
        }

        WayPoint first = null;
        WayPoint second = null;
        float firstScore = float.MinValue;
        float secondScore = float.MinValue;

        // Sorts through the dictionary and keeps track of the waypoints with the highest and second highest scores
        foreach (var kvp in wayPoints)
        {
            if (kvp.Value > firstScore)
            {
                second = first;
                secondScore = firstScore;
                first = kvp.Key;
                firstScore = kvp.Value;
            }
            else if (kvp.Value > secondScore)
            {
                second = kvp.Key;
                secondScore = kvp.Value;
            }
        }

        // use highestWayPointScoreSelectionWeight to randomly (weighted) get the waypoint with the highest or 2nd highest score
        float random = Random.Range(range.x, range.y);
        return random < highestWayPointScoreSelectionWeight ? first : second;
    }

    /// <summary>
    ///     Check whether or not the player or player's camera reference is null.
    /// </summary>
    /// <returns>True if either the player or player's camera reference is null. If neither are null, return false.</returns>
    private bool IsPlayerOrPlayerCameraNull()
    {
        if (player == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}'s player reference null. Attempting to get player reference.");
#endif
            player = GetPlayerReference();

            // After getting the reference, check again
            if (player == null)
            {
                return true;
            }
        }

        if (playerCamera == null)
        {
            playerCamera = player.GetComponent<PlayerReferences>().playerCamera;

            if (playerCamera == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{name}'s playerCamera reference null and was not able to get PlayerReferences component from Player.");
#endif
                return true;
            }

        }

        return false;
    }
}
