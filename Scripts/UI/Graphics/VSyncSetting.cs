using Michsky.MUIP;
using UnityEngine;

public class VSyncSetting : GraphicsSetting
{
    [Header("Toggle")]
    [SerializeField] private CustomToggle toggle;

    [Header("FPS Limit")]
    [SerializeField] private GameObject[] fpsLimitObjects;

    private void Start()
    {
        toggle.toggleObject.onValueChanged.AddListener(QueueVSyncChange);
        UpdateToggle();
    }

    private void OnEnable()
    {
        // Make sure toggle UI gets updated if the player makes a change, leaves the graphics page without applying, and comes back
        if (SettingsManager.instance != null && toggle.toggleObject != null)
        {
            UpdateToggle();
        }
    }

    private void OnDestroy()
    {
        // Note: Start() and OnDestroy() is used instead of OnEnable() and OnDisable() because CustomToggle's toggle.toggleObject uses
        // GetComponent<> in Awake(), so there was a change that toggle.toggleObject was null when this script attempted to subscribe to 
        // the Unity event. By using Start() and OnDestroy(), the null reference exception issue is bypassed completely.
        if (toggle.toggleObject != null && toggle.toggleObject.onValueChanged != null)
        {
            toggle.toggleObject.onValueChanged.RemoveListener(QueueVSyncChange);
        }
    }

    /// <summary>
    ///     Apply the vsync change.
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

        SettingsManager.instance.EnableVSync(toggle.toggleObject.isOn);
    }

    /// <summary>
    ///     Check if the toggleObject.isOn is not equal to SettingsManager.instance.IsVsyncEnabled().
    /// </summary>
    /// <returns>True if they are different, false if they are the same.</returns>
    public override bool Changed()
    {
        return toggle.toggleObject.isOn != SettingsManager.instance.IsVSyncEnabled();
    }

    private void QueueVSyncChange(bool vsyncEnabled)
    {
        QueueSettingChange();
        HideFPSLimit(vsyncEnabled);
    }

    private void UpdateToggle()
    {
        toggle.toggleObject.isOn = SettingsManager.instance.IsVSyncEnabled();
        toggle.UpdateState();
        HideFPSLimit(toggle.toggleObject.isOn);
    }

    private void HideFPSLimit(bool hidden)
    {
        for (int i = 0; i < fpsLimitObjects.Length; i++)
        {
            fpsLimitObjects[i].SetActive(!hidden);
        }
    }
}
