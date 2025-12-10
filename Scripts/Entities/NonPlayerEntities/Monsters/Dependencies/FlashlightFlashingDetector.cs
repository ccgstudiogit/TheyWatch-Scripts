using System;
using UnityEngine;

public class FlashlightFlashingDetector : MonoBehaviour
{
    public event Action OnMaxFlashesReached;

    [Header("Raycast Target Reference")]
    [Tooltip("This is used instead of transform.position as the target raycast from the player's flashlight. This is used " +
        "for more accurate raycasting, otherwise the raycast will target the ground causing a false negative")]
    [SerializeField] private Transform raycastTarget;

    [Header("Flash Detection Settings")]
    [Tooltip("OnMaxFlashesReached event will fire off once this script detects it has been flashed this many times")]
    [SerializeField] private int _minTimesFlashed = 3;
    public int minTimesFlashed => _minTimesFlashed;

    private int timesFlashed;
    private float timeSinceLastFlash;

    [Tooltip("The times flashed counter will reset to 0 after this amount of seconds after no longer being flashed")]
    [SerializeField] private float resetTimesFlashedAfterXSeconds = 1.25f;

    private void Start()
    {
        timeSinceLastFlash = 0f;
    }

    private void OnEnable()
    {
        ResetTimesFlashed();
    }

    private void Update()
    {
        HandleFrameChecks();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FlashlightCollisions flashlightCollisions) && flashlightCollisions.enabled)
        {
            // Only count the flash if there are no other game objects between this object and the flashlight
            if (flashlightCollisions.IsVisible(this, raycastTarget))
            {
                timesFlashed++;
                timeSinceLastFlash = 0f;

                if (timesFlashed >= minTimesFlashed)
                {
                    OnMaxFlashesReached?.Invoke();
                }
            }
        }
    }

    private void HandleFrameChecks()
    {
        timeSinceLastFlash += Time.deltaTime;

        if (timesFlashed > 0 && timeSinceLastFlash > resetTimesFlashedAfterXSeconds)
        {
            ResetTimesFlashed();
        }
    }

    private void ResetTimesFlashed()
    {
        timesFlashed = 0;
    }

    /// <summary>
    ///     Set the amount of times flashed required to fire off the OnMaxFlashesReached event.
    /// </summary>
    /// <param name="newMinTimesFlashed">The new minimum amount of times flashed required.</param>
    public void SetMinTimesFlashed(int newMinTimesFlashed)
    {
        _minTimesFlashed = newMinTimesFlashed;
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name}'s new minTimesFlashed = " + minTimesFlashed);
#endif
    }
}
