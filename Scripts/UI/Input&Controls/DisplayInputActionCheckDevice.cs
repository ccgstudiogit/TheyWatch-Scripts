using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(DisplayInputAction))]
public class DisplayInputActionCheckDevice : MonoBehaviour
{
    [Header("Check Device Notification Settings")]
    [SerializeField] private string checkDeviceMessage = "(View Message)";

    [Tooltip("When a message is sent to the player's wrist device, the display input action text will alternate " +
        "it's starting color and this color")]
    [SerializeField] private Color messageQueuedColor = Color.red;

    private Color textBaseColor; // Cache a reference to the text's starting color
    private ColorBlock sliderBaseColorBlock; // Cache a reference to the slider's starting block colors

    [Tooltip("How quickly the text's color will change from the base color and messageQueuedColor")]
    [SerializeField, Min(0)] private float colorChangeTime = 1f;

    // Keeps track of if a device message has been sent to the device and when the player looks at the device. If a
    // message is sent, the notification goes away after the player checks the device
    public bool messageQueued { get; set; }

    private DisplayInputAction displayInputAction;

    private void Awake()
    {
        displayInputAction = GetComponent<DisplayInputAction>();

        messageQueued = false;
        textBaseColor = displayInputAction.displayInputActionText.color;
        sliderBaseColorBlock = displayInputAction.slider.colors;
    }

    /// <summary>
    ///     This coroutine handles displaying the proper notifications if a message has been sent to the player's wrist device.
    ///     It starts by updating the text and starting the MessageQueuedRoutine(), which updates the text's colors. It also
    ///     shows a display on how much time isremaining on the message. After the time is up, the text is reset back to normal.
    /// </summary>
    public IEnumerator DeviceHasMessageRoutine(float duration)
    {
        // Add the checkDeviceMessage to the end of the regular display text
        displayInputAction.UpdateText(checkDeviceMessage);

        // Don't start the changing colors routine if the player is already looking at the device
        if (!displayInputAction.playerCurrentlyInInput)
        {
            StartCoroutine(MessageQueuedRoutine(textBaseColor, sliderBaseColorBlock, messageQueuedColor, colorChangeTime));
        }

        // Setup slider
        displayInputAction.slider.value = 1;
        displayInputAction.slider.gameObject.SetActive(true);

        float lerp = 0f;

        // Update the slider's value based on the duration of the message
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            displayInputAction.slider.value = Mathf.Lerp(1, 0, lerp);

            yield return null;
        }

        messageQueued = false; // Make sure the text's color doesn't keep changing forever if the player never checks the device
        displayInputAction.slider.gameObject.SetActive(false);

        // Change the text back to what it's regular display message is supposed to be
        displayInputAction.UpdateText();
    }

    /// <summary>
    ///     This coroutine changes the text's color from the base color to a specified color if a message has been sent to the player's
    ///     wrist device. Once the player checks the device, the text's color stops changing and goes back to the base color.
    /// </summary>
    private IEnumerator MessageQueuedRoutine(Color textBaseColor, ColorBlock sliderBaseColor, Color changeColor, float duration)
    {
        bool changingToBaseColor = false;
        messageQueued = true;

        // Change colors only while the message is queued
        while (messageQueued)
        {
            float lerp = 0f;

            // Text color
            Color textStartingColor = displayInputAction.displayInputActionText.color;
            Color textDesiredColor = changingToBaseColor ? textBaseColor : changeColor;

            // Slider color
            Color sliderStartingColor = displayInputAction.slider.colors.disabledColor;
            Color sliderDesiredColor = changingToBaseColor ? sliderBaseColor.disabledColor : changeColor;

            while (lerp < 1)
            {
                // Break out of the loop once the player checks the device
                if (!messageQueued)
                {
                    break;
                }

                lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
                displayInputAction.displayInputActionText.color = Color.Lerp(textStartingColor, textDesiredColor, lerp); // Text color

                // Slider colors
                ColorBlock cb = displayInputAction.slider.colors;
                cb.disabledColor = Color.Lerp(sliderStartingColor, sliderDesiredColor, lerp);
                displayInputAction.slider.colors = cb;

                yield return null;
            }

            changingToBaseColor = !changingToBaseColor;
        }

        // After the player checks the device, change the colors back to normal and stop the color changing
        displayInputAction.displayInputActionText.color = textBaseColor;
        displayInputAction.slider.colors = sliderBaseColor;
    }
}
