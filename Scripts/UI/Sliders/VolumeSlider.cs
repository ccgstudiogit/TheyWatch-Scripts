using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioType audioType;

    [SerializeField] private bool remapToAudioManagerMinMax = true;
    [SerializeField] private bool useWholeNumbers = true;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        float volume;

        switch (audioType)
        {
            case AudioType.Master:
                volume = PlayerPrefs.GetFloat("masterVol", AudioManager.defaultVolume);
                break;
            case AudioType.Music:
                volume = PlayerPrefs.GetFloat("musicVol", AudioManager.defaultVolume);
                break;
            case AudioType.SFX:
                volume = PlayerPrefs.GetFloat("sfxVol", AudioManager.defaultVolume);
                break;
            case AudioType.UI:
                volume = PlayerPrefs.GetFloat("uiVol", AudioManager.defaultVolume);
                break;
            case AudioType.Footstep:
                volume = PlayerPrefs.GetFloat("footstepVol", AudioManager.defaultVolume);
                break;
            default:
                volume = AudioManager.defaultVolume;
                break;
        }

        if (remapToAudioManagerMinMax)
        {
            slider.minValue = AudioManager.instance.minVolume;
            slider.maxValue = AudioManager.instance.maxVolume;
        }

        if (useWholeNumbers)
        {
            slider.wholeNumbers = true;
        }

        slider.value = volume;
        slider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDestroy()
    {
        if (slider != null && slider.onValueChanged != null)
        {
            slider.onValueChanged.RemoveListener(SetVolume);
        }
    }

    private void SetVolume(float volume)
    {
        if (AudioManager.instance == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name} is attempting to change a volume but AudioManager.instance is null.");
#endif
            return;
        }

        switch (audioType)
        {
            case AudioType.Master:
                AudioManager.instance.SetMasterVolume(volume);
                break;
            case AudioType.Music:
                AudioManager.instance.SetMusicVolume(volume);
                break;
            case AudioType.SFX:
                AudioManager.instance.SetSFXVolume(volume);
                break;
            case AudioType.UI:
                AudioManager.instance.SetUIVolume(volume);
                break;
            case AudioType.Footstep:
                AudioManager.instance.SetFootstepVolume(volume);
                break;
        }
    }
}
