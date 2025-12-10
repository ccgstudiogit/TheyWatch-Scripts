using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// DO NOT USE ** LEAVING THIS SCRIPT IN THE PROJECT FOR POSSIBLE FUTURE REFERENCE
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static event Action<bool> OnCollectableSight; // Bool lets any subscribers know whether or not CollectableSight is active

    public static event Action OnPausePressed;

    [Header("References")]
    [SerializeField] private Animator armsAnimator;
    [SerializeField] private GameObject cinemachineCam;
    private CinemachineInputAxisController cinemachineInputAxisController;
    [SerializeField] private Flashlight flashlight;
    private PlayerActions playerActions;
    private CharacterController characterController;
    private AudioSource audioSource;

    [Header("Movement Values")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;
    private Vector3 moveDir = Vector3.zero;
    private Vector2 moveInput = Vector2.zero;
    private float moveSpeed = 0;
    private bool isSprinting;
    
    [Header("Look Sensitivity")]
    [SerializeField] private float lookSensitivity = 10f;
    
    [Header("Animation Values")]
    [Tooltip("Dampens the time it takes to transition between animations (if set to 0, animations will immediately transition)")]
    [SerializeField] private float animDampenSpeed = 0.15f;
    private bool canPlayAnimations;

    [Header("Headbob")]
    [SerializeField] private GameObject camPos;
    private bool camPosExists;
    [SerializeField] private bool enableHeadBob = true;
    [Tooltip("The minimum speed needed for the headbob to take place")]
    [SerializeField] private float minMovementValue = 0.1f;
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    private float defaultYPos = 0; // The position at which the camera rests/resets to while the player is not moving
    private float timer;

    [Header("Sound Effects")]
    [SerializeField] private float baseStepSpeed = 0.425f;
    [SerializeField] private Vector2 baseVolume = new Vector2(0.7f, 0.75f);
    [SerializeField] private float sprintStepMultiplier = 0.65f;
    [SerializeField] private Vector2 sprintVolume = new Vector2(1f, 1f);
    [Tooltip("The minimum move speed the player must be moving in order for a footstep SFX to play")]
    [SerializeField] private float minMoveSpeed = 0.1f;
    [SerializeField] private SoundEffectSO footstepSFX;
    private bool audioSourceExists = false;
    private float footstepTimer = 0f;
    private float GetCurrentOffset => isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    private void Awake()
    {
        GetReferences();
    }

    private void Start()
    {
        InitializeVariables();
        StartCoroutine(ForceCinemachineCameraUpdate());
    }

    private void OnEnable()
    {
        playerActions.Base.Sprint.started += OnSprintStarted;
        playerActions.Base.Sprint.canceled += OnSprintCanceled;

        //playerActions.Base.Flashlight.performed += OnFlashlightPerformed;

        //playerActions.Base.CollectableSight.started += OnCollectableSightStarted;
        //playerActions.Base.CollectableSight.canceled += OnCollectableSightCanceled;

        playerActions.Base.Pause.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        playerActions.Base.Sprint.started -= OnSprintStarted;
        playerActions.Base.Sprint.canceled -= OnSprintCanceled;

        //playerActions.Base.Flashlight.performed -= OnFlashlightPerformed;

        //playerActions.Base.CollectableSight.performed -= OnCollectableSightStarted;
        //playerActions.Base.CollectableSight.canceled -= OnCollectableSightCanceled;

        playerActions.Base.Pause.performed -= OnPausePerformed;
    }

    private void Update()
    {
        GetInput();
        HandleMovement();
        HandleAnimations();
        HandleHeadBob();
        HandleFootsteps();
    }

    private void GetReferences()
    {
        characterController = GetComponent<CharacterController>();

        if (armsAnimator != null)
            canPlayAnimations = true;
        else
            Debug.LogWarning($"{name} does not have an animator reference for armsAnimator variable. Unable to play arms animations.");

        if (InputManager.instance != null)
            playerActions = InputManager.instance.Actions;
        else
            Debug.LogError($"InputManager not found. Unable to process inputs in {name}");

        if (flashlight == null)
            Debug.Log("Flashlight reference not assigned. Player does not have a flashlight.");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogWarning($"{name} does not have an audio source. Unable to play sound effects.");
        else
            audioSourceExists = true;
    }

    private void InitializeVariables()
    {
        moveSpeed = walkSpeed;

        if (cinemachineCam != null)
        {   
            cinemachineInputAxisController = cinemachineCam.GetComponent<CinemachineInputAxisController>();

            // Set the sensitivity for each controller
            foreach (var controller in cinemachineInputAxisController.Controllers)
            {
                controller.Input.Gain *= lookSensitivity;
            }
        }
        else
        {
            Debug.LogError($"{name}'s cinemachineCam null. Disabling PlayerController script.");
            enabled = false;
        }

        // For headbob effect
        if (camPos != null)
            camPosExists = true;
        defaultYPos = camPos.transform.localPosition.y;
    }

    // Fixes an issue where the player arms model is invisible until input is registered
    // (the player's arms would have stayed invisible indefinitely if the player did not move the mouse)
    private IEnumerator ForceCinemachineCameraUpdate()
    {
        if (cinemachineCam == null)
            yield break;

        yield return null;

        CinemachinePanTilt camPanTilt = cinemachineCam.GetComponent<CinemachinePanTilt>();
        if (camPanTilt != null)
        {
            camPanTilt.PanAxis.Value = 5f;

            yield return null;

            camPanTilt.PanAxis.Value = 0f;
        }
    }

    private void GetInput()
    {
        moveInput = playerActions.Base.Move.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        moveDir = (moveInput.y * cinemachineCam.transform.forward) + (moveInput.x * cinemachineCam.transform.right);
        float magnitude = Mathf.Clamp01(moveDir.magnitude) * moveSpeed;
        characterController.SimpleMove(moveDir.normalized * magnitude);
    }

    private void HandleAnimations()
    {
        if (!canPlayAnimations)
            return;

        if (moveDir == Vector3.zero)
            armsAnimator.SetFloat("speed", 0f, animDampenSpeed, Time.deltaTime);
        else if (moveDir != Vector3.zero && moveSpeed <= walkSpeed)
            armsAnimator.SetFloat("speed", 0.5f, animDampenSpeed, Time.deltaTime);
        else if (moveDir != Vector3.zero && moveSpeed > walkSpeed)
            armsAnimator.SetFloat("speed", 1f, animDampenSpeed, Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        if (!enableHeadBob || !camPosExists)
            return;

        if (Mathf.Abs(moveDir.x) > minMovementValue || Mathf.Abs(moveDir.z) > minMovementValue)
        {
            timer += Time.deltaTime * (isSprinting ? sprintBobSpeed : walkBobSpeed);
            camPos.transform.localPosition = new Vector3(
                camPos.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isSprinting ? sprintBobAmount : walkBobAmount),
                camPos.transform.localPosition.z
            );
        }
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded || !audioSourceExists)
            return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0 && (Mathf.Abs(moveDir.x) > minMoveSpeed || Mathf.Abs(moveDir.z) > minMoveSpeed))
        {
            if (Physics.Raycast(cinemachineCam.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    // TODO: Add in cases for various footstep SFX
                    default:
                        footstepSFX.Play(GetFootstepVolume(), audioSource);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }

    private Vector2 GetFootstepVolume()
    {
        if (isSprinting)
            return sprintVolume;
        else
            return baseVolume;
    }

    private void OnSprintStarted(InputAction.CallbackContext ctx)
    {
        moveSpeed = sprintSpeed;
        isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        moveSpeed = walkSpeed;
        isSprinting = false;
    }

    private void OnFlashlightPerformed(InputAction.CallbackContext ctx)
    {
        
    }

    private void OnCollectableSightStarted(InputAction.CallbackContext ctx)
    {
        OnCollectableSight?.Invoke(true);
    }

    private void OnCollectableSightCanceled(InputAction.CallbackContext ctx)
    {
        OnCollectableSight?.Invoke(false);
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        OnPausePressed?.Invoke();
    }
}
