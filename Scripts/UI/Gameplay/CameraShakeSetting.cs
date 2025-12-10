using UnityEngine;
using Michsky.MUIP;

public class CameraShakeSetting : MonoBehaviour
{
    [SerializeField] private CustomToggle toggle;

    private void Start()
    {
        toggle.toggleObject.onValueChanged.AddListener(EnableCameraShake);
        UpdateToggle();
    }

    private void OnEnable()
    {
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
            toggle.toggleObject.onValueChanged.RemoveListener(EnableCameraShake);
        }
    }

    private void EnableCameraShake(bool enable)
    {
        SettingsManager.instance.EnableCameraShake(enable);
    }

    private void UpdateToggle()
    {
        toggle.toggleObject.isOn = SettingsManager.instance.IsCameraShakeEnabled();
        toggle.UpdateState();
    }
}
