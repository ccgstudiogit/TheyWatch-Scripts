using Michsky.MUIP;
using UnityEngine;

public class PixelationSetting : GraphicsSetting
{
    [SerializeField] private HorizontalSelector pixelationSelector;

    private bool initalized = false;

    private void Start()
    {
        initalized = true;
        UpdateIndex();
    }

    private void OnEnable()
    {
        pixelationSelector.onValueChanged.AddListener(QueuePixelationChange);

        // Saves an unnecessary UpdateIndex() call when starting up for the first time
        if (initalized)
        {
            UpdateIndex();
        }
    }

    private void OnDisable()
    {
        pixelationSelector.onValueChanged.RemoveListener(QueuePixelationChange);
    }

    /// <summary>
    ///     Apply the pixelation changes.
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

        SettingsManager.instance.SetPixelation((Pixelation)pixelationSelector.index);
    }

    /// <summary>
    ///     Check if pixelationSelector's index is not equal to the game's current pixelation setting.
    /// </summary>
    /// <returns>True if they are different, false if they are the same.</returns>
    public override bool Changed()
    {
        return (Pixelation)pixelationSelector.index != SettingsManager.instance.GetCurrentPixelation();
    }

    private void QueuePixelationChange(int index)
    {
        QueueSettingChange();
    }

    private void UpdateIndex()
    {
        pixelationSelector.index = (int)SettingsManager.instance.GetCurrentPixelation();
        pixelationSelector.UpdateUI();
    }
}
