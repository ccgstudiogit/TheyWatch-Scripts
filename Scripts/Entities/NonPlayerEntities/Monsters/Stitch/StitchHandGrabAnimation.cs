using System;
using UnityEngine;

/// <summary>
///     This script is used in the Backrooms End scene to reach Stitch's hand out to the player and grab the player.
/// </summary>
public class StitchHandGrabAnimation : MonoBehaviour
{
    // Lets BackroomsEndLevelController know when Stitch's hand reaches the player's camera
    public event Action OnStitchGrabbedPlayer;

    [Header("Animator Reference")]
    [Tooltip("If the animator is referenced, the animator will be turned off when the grab animation starts")]
    [SerializeField] private Animator animator;

    // The arm bone rotates to a starting position, then the palm bone is the bone that will actually move towards the player
    [Header("Bone References")]
    [SerializeField] private Transform armBone;
    [SerializeField] private Transform palmBone;

    [Header("Arm Rotation")]
    [Tooltip("The speed at which the arm bone rotates from its starting rotation to the target rotation")]
    [SerializeField] private float rotationSpeedMultiplier = 2f;
    [SerializeField] private Quaternion targetRotation;
    private Quaternion startingRotation;
    private float slerp;
    private bool rotating;

    [Header("Hand Grab")]
    [Tooltip("The speed at which Stitch's palm bone will move towards the player")]
    [SerializeField] private float moveSpeed = 40f;
    [Tooltip("An offset applied to the hand while it's moving")]
    [SerializeField] private Vector3 offset = new Vector3(0, -0.075f, 0); // This helps to make sure the hand closer aligns to the camera
    [Tooltip("Once Stitch's hand gets this close to the player's camera, the event OnStitchGrabbedCamera is fired off")]
    [SerializeField] private float minDistanceToPlayerCam = 0.5f;
    private Vector3 direction; // Keeps track of the direction towards the player's camera

    private GameObject player;
    private Transform cameraTransform;

    private bool started;
    private bool grabbed;

    private void Awake()
    {
        if (armBone == null || palmBone == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s armBone || palmBone null! Disabling this script.");
#endif
            enabled = false;
            return;
        }

        started = false;
    }

    private void LateUpdate()
    {
        // For this animation to start, StartGrabAnimation() must be called by another script
        if (!started)
        {
            return;
        }

        if (rotating)
        {
            armBone.localRotation = Quaternion.Slerp(startingRotation, targetRotation, slerp * rotationSpeedMultiplier);
            slerp += Time.deltaTime;

            if (slerp >= 1)
            {
                rotating = false;
            }
        }
        // Wait until the arm bone is done rotating to position before starting to move the palm bone
        else if (!grabbed)
        {
            if (cameraTransform == null)
            {
                if (player == null)
                {
                    player = LevelController.instance.GetPlayer();
                }

                if (player != null && player.TryGetComponent(out PlayerReferences playerReferences))
                {
                    cameraTransform = playerReferences.playerCamera.transform;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"{gameObject.name} couldn't find the player camera!");
#endif
                    return;
                }
            }

            // Move the palm bone towards the player's camera
            direction = (cameraTransform.position - palmBone.position).normalized;
            palmBone.position += direction * Time.deltaTime * moveSpeed;
            palmBone.position += offset;

            if (Vector3.Distance(palmBone.position, cameraTransform.position) < minDistanceToPlayerCam)
            {
                OnStitchGrabbedPlayer?.Invoke();
                grabbed = true;
            }
        }
    }

    /// <summary>
    ///     Starts Stitch's hand grab animation.
    /// </summary>
    public void StartGrabAnimation()
    {
        // Make sure that if the script is disabled, the below code doesn't run so that renaming the bones does not interfere
        // with animations. Also make sure the hand grab animation is not started again after it has already been started
        if (!enabled || started)
        {
            return;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        startingRotation = armBone.localRotation;
        slerp = 0f;
        rotating = true;

        player = LevelController.instance.GetPlayer();

        if (player != null && player.TryGetComponent(out PlayerReferences playerReferences))
        {
            cameraTransform = playerReferences.playerCamera.transform;
        }

        grabbed = false;
        started = true;
    }
}
