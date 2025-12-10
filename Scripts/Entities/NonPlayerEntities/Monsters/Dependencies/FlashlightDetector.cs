using System;
using UnityEngine;

public class FlashlightDetector : MonoBehaviour
{
    public event Action OnMaxTimeInFlashlight;

    [Header("Raycast Target Reference")]
    [Tooltip("This is used instead of transform.position as the target raycast from the player's flashlight. This is used " +
        "for more accurate raycasting, otherwise the raycast will target the ground causing a false negative")]
    [SerializeField] private Transform raycastTarget;

    [Header("Event Action Settings")]
    [Tooltip("This setting is used to make sure that OnMaxTimeInFlashlight is not invoked multiple times in a short window")]
    [SerializeField] private float eventCooldownLength = 5f;
    private bool onEventCooldown;

    [Header("Light Behavior Settings")]
    [Tooltip("The maximum time this object can be lit by the flashlight before the event OnMaxTimeInFlashlight is fired off")]
    [SerializeField] private float maxTimeInFlashlight = 0.5f;
    private float elapsedTimeInFlashlight;

    // Handles making sure that the game object is not only within the flashlight's collider, but that no
    // other game object is in between this object and the flashlight
    private FlashlightCollisions flashlightCollisions;
    private bool inFlashlightCollider;

    // This is only true if this object is within the flashlight's collider and no other object is in between
    private bool isLitByFlashlight;

    private void Start()
    {
        onEventCooldown = false;
        isLitByFlashlight = false;

#if UNITY_EDITOR
        if (raycastTarget == null)
        {
            Debug.LogWarning($"{gameObject.name}'s raycastTarget null. Please assign a reference.");
        }
#endif
    }

    private void OnEnable()
    {
        // This is in OnEnable to prevent an issue where if flashlight detector is turned off and then turned on again,
        // the event OnMaxTimeInFlashlight would fire off because elapsedTimeInFlashlight would still be above the threshold
        elapsedTimeInFlashlight = 0f;
    }

    private void Update()
    {
        HandleFrameChecks();
    }

    private void FixedUpdate()
    {
        HandlePhysicsChecks();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FlashlightCollisions flashlightCollisions) && flashlightCollisions.enabled)
        {
            this.flashlightCollisions = flashlightCollisions;
            inFlashlightCollider = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out FlashlightCollisions flashlightCollisions))
        {
            this.flashlightCollisions = flashlightCollisions;
            inFlashlightCollider = false;
        }
    }

    private void HandleFrameChecks()
    {
        // Fixes an issue where if this game object is within the light's collider but the collider is turned off, OnTriggerExit()
        // would be not properly called which kept InFlashlightCollider to be true, even though the collider itself was turned off
        if (inFlashlightCollider && !flashlightCollisions.IsColliderGameObjectActive())
        {
            inFlashlightCollider = false;
        }

        // isLitByFlashlight is set within FixedUpdate()
        if (isLitByFlashlight)
        {
            elapsedTimeInFlashlight += Time.deltaTime;

            if (elapsedTimeInFlashlight > maxTimeInFlashlight && !onEventCooldown)
            {
                OnMaxTimeInFlashlight?.Invoke();

                // Cooldown is used to make sure OnMaxTimeInFlashlight is not fired off multiple times in a short window
                onEventCooldown = true;
                Invoke(nameof(ResetCooldown), eventCooldownLength);
            }
        }
        // If this object is not currently lit by the flashlight but has spent time in the flashlight, reduce the elapsed time until back to 0f
        else if (elapsedTimeInFlashlight > 0f)
        {
            elapsedTimeInFlashlight -= Time.deltaTime;

            if (elapsedTimeInFlashlight < 0f)
            {
                elapsedTimeInFlashlight = 0f;
            }
        }
    }

    private void HandlePhysicsChecks()
    {
        if (!inFlashlightCollider || flashlightCollisions == null)
        {
            isLitByFlashlight = false;
            return;
        }

        // This is done in FixedUpdate because IsVisible() relies on raycasting (this doesn't necessarily need to be in FixedUpdate(), but
        // I figure I'd play it safe since this is calculated every physics frame)
        isLitByFlashlight = flashlightCollisions.IsVisible(this, raycastTarget);
    }

    private void ResetCooldown()
    {
        onEventCooldown = false;
    }

    /// <summary>
    ///     Set a new maximum amount of time for this object to spend in the flashlight before firing off the OnMaxTimeInFlashlight event.
    /// </summary>
    /// <param name="newMaxTime">The maximum amount of time spent in the flashlight before firing off the OnMaxTimeInFlashlight event.</param>
    public void SetMaxTimeInFlashlight(float newMaxTime)
    {
        maxTimeInFlashlight = newMaxTime;
    }
}
