using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DisplayInputAction))]
public class DisplayInputActionCooldown : MonoBehaviour
{
    private DisplayInputAction displayInputAction;

    private void Awake()
    {
        displayInputAction = GetComponent<DisplayInputAction>();
    }

    private void OnEnable()
    {
        HandheldCamera.OnPictureTaken += HandleCooldown;

        EMPDevice.OnEMP += HandleCooldown;
    }

    private void OnDisable()
    {
        HandheldCamera.OnPictureTaken -= HandleCooldown;

        EMPDevice.OnEMP -= HandleCooldown;
    }

    /// <summary>
    ///     Display how much time is left on a cooldown (camera flash, emp device) via displayInputAction.slider
    /// </summary>
    /// <param name="duration">The duration of the cooldown.</param>
    private void HandleCooldown(float duration)
    {
        StartCoroutine(LerpSlider(duration));
    }

    /// <summary>
    ///     This coroutine handles lerping the slider value from 1 to 0 to display how much time is left on the cooldown.
    /// </summary>
    /// <param name="duration">The duration of the cooldown.</param>
    private IEnumerator LerpSlider(float duration)
    {
        float lerp = 0f;

        // Setup slider
        displayInputAction.slider.value = 1;
        displayInputAction.slider.gameObject.SetActive(true);

        // Update the slider's value based on the duration of the flash cooldown
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            displayInputAction.slider.value = Mathf.Lerp(1, 0, lerp);

            yield return null;
        }

        displayInputAction.slider.gameObject.SetActive(false);
    }
}
