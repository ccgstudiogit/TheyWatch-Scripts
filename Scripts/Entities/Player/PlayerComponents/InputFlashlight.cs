using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputFlashlight : PlayerInput
{
    public static event Action OnFlashlightToggle;

    protected override void OnEnable()
    {
        if (playerActions != null)
        {
            playerActions.Base.Flashlight.performed += OnToggleFlashlight;
        }
    }

    protected override void OnDisable()
    {
        if (playerActions != null)
        {
            playerActions.Base.Flashlight.performed -= OnToggleFlashlight;
        }
    }

    private void OnToggleFlashlight(InputAction.CallbackContext ctx)
    {
        if (AreInputsEnabled())
        {
            OnFlashlightToggle?.Invoke();
        }
    }
}
