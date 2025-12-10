using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UniversalPlayerInput : PlayerInput
{
    public static event Action OnSprintStarted;
    public static event Action OnSprintCanceled;

    public static event Action OnCrouchStarted;
    public static event Action OnCrouchCanceled;

    // Lets LevelController know to enter the pause menu
    public static event Action OnEnterPauseMenu;

    protected override void OnEnable()
    {
        // Make sure inputs are enabled when this script is enabled
        if (InputManager.instance != null && !InputManager.instance.inputsEnabled)
        {
            InputManager.instance.EnableInputs();
        }

        if (playerActions != null)
        {
            playerActions.Base.Sprint.started += SprintStarted;
            playerActions.Base.Sprint.canceled += SprintCanceled;

            playerActions.Base.Crouch.started += CrouchStarted;
            playerActions.Base.Crouch.canceled += CrouchCanceled;

            playerActions.Base.Pause.performed += OnPausePerformed;
        }
    }

    protected override void OnDisable()
    {
        // Make sure inputs are disabled when this script is disabled
        if (InputManager.instance != null && InputManager.instance.inputsEnabled)
        {
            InputManager.instance.DisableInputs();
        }

        if (playerActions != null)
        {
            playerActions.Base.Sprint.started -= SprintStarted;
            playerActions.Base.Sprint.canceled -= SprintCanceled;

            playerActions.Base.Crouch.started -= CrouchStarted;
            playerActions.Base.Crouch.canceled -= CrouchCanceled;

            playerActions.Base.Pause.performed -= OnPausePerformed;
        }
    }

    /// <summary>
    ///     Get the movement input from playerActions.Base.Move.ReadValue<Vector2>().
    /// </summary>
    /// <returns>A Vector2 with movement input.</returns>
    public Vector2 GetMovementInput()
    {
        return playerActions.Base.Move.ReadValue<Vector2>();
    }

    private void SprintStarted(InputAction.CallbackContext ctx)
    {
        if (!AreInputsEnabled())
        {
            return;
        }

        OnSprintStarted?.Invoke();
    }

    private void SprintCanceled(InputAction.CallbackContext ctx)
    {
        if (!AreInputsEnabled())
        {
            return;
        }

        OnSprintCanceled?.Invoke();
    }

    private void CrouchStarted(InputAction.CallbackContext ctx)
    {
        if (!AreInputsEnabled())
        {
            return;
        }

        OnCrouchStarted?.Invoke();
    }

    private void CrouchCanceled(InputAction.CallbackContext ctx)
    {
        if (!AreInputsEnabled())
        {
            return;
        }

        OnCrouchCanceled?.Invoke();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (!AreInputsEnabled())
        {
            return;
        }

        OnEnterPauseMenu?.Invoke();
    }
}
