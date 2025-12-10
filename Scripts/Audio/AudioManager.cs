using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Master Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("Min/Max Volume")]
    [field: SerializeField] public int minVolume { get; private set; } = 0;
    [field: SerializeField, Min(100)] public int maxVolume { get; private set; } = 200;

    public const int defaultVolume = 100;

    private const float minRemapValue = 0.0001f; // This is near-zero as exact 0 doesn't work with logarithmic scale
    private float maxRemapValue => maxVolume / defaultVolume; // Convert to something usable in logarithmic scale

    // PlayerPrefs variables
    private const string masterVolStr = "masterVol";
    private const string musicVolStr = "musicVol";
    private const string sfxVolStr = "sfxVol";
    private const string uiVolStr = "uiVol";
    private const string footstepVolStr = "footstepVol";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (masterMixer != null)
        {
            LoadAudioPlayerPrefs();
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"{name}'s masterMixer reference null. Please assign a reference");
        }
#endif
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    ///     Used to initially load audio settings from player preferences.
    /// </summary>
    private void LoadAudioPlayerPrefs()
    {
        float masterVol = PlayerPrefs.GetFloat(masterVolStr, defaultVolume);
        float musicVol = PlayerPrefs.GetFloat(musicVolStr, defaultVolume);
        float sfxVol = PlayerPrefs.GetFloat(sfxVolStr, defaultVolume);
        float uiVol = PlayerPrefs.GetFloat(uiVolStr, defaultVolume);
        float footstepVol = PlayerPrefs.GetFloat(footstepVolStr, defaultVolume);

        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
        SetUIVolume(uiVol);
        SetFootstepVolume(footstepVol);
    }

    /// <summary>
    ///     Sets the master volume to a new specified value.
    /// </summary>
    public void SetMasterVolume(float value)
    {
        // Saving to PlayerPrefs before converting the value makes it a lot easier for VolumeSlider.cs to get the
        // saved volume right away as is without needing to worry about remapping it back to a slider scale
        value = Mathf.Clamp(value, minVolume, maxVolume);
        PlayerPrefs.SetFloat(masterVolStr, value);

        // Converts to logarithm to the base of 10. This is done because Unity expects Mixer.SetFloat() to be in
        // logarithmic scale, which better matches human perception of sound
        value = HelperMethods.Remap(value, minVolume, maxVolume, minRemapValue, maxRemapValue);
        masterMixer.SetFloat(masterVolStr, Mathf.Log10(value) * 20);
    }

    /// <summary>
    ///     Sets the music volume to a new specified value.
    /// </summary>
    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, minVolume, maxVolume);
        PlayerPrefs.SetFloat(musicVolStr, value);

        value = HelperMethods.Remap(value, minVolume, maxVolume, minRemapValue, maxRemapValue);
        masterMixer.SetFloat(musicVolStr, Mathf.Log10(value) * 20);
    }

    /// <summary>
    ///     Sets the SFX volume to a new specified value.
    /// </summary>
    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, minVolume, maxVolume);
        PlayerPrefs.SetFloat(sfxVolStr, value);

        value = HelperMethods.Remap(value, minVolume, maxVolume, minRemapValue, maxRemapValue);
        masterMixer.SetFloat(sfxVolStr, Mathf.Log10(value) * 20);
    }

    /// <summary>
    ///     Sets the UI SFX volume to a new specified value.
    /// </summary>
    public void SetUIVolume(float value)
    {
        value = Mathf.Clamp(value, minVolume, maxVolume);
        PlayerPrefs.SetFloat(uiVolStr, value);

        value = HelperMethods.Remap(value, minVolume, maxVolume, minRemapValue, maxRemapValue);
        masterMixer.SetFloat(uiVolStr, Mathf.Log10(value) * 20);
    }

    /// <summary>
    ///     Sets the player's and monsters' footstep SFX volume to a new specified value.
    /// </summary>
    public void SetFootstepVolume(float value)
    {
        value = Mathf.Clamp(value, minVolume, maxVolume);
        PlayerPrefs.SetFloat(footstepVolStr, value);

        value = HelperMethods.Remap(value, minVolume, maxVolume, minRemapValue, maxRemapValue);
        masterMixer.SetFloat(footstepVolStr, Mathf.Log10(value) * 20);
    }
}
