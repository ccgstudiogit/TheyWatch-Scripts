using UnityEngine;

public class Lamppost : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light lamppostLight;
    [Tooltip("This reference is used to swap the material of the glass if needed (such as turning the lamppost off)")]
    [SerializeField] private MeshRenderer glassMeshRenderer;
    [SerializeField] private Material offMaterial;

    [Header("Hardmode Settings")]
    [SerializeField] private bool turnOffLightInHM = true;

    private void Start()
    {
        // Turns off the light in hardmode
        if (turnOffLightInHM && LevelController.instance != null && LevelController.instance is IHMLevelController)
        {
            TurnOff();
        }
    }

    /// <summary>
    ///     Disables the light and replaces the emissive glass material with a non-emissive grey glass.
    /// </summary>
    public void TurnOff()
    {
        lamppostLight.enabled = false;
        glassMeshRenderer.material = offMaterial;
    }
}
