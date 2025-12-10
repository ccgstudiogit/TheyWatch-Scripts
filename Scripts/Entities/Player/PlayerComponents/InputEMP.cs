using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputEMP : PlayerInput
{
    public static event Action OnEMP; // For activating the animation

    protected override void OnEnable()
    {
        if (playerActions != null)
        {
            playerActions.Base.EMP.performed += OnEMPPerformed;
        }
    }

    protected override void OnDisable()
    {
        if (playerActions != null)
        {
            playerActions.Base.EMP.performed -= OnEMPPerformed;
        }
    }

    private void OnEMPPerformed(InputAction.CallbackContext ctx)
    {
        if (AreInputsEnabled())
        {
            OnEMP?.Invoke();
        }
    }
}
