using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimations : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Dampens the time it takes to transition between animations (if set to 0, animations will immediately transition)")]
    [SerializeField] private float animDampenSpeed = 0.15f;

    [Header("Animation Thresholds")]
    [Header("- Only Edit If Animator Thresholds Change -")]
    [Tooltip("This threshold should match the animator's idle threshold in the movement blendtree")]
    [SerializeField] private float idleAnimThreshold = 0f;
    [Tooltip("This threshold should match the animator's walk threshold in the movement blendtree")]
    [SerializeField] private float walkAnimThreshold = 0.5f;
    [Tooltip("This threshold should match the animator's sprint threshold in the movement blendtree")]
    [SerializeField] private float sprintAnimThreshold = 1f;

    [Tooltip("This int should match the animator's check device int trigger (1 == check device, 0 == not checking device)")]
    [SerializeField] private int checkDeviceTrigger = 1;
    private int noLongerCheckingDeviceTrigger = 0;

    [Header("Animation Parameter Names")]
    [Header("- Only Edit If Animator Parameter Names Change -")]
    [Tooltip("This string should match the speed parameter name in the animator exactly")]
    [SerializeField] private string speedStr = "speed";
    [Tooltip("This string should match the check device parameter name in the animator exactly")]
    [SerializeField] private string checkDeviceStr = "checkDevice";

    private const float movementThreshold = 0.001f; // Makes sure floating-point errors don't cause subtle movements when idling

    private Animator armsAnimator; // Set by PlayerConfigSO in SpawnPlayerHandler
    private PlayerMovement playerMovement;

    private void Awake()
    {   
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        // Makes sure that Update() won't run into null-reference exception issues. Script will be re-enabled
        // once an animator and animator controller are assigned via SetAnimatorAndAnimatorController()
        if (armsAnimator == null)
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        InputCheckDevice.OnCheckDevice += HandleCheckDeviceAnimation;

        InputEMP.OnEMP += HandleEMPAnimation;
    }

    private void OnDisable()
    {
        InputCheckDevice.OnCheckDevice -= HandleCheckDeviceAnimation;

        InputEMP.OnEMP -= HandleEMPAnimation;
    }

    private void Update()
    {
        HandleMovementAnimations();
    }

    /// <summary>
    ///     Externally set the player's Animator component and Runtime Animator Controller component.
    /// </summary>
    public void SetAnimatorAndAnimatorController(Animator animator, RuntimeAnimatorController controller)
    {
        armsAnimator = animator;
        armsAnimator.runtimeAnimatorController = controller;

        if (!enabled)
        {
            enabled = true;
        }
    }

    private void HandleMovementAnimations()
    {
        if (!playerMovement.enabled)
        {
            if (armsAnimator.GetFloat(speedStr) > idleAnimThreshold)
            {
                armsAnimator.SetFloat(speedStr, idleAnimThreshold, animDampenSpeed, Time.deltaTime);
            }

            return;
        }

        if (playerMovement.moveDir.sqrMagnitude < movementThreshold)
        {
            armsAnimator.SetFloat(speedStr, idleAnimThreshold, animDampenSpeed, Time.deltaTime);
        }
        else if (playerMovement.moveDir.sqrMagnitude > movementThreshold && playerMovement.moveSpeed <= playerMovement.walkSpeed)
        {
            armsAnimator.SetFloat(speedStr, walkAnimThreshold, animDampenSpeed, Time.deltaTime);
        }
        else if (playerMovement.moveDir.sqrMagnitude > movementThreshold && playerMovement.moveSpeed > playerMovement.walkSpeed)
        {
            armsAnimator.SetFloat(speedStr, sprintAnimThreshold, animDampenSpeed, Time.deltaTime);
        }
    }

    private void HandleCheckDeviceAnimation(bool checkingDevice)
    {
        armsAnimator.SetInteger(checkDeviceStr, checkingDevice ? checkDeviceTrigger : noLongerCheckingDeviceTrigger);
    }

    private void HandleEMPAnimation()
    {
        armsAnimator.SetTrigger("emp");
    }
}
