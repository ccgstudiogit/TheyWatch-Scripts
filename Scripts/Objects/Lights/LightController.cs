using UnityEngine;

[RequireComponent(typeof(Light), typeof(MonitorDistanceToPlayer))]
public class LightController : MonoBehaviour
{
    [Tooltip("If this light's distance exceeds this value, this light will be turned off. It will be turned back on if " +
        "the player comes back within this range")]
    [SerializeField, Min(0)] private float maxDistanceToPlayer = 75f;
    [Tooltip("If the setting is currently low or medium quality, this is the maximum distance to the player. Also note: the " + 
        "FlickeringLight component is also disabled with lower quality settings to help with CPU overhead which was the bottleneck")]
    [SerializeField, Min(0)] private float lowerQualityMaxDistance = 32.5f;

    private Light thisLight;
    private MonitorDistanceToPlayer monitorDistanceToPlayer;

    // If this light also has a FlickeringLight component, disable that as well since there is no need to have that script
    // active on a light that is not currently on
    private FlickeringLight flickeringLight;
    private bool flickeringLightEnabled = true;

    private Camera mainCamera;

    private void Awake()
    {
        thisLight = GetComponent<Light>();
        monitorDistanceToPlayer = GetComponent<MonitorDistanceToPlayer>();
        flickeringLight = GetComponent<FlickeringLight>();
    }

    private void OnEnable()
    {
        monitorDistanceToPlayer.DistanceToPlayer += GetDistanceToPlayer;
    }

    private void OnDisable()
    {
        monitorDistanceToPlayer.DistanceToPlayer -= GetDistanceToPlayer;
    }

    private void GetDistanceToPlayer(float distance)
    {
        Quality currentQuality = SettingsManager.instance.GetCurrentQuality();

        switch (currentQuality)
        {
            case Quality.Low:
            case Quality.Medium:
                LowerQualitySettingCheck(distance);
                break;
            case Quality.Ultra:
            case Quality.High:
            default:
                DefaultCheck(distance);
                break;
        }
    }

    /// <summary>
    ///     Enable or disable this light. If this light also has a FlickeringLight component, enable/disable that too.
    /// </summary>
    /// <param name="enableLight">Whether or not this light should be enabled.</param>
    public void EnableLight(bool enableLight)
    {
        thisLight.enabled = enableLight;
        EnableFlickeringLight(enableLight);
    }

    /// <summary>
    ///     Handles enabling/disabling this light when the game's quality setting is set to low or medium.
    /// </summary>
    /// <param name="distance">The distance of this game objec to the player.</param>
    private void LowerQualitySettingCheck(float distance)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Disable the flickering light script completely when on lower quality settings
        if (IsFlickeringEnabled())
        {
            EnableFlickeringLight(false);
        }

        if (distance < lowerQualityMaxDistance && !thisLight.enabled)
        {
            // Only activate this light if the main camera is not null (meaning the player has been loaded in) and this game object is
            // visible to the camera (within the camera's view frustrum)
            if (mainCamera != null)
            {
                if (HelperMethods.IsVisible(mainCamera, gameObject))
                {
                    EnableLight(true);
                }
            }
            // If the main camera was null, it means the player has not loaded into the level yet. In that case, just enable the light
            else
            {
                EnableLight(true);
            }
        }
        // Disable the light completely without bothering to check if it's within the camera's view if the player is far enough away
        else if (distance >= lowerQualityMaxDistance && thisLight.enabled)
        {
            EnableLight(false);
        }
    }

    /// <summary>
    ///     The default distance check where if the player is far enough away, this light will be disabled but if close enough this light
    ///     will be re-enabled.
    /// </summary>
    /// <param name="distance">The distance of this game object to the player.</param>
    private void DefaultCheck(float distance)
    {
        if (distance > maxDistanceToPlayer && thisLight.enabled)
        {
            EnableLight(false);
        }
        else if (distance <= maxDistanceToPlayer && !thisLight.enabled)
        {
            EnableLight(true);
        }
    }

    /// <summary>
    ///     Enable or disable this game object's FlickeringLight component (if it does not have one, nothing will happen).
    /// </summary>
    /// <param name="enableFlicker">Whether or not the FlickeringLight component should be enabled.</param>
    public void EnableFlickeringLight(bool enableFlicker)
    {
        if (flickeringLight != null)
        {
            flickeringLightEnabled = enableFlicker;
            flickeringLight.enabled = enableFlicker;
        }
        // If flickeringLight is null, this game object does not have that component and just set the flag to false
        else
        {
            flickeringLightEnabled = false;
        }
    }

    /// <summary>
    ///     Check if this game object's FlickeringLight component is enabled.
    /// </summary>
    /// <returns>True if enabled, false if otherwise. Also returns false if there is no FlickeringLight component.</returns>
    public bool IsFlickeringEnabled()
    {
        return flickeringLightEnabled;
    }
}
