using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int health = 100;

    [Header("Taking Damage SFX")]
    [SerializeField] private AudioSource source;
    [SerializeField] private SoundEffectSO impactSFX;

    [Header("Screenshake")]
    [SerializeField] private CinemachineImpulseSource cameraShakeSource;

    [Header("Blood Effect")]
    [SerializeField] private ScriptableRendererFeature bloodEffectFeature;
    [SerializeField] private Material bloodEffectMat;

    [Header("Distortion")]
    [Tooltip("How long it takes for the distortion to become fully active")]
    [SerializeField, Min(0.05f)] private float toDuration = 0.05f;
    [Tooltip("How long it takes for the distortion to go from fully active back to no longer being active")]
    [SerializeField, Min(0.3f)] private float fromDuration = 0.6f;

    private void OnDisable()
    {
        if (bloodEffectFeature.isActive)
        {
            bloodEffectFeature.SetActive(false);
        }
    }

    /// <summary>
    ///     Reduce health by a certain amount (negative values add health).
    /// </summary>
    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    public int GetHealth()
    {
        return health;
    }

    public void PlayImpactSFX()
    {
        impactSFX.Play(source);
    }

    public void PlayScreenShake()
    {
        // Screen shake is played regardless of whether or not the enable screen shake is enabled/disabled in gameplay settings. This
        // is to make sure the monster's disappearance is not noticeable and, combined with the distortion, the screen shake actually
        // isn't really visible.
        cameraShakeSource.GenerateImpulse();
    }

    /// <summary>
    ///     Activates the blood effect scriptable renderer feature.
    /// </summary>
    public void ActivateBloodEffect()
    {
        if (!bloodEffectFeature.isActive)
        {
            bloodEffectFeature.SetActive(true);
        }
    }

    public void PlayDistortion()
    {
        LevelController.instance.TemporaryFullscreenDistortion(toDuration, fromDuration);
    }
}
