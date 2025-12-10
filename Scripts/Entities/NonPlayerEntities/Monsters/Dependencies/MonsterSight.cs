using System;
using UnityEngine;

public class MonsterSight : MonoBehaviour
{
    public event Action OnPlayerSeen;

    public bool isPlayerCurrentlySeen { get; private set; } = false;

    [Tooltip("The monster will be able to see and chase the player if the player object is set to the same layer is this layer mask")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Colliders that have these layers will block this monster's sight to the player")]
    [SerializeField] private LayerMask obstacleLayers;

    [SerializeField] private float radius = 6f;
    [SerializeField, Range(0, 360)] private float angle = 90f;
#if UNITY_EDITOR
    // Used for visualizing monster sight in the editor
    public float fovRadius => radius;
    public float fovAngle => angle;
#endif

    [Tooltip("Mainly used for the y-value: if the y-value is 0, the Monster is unable to see the player due to sight being too low")]
    [SerializeField] private Vector3 visionOffset = new Vector3(0, 1f, 0);

    private void Start()
    {
        // Makes sure if the Player layer is not added to playerLayer in the inspector, add it here
        HelperMethods.AddLayerToLayerMask(ref playerLayer, "Player");

        // Makes sure that at the very least, obstacleLayers has Default layer
        HelperMethods.AddLayerToLayerMask(ref obstacleLayers, "Default");
    }

    private void FixedUpdate()
    {
        HandleMonsterSight();
    }

    // *NOTE* due to vision offset, there is some unexpected behavior regarding sight. If the player is crouched below a collider,
    // monster sight technically will make isPlayerCurrentlySeen false even if there is clear line of sight between the monster and
    // player. The reason for this is due to vision offset, but I am leaving this for now because I think this behavior will work
    // for what this project needs (if the player is hiding (crouching) under a collider, the monster should stop chasing)
    private void HandleMonsterSight()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position + visionOffset, radius, playerLayer);

        if (rangeChecks.Length > 0)
        {
            Transform target = null;

            // Since it is likely other colliders will have the Player layer (such as radar hitbox, flashlight collider, etc.), loop through
            // the colliders found from OverlapSphere and get the collider game object that has the Player tag
            for (int i = 0; i < rangeChecks.Length; i++)
            {
                if (rangeChecks[i].gameObject.tag == "Player")
                {
                    target = rangeChecks[i].transform;
                    break;
                }
            }

            if (target == null)
            {
                return;
            }

            Vector3 directionToTarget = (target.position - (transform.position + visionOffset)).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (distanceToTarget <= radius && !Physics.Raycast(transform.position + visionOffset, directionToTarget, distanceToTarget, obstacleLayers))
                {
#if UNITY_EDITOR
                    Debug.DrawRay(transform.position + visionOffset, directionToTarget * distanceToTarget, Color.green);
#endif
                    if (!isPlayerCurrentlySeen)
                    {
                        OnPlayerSeen?.Invoke();
                    }

                    isPlayerCurrentlySeen = true;
                }
                else
                {
                    isPlayerCurrentlySeen = false;
                }
            }
            else
            {
                isPlayerCurrentlySeen = false;
            }
        }
        else
        {
            isPlayerCurrentlySeen = false;
        }
    }

    /// <summary>
    ///     Resets isPlayerCurrentlySeen to false. This can be used to make sure that if a monster exits chase state and the player does
    ///     not happen to leave the monster's sight, the even OnPlayerSeen will not fire off unless the player exits the monster's sight
    ///     and re-enters, in which the event will be fired off again.
    /// </summary>
    public void ResetIsPlayerCurrentlySeen()
    {
        isPlayerCurrentlySeen = false;
    }

#if UNITY_EDITOR
    // Used for Editor script: MonsterFieldOfViewEditor
    public Vector3 GetPositionPlusOffset()
    {
        return transform.position + visionOffset;
    }
#endif
}
