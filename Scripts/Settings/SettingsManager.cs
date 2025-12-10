using System;
using System.Collections.Generic;
using UnityEngine;

// For PlayerPrefs: 0 == Fullscreen, 1 == Windowed
[Serializable]
public enum WindowMode
{
    Fullscreen,
    Windowed
}

[Serializable]
public enum Quality
{
    Low,
    Medium,
    High,
    Ultra
}

[Serializable]
public enum Pixelation
{
    High, // Default (I made the game with pixelation in mind, but want to include the setting for anyone who doesn't like it)
    Low
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    private Resolution[] resolutions;
    private double screenRefreshRate;

    // A list of the user's resolution options as strings (to display the options in the ui)
    public List<string> resolutionOptions { get; private set; } = new List<string>();

    // The filtered resolutions list contains the resolutions that match the screen's refresh rate
    private List<Resolution> filteredResolutions = new List<Resolution>();
    public int currentResolutionIndex { get; private set; }

    [Header("Render Texture")]
    [Tooltip("The render texture that is used to render the game")]
    [SerializeField] private RenderTexture renderTexture;

    [Tooltip("The aspect ratio will by multiplied by this amount, which will be the new resolution (lower = lower resolution)")]
    [SerializeField] private int highPixelationAspectRatioMultiplier = 20;
    [Tooltip("The aspect ratio will by multiplied by this amount, which will be the new resolution (higher = higher resolution)")]
    [SerializeField] private int lowPixelationAspectRatioMultiplier = 40;

    // Makes sure the pixelation aspect ratio multiplier multiplies by a minimum resolution to prevent small aspect
    // ratios, such as 3:2 or 4:3, from becoming tiny resolutions like 60x40 and 80x60
    private Vector2Int minAspectRatio = new Vector2Int(16, 9);

    [Header("FPS Limit")]
    [field: SerializeField] public int minFPSLimit { get; private set; } = 30;
    [field: SerializeField] public int maxFPSLimit { get; private set; } = 240;

    // PlayerPrefs graphics setting variables
    private const string windowModeStr = "windowMode";
    private WindowMode defaultWindowMode = WindowMode.Fullscreen;

    private const string qualityStr = "quality";
    private Quality defaultQuality = Quality.Ultra;

    private const string pixelationStr = "pixelation";
    private Pixelation defaultPixelation = Pixelation.High;

    private const string vsyncStr = "vsync";
    private int defaultVsync = 0; // Default is vsync off

    private const string shouldLimitFpsStr = "shouldLimitFps";
    private int shouldLimitFPSDefault = 1; // Default is FPS Limit on (1 == fps limit on, 0 == fps limit off)
    private const string fpsLimitStr = "fpsLimit";
    private int defaultFpsLimit = 240; // This should be maxFPSLimit

    // PlayerPrefs gameplay settings variables
    private const string deviceTipsStr = "deviceTips";
    private int defaultDeviceTips = 1; // 1 == on, 0 == off

    private const string cameraShakeStr = "cameraShake";
    private int defaultCameraShake = 1; // 1 == on, 0 == off

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            screenRefreshRate = Screen.currentResolution.refreshRateRatio.value;
            GetFilteredResolutions(screenRefreshRate);

            // Load the chosen qualities/settings (or default if loading in for the first time)
            LoadQualitySetting();
            SetPixelation(GetCurrentPixelation());
            EnableVSync(IsVSyncEnabled());

            if (!IsVSyncEnabled() && IsFPSLimitEnabled())
            {
                SetFPSLimit(GetCurrentFPSLimit());
            }

            // Load the chosen gameplay settings
            EnableDeviceTips(AreDeviceTipsEnabled());
            EnableCameraShake(IsCameraShakeEnabled());
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        if (renderTexture == null)
        {
            Debug.LogWarning($"{name}'s renderTexture reference null, unable to change pixelation setting.");
        }
#endif
    }

    /// <summary>
    ///     Sets the game's window mode to the desired mode.
    /// </summary>
    /// <param name="mode">The WindowMode that the game should be set to.</param>
    public void SetWindowMode(WindowMode mode)
    {
        Screen.fullScreen = mode == WindowMode.Fullscreen;
        PlayerPrefs.SetInt(windowModeStr, (int)mode); // 0 == Fullscreen, 1 == Windowed
    }

    /// <summary>
    ///     Get the game's current window mode.
    /// </summary>
    /// <returns>WindowMode.</returns>
    public WindowMode GetWindowMode()
    {
        return (WindowMode)PlayerPrefs.GetInt(windowModeStr, (int)defaultWindowMode);
    }

    /// <summary>
    ///     Gets the screens current resolutions, filters them by refresh rates, and adds them to the filteredResoltions and
    ///     resolutionOptions lists.
    /// </summary>
    /// <param name="refreshRate">The refresh rate needed to add the resolutions to the lists.</param>
    private void GetFilteredResolutions(double refreshRate)
    {
        resolutions = Screen.resolutions;

        // Get the filtered resolutions
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].refreshRateRatio.value == refreshRate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        // Add the filtered resolutions to resolutionOptions and assign the currentResolutionIndex based on the current resolution
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            resolutionOptions.Add(option);

            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
    }

    /// <summary>
    ///     Sets the screen's resolution to the desired resolution via an index.
    /// </summary>
    public void SetResolution(int index)
    {
        if (index >= filteredResolutions.Count)
        {
#if UNITY_EDITOR
            Debug.LogError($"Invalid index {index} in SettingsManager SetResolution().");
#endif
            return;
        }

        Screen.SetResolution(
            filteredResolutions[index].width,
            filteredResolutions[index].height,
            GetWindowMode() == WindowMode.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed
        );

        currentResolutionIndex = index;
        this.InvokeAfterFrames(() => GetNewRenderTexture(GetCurrentPixelation()), 1); // Wait one frame to make sure screen resolution is set
    }

    /// <summary>
    ///     Load whatever quality setting is currently in PlayerPrefs.
    /// </summary>
    private void LoadQualitySetting()
    {
        SetQuality((Quality)PlayerPrefs.GetInt(qualityStr, (int)defaultQuality));
    }

    /// <summary>
    ///     Set the game's quality to a new quality.
    /// </summary>
    /// <param name="quality">The quality the game should be set to.</param>
    public void SetQuality(Quality quality)
    {
        QualitySettings.SetQualityLevel((int)quality);

        // Re-apply vsync and FPS limit after changing quality (prevents bugs from occuring with vsync/fps limits not working
        // when changing quality)
        EnableVSync(IsVSyncEnabled());
        if (!IsVSyncEnabled()) // If vsync is not enabled, re-apply fps limit settings
        {
            if (IsFPSLimitEnabled())
            {
                SetFPSLimit(GetCurrentFPSLimit());
            }
            else
            {
                DisableTargetFramerate();
            }
        }

        PlayerPrefs.SetInt(qualityStr, (int)quality);
    }

    /// <summary>
    ///     Gets the game's current quality setting.
    /// </summary>
    /// <returns>Quality.</returns>
    public Quality GetCurrentQuality()
    {
        return (Quality)PlayerPrefs.GetInt(qualityStr, (int)defaultQuality);
    }

    /// <summary>
    ///     Set the game's pixelation to a new setting.
    /// </summary>
    /// <param name="pixelation">The pixelation that the game should render with.</param>
    public void SetPixelation(Pixelation pixelation)
    {
        PlayerPrefs.SetInt(pixelationStr, (int)pixelation);
        GetNewRenderTexture(pixelation);
    }

    /// <summary>
    ///     Resize the render texture based on the screen's resolution and current pixelation setting.
    /// </summary>
    /// <param name="pixelation"></param>
    private void GetNewRenderTexture(Pixelation pixelation)
    {
        Vector2Int baseAspectRatio = GetAspectRatio(Screen.width, Screen.height);
        Vector2Int targetAspectRatio = baseAspectRatio;

        // Make sure the target aspect ratio is scaled up to the minimum set in the inspector (most likely 16:9). By scaling the target
        // aspect ratio up, this prevents lower aspect ratios, such as 3:2 and 4:3, from being way too pixelated, since the pixelation
        // would multiply the aspect ratio by 10, resulting in an extremely low resolution of 60x40 and 80x60. Scaling it up to be around
        // 16:9 prevents the extremely low resolution
        for (int attempts = 0, maxAttempts = 15; attempts <= maxAttempts; attempts++)
        {
            if (targetAspectRatio.x >= minAspectRatio.x && targetAspectRatio.y >= minAspectRatio.y)
            {
                break;
            }

            targetAspectRatio += baseAspectRatio;
        }

        Vector2Int pixelatedAspectRatio = GetRenderTextureResolution(pixelation, targetAspectRatio);
        ResizeRenderTexture(renderTexture, pixelatedAspectRatio.x, pixelatedAspectRatio.y);
        RefreshCameraAspect();
    }

    /// <summary>
    ///     Get the aspect ratio of a resolution (like 16:9, 21:9, 3:2, etc.). If needed, the aspect ratio is also rounded
    ///     down to the nearest ratio, for example if the resolution is 1,400x500 (14:5, 2.8), it is treated as 21:9 (2.333...).
    /// </summary>
    /// <param name="width">The resolution's width.</param>
    /// <param name="height">The resolution's height.</param>
    /// <returns>A Vector2Int containing the aspect ratio.</returns>
    private Vector2Int GetAspectRatio(int width, int height)
    {
        // tolerance makes sure that if an aspect ratio is slightly smaller than closest ratio, the closest ratio is chosen rather than
        // it being scaled down (e.g. if the aspect ratio is 1.76655, it's still considered 16:9 rather than going down to 16:10)
        const float tolerance = 0.05f;
        float aspectRatio = (float)width / height;

        // Make sure aspect ratios are in descending order (biggest at top, smallest at bottom)
        Vector2Int[] commonRatios = new Vector2Int[]
        {
            new Vector2Int(32, 9), // aspectRatio == 3.555...
            new Vector2Int(21, 9), // aspectRatio == 2.333...
            new Vector2Int(32, 15), // aspectRatio == 2.1333...
            new Vector2Int(16, 9), // aspectRatio == 1.777...
            new Vector2Int(16, 10), // aspectRatio == 1.6
            new Vector2Int(3, 2), // aspectRatio == 1.5
            new Vector2Int(4, 3), // aspectRatio == 1.333...
            new Vector2Int(5, 4) // aspectRatio == 1.25
        };

        for (int i = 0; i < commonRatios.Length; i++)
        {
            float targetRatio = (float)commonRatios[i].x / commonRatios[i].y;

            if (aspectRatio >= targetRatio - tolerance)
            {
                return commonRatios[i];
            }
        }

        // Fallback just in case, return a 16:9 aspect ratio
        return new Vector2Int(16, 9);
    }

    /// <summary>
    ///     Resizes a render texture.
    /// </summary>
    /// <param name="tex">The render texture to be resized.</param>
    /// <param name="width">The new width of the render texture.</param>
    /// <param name="height">The new height of the render texture.</param>
    private void ResizeRenderTexture(RenderTexture tex, int width, int height)
    {
        if (tex.width != width || tex.height != height)
        {
            tex.Release();
            tex.width = width;
            tex.height = height;
            tex.Create();
        }
    }

    /// <summary>
    ///     Forces a refresh of all of the active cameras' aspect ratios.
    /// </summary>
    private void RefreshCameraAspect()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].ResetAspect();
        }
    }

    /// <summary>
    ///     Gets a resolution that can be used for the render texture based on the desired pixelation setting and the target aspect ratio.
    /// </summary>
    /// <param name="pixelation">The pixelation setting that should influence the render texture's resolution.</param>
    /// <param name="targetAspectRatio">The aspect ratio that the resolution should have.</param>
    /// <returns>A Vector2Int with the new resolution.</returns>
    private Vector2Int GetRenderTextureResolution(Pixelation pixelation, Vector2Int targetAspectRatio)
    {
        switch (pixelation)
        {
            case Pixelation.Low:
                return new Vector2Int(
                    targetAspectRatio.x * lowPixelationAspectRatioMultiplier,
                    targetAspectRatio.y * lowPixelationAspectRatioMultiplier
                );
            case Pixelation.High:
                return new Vector2Int(
                    targetAspectRatio.x * highPixelationAspectRatioMultiplier,
                    targetAspectRatio.y * highPixelationAspectRatioMultiplier
                );
            // Default to high pixelation
            default:
                return new Vector2Int(
                    targetAspectRatio.x * highPixelationAspectRatioMultiplier,
                    targetAspectRatio.y * highPixelationAspectRatioMultiplier
                );
        }
    }

    /// <summary>
    ///     Get the game's current pixelation setting.
    /// </summary>
    /// <returns>Pixelation.</returns>
    public Pixelation GetCurrentPixelation()
    {
        return (Pixelation)PlayerPrefs.GetInt(pixelationStr, (int)defaultPixelation);
    }

    /// <summary>
    ///     Enable or disable vsync.
    /// </summary>
    /// <param name="enable">Whether or not vsync should be enabled.</param>
    public void EnableVSync(bool enable)
    {
        QualitySettings.vSyncCount = enable ? 1 : 0; // 1 == enabled, 0 == disabled
        PlayerPrefs.SetInt(vsyncStr, enable ? 1 : 0);
    }

    /// <summary>
    ///     Check if vsync is currently enabled.
    /// </summary>
    /// <returns>True if vsync is enabled, false if otherwise.</returns>
    public bool IsVSyncEnabled()
    {
        return PlayerPrefs.GetInt(vsyncStr, defaultVsync) == 1;
    }

    /// <summary>
    ///     Enable or disable an FPS limit.
    /// </summary>
    /// <param name="enable">Whether or not the FPS should be limited.</param>
    public void EnableFPSLimit(bool enable)
    {
        if (enable)
        {
            SetFPSLimit(GetCurrentFPSLimit());
        }
        else
        {
            DisableTargetFramerate();
        }

        PlayerPrefs.SetInt(shouldLimitFpsStr, enable ? 1 : 0);
    }

    /// <summary>
    ///     Disables the target framerate setting and allows Unity to render as many frames as possible (assuming
    ///     vsync is also off).
    /// </summary>
    private void DisableTargetFramerate()
    {
        // Setting the targetFrameRate to 0 tells Unity to revert back to default (render as many frames as possible)
        Application.targetFrameRate = 0;
    }

    /// <summary>
    ///     Sets Application.targetFrameRate to a new target.
    /// </summary>
    /// <param name="targetFrameRate">The new target framerate.</param>
    private void SetTargetFramerate(int targetFrameRate)
    {
        Application.targetFrameRate = targetFrameRate;
    }

    /// <summary>
    ///     Check if an FPS limit is currently enabled.
    /// </summary>
    /// <returns>True if FPS limit is enabled, false if otherwise.</returns>
    public bool IsFPSLimitEnabled()
    {
        return PlayerPrefs.GetInt(shouldLimitFpsStr, shouldLimitFPSDefault) == 1;
    }

    /// <summary>
    ///     Set a new FPS limit (clamped between minFPSLimit and maxFPSLimit).
    /// </summary>
    /// <param name="limit">The new FPS limit.</param>
    public void SetFPSLimit(int limit)
    {
        limit = Mathf.Clamp(limit, minFPSLimit, maxFPSLimit);
        SetTargetFramerate(limit);
        PlayerPrefs.SetInt(fpsLimitStr, limit);
    }

    /// <summary>
    ///     Get the game's current FPS Limit.
    /// </summary>
    /// <returns>An int.</returns>
    public int GetCurrentFPSLimit()
    {
        return PlayerPrefs.GetInt(fpsLimitStr, defaultFpsLimit);
    }

    /// <summary>
    ///     Enable or disable generic device tips. Note: this does not disable important device messages, just
    ///     generic ones such as Hedge Maze's opening messages.
    /// </summary>
    /// <param name="enable">Whether or not generic device tips</param>
    public void EnableDeviceTips(bool enable)
    {
        PlayerPrefs.SetInt(deviceTipsStr, enable ? 1 : 0);
    }

    /// <summary>
    ///     Check whether or not generic device tips are enabled.
    /// </summary>
    /// <returns>True if device tips are enabled, false if otherwise.</returns>
    public bool AreDeviceTipsEnabled()
    {
        return PlayerPrefs.GetInt(deviceTipsStr, defaultDeviceTips) == 1;
    }

    /// <summary>
    ///     Enable or disable cinemachine camera shake.
    /// </summary>
    /// <param name="enable">Whether or not camera shake should be enabled.</param>
    public void EnableCameraShake(bool enable)
    {
        PlayerPrefs.SetInt(cameraShakeStr, enable ? 1 : 0);
    }

    /// <summary>
    ///     Check if camera shake is enabled or disabled.
    /// </summary>
    /// <returns>True if camera shake is enabled, false if otherwise.</returns>
    public bool IsCameraShakeEnabled()
    {
        return PlayerPrefs.GetInt(cameraShakeStr, defaultCameraShake) == 1;
    }
}
