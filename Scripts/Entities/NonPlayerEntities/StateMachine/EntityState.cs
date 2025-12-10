using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class EntityState : MonoBehaviour
{
    // This is for specific entities' scripts to know whether or not this specific state was able to get a reference
    // to the player. If no reference was found, the entity will most likely just want to switch states (this is mainly
    // here for testing purposes, I don't want entities to do nothing while I test states if they get stuck trying to get
    // a player reference)
    public static event Action OnPlayerReferenceNotFound;

    protected GameObject player;
    protected Camera playerCamera;

    // The maximum time a state will attempt to get a player reference before entity swaps to another state
    private float maxTimeToGetPlayerReference = 5f;
    private float timeSpentGettingPlayerReference;

    protected Entity entity;
    protected EntityStateMachine stateMachine;

    private NavMeshPath navMeshPath;
    private NavMeshTriangulation navMeshData;

    private bool isMovingToPosition;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();

        if (entity != null)
        {
            stateMachine = entity.stateMachine;
            navMeshPath = new NavMeshPath();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"{gameObject.name} could not find Entity component in parent. Entity and stateMachine are null.");
#endif
        }
    }

    protected virtual void Start()
    {
        navMeshData = NavMesh.CalculateTriangulation();
    }

    /// <summary>
    ///     Called from EntityStateMachine.cs on initialization and everytime this entity enters this specific state after leaving a different state.
    /// </summary>
    public virtual void EnterState()
    {
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name} EnterState()");
#endif
        timeSpentGettingPlayerReference = 0f;

        if (player == null)
        {
            player = GetPlayerReference();
        }
    }

    /// <summary>
    ///     Called from EntityStateMachine.cs everytime this entity exits this specific state right before changing to a new state.
    /// </summary>
    public virtual void ExitState()
    {
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name} ExitState()");
#endif
    }

    /// <summary>
    ///     Called every frame update in Entity.cs's Update().
    /// </summary>
    public virtual void FrameUpdate() { CheckIfEntityIsDoneMoving(); }

    /// <summary>
    ///     Called every physics frame update in Entity.cs's FixedUpdate().
    /// </summary>
    public virtual void PhysicsUpdate() { }

    /// <summary>
    ///     Can be used by entities to trigger animation events.
    /// </summary>
    /// <param name="triggerType">The type of event to trigger.</param>
    public virtual void AnimationTriggerEvent(Entity.AnimationTriggerType triggerType) { }

    /// <summary>
    ///     Move to a position based on a Vector3 position.
    /// </summary>
    protected void MoveToPosition(Vector3 pos)
    {
        isMovingToPosition = true;
        entity.SetDestination(pos);
    }

    /// <summary>
    ///     Move to a position based on a transform.
    /// </summary>
    protected void MoveToTransform(Transform trans)
    {
        isMovingToPosition = true;
        entity.SetDestination(trans);
    }

    private void CheckIfEntityIsDoneMoving()
    {
        if (isMovingToPosition && entity.AtEndOfPath())
        {
            isMovingToPosition = false;
        }
    }

    /// <summary>
    ///     Smoothly look at a transform.
    /// </summary>
    /// <param name="target">The target transform this entity should look at.</param>
    /// <param name="pivot">The transform that will be rotated (should be the entity root).</param>
    /// <param name="rotationSpeed">The speed at which this entity will rotate and track the target.</param>
    /// <param name="lockXAxis">Prevent's the x-axis rotation from moving.</param>
    /// <param name="lockYAxis">Prevent's the y-axis rotation from moving.</param>
    /// <param name="lockZAxis">Prevent's the z-axis rotation from moving.</param>
    protected void LookAtTarget(Transform target, Transform pivot, float rotationSpeed, bool lockXAxis = true, bool lockYAxis = false, bool lockZAxis = true)
    {
        Quaternion rotation = Quaternion.LookRotation(target.position - pivot.position);

        rotation.x = lockXAxis ? pivot.rotation.x : rotation.x;
        rotation.y = lockYAxis ? pivot.rotation.y : rotation.y;
        rotation.z = lockZAxis ? pivot.rotation.z : rotation.z;

        pivot.rotation = Quaternion.Slerp(pivot.rotation, rotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    ///     Finds a random position on the navmesh. Also makes sure the random position leads to a valid path for this entity.
    /// </summary>
    /// <param name="minDistance">The minimum distance the random position should be from this entity.</param>
    /// <returns>A Vector3 position that is located somewhere on the navmesh.</returns>
    protected Vector3 FindNewRandomPositionOnNavMesh(float minDistance = 0f)
    {
        if (navMeshData.indices == null)
        {
#if UNITY_EDITOR
            Debug.LogError("navMeshData.indices null. Returning Vector3.zero for now.");
#endif
            return Vector3.zero;
        }

        // Choose a random triangle -- since triangles are made up of 3 indices (triangle 1 = indices[0], indices[1], indices[2]),
        // dividing by 3, choosing a random int, then multiplying by 3 ensures that a valid stating index of a triangle is selected
        int triangleIndex = UnityEngine.Random.Range(0, navMeshData.indices.Length / 3) * 3;

        // Get the randomly selected triangle's vertices
        Vector3 v0 = navMeshData.vertices[navMeshData.indices[triangleIndex]];
        Vector3 v1 = navMeshData.vertices[navMeshData.indices[triangleIndex + 1]];
        Vector3 v2 = navMeshData.vertices[navMeshData.indices[triangleIndex + 2]];

        Vector3 randomPoint = GetRandomPointInTriangle(v0, v1, v2);

        // If the path is not valid or the minimum distance requirement is not met, get a new point
        if (!IsPathValid(randomPoint) || (minDistance > 0.1f && Vector3.Distance(transform.position, randomPoint) < minDistance))
        {
            randomPoint = FindNewRandomPositionOnNavMesh(minDistance);
        }

#if UNITY_EDITOR
        // Draws a blue ray straight upwards for easier identification of the selected points
        Debug.DrawRay(randomPoint, Vector3.up * 3f, Color.blue, 3);
#endif

        return randomPoint;
    }

    /// <summary>
    ///     Calculates a random position within a triangle using Barycentric coordinates.
    /// </summary>
    /// <returns>A Vector3 position that is within the specified triangle's vertices.</returns>
    private Vector3 GetRandomPointInTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        // Use Barycentric coordinates to get any point in a triangle: P = v0 + r1(v1 - v0) + r2(v2 - v0)
        float r1 = UnityEngine.Random.value;
        float r2 = UnityEngine.Random.value;

        // Make sure the point lies inside the triangle by ensuring r1 >= 0, r2 >= 0, r1 + r2 <= 1
        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        return v0 + r1 * (v1 - v0) + r2 * (v2 - v0);
    }

    /// <summary>
    ///     Finds a random position on the navmesh within a specified range to either this entity or a different game object: centerOfRange.
    /// </summary>
    /// <param name="range">The range around this entity or centerOfRange to use.</param>
    /// <param name="takeOtherObjectsIntoAccount">If true, a raycast is used to make sure the position is does not have any colliders in its way.</param>
    /// <param name="centerOfRange">Can be used to override the range's center. By default, this is null so that this entity is center of the range.</param>
    /// <returns>A Vector3 position on the navmesh that is randomly selected within the specified range.</returns>
    protected Vector3 FindNewRandomPositionInRange(float range, bool takeOtherObjectsIntoAccount = false, GameObject centerOfRange = null)
    {
        if (FindRandomPointOnNavMeshWithinRange(range, out Vector3 targetPosition, takeOtherObjectsIntoAccount, centerOfRange))
        {
#if UNITY_EDITOR
            Debug.DrawRay(targetPosition, Vector3.up, Color.black, 3);
#endif
            return targetPosition;
        }

        return Vector3.zero;
    }

    /// <summary>
    ///     Attempts to find a random point on the navmesh within a specified range. In addition to returning true or false, this also uses out
    ///     Vector3 targetPosition to return the random position that was found to be valid based on the parameters.
    /// </summary>
    /// <returns>True if a random point was found, false if not (and out targetPosition is set to Vector3.zero).</returns>
    private bool FindRandomPointOnNavMeshWithinRange(float range, out Vector3 targetPosition, bool checkForOtherObjects, GameObject centerOfRange)
    {
        int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Get a random point in a sphere's range, either from this entity's position or from centerOfRange if that is not null
            Vector3 randomPoint = GetRandomPointInSphere(centerOfRange != null ? centerOfRange.transform.position : transform.position, range);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, entity.GetNavMeshFilter()) && IsPathValid(hit.position))
            {
                // If checking for other objects is necessary, perform a raycast for collision detection
                if (checkForOtherObjects)
                {
                    Vector3 direction = hit.position - transform.position;

                    if (!Physics.Raycast(transform.position, direction.normalized, direction.magnitude, LayerMask.GetMask("Default")))
                    {
                        targetPosition = hit.position;
                        return true;
                    }
                }

                // I'm pretty sure this is not working the way I intended, I saw this like this and noticed I should use an else statement,
                // but ran into the problem of entities pretty much always resorting to Vector3.zero so I'm removing the else statement and
                // leaving this as is. I'm leaving this comment for possible future use and would advise coming up with a better method but for
                // I'm leaving this because I'm lazy and this works well for the most part
                targetPosition = hit.position;
                return true;
            }
        }

        // If maxAttempts was reached, just set targetPosition to be zero and return false
        targetPosition = Vector3.zero;
        return false;
    }

    /// <summary>
    ///     Checks if a path is valid by using NavMesh.CalculatePath() for this entity to move from its current position to a specified position.
    /// </summary>
    /// <param name="toThisPosition">The desired position to move this entity to.</param>
    /// <returns>True if the path is valid, false if the path is not valid.</returns>
    protected bool IsPathValid(Vector3 toThisPosition)
    {
        return NavMesh.CalculatePath(transform.position, toThisPosition, entity.GetNavMeshFilter(), navMeshPath);
    }

    private Vector3 GetRandomPointInSphere(Vector3 centerPosition, float range)
    {
        return centerPosition + UnityEngine.Random.insideUnitSphere * range;
    }

    /// <summary>
    ///     Checks if this entity is currently in the process of moving to a destination.
    /// </summary>
    /// <returns>True if this entity is currently moving to destination, false if not.</returns>
    protected bool IsEntityMovingToPos()
    {
        return isMovingToPosition;
    }

    /// <summary>
    ///     Creates a raycast from the player's camera position to a target's (most likely a waypoint in this case) position. If the
    ///     raycast intersects any object between the player's camera and target position, the target is considered to be behind cover.
    /// </summary>
    /// <param name="target">The target game object.</param>
    /// <param name="offset">Offsets the raycast from the target.transform.position.</param>
    /// <param name="layerMask">Any collider that has this mask will be able to be hit and checked by the raycast.</param>
    /// <returns>True if the raycast intersects an collider in the layer mask, false if not.</returns>
    protected bool BehindCover(GameObject target, Vector3 offset, LayerMask layerMask, float coverRaycastCheckTime = 3f)
    {
        if (playerCamera == null)
        {
            if (player != null && player.TryGetComponent(out PlayerReferences playerReferences))
            {
                playerCamera = playerReferences.playerCamera;
            }
            else
            {
                return false;
            }
        }

        // Optional offset is used to make sure that if the target is waypoint, the raycast does not hit the ground and return true
        Vector3 direction = Vector3.Normalize(target.transform.position + offset - playerCamera.transform.position);
        float distance = Vector3.Distance(target.transform.position + offset, playerCamera.transform.position);

        if (Physics.Raycast(playerCamera.transform.position, direction, out _, distance, layerMask))
        {
#if UNITY_EDITOR
            Debug.DrawRay(playerCamera.transform.position, direction * distance, Color.red, coverRaycastCheckTime);
#endif
            return true;
        }
        else
        {
#if UNITY_EDITOR
            Debug.DrawRay(playerCamera.transform.position, direction * distance, Color.green, coverRaycastCheckTime);
#endif
            return false;
        }
    }

    /// <summary>
    ///     Check if an empty game object is within the Camera's frustrum.
    /// </summary>
    /// <param name="emptyGameObject">The target empty game object.</param>
    /// <param name="planes">The player camera's frustrum planes.</param>
    /// <returns>True if the empty game object is within the camera's view, false if otherwise.</returns>
    protected bool WithinCameraFrustrum(GameObject emptyGameObject, Plane[] planes)
    {
        return GeometryUtility.TestPlanesAABB(planes, new Bounds(emptyGameObject.transform.position, Vector3.one));
    }

    /// <summary>
    ///     Check if an empty game object is within the Camera's frustrum (calculates the planes each time this is called).
    /// </summary>
    /// <param name="emptyGameObject">The target empty game object.</param>
    /// <returns>True if the empty game object is within the camera's view, false if otherwise.</returns>
    protected bool WithinCameraFrustrum(GameObject emptyGameObject)
    {
        if (playerCamera == null)
        {
            if (player != null && player.TryGetComponent(out PlayerReferences playerReferences))
            {
                playerCamera = playerReferences.playerCamera;
            }
            else
            {
                return false;
            }
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        return GeometryUtility.TestPlanesAABB(planes, new Bounds(emptyGameObject.transform.position, Vector3.one));
    }

    /// <summary>
    ///     Attempt to get a reference to the player by using GameObject.FindWithTag("Player").
    /// </summary>
    /// <returns>If found, returns the game object that has the "Player" tag. Otherwise, returns null.</returns>
    protected GameObject GetPlayerReference()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            playerCamera = playerObj.GetComponent<PlayerReferences>().playerCamera;
            return playerObj;
        }

        timeSpentGettingPlayerReference += Time.deltaTime;

        if (timeSpentGettingPlayerReference > maxTimeToGetPlayerReference)
        {
            OnPlayerReferenceNotFound?.Invoke();
        }

        return null;
    }
}
