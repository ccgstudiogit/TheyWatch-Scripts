using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(PlayerReferences))]
public class PlayerLook : MonoBehaviour
{
    private CinemachineInputAxisController cinemachineInputAxisController; // If this script is enabled/disabled, it will also enable/disable input axis controller
    private CinemachinePanTilt cinemachinePanTilt;
    private PlayerReferences playerReferences;

    private void Awake()
    {
        playerReferences = GetComponent<PlayerReferences>();
        
        if (playerReferences?.cinemachineCam == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"playerReferences.cinemachineCam is null. Disabling PlayerLook.cs");
#endif
            enabled = false;
            return;
        }

        cinemachineInputAxisController = playerReferences.cinemachineCam.GetComponent<CinemachineInputAxisController>();
        cinemachinePanTilt = playerReferences.cinemachineCam.GetComponent<CinemachinePanTilt>();
    }

    private void Start()
    {
        UpdateSensitivity(InputManager.instance.GetCurrentSensitivity()); // Initialize look sensitivity
        StartCoroutine(ForceCinemachineCameraUpdate());
    }

    private void OnEnable()
    {
        if (cinemachineInputAxisController != null && !cinemachineInputAxisController.enabled)
        {
            cinemachineInputAxisController.enabled = true;
        }

        InputManager.OnUpdateSensitivity += UpdateSensitivity;
    }

    private void OnDisable()
    {
        if (cinemachineInputAxisController != null && cinemachineInputAxisController.enabled)
        {
            cinemachineInputAxisController.enabled = false;
        }

        InputManager.OnUpdateSensitivity -= UpdateSensitivity;
    }

    // Fixes an issue where the player arms model is invisible until input is registered
    // (the player's arms would have stayed invisible indefinitely if the player did not move the mouse)
    private IEnumerator ForceCinemachineCameraUpdate()
    {
        yield return null;

        if (cinemachinePanTilt != null)
        {
            cinemachinePanTilt.PanAxis.Value = 5f;
            yield return null;
            cinemachinePanTilt.PanAxis.Value = 0f;
        }
    }

    private void UpdateSensitivity(float newSensitivity)
    {
        foreach (var controller in cinemachineInputAxisController.Controllers)
        {
            controller.Input.Gain = controller.Input.Gain < 0 ? -newSensitivity : newSensitivity;
        }
    }
}
