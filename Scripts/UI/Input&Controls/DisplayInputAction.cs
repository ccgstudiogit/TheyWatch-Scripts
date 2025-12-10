using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DisplayInputAction : MonoBehaviour
{
    [Header("Fade Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("The text is hidden whenever the player is checking their device")]
    [SerializeField] private float fadeTime = 0.3f;

    public Slider slider;
    public bool playerCurrentlyInInput { get; set; } // Prevents starting the message notification if the player is already checking the device
    public TextMeshProUGUI displayInputActionText { get; private set; }

    private InputAction inputActionToBeDisplayed;
    private string inputMessage;

    private DisplayInputActionCheckDevice displayInputActionCheckDevice;

    // This should be whatever the input action asset for the check device input is called
    private const string checkDeviceInputName = "CheckDevice";

    private void Awake()
    {
        displayInputActionText = GetComponent<TextMeshProUGUI>();
        displayInputActionCheckDevice = GetComponent<DisplayInputActionCheckDevice>();

        playerCurrentlyInInput = false;
        slider.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (LevelController.instance == null)
        {
            enabled = false;
            return;
        }

        inputActionToBeDisplayed = LevelController.instance.inputActionReference;
        inputMessage = LevelController.instance.inputMessage;

        UpdateText();
    }

    private void OnEnable()
    {
        InputCheckDevice.OnCheckDevice += ShowHideText;
        WristDevice.OnMessageReceived += HandleMessageReceivedToDevice;

        InputManager.OnControlSchemeChanged += UpdateText;
        InputManager.OnRebindComplete += UpdateText; // Makes sure that if the player rebinds the input action, the display message is updated
    }

    private void OnDisable()
    {
        InputCheckDevice.OnCheckDevice -= ShowHideText;
        WristDevice.OnMessageReceived -= HandleMessageReceivedToDevice;

        InputManager.OnControlSchemeChanged -= UpdateText;
        InputManager.OnRebindComplete -= UpdateText;
    }

    /// <summary>
    ///     Updates displayInputActionText.text to display the keybind for either the gamepad or keyboard (whichever is active) followed by a custom 
    ///     input message.
    /// </summary>
    public void UpdateText()
    {
        UpdateText("");
    }

    /// <summary>
    ///     Updates displayInputActionText.text to display the keybind for either the gamepad or keyboard (whichever is active) followed by a custom 
    ///     input message.
    /// </summary>
    /// <param name="additionalText">Additional text that can be added at the end of the message.</param>
    public void UpdateText(string additionalText)
    {
        if (InputManager.instance != null)
        {
            // Get the binding name of the control depending on whether the player is using a gamepad or keyboard & mouse
            string bindingName = InputManager.instance.Actions.FindAction(
                inputActionToBeDisplayed.name).GetBindingDisplayString(InputManager.instance.usingGamepad ? 1 : 0
            );

            displayInputActionText.text = $"[{bindingName}]: {inputMessage} {additionalText}";
        }
    }

    /// <summary>
    ///     Hides the string whenever the specified input action is started (for example, if the player is checking the device, 
    ///     this makes sure the text isn't partially blocking the device's screen).
    /// </summary>
    /// <param name="inputActive">Whether or not the input is active (most likely check device or collectable sight).</param>
    private void ShowHideText(bool inputActive)
    {
        playerCurrentlyInInput = inputActive;

        // Updates messageQueued so that if the player looks at a message, the message queued notification coroutine stops as
        // the player has looked at the message
        if (inputActive && displayInputActionCheckDevice != null)
        {
            displayInputActionCheckDevice.messageQueued = false;
        }

        // If the input is active (e.g. the player is checking the device), hide the display message by setting the alpha to 0.
        // Once the player is finished checking the device, set the alpha back to 1
        float targetAlpha = inputActive ? 0 : 1;
        StartCoroutine(CanvasGroupLerpAlpha(canvasGroup, targetAlpha, fadeTime));
    }

    /// <summary>
    ///     Lerps the canvas group alpha.
    /// </summary>
    private IEnumerator CanvasGroupLerpAlpha(CanvasGroup canvas, float targetAlpha, float duration)
    {
        // Wait a frame so that if MessageQueuedRoutine is active, it no longer is active and doesn't interfere with this routine
        yield return null;

        float lerp = 0f;
        float startAlpha = canvas.alpha;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);

            yield return null;
        }

        canvas.alpha = targetAlpha;
    }

    /// <summary>
    ///     Starts the process of displaying a message notification to the player's wrist device.
    /// </summary>
    /// <param name="time">The display duration of the message.</param>
    private void HandleMessageReceivedToDevice(float time)
    {
        // NOTE: Checking and making sure the input action matches this string is bad practice, but I'm leaving it as is since this is
        // just going to prevent the view message routine from being started in backrooms HM end scene, where the input action being
        // displayed is [F]: Camera Flash and not [R]: Check Device.
        if (displayInputActionCheckDevice != null && inputActionToBeDisplayed.name == checkDeviceInputName)
        {
            StartCoroutine(displayInputActionCheckDevice.DeviceHasMessageRoutine(time));
        }
    }
}
