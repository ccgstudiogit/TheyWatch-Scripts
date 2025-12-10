using UnityEngine;

public class WindowModeSetting : GraphicsSetting
{
    [Header("Buttons")]
    [SerializeField] private ButtonController fullscreenButton;
    [SerializeField] private ButtonController windowedButton;

    private SelectedButtonController selectedButtonController = null;
    private bool selectedButtonControllerInitialized = false;

    private WindowMode currentMode;

    private void Start()
    {
        if (selectedButtonController == null)
        {
            selectedButtonController = new SelectedButtonController();

            selectedButtonController.Add(fullscreenButton);
            selectedButtonController.Add(windowedButton);

            selectedButtonControllerInitialized = true;
        }

        UpdateSelectedButton();
    }

    private void OnEnable()
    {
        if (SettingsManager.instance != null)
        {
            currentMode = SettingsManager.instance.GetWindowMode();
            UpdateSelectedButton();
        }
    }

    /// <summary>
    ///     Checks if this WindowModeSettings.cs' currentMode is different than SettingsManager.instance.GetWindowMode()'s mode.
    /// </summary>
    /// <returns>True if different, false if they are the same.</returns>
    public override bool Changed()
    {
        return currentMode != SettingsManager.instance.GetWindowMode();
    }

    /// <summary>
    ///     Apply the change in window mode via SettingsManager.
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

        SettingsManager.instance.SetWindowMode(currentMode);
    }

    /// <summary>
    ///     Change WindowModeSettings.cs' current window mode (currentMode). This does not actively change the mode, just sets the
    ///     currentMode to the newMode--to actually change the mode, WindowModeSetting.Apply() must be used.
    /// </summary>
    /// <param name="newMode">Sets the new WindowMode to be used via an int (0 = Fullscreen, 1 = Windowed).</param>
    public void ChangeMode(int newMode)
    {
        currentMode = (WindowMode)newMode;

        // Update the selected button
        UpdateSelectedButton();

        // Add the change to GraphicsSettingsController's queue
        QueueSettingChange();
    }

    private void UpdateSelectedButton()
    {
        // Make sure that selectedButtonController is created and the buttons are added before calling SetSelectedButton(). The reason
        // selectedButtonController is created in Start() and not Awake() is that there is a bug in that if the SelectButton.cs' Awake()
        // is called after this script's Awake() and OnEnable(), the colors would become default colors with an alpha of 0 and not the
        // actual text/image colors, resulting in the buttons not showing up at all unless hovered over.
        if (!selectedButtonControllerInitialized)
        {
            return;
        }

        selectedButtonController.SetSelectedButton(currentMode == WindowMode.Fullscreen ? fullscreenButton : windowedButton);
    }
}
