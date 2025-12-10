using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UniversalPlayerInput), typeof(PlayerReferences), typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float _walkSpeed = 3f;
    public float walkSpeed => _walkSpeed;

    [SerializeField] private float _sprintSpeed = 5f;
    public float sprintSpeed => _sprintSpeed;

    [SerializeField] private float _crouchSpeed = 2f;
    public float crouchSpeed => _crouchSpeed;

    // This is here to make sure that if the player is in crouch animation and sprints, sprint will start automatically once player
    // is no longer in crouch animation (felt very weird to play when crouching and then wanting to sprint immediately after as I
    // would let go of crouch input and press sprint input but player would not start sprinting)
    private Coroutine queueSprintRoutine = null;
    private bool CanSprint => !isCrouching;

    // ***NOTE***
    // I was going to make crouch it's own separate script, but since I plan to make crouch available at all times (and since
    // it might feel weird to the player to be able to crouch in one level and not in another), I decided I would lump crouch into
    // PlayerMovement since it also heavily relies on sprint (I don't want the player to crouch if sprinting nor sprint if crouching).
    // Also, since PlayerHeadbob and PlayerFootstepAudio need crouch information, I also believe it would be easier to keep crouch
    // in PlayerMovement as another type of universal input that other scripts can easily access.
    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 1.25f;
    private float standingHeight;

    [SerializeField] private float timeToCrouch = 0.2f;
    private float crouchAnimationTimeElapsed;

    [Tooltip("Recommended to keep at an extremely small value (<= 0.002). This float prevents jitterness caused by " +
        "the collider digging into the floor when moving up from the crouched position by making random, small movements")]
    [SerializeField] private float upwardCrouchSmoother = 0.0004f;

    [Tooltip("The crouch's collision detection will only work against objects with these layers (recommended to avoid the player " + 
        "layer since there could be conflicts with the flashlight collider sending a false positive)")]
    [SerializeField] private LayerMask crouchLayerMaskCollisionDetection;

    private Vector3 crouchingCenter = new Vector3(0f, 0.5f, 0f);
    private Vector3 standingCenter = Vector3.zero;

    // For managing crouch state
    private bool crouchInputActive;
    private bool CanCrouch => !isSprinting;

    // For detecting colliders above player when crouching
    private bool isUnderCollider;
    private float sphereRadius;
    private float sphereCastDistance;

    public Vector3 moveDir { get; private set; } = Vector3.zero;
    private Vector2 moveInput = Vector2.zero;

    public float moveSpeed { get; private set; } = 0;

    // Used by PlayerFootstep for footstep volumes
    public bool isSprinting { get; private set; }
    public bool isCrouching { get; private set; }

    public CharacterController characterController { get; private set; }
    private UniversalPlayerInput universalPlayerInput;
    private PlayerReferences playerReferences;

    private void Awake()
    {
        universalPlayerInput = GetComponent<UniversalPlayerInput>();
        playerReferences = GetComponent<PlayerReferences>();
        characterController = GetComponent<CharacterController>();

        if (playerReferences.cinemachineCam == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Since playerReferences.cinemachineCam is null, PlayerMovement.cs must be disabled.");
#endif
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        moveSpeed = walkSpeed;

        // For crouch
        standingHeight = characterController.height;
        sphereRadius = characterController.radius;
        sphereCastDistance = standingHeight - crouchHeight;
        crouchAnimationTimeElapsed = 0f;
    }

    private void OnEnable()
    {
        UniversalPlayerInput.OnSprintStarted += HandleSprintStarted;
        UniversalPlayerInput.OnSprintCanceled += HandleSprintCanceled;

        UniversalPlayerInput.OnCrouchStarted += HandleCrouchStarted;
        UniversalPlayerInput.OnCrouchCanceled += HandleCrouchCanceled;
    }

    private void OnDisable()
    {
        UniversalPlayerInput.OnSprintStarted -= HandleSprintStarted;
        UniversalPlayerInput.OnSprintCanceled -= HandleSprintCanceled;

        UniversalPlayerInput.OnCrouchStarted -= HandleCrouchStarted;
        UniversalPlayerInput.OnCrouchCanceled -= HandleCrouchCanceled;
    }

    private void Update()
    {
        if (!universalPlayerInput.enabled)
        {
            return;
        }

        GetInput();
        HandleMovement();
        HandleCrouch();
    }

    private void FixedUpdate()
    {
        HandleColliderAbovePlayerCheck();
    }

    private void GetInput()
    {
        moveInput = universalPlayerInput.GetMovementInput();
    }

    /// <summary>
    ///     Handles movement based on the move input from the player and uses SimpleMove() to move the character controller.
    /// </summary>
    private void HandleMovement()
    {
        moveDir = (moveInput.y * playerReferences.cinemachineCam.transform.forward) + (moveInput.x * playerReferences.cinemachineCam.transform.right);
        float magnitude = Mathf.Clamp01(moveDir.magnitude) * moveSpeed;
        characterController.SimpleMove(moveDir.normalized * magnitude);
    }

    /// <summary>
    ///     Uses a spherecast and sets isUnderCollider to be true or false based on whether or not the player is currently under a collider.
    /// </summary>
    private void HandleColliderAbovePlayerCheck()
    {
        if (!isCrouching)
        {
            return;
        }

        Vector3 sphereOrigin = transform.position + Vector3.up * sphereRadius;

        // A SphereCast is used instead of a RayCast to provide more realistic collision detection
        bool colliderDetected = Physics.SphereCast(
            sphereOrigin,
            sphereRadius,
            Vector3.up,
            out RaycastHit hit,
            sphereCastDistance,
            crouchLayerMaskCollisionDetection
        );

        isUnderCollider = colliderDetected && hit.distance < sphereCastDistance;

#if UNITY_EDITOR
        Color debugColor = isUnderCollider ? Color.red : Color.green;
        Debug.DrawLine(sphereOrigin, sphereOrigin + Vector3.up * sphereCastDistance, debugColor);
#endif
    }

    private void HandleSprintStarted()
    {
        if (!CanSprint)
        {
            // Queues sprint if able
            if (isCrouching && queueSprintRoutine == null)
            {
                queueSprintRoutine = StartCoroutine(QueueSprint());
            }

            return;
        }

        isSprinting = true;
        moveSpeed = sprintSpeed;
    }

    /// <summary>
    ///     A coroutine that is useful for queueing the sprint if the sprint input is pressed but the player is
    ///     currently unable to sprint for any reason.
    /// </summary>
    private IEnumerator QueueSprint()
    {
        while (!CanSprint)
        {
            yield return null;
        }

        HandleSprintStarted();
        queueSprintRoutine = null;
    }

    private void HandleSprintCanceled()
    {
        if (!CanSprint)
        {
            if (queueSprintRoutine != null)
            {
                StopCoroutine(queueSprintRoutine);
                queueSprintRoutine = null;
            }

            return;
        }

        isSprinting = false;
        moveSpeed = walkSpeed;
    }

    private void HandleCrouchStarted()
    {
        crouchInputActive = true;
    }

    private void HandleCrouchCanceled()
    {
        crouchInputActive = false;
    }
    
    private void HandleCrouch()
    {
        // In the process of crouching
        if (crouchInputActive && CanCrouch && characterController.height > crouchHeight)
        {
            if (moveSpeed > crouchSpeed)
            {
                moveSpeed = crouchSpeed;
            }

            CrouchLerp(crouchingCenter, crouchHeight);
            isCrouching = true;
        }
        // In the process of standing up
        else if (!crouchInputActive && characterController.height < standingHeight && !isUnderCollider)
        {
            if (moveSpeed < walkSpeed)
            {
                moveSpeed = walkSpeed;
            }

            CrouchLerp(standingCenter, standingHeight);

            // Makes sure that there are no jittery movements when moving back up from a crouch
            characterController.Move(Random.onUnitSphere * upwardCrouchSmoother);
            isCrouching = false;
        }
        // Currently standing
        else if (crouchAnimationTimeElapsed > 0)
        {
            crouchAnimationTimeElapsed = 0;
        }
    }

    /// <summary>
    ///     Lerps between either the standing height or crouching height.
    /// </summary>
    /// <param name="targetCenter">The target center of the character controller.</param>
    /// <param name="targetHeight">The target height of the character controller.</param>
    private void CrouchLerp(Vector3 targetCenter, float targetHeight)
    {
        Vector3 currentCenter = characterController.center;
        float currentHeight = characterController.height;
        float lerp = crouchAnimationTimeElapsed / timeToCrouch;

        if (lerp <= 1)
        {
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, lerp);
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, lerp);
        }
        else
        {
            characterController.center = targetCenter;
            characterController.height = targetHeight;
        }

        crouchAnimationTimeElapsed += Time.deltaTime;
    }
}
