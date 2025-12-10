using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerReferences))]
public class PlayerFootstepAudio : MonoBehaviour
{
    // Lets monsters know the location of the player's footstep if within range (Vector3 == player's position). Note: this event
    // does not fire off if the player is crouching
    public event Action<Vector3> OnAudibleFootstep;

    [Header("Audio Parameters")]
    [Tooltip("The base speed at which footstep audio will be played")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private Vector2 baseVolume = new Vector2(0.775f, 0.825f);

    [Tooltip("A multiplier which affects the frequency of footsteps while sprinting (lower number = more frequent)")]
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private Vector2 sprintVolume = new Vector2(1f, 1f);

    [Tooltip("A multiplier which affects the frequency of footsteps  while crouching (higher number = less frequent)")]
    [SerializeField] private float crouchStepMultiplier = 1.25f;
    [SerializeField] private Vector2 crouchVolume = new Vector2(0.5f, 0.55f);

    private Vector2 GetVolume => playerMovement.isSprinting ? sprintVolume : playerMovement.isCrouching ? crouchVolume : baseVolume;

    [Tooltip("The minimum move speed the player must be moving in order for a footstep SFX to play")]
    [SerializeField] private float minMoveSpeed = 0.1f;
    private float footstepTimer = 0f;
    private float GetCurrentOffset => (
        playerMovement.isCrouching ? baseStepSpeed * crouchStepMultiplier :
        playerMovement.isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed
    );

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO defaultFootstepSFX;
    [SerializeField] private SoundEffectSO grassFootstepSFX;
    [SerializeField] private SoundEffectSO outdoorTileFootstepSFX;
    [SerializeField] private SoundEffectSO carpetFootstepSFX;
    [SerializeField] private SoundEffectSO metalFootstepSFX;

    [Header("Raycast Collisions")]
    [Tooltip("Makes sure the raycast only interacts with colliders that have this layer")]
    [SerializeField] private LayerMask groundLayerMask;

    private PlayerMovement playerMovement;
    private PlayerReferences playerReferences;
    private AudioSource audioSource;

    private void Awake()
    {   
        playerMovement = GetComponent<PlayerMovement>();
        playerReferences = GetComponent<PlayerReferences>();
    }

    private void Start()
    {
        if (playerReferences.audioSource == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Since playerReferences.audioSource is null, PlayerFootstepAudio.cs is now disabled.");
#endif
            enabled = false;
            return;
        }
        else
        {
            audioSource = playerReferences.audioSource;
        }
    }

    private void Update()
    {
        HandleFootstepAudio();
    }

    private void HandleFootstepAudio()
    {
        if (!playerMovement.enabled)
        {
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0 && (Mathf.Abs(playerMovement.moveDir.x) > minMoveSpeed || Mathf.Abs(playerMovement.moveDir.z) > minMoveSpeed))
        {
            if (Physics.Raycast(playerReferences.cinemachineCam.transform.position, Vector3.down, out RaycastHit hit, 3, groundLayerMask))
            {
                switch (hit.collider.tag)
                {
                    case "Ground/OutdoorTile":
                        outdoorTileFootstepSFX.Play(GetVolume, audioSource);
                        break;
                    case "Ground/Carpet":
                        carpetFootstepSFX.Play(GetVolume, audioSource);
                        break;
                    case "Ground/Metal":
                        metalFootstepSFX.Play(GetVolume, audioSource);
                        break;
                    case "Ground/Grass":
                        grassFootstepSFX.Play(GetVolume, audioSource);
                        break;
                    default:
                        defaultFootstepSFX.Play(GetVolume, audioSource);
                        break;
                }

                if (!playerMovement.isCrouching)
                {
                    OnAudibleFootstep?.Invoke(transform.position);
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }
}
