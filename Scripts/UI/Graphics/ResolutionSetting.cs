using Michsky.MUIP;
using UnityEngine;

public class ResolutionSetting : GraphicsSetting
{
    [Header("References")]
    [SerializeField] private HorizontalSelector resolutionSelector;
    [Tooltip("In the event that there are no resolution options, this reference is used to disable the text.")]
    [SerializeField] private GameObject resolutionsText;

    [Header("Selector Settings")]
    [SerializeField] private GameObject indicators;
    [Tooltip("If the indicators go beyond this amount, the indicators will be disabled")]
    [SerializeField] private int maxIndicators = 30;

    private bool initalized = false;

    private void Start()
    {
        CreateResolutionList();

        // Set the resolutionSelector's current index to the current resolution
        UpdateIndex();
        initalized = true;
    }

    private void OnEnable()
    {
        resolutionSelector.onValueChanged.AddListener(QueueResolutionChange);

        // Doing this in both Start() and OnEnable() is necessary to make sure that the correct index is chosen each time
        // on startup and whenever the graphics menu is enabled
        if (initalized)
        {
            UpdateIndex();
        }
    }

    private void OnDisable()
    {
        resolutionSelector.onValueChanged.RemoveListener(QueueResolutionChange);
    }

    /// <summary>
    ///     Apply the resolution changes.
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

        SettingsManager.instance.SetResolution(resolutionSelector.index);
    }

    /// <summary>
    ///     Check if the resolutionSelector's index is not equal to the SettingsManager's currentResolutionIndex.
    /// </summary>
    /// <returns>True if they are different, false if they are the same.</returns>
    public override bool Changed()
    {
        return resolutionSelector.index != SettingsManager.instance.currentResolutionIndex;
    }

    private void QueueResolutionChange(int index)
    {
        QueueSettingChange();
    }

    /// <summary>
    ///     Add the available resolutions to the resolution selector.
    /// </summary>
    private void CreateResolutionList()
    {
        // This is mainly here for editing purposes, as I found I have issues when I have the debug console in another window because
        // SettingsManager is trying to get resolutions that match the screen's refresh rate but couldn't find any (since my monitors have
        // different refresh rates)
        if (SettingsManager.instance.resolutionOptions.Count < 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning("SettingsManager.instance.resolutionOptions.Count is less than 1. Disabling ResolutionSettings.cs");
#endif
            enabled = false;
            gameObject.SetActive(false);
            resolutionsText?.SetActive(false);
            return;
        }

        // Get the resolutions and create an item for each resolution
        resolutionSelector.items.Clear();
        for (int i = 0; i < SettingsManager.instance.resolutionOptions.Count; i++)
        {
            resolutionSelector.CreateNewItem(SettingsManager.instance.resolutionOptions[i]);
        }

        // Check if there are too many items to display indicators
        if (indicators != null && resolutionSelector.items.Count > maxIndicators)
        {
            indicators.SetActive(false);
            resolutionSelector.enableIndicators = false;
        }
    }

    private void UpdateIndex()
    {
        resolutionSelector.index = SettingsManager.instance.currentResolutionIndex;
        resolutionSelector.UpdateUI();
    }
}
