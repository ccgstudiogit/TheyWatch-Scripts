using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GamepadUISensitivitySlider : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Set the min/max values of the slider to match the min/max cursor speeds in GamepadCursor
        slider.minValue = GamepadCursor.instance.minCursorSpeed;
        slider.maxValue = GamepadCursor.instance.maxCursorSpeed;

        slider.value = GamepadCursor.instance.GetCursorSpeed();
        slider.onValueChanged.AddListener(SetGamepadCursorSpeed);
    }

    private void OnDestroy()
    {
        if (slider != null && slider.onValueChanged != null)
        {
            slider.onValueChanged.RemoveListener(SetGamepadCursorSpeed);
        }
    }

    private void SetGamepadCursorSpeed(float speed)
    {
        GamepadCursor.instance.SetCursorSpeed(speed);
    }
}
