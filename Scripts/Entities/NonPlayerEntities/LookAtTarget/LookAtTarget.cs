using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [Header("Necessary References")]
    [Tooltip("This bone from an armature is used to track a target (Any other game object can be used instead as well)")]
    [SerializeField] private Transform targetBone;
    [Tooltip("This should be an empty game object that has the same parent as the targetBone but the same position " +
        "and rotation as the targetBone (start off by making the game object a child of the targetBone, then move the " +
        "empty game object to be a child of the targetBone's parent object)")]
    [SerializeField] private Transform forwardDirection;

    [Header("Track Target Settings")]
    [SerializeField] private float lookSpeed = 7f;
    [SerializeField] private float minAngle = -75f;
    [SerializeField] private float maxAngle = 75f;

    [Header("Object Reference")]
    [Tooltip("If enabled, objectToBeLookedAt will be overridden and the entity will look at the player")]
    [SerializeField] private bool trackPlayer = true;
    [SerializeField] private GameObject trackedObj = null;

    // Helpers
    private bool isLooking;
    private bool keepLooking = true; // Can be used to force the entity to not look even if the angle is within the min/max
    private Quaternion lastRotation;
    private float boneResetTimer;

    private GameObject player = null;

    private void Start()
    {
        if (targetBone == null || forwardDirection == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}'s targetBone || forwardDirection reference null. Disabling LookAtTarget.cs");
#endif
            enabled = false;
            return;
        }

        isLooking = false;
    }

    // LateUpdate() is used instead of Update() so that the entity's animator does not override the new rotation
    private void LateUpdate()
    {
        TrackObject(trackPlayer ? player != null ? player : LevelController.instance.GetPlayer() : trackedObj);
    }

    /// <summary>
    ///     Handles tracking a transform by smoothly rotating the targetBone's rotation towards the target object's transform.
    ///     This method also handles smoothly looking away (back to the targetBone's normal rotation) when the target object's
    ///     transform moves beyond the min/max angle.
    /// </summary>
    /// <param name="obj">The game object that should be looked at.</param>
    private void TrackObject(GameObject obj)
    {
        if (obj == null)
        {
#if UNITY_EDITOR
            Debug.Log($"{gameObject.name}'s {name} obj in TrackObject() null.");
#endif
            return;
        }
        
        // Makes sure that the target bone's rotation does not exceed the min/max angles
        Vector3 direction = (obj.transform.position - targetBone.position).normalized;
        float angle = Vector3.SignedAngle(direction, forwardDirection.forward, forwardDirection.up);

        if (angle > minAngle && angle < maxAngle && keepLooking)
        {
            if (!isLooking)
            {
                isLooking = true;
                lastRotation = targetBone.rotation;
            }

            // Smooth the look rotation
            Quaternion targetRotation = Quaternion.LookRotation(obj.transform.position - targetBone.position);
            lastRotation = Quaternion.Slerp(lastRotation, targetRotation, lookSpeed * Time.deltaTime);

            targetBone.rotation = lastRotation;

            boneResetTimer = 0.5f;
        }
        // Rotate the target bone back to its normal position if the angle is not within the min/max
        else if (isLooking)
        {
            lastRotation = Quaternion.Slerp(lastRotation, forwardDirection.rotation, lookSpeed * Time.deltaTime);
            targetBone.rotation = lastRotation;

            boneResetTimer -= Time.deltaTime;

            if (boneResetTimer <= 0)
            {
                targetBone.rotation = forwardDirection.rotation;
                isLooking = false;
            }
        }
    }

    /// <summary>
    ///     Set LookAtTarget's trackPlayer. If enabled, LookAtTarget will automatically just track the player. Otherwise,
    ///     LookAtTarget will track objbectToBeLookedAt.
    /// </summary>
    /// <param name="shouldTrackPlayer">Whether or not LookAtTarget should track the player.</param>
    public void SetTrackPlayer(bool shouldTrackPlayer)
    {
        trackPlayer = shouldTrackPlayer;
    }

    /// <summary>
    ///     Check if LookAtTarget is currently tracking player (trackPlayer set to true).
    /// </summary>
    /// <returns>trackPlayer's value.</returns>
    public bool IsTrackingPlayer()
    {
        return trackPlayer;
    }

    /// <summary>
    ///     Set LookAtTarget's target object.
    /// </summary>
    /// <param name="trans">The target game object.</param>
    public void SetObjectToBeTracked(GameObject obj)
    {
        trackedObj = obj;
    }

    /// <summary>
    ///     Enable or disable looking at the target.
    /// </summary>
    /// <param name="look">Whether or not this entity should be looking at the target.</param>
    public void SetLooking(bool look)
    {
        keepLooking = look;
    }

    /// <summary>
    ///     Check whether or not this entity has looking enabled.
    /// </summary>
    /// <returns>True if the entity can track a target, false if otherwise.</returns>
    public bool CanLook()
    {
        return keepLooking;
    }
}
