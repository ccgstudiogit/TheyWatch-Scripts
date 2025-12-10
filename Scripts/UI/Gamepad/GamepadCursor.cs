using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class GamepadCursor : MonoBehaviour
{
    // IMPORTANT: In order for the virtual mouse to function, a virtual mouse MUST be added to the gamepad control scheme

    public static GamepadCursor instance { get; private set; }

    [Header("Input Manager Reference")]
    [SerializeField] private InputManager inputManager;

    [Header("Player Input Reference")]
    [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;

    [Header("Cursor")]
    [SerializeField] private RectTransform cursorTransform;

    [field: SerializeField] public float minCursorSpeed { get; private set; } = 0.1f;
    [field: SerializeField] public float maxCursorSpeed { get; private set; } = 5f;

    public const float defaultCursorSpeed = 1f;

    private float cursorSpeed; // The current cursor speed
    private const float cursorSpeedMultiplier = 1000f; // SetCursorSpeed() handles this multiplier

    private const string cursorSensitivityStr = "gamepadCursorSensitivity"; // For PlayerPrefs

    [Tooltip("The gamepad cursor's padding near the edges")]
    [SerializeField] private float padding = 15f;

    [Header("Canvas Reference")]
    [Tooltip("The gamepad cursor will stay within this canvas' rect transform")]
    [SerializeField] private RectTransform canvasRectTransform;

    private Mouse currentMouse;
    private Mouse virtualMouse;
    private bool previousMouseState;

    private string previousControlScheme = ""; // Helps to keep track of swapping between control schemes

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetCursorSpeed(GetCursorSpeed());
    }

    private void OnEnable()
    {
        currentMouse = Mouse.current;

        if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouse.added)
        {
            InputSystem.AddDevice(virtualMouse);
        }

        InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

        if (cursorTransform != null)
        {
            Vector2 position = cursorTransform.anchoredPosition;
            InputState.Change(virtualMouse.position, position); // Set the starting position of the gamepad cursor
        }

        // Similar to a LateUpdate() function
        InputSystem.onAfterUpdate += UpdateMotion;
        playerInput.onControlsChanged += OnControlsChanged;
    }

    private void OnDisable()
    {
        if (virtualMouse != null && virtualMouse.added)
        {
            InputSystem.RemoveDevice(virtualMouse);
        }

        InputSystem.onAfterUpdate -= UpdateMotion;
        playerInput.onControlsChanged -= OnControlsChanged;
    }

    /// <summary>
    ///     Updates the virtual mouse accordingly.
    /// </summary>
    private void UpdateMotion()
    {
        if (virtualMouse == null || Gamepad.current == null)
        {
            return;
        }

        // Return out of this function if the cursor should not be displayed
        if (!ShouldDisplayCursor())
        {
            return;
        }

        Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
        deltaValue *= cursorSpeed * Time.unscaledDeltaTime;

        Vector2 currentPosition = virtualMouse.position.ReadValue();
        Vector2 newPosition = currentPosition + deltaValue;

        // Make sure the cursor stays clamped within the game view
        newPosition.x = Mathf.Clamp(newPosition.x, padding, Screen.width - padding);
        newPosition.y = Mathf.Clamp(newPosition.y, padding, Screen.height - padding);

        // Update the virtual mouse's position
        InputState.Change(virtualMouse.position, newPosition);
        InputState.Change(virtualMouse.delta, deltaValue);

        // Map the gamepad's A button to the mouse's left click
        bool aButtonIsPressed = Gamepad.current.aButton.IsPressed();
        if (previousMouseState != aButtonIsPressed)
        {
            virtualMouse.CopyState<MouseState>(out var mouseState);
            mouseState.WithButton(MouseButton.Left, aButtonIsPressed);
            InputState.Change(virtualMouse, mouseState);
            previousMouseState = aButtonIsPressed;
        }

        AnchorCursor(newPosition);
    }

    /// <summary>
    ///     Handles anchoring the virtual mouse's position relative to the screen.
    /// </summary>
    /// <param name="position">The Vector2 position that should be anchored.</param>
    private void AnchorCursor(Vector2 position)
    {
        // If the canvas's render mode is ScreenSpace.Camera, a camera reference is required instead of null
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, position, null, out Vector2 anchoredPosition);
        cursorTransform.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    ///     Handles switching between the mouse cursor and gamepad cursor.
    /// </summary>
    private void OnControlsChanged(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        // Switching to keyboard and mouse
        if (playerInput.currentControlScheme == InputManager.keyboardScheme && previousControlScheme != InputManager.keyboardScheme)
        {
            cursorTransform.gameObject.SetActive(false); // Gamepad cursor

            currentMouse.WarpCursorPosition(virtualMouse.position.ReadValue());
            Cursor.visible = true; // Mouse cursor

            previousControlScheme = InputManager.keyboardScheme;
        }
        // Switching to gamepad
        else if (playerInput.currentControlScheme == InputManager.gamepadScheme && previousControlScheme != InputManager.gamepadScheme)
        {
            Cursor.visible = false; // Mouse cursor

            InputState.Change(virtualMouse.position, currentMouse.position.ReadValue());
            AnchorCursor(currentMouse.position.ReadValue());
            cursorTransform.gameObject.SetActive(true); // Gamepad cursor

            previousControlScheme = InputManager.gamepadScheme;
        }
    }

    /// <summary>
    ///     Checks if the gamepad cursor should be displayed based on whether or not the UI map is enabled. Also
    ///     handles turning the gamepad cursor game object on/off depending on if the UI map is enabled/disabled.
    /// </summary>
    /// <returns>True if the ui map is enabled, false if otherwise.</returns>
    private bool ShouldDisplayCursor()
    {
        // If the player is not interacting with UI, it can be assumed the player is actively playing a level and
        // the gamepad cursor should be off and not interfere with gamepad input
        if (playerInput.currentControlScheme != InputManager.gamepadScheme || !inputManager.IsUIMapEnabled())
        {
            if (cursorTransform.gameObject.activeSelf)
            {
                cursorTransform.gameObject.SetActive(false);
            }

            return false;
        }
        else
        {
            if (!cursorTransform.gameObject.activeSelf)
            {
                cursorTransform.gameObject.SetActive(true);
            }

            return true;
        }
    }

    /// <summary>
    ///     Set the gamepad's cursor speed to a new speed.
    /// </summary>
    /// <param name="speed">The new speed.</param>
    public void SetCursorSpeed(float speed)
    {
        // The unchanged speed is sent to PlayerPrefs so that GamepadUISensitivitySlider has an easier time getting its value
        // from PlayerPrefs without having to do any conversions to/from the cursorSpeedMultiplier
        speed = Mathf.Clamp(speed, minCursorSpeed, maxCursorSpeed);
        PlayerPrefs.SetFloat(cursorSensitivityStr, speed);

        cursorSpeed = speed * cursorSpeedMultiplier;
    }

    /// <summary>
    ///     Get the gamepad cursor's saved speed from PlayerPrefs.
    /// </summary>
    /// <returns>A float of the gamepad's cursor speed.</returns>
    public float GetCursorSpeed()
    {
        return PlayerPrefs.GetFloat(cursorSensitivityStr, defaultCursorSpeed);
    }
}
