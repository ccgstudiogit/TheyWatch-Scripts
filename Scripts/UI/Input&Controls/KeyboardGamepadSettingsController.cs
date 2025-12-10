using UnityEngine;
using Michsky.MUIP;

public class KeyboardGamepadSettingsController : MonoBehaviour
{
    [Header("Window Manager Reference")]
    [Tooltip("This reference is used to make sure the selected button matches the open window")]
    [SerializeField] private WindowManager controlsWindowManager;

    [Header("Button References")]
    // These buttons are used to swap between the keyboard and gamepad re-binding options
    [SerializeField] private ButtonController keyboardButton;
    [SerializeField] private ButtonController gamepadButton;

    // If a gamepad is not detected, this game object will be active
    [SerializeField] private GameObject noGamepadDetected;
    [SerializeField] private GameObject gamepadDetected;

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        if (InputManager.instance != null)
        {
            UpdateUI();
        }

        InputManager.OnGamepadConnected += HandleGamepadUpdate;
    }

    private void OnDisable()
    {
        InputManager.OnGamepadConnected -= HandleGamepadUpdate;
    }

    private void UpdateUI()
    {
        if (InputManager.instance.gamepadConnected)
        {
            gamepadDetected.SetActive(true);
            noGamepadDetected.SetActive(false);
        }
        else
        {
            gamepadDetected.SetActive(false);
            noGamepadDetected.SetActive(true);
        }
    }

    private void HandleGamepadUpdate(bool connected)
    {
        if (connected)
        {   
            gamepadDetected.SetActive(true);
            noGamepadDetected.SetActive(false);
        }
        else
        {
            gamepadDetected.SetActive(false);
            noGamepadDetected.SetActive(true);
        }
    }
}
