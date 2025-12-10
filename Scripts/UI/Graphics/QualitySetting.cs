using Michsky.MUIP;
using UnityEngine;

public class QualitySetting : GraphicsSetting
{
    [SerializeField] private HorizontalSelector qualitySelector;

    private bool initalized = false;

    private void Start()
    {
        initalized = true;
        UpdateIndex();
    }

    private void OnEnable()
    {
        qualitySelector.onValueChanged.AddListener(QueueQualityChange);

        // Saves an unnecessary UpdateIndex() call when starting up for the first time
        if (initalized)
        {
            UpdateIndex();
        }
    }

    private void OnDisable()
    {
        qualitySelector.onValueChanged.RemoveListener(QueueQualityChange);
    }

    /// <summary>
    ///     Apply the quality changes.
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

        SettingsManager.instance.SetQuality((Quality)qualitySelector.index);
    }

    /// <summary>
    ///     Check if qualitySelector's index is not equal to the game's current quality.
    /// </summary>
    /// <returns>True if they are different, false if they are the same.</returns>
    public override bool Changed()
    {
        return (Quality)qualitySelector.index != SettingsManager.instance.GetCurrentQuality();
    }

    private void QueueQualityChange(int index)
    {
        QueueSettingChange();
    }

    private void UpdateIndex()
    {
        qualitySelector.index = (int)SettingsManager.instance.GetCurrentQuality();
        qualitySelector.UpdateUI();
    }
}
