using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Michsky.MUIP;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }
    public bool inputsEnabled { get; private set; } = true;

    private PlayerActions playerActions;
    public PlayerActions Actions => playerActions;

    // Keeping track of gamepad/keyboard & mouse
    [Header("Player Input")]
    [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;

    // Scheme names must exactly match the control scheme name from the input actions asset
    public const string keyboardScheme = "Keyboard";
    public const string gamepadScheme = "Gamepad";

    public static event Action OnControlSchemeChanged; // Lets listeners know when the controls change from keyboard to gamepad and vice versa
    public static event Action<bool> OnGamepadConnected; // Lets listeners know when a gamepad has been connected/disconnected

    public bool gamepadConnected => Gamepad.all.Count > 0;
    public bool usingGamepad => playerInput.currentControlScheme == gamepadScheme;

    // Rebind events
    public static event Action OnRebindComplete;
    public static event Action OnRebindCanceled;
    public static event Action<InputAction, int> OnRebindStarted; // InputAction == keybinding, int == binding index

    // Sends a message to PlayerLook.cs to update sensitivity
    public static event Action<float> OnUpdateSensitivity;

    [Header("Rebind Settings")]
    [Tooltip("The text that is displayed whilst the player is currently in the process of rebinding a keybind")]
    [SerializeField] private string currentlyRebindingStatusText = ". . .";

    [Header("Sensitivity Settings")]
    [field: SerializeField] public float minSensitivity { get; private set; } = 0.1f;
    [field: SerializeField] public float maxSensitivity { get; private set; } = 10f;

    private string sensitivityStr = "sensitivity";
    private float defaultSensitivity = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            playerActions = new PlayerActions();
            playerActions.Enable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Make sure the re-binds are loaded on game start
        LoadBindingOverrides();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += CheckDeviceChanges;
        playerInput.onControlsChanged += HandleOnControlsChanged;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= CheckDeviceChanges;
        playerInput.onControlsChanged -= HandleOnControlsChanged;
    }

    private void OnDestroy()
    {
        if (playerActions != null)
        {
            playerActions.Disable();
            playerActions = null;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    ///     This method is subscribed to InputSystem.onDeviceChange and monitors if there has been a change in
    ///     devices (mainly used for detecting if a gamepad was connected or disconnected). Whenever there is a
    ///     change, OnGamepadConnected<bool> is invoked.
    /// </summary>
    private void CheckDeviceChanges(InputDevice device, InputDeviceChange change)
    {
        OnGamepadConnected?.Invoke(gamepadConnected);
    }

    /// <summary>
    ///     This method is subscribed to PlayerInput.onControlsChanged and lets listeners know when the player switches
    ///     from keyboard to gamepad and vice versa. Helpful for updating in-game UI to match whatever control scheme
    ///     the player is currently using.
    /// </summary>
    private void HandleOnControlsChanged(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        OnControlSchemeChanged?.Invoke();
    }

    /// <summary>
    ///     Enable or disable PlayerActions.Base map.
    /// </summary>
    public void EnableBaseMap(bool enabled)
    {
        if (playerActions == null)
        {
            playerActions = new PlayerActions();
        }

        (enabled ? (Action)(() => playerActions.Base.Enable()) : (() => playerActions.Base.Disable()))();
    }

    /// <summary>
    ///     Enable or disable PlayerActions.UI map.
    /// </summary>
    public void EnableUIMap(bool enabled)
    {
        if (playerActions == null)
        {
            playerActions = new PlayerActions();
        }

        (enabled ? (Action)(() => playerActions.UI.Enable()) : (() => playerActions.UI.Disable()))();
    }

    /// <summary>
    ///     Check whether or not the Base map is enabled.
    /// </summary>
    /// <returns>True if the Base map is enabled, false if otherwise.</returns>
    public bool IsBaseMapEnabled()
    {
        return playerActions.Base.enabled;
    }

    /// <summary>
    ///     Check whether or not the UI map is enabled.
    /// </summary>
    /// <returns>True if the UI map is enabled, false if otherwise.</returns>
    public bool IsUIMapEnabled()
    {
        return playerActions.UI.enabled;
    }

    /// <summary>
    ///     Get an InputAction based on the action map and action name.
    /// </summary>
    /// <param name="actionMap">The map's name.</param>
    /// <param name="actionName">The action's name.</param>
    /// <returns>An InputAction.</returns>
    public InputAction GetAction(string actionMap, string actionName)
    {
        return playerActions.FindAction(actionMap + "/" + actionName);
    }

    /// <summary>
    ///     Set inputsEnabled to be true.
    /// </summary>
    public void EnableInputs()
    {
        inputsEnabled = true;
    }

    /// <summary>
    ///     Set inputsEnabled to be false.
    /// </summary>
    public void DisableInputs()
    {
        inputsEnabled = false;
    }

    /// <summary>
    ///     Fires off an event that lets listeners know (most likely PlayerLook.cs) to adjust look sensitivity.
    /// </summary>
    /// <param name="value">The new sensitivity value.</param>
    public void SetSensitivity(float value)
    {
        value = Mathf.Clamp(value, minSensitivity, maxSensitivity);
        value = Mathf.Round(value * 100) / 100; // Round to 2 decimal places

        PlayerPrefs.SetFloat(sensitivityStr, value);
        OnUpdateSensitivity?.Invoke(value); // Sends a message to PlayerLook to handle sensitivity changes
    }

    /// <summary>
    ///     Get the current sensitivity setting.
    /// </summary>
    /// <returns>A float of the current sensitivity.</returns>
    public float GetCurrentSensitivity()
    {
        return PlayerPrefs.GetFloat(sensitivityStr, defaultSensitivity);
    }

    /// <summary>
    ///     Begin the key rebind process. For composite rebinding, make sure bindingIndex is set to the desired composite
    ///     binding.
    /// </summary>
    /// <param name="actionName">The name of the action that should be re-bound.</param>
    /// <param name="bindingIndex">The index of the action specific action.</param>
    /// <param name="rebindButton">The button that started the rebind process.</param>
    /// <param name="excludeMouse">Whether or not the mouse should be excluded from counting as a binding.</param>
    public void StartRebind(string actionName, int bindingIndex, ButtonManager rebindButton, bool excludeMouse)
    {
        // Gets the InputAction off of the C# generated script and not the scriptable object
        InputAction action = playerActions.asset.FindAction(actionName);

        if (action == null || action.bindings.Count <= bindingIndex)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} couldn't find action or binding.");
#endif
            return;
        }

        DoRebind(action, bindingIndex, rebindButton, excludeMouse);
    }

    /// <summary>
    ///     Handles the actual rebind process.
    /// </summary>
    /// <param name="actionToRebind">The action that should be re-bound.</param>
    /// <param name="bindingIndex">The index of the action.</param>
    /// <param name="rebindButton">The button that started the rebind process.</param>
    /// <param name="excludeMouse">Whether or not the mouse should be excluded.</param>
    private void DoRebind(InputAction actionToRebind, int bindingIndex, ButtonManager rebindButton, bool excludeMouse)
    {
        if (actionToRebind == null || bindingIndex < 0)
        {
            return;
        }

        rebindButton.buttonText = currentlyRebindingStatusText;
        rebindButton.UpdateUI();

        // Disable the action while disabling to prevent unexpected behavior (gets re-enabled in OnComplete and OnCancel)
        actionToRebind.Disable();

        // Does not start any rebinding action but instead creates an instance of the object that does the rebinding
        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        rebind.OnComplete(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose(); // Make sure to dispose of the operation to avoid memory leaks

            SaveBindingOverride(actionToRebind);
            OnRebindComplete?.Invoke();
        });

        rebind.OnCancel(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            OnRebindCanceled?.Invoke();
        });

        rebind.WithCancelingThrough("<Keyboard>/escape");

        if (excludeMouse)
        {
            rebind.WithControlsExcluding("Mouse");
        }

        // Necessary for composite binding
        if (actionToRebind.bindings[bindingIndex].isPartOfComposite && bindingIndex < actionToRebind.bindings.Count)
        {
            rebind.WithTargetBinding(bindingIndex);
        }

        OnRebindStarted?.Invoke(actionToRebind, bindingIndex);
        rebind.Start(); // Actually starts the rebinding process
    }

    /// <summary>
    ///     Get the binding name of an action.
    /// </summary>
    /// <param name="actionName">The input action.</param>
    /// <param name="bindingIndex">The index of the input action.</param>
    /// <returns>A string of the binding name.</returns>
    public string GetBindingName(string actionName, int bindingIndex)
    {
        // This is here to prevent extreme edge cases where playerActions is null (shouldn't happen normally)
        if (playerActions == null)
        {
            playerActions = new PlayerActions();
        }

        InputAction action = playerActions.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    /// <summary>
    ///     Uses PlayerPrefs to save a binding override as a string.
    /// </summary>
    /// <param name="action"></param>
    private void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    /// <summary>
    ///     Loads previously overridden bindings.
    /// </summary>
    /// <param name="actionName">The action's name.</param>
    public void LoadBindingOverride(string actionName)
    {
        if (actionName == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("actionName null in LoadBindingOverride().");
#endif
            return;
        }

        // This is here to prevent extreme edge cases where playerActions is null (shouldn't happen normally)
        if (playerActions == null)
        {
            playerActions = new PlayerActions();
        }

        InputAction action = playerActions.asset.FindAction(actionName);

        if (action == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"actionName {actionName} not found.");
#endif
            return;
        }

        for (int i = 0; i < action.bindings.Count; i++)
        {
            // Make sure the binding override exists
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
            {
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }
    }

    /// <summary>
    ///     This method can be used to load all overrides for all input actions (should be used once on game
    ///     start so that all of the actions have the correct bindings).
    /// </summary>
    private void LoadBindingOverrides()
    {
        if (playerActions == null)
        {
            playerActions = new PlayerActions();
        }

        InputActionMap baseMap = playerActions.Base;

        for (int i = 0; i < baseMap.actions.Count; i++)
        {
            string actionName = baseMap.actions[i].name;
            LoadBindingOverride(actionName);
        }
    }

    /// <summary>
    ///     Reset a binding to its default set in PlayerActions.
    /// </summary>
    /// <param name="actionName">The action's name.</param>
    /// <param name="bindingIndex">The index of the binding.</param>
    public void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = playerActions.asset.FindAction(actionName);

        if (action == null || action.bindings.Count <= bindingIndex)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} could not find action or binding.");
#endif
            return;
        }

        action.RemoveBindingOverride(bindingIndex);
        SaveBindingOverride(action); // Make sure to save the reset
        OnRebindComplete?.Invoke(); // Sends an update to DisplayInputAction.cs so it will display the updated keybinding
    }
}
