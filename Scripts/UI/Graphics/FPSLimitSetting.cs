using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;

public class FPSLimitSetting : GraphicsSetting
{
    [Header("Toggle")]
    [SerializeField] private CustomToggle toggle;

    [Header("Slider")]
    [SerializeField] private Slider slider;

    private void Start()
    {
        toggle.toggleObject.onValueChanged.AddListener(QueueFPSLimitChange);
        slider.onValueChanged.AddListener(QueueFPSLimitChange);

        // Apply the FPS min/max limits to the slider
        slider.minValue = SettingsManager.instance.minFPSLimit;
        slider.maxValue = SettingsManager.instance.maxFPSLimit;

        UpdateUI();
    }

    private void OnEnable()
    {
        if (SettingsManager.instance != null && toggle.toggleObject != null)
        {
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        // Note: Start() and OnDestroy() is used instead of OnEnable() and OnDisable() because CustomToggle's toggle.toggleObject uses
        // GetComponent<> in Awake(), so there was a change that toggle.toggleObject was null when this script attempted to subscribe to 
        // the Unity event. By using Start() and OnDestroy(), the null reference exception issue is bypassed completely.
        if (toggle.toggleObject != null && toggle.toggleObject.onValueChanged != null)
        {
            toggle.toggleObject.onValueChanged.RemoveListener(QueueFPSLimitChange);
        }

        slider.onValueChanged.RemoveListener(QueueFPSLimitChange);
    }

    /// <summary>
    ///     Apply the FPS limit change(s).
    /// </summary>
    public override void Apply()
    {
        if (!Changed())
        {
#if UNITY_EDITOR
            LogApplyNoChangeWarning();
#endif
            return;
        }

        if (FPSLimitToggleChange())
        {
            SettingsManager.instance.EnableFPSLimit(toggle.toggleObject.isOn);
        }

        // Checking if the toggle is on prevents an issue where the fps limit is set even if fps limit is turned off. Like this:
        // FPS limit currently on -> change FPS limit -> turn FPS limit off -> fps limit SHOULD be off but since there was a slider
        // change, an FPS limit would still be set (even though the FPS limit toggle was off)
        if (FPSLimitSliderChange() && toggle.toggleObject.isOn)
        {
            SettingsManager.instance.SetFPSLimit((int)slider.value);
        }
    }

    /// <summary>
    ///     Check if the limit toggle or fps limit slider changed compared to SettingsManager's saved settings.
    /// </summary>
    /// <returns>True if either one is changed, false if neither have changed.</returns>
    public override bool Changed()
    {
        return FPSLimitToggleChange() || FPSLimitSliderChange();
    }

    private void QueueFPSLimitChange(bool enabled)
    {
        QueueSettingChange();
        slider.gameObject.SetActive(enabled);
    }

    private void QueueFPSLimitChange(float value)
    {
        QueueSettingChange();
    }

    private bool FPSLimitToggleChange()
    {
        return toggle.toggleObject.isOn != SettingsManager.instance.IsFPSLimitEnabled();
    }

    private bool FPSLimitSliderChange()
    {
        return (int)slider.value != SettingsManager.instance.GetCurrentFPSLimit();
    }

    private void UpdateUI()
    {
        toggle.toggleObject.isOn = SettingsManager.instance.IsFPSLimitEnabled();
        toggle.UpdateState();

        slider.value = SettingsManager.instance.GetCurrentFPSLimit();
        slider.gameObject.SetActive(toggle.toggleObject.isOn); // Turn the FPS slider on only if the user wants to apply an FPS limit
    }
}
