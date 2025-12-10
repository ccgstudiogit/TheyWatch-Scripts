using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Player movement is required because checking the device whilst sprinting is not allowed
[RequireComponent(typeof(PlayerMovement))]
public class InputCheckDevice : PlayerInput
{
    public static event Action<bool> OnCheckDevice;

    private PlayerMovement playerMovement;
    private bool checkingDevice = false;

    protected override void Awake()
    {
        base.Awake();
        playerMovement = GetComponent<PlayerMovement>();
    }

    protected override void OnEnable()
    {
        if (playerActions != null)
        {
            playerActions.Base.CheckDevice.started += OnCheckDeviceStarted;
            playerActions.Base.CheckDevice.canceled += OnCheckDeviceCanceled;

            playerActions.Base.Sprint.started += OnSprintStarted;
        }
    }

    protected override void OnDisable()
    {
        if (playerActions != null)
        {
            playerActions.Base.CheckDevice.started -= OnCheckDeviceStarted;
            playerActions.Base.CheckDevice.canceled -= OnCheckDeviceCanceled;

            playerActions.Base.Sprint.started -= OnSprintStarted;
        }
    }

    private void OnCheckDeviceStarted(InputAction.CallbackContext ctx)
    {
        // The player should not be able to check the device if they are sprinting
        if (AreInputsEnabled() && !playerMovement.isSprinting)
        {
            CheckDevice(true);
        }
    }

    private void OnCheckDeviceCanceled(InputAction.CallbackContext ctx)
    {
        if (AreInputsEnabled() && checkingDevice)
        {
            CheckDevice(false);
        }
    }

    /// <summary>
    ///     Makes sure that if the player is checking their device and starts sprinting, they will no longer be able
    ///     to look at the device whilst sprinting.
    /// </summary>
    private void OnSprintStarted(InputAction.CallbackContext ctx)
    {
        if (AreInputsEnabled() && checkingDevice)
        {
            CheckDevice(false);
        }
    }

    /// <summary>
    ///     Whether or not the player should check the device.
    /// </summary>
    private void CheckDevice(bool check)
    {
        checkingDevice = check;
        OnCheckDevice?.Invoke(check);
    }
}
