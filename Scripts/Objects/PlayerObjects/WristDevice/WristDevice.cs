using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WristDevice : MonoBehaviour
{
    // This is to let DisplayInputAction.cs know that a message has been received on the wrist device (float sends how long
    // the message will be displayed on the device for)
    public static event Action<float> OnMessageReceived;

    [Header("- Disable Settings -")]
    [SerializeField] private GameObject staticScreen;
    [field: SerializeField] public bool deviceDisabled { get; private set; } // If the device is disabled, the staticScreen will be set active

    [Header("- Display Incoming Messages -")]
    [Header("References")]
    [Tooltip("This game object is turned off/on depending on if there are messages being sent to the device")]
    [SerializeField] private GameObject messages;
    [Tooltip("This rect is used for the animation to change the screen from radar to messages")]
    [SerializeField] private RectTransform messagesBackgroundRect;
    [SerializeField] private TextMeshProUGUI messagesText;
    // Whenever a new message is displayed, the startingColor will be used to make sure that any previous color overrides do
    // not stick around
    private Color startingColor;
    [Tooltip("This slider is used to display how much time is left for displaying the message")]
    [SerializeField] private Slider slider;

    [Header("Animation")]
    [Tooltip("How long the message animation length should be in seconds")]
    [SerializeField, Min(0)] private float messageAnimationTime = 0.19f;
    [Tooltip("The starting/ending size of the messages background when an animation begins and then ends")]
    [SerializeField] private Vector2 messagesStartingEndingSize = new Vector2(0.2f, 0.2f);

    [Header("Settings")]
    [Tooltip("How long the display message should last on the device. Note: This can be overridden by LevelController's " +
        "SendMessageToWristDevice(string message, float durationOverride = -1)")]
    [SerializeField] private float displayMessageTime = 7f;

    [Header("- Sound Effects -")]
    [Tooltip("The audio source that handles playing one-and-done SFX")]
    [SerializeField] private AudioSource sfxOneShotAudioSource;

    [Tooltip("If enabled, if the wrist device receives a message whilst the player is already looking at " +
        "it, no vibrate SFX will be played")]
    [SerializeField] private bool disableVibrateWhenLookingAtDevice = true;
    [SerializeField] private SoundEffectSO vibrateSFX;

    [Tooltip("The sound that will be played whenever the player checks the device")]
    [SerializeField] private SoundEffectSO deviceOnSFX;
    [Tooltip("The sound that will be played whenever the player stops looking at the device")]
    [SerializeField] private SoundEffectSO deviceOffSFX;

    // Currently plays in Hedge Maze HM when the player fails to follow an instruction
    [SerializeField] private SoundEffectSO deviceErrorSFX;

    private Stunnable stunnable;

    // Prevents a bug when using a gamepad as the deviceOnSFX would play both times (both initially checking and
    // no longer checking device) as using a trigger on a gamepad would call CheckingDevice() twice on cancel, with
    // the first call currentlyCheckingDevice would be true and the second call would have currentlyCheckingDevice be
    // false, which created an unnecessary deviceOnSFX when no longer looking at the device
    private bool previousCheckDeviceFlag;

    // Makes sure only one message is displayed at a time
    private Coroutine displayMessageRoutine = null;

    // Used to stop incoming message SFX if the player is looking at the device
    public bool playerLookingAtDevice { get; private set; } = false;

    // Lets Radar.cs know to not play radar SFX when a message is being displayed
    public bool messageIsDisplayed { get; private set; } = false;

    private void Awake()
    {
        stunnable = GetComponent<Stunnable>();

        messages.SetActive(false);
        startingColor = messagesText.color;
    }

    private void Start()
    {
        if (deviceDisabled)
        {
            DisableDevice();
        }
    }

    private void OnEnable()
    {
        LevelController.OnSendMessageToWristDevice += HandleDisplayMessage;
        LevelController.OnChangeWristDeviceMessage += ChangeMessageText;
        LevelController.OnChangeWristDeviceMessageColor += ChangeMessageTextColor;
        LevelController.OnDisableWristDevice += HandleDisablingOfDevice;

        InputCheckDevice.OnCheckDevice += CheckingDevice;

        // Makes sure the device is not re-enabled in Factory's hardmode since the device should always be disabled
        if (!(LevelController.instance is FactoryHMLevelController) && stunnable != null)
        {
            stunnable.OnStunned += DisableDevice;
            stunnable.OnStunEnd += EnableDevice;
        }
    }

    private void OnDisable()
    {
        LevelController.OnSendMessageToWristDevice -= HandleDisplayMessage;
        LevelController.OnChangeWristDeviceMessage -= ChangeMessageText;
        LevelController.OnChangeWristDeviceMessageColor -= ChangeMessageTextColor;
        LevelController.OnDisableWristDevice -= HandleDisablingOfDevice;

        InputCheckDevice.OnCheckDevice -= CheckingDevice;

        if (!(LevelController.instance is FactoryHMLevelController) && stunnable != null)
        {
            stunnable.OnStunned -= DisableDevice;
            stunnable.OnStunEnd -= EnableDevice;
        }
    }

    private void HandleDisplayMessage(string message, float durationOverride)
    {
        // Don't bother displaying messages if the device is disabled
        if (deviceDisabled)
        {
            return;
        }

        if (displayMessageRoutine != null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} received a message but a message is already being displayed!");
#endif
            return;
        }

        float duration = durationOverride > 0.1f ? durationOverride : displayMessageTime;

        PlayVibrateSFX();

        // Make sure a notification message displays with DisplayInputAction.cs
        OnMessageReceived?.Invoke(duration);
        messagesText.text = message;
        ResetMessageTextColor(); // Make sure the color is the default

        displayMessageRoutine = StartCoroutine(DisplayMessageRoutine(duration));
    }

    private void HandleDisablingOfDevice(bool shouldDisable)
    {
        if (shouldDisable && !deviceDisabled)
        {
            DisableDevice();
        }
        else if (!shouldDisable && deviceDisabled)
        {
            EnableDevice();
        }
    }

    private IEnumerator DisplayMessageRoutine(float totalMessageDuration)
    {
        messageIsDisplayed = true;
        slider.value = 1;

        messagesText.gameObject.SetActive(false); // Make sure text doesn't appear until after the animation ends
        messages.SetActive(true);

        // Turn the message screen on with the animation
        yield return StartCoroutine(TurnOnScreenAnimation(messagesBackgroundRect, messagesStartingEndingSize, messageAnimationTime));

        // After the animation finishes, display the text
        messagesText.gameObject.SetActive(true);

        // Display the message for the specified amount of time (displayMessageLength) and update slider
        float lerp = 0f;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / totalMessageDuration);
            slider.value = Mathf.Lerp(1, 0, lerp);

            yield return null;
        }

        // Turn off messages text before starting the animation
        messagesText.gameObject.SetActive(false);

        // Turn the message screen off with the animation
        yield return StartCoroutine(TurnOffScreenAnimation(messagesBackgroundRect, messagesStartingEndingSize, messageAnimationTime));

        messages.SetActive(false);
        displayMessageRoutine = null;

        messageIsDisplayed = false;
    }

    private IEnumerator TurnOnScreenAnimation(RectTransform rect, Vector2 startingEndingSize, float duration)
    {
        // Set the starting size
        rect.localScale = startingEndingSize;

        float lerp = 0f;

        // Animate the x-scale first
        while (lerp < 1)
        {
            // duration is divided by 2 since there will be 2 separate animations, this makes sure the entire animation fits within
            // the specified duration window
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / (duration / 2));
            rect.localScale = new Vector2(Mathf.Lerp(startingEndingSize.x, 1, lerp), rect.localScale.y);

            yield return null;
        }

        rect.localScale = new Vector2(1, rect.localScale.y);
        lerp = 0f;

        // Animate the y-scale
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / (duration / 2));
            rect.localScale = new Vector2(rect.localScale.x, Mathf.Lerp(startingEndingSize.y, 1, lerp));

            yield return null;
        }

        rect.localScale = new Vector2(1, 1);
    }

    private IEnumerator TurnOffScreenAnimation(RectTransform rect, Vector2 startingEndingSize, float duration)
    {
        Vector2 startingSize = new Vector2(rect.localScale.x, rect.localScale.y);
        float lerp = 0f;

        // Animate the y-scale first
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / (duration / 2));
            rect.localScale = new Vector2(rect.localScale.x, Mathf.Lerp(startingSize.y, startingEndingSize.y, lerp));

            yield return null;
        }

        rect.localScale = new Vector2(rect.localScale.x, startingEndingSize.y);
        lerp = 0f;

        // Animate the x-scale
        while (lerp < 1)
        {
            // duration is divided by 2 since there will be 2 separate animations, this makes sure the entire animation fits within
            // the specified duration window
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / (duration / 2));
            rect.localScale = new Vector2(Mathf.Lerp(startingSize.x, startingEndingSize.x, lerp), rect.localScale.y);

            yield return null;
        }

        rect.localScale = startingEndingSize;
    }

    private void DisableDevice()
    {
        deviceDisabled = true;
        staticScreen.SetActive(true);
    }

    private void EnableDevice()
    {
        deviceDisabled = false;
        staticScreen.SetActive(false);
    }

    private void CheckingDevice(bool currentlyCheckingDevice)
    {
        playerLookingAtDevice = currentlyCheckingDevice;

        if (currentlyCheckingDevice)
        {
            // Prevents a bug: make sure the deviceOnSFX does not play when no longer checking the device. This issue
            // only happens when the user is using a trigger on a gamepad
            if (previousCheckDeviceFlag == false && deviceOnSFX != null)
            {
                deviceOnSFX.PlayOneShot(sfxOneShotAudioSource);
            }

            previousCheckDeviceFlag = true;
        }
        else
        {
            if (deviceOffSFX != null)
            {
                deviceOffSFX.PlayOneShot(sfxOneShotAudioSource);
            }
            
            previousCheckDeviceFlag = false;
        }
    }

    /// <summary>
    ///     Plays the vibrate SFX if the player is not currently checking the device.
    /// </summary>
    private void PlayVibrateSFX()
    {
        if (deviceDisabled)
        {
            return;
        }
        
        if (disableVibrateWhenLookingAtDevice && !playerLookingAtDevice && vibrateSFX != null)
        {
            vibrateSFX.PlayOneShot(sfxOneShotAudioSource);
        }
        else if (!disableVibrateWhenLookingAtDevice && vibrateSFX != null)
        {
            vibrateSFX.PlayOneShot(sfxOneShotAudioSource);
        }
    }

    /// <summary>
    ///     Updates the message text if a message is currently being shown. Also plays the device error SFX (currently this
    ///     is setup the way it is so that if the player fails an instruction in Hedge Maze HM, an error SFX will play).
    /// </summary>
    /// <param name="message">The updated message.</param>
    private void ChangeMessageText(string message)
    {
        PlayDeviceErrorSFX();
        messagesText.text = message;
    }

    /// <summary>
    ///     Changes the message text color. Note: this change is not permanent as every time a new message is received the
    ///     color is reset back to its starting color via ResetMessageTextColor() so this is only useful to change the color
    ///     of the current message.
    /// </summary>
    /// <param name="color">The new message text color.</param>
    private void ChangeMessageTextColor(Color color)
    {
        messagesText.color = color;
    }

    /// <summary>
    ///     Changes the messagesText.color back to its starting color.
    /// </summary>
    private void ResetMessageTextColor()
    {
        messagesText.color = startingColor;
    }

    /// <summary>
    ///     Plays the device error sfx with sfxOneShotAudioSource.
    /// </summary>
    private void PlayDeviceErrorSFX()
    {
        if (deviceErrorSFX != null)
        {
            deviceErrorSFX.PlayOneShot(sfxOneShotAudioSource);
        }
    }
}
