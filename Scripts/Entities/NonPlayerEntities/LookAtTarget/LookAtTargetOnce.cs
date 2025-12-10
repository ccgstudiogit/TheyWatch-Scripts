using UnityEngine;

/// <summary>
///     Similar to LookAtTarget.cs, but this script looks at a target initially and then stays looking in that direction until
///     Stop() is called. Usefull for overriding animators and making sure an entity is looking at one spot until told not to.
/// </summary>
public class LookAtTargetOnce : MonoBehaviour
{
    [Header("Necessary References")]
    [Tooltip("This bone from an armature is used to track a target (Any other game object can be used instead as well)")]
    [SerializeField] private Transform targetBone;
    [Tooltip("This should be an empty game object that has the same parent as the targetBone but the same position " +
        "and rotation as the targetBone (start off by making the game object a child of the targetBone, then move the " +
        "empty game object to be a child of the targetBone's parent object)")]
    [SerializeField] private Transform forwardDirection;

    [Header("Look At Target Settings")]
    [SerializeField] private float minAngle = -75f;
    [SerializeField] private float maxAngle = 75f;

    // Keep looking at particular spot
    private bool looking;
    private Quaternion targetRotation;

    private void LateUpdate()
    {
        if (looking)
        {
            targetBone.rotation = targetRotation;
        }
    }

    /// <summary>
    ///     Look at a target's direction. Does not track, simply looks at that direction and stays looking until Stop() is called.
    ///     Note: if the angle exceeds what is set in the inspector, nothing will happen.
    /// </summary>
    /// <param name="target">The target's transform to look at.</param>
    /// <param name="offset">Optional offset that can be used to add to the target's position.</param>
    public void SetTargetAndLook(Transform target, Vector3 offset = new Vector3())
    {
        // Makes sure that the target bone's rotation does not exceed the min/max angles
        Vector3 direction = (target.position + offset - targetBone.position).normalized;
        float angle = Vector3.SignedAngle(direction, forwardDirection.forward, forwardDirection.up);

        if (angle > minAngle && angle < maxAngle)
        {
            targetRotation = Quaternion.LookRotation(target.position + offset - targetBone.position);
            looking = true;
        }
    }

    /// <summary>
    ///     Stop looking at the direction of the target.
    /// </summary>
    public void Stop()
    {
        looking = false;
    }
}
