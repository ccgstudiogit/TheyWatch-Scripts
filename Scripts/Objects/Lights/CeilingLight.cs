using UnityEngine;

public class CeilingLight : MonoBehaviour
{
    [Header("Light Reference")]
    [SerializeField] private Light ceilingLightSource;

    [Header("Material")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material offMaterial;
    private Material onMaterial;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource buzzAudioSource;
    [SerializeField] private AudioSource switchSFXAudioSource;
    [Tooltip("The sound effect that will play when this light is turned on/off")]
    [SerializeField] private SoundEffectSO switchSFX;

    [Header("HardMode Settings")]
    [Tooltip("If enabled, this ceiling light will be turned off if it is currently a hardmode level")]
    [SerializeField] private bool turnOffIfHM;

    private void Awake()
    {
        
        if (meshRenderer != null)
        {
            onMaterial = meshRenderer.material;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s meshRenderer reference null.");
#endif
        }
    }

    private void Start()
    {
        if (turnOffIfHM && LevelController.instance != null && LevelController.instance is IHMLevelController)
        {
            TurnOff();
        }
    }

    /// <summary>
    ///     Turn off this ceiling's light source.
    /// </summary>
    public void TurnOff()
    {
        if (ceilingLightSource != null)
        {
            ceilingLightSource.enabled = false;
        }

        if (meshRenderer != null && offMaterial != null)
        {
            meshRenderer.material = offMaterial;
        }

        if (buzzAudioSource != null)
        {
            buzzAudioSource.gameObject.SetActive(false);
        }
    }

    /// <summary>
    ///     Turn on this ceiling's light source.
    /// </summary>
    public void TurnOn()
    {
        if (ceilingLightSource != null)
        {
            ceilingLightSource.enabled = true;
        }

        if (meshRenderer != null && onMaterial != null)
        {
            meshRenderer.material = onMaterial;
        }

        if (buzzAudioSource != null)
        {
            buzzAudioSource.gameObject.SetActive(true);
        }
    }

    /// <summary>
    ///     Play the switch sound effect.
    /// </summary>
    public void PlaySwitchSFX()
    {
        if (switchSFXAudioSource != null && switchSFX != null)
        {
            switchSFX.Play(switchSFXAudioSource);
        }
    }
}
