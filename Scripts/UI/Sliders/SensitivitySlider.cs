using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SensitivitySlider : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        slider.minValue = InputManager.instance.minSensitivity;
        slider.maxValue = InputManager.instance.maxSensitivity;
        slider.value = InputManager.instance.GetCurrentSensitivity();
        
        slider.onValueChanged.AddListener(SetSensitivity);
    }

    private void OnDestroy()
    {
        if (slider != null && slider.onValueChanged != null)
        {
            slider.onValueChanged.RemoveListener(SetSensitivity);
        }
    }

    private void SetSensitivity(float value)
    {
        InputManager.instance.SetSensitivity(value);
    }
}
