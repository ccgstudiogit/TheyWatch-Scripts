using System;
using System.Collections;
using UnityEngine;

public class EMPDevice : MonoBehaviour
{
    // Lets DisplayInputActionCooldown.cs display the picture cooldown length via the slider (float is the cooldown length)
    public static event Action<float> OnEMP;

    public bool empEnabled = true;

    [Header("Collider")]
    [SerializeField] private CapsuleCollider empCollider;

    [Header("EMP Size")]
    [Tooltip("The maximum size of the capsule collider's radius")]
    [SerializeField, Min(0f)] private float maxColRadius = 14f;
    private float empColStartRadius; // For resetting the collider's size after the max size has been reached
    [Tooltip("The time it takes for the EMP collider to go from its starting radius to the maxColRadius")]
    [SerializeField, Min(0f)] private float timeToMaxRadius = 0.75f;

    [Header("Cooldown")]
    [SerializeField, Min(0f)] private float cooldownDuration = 30f;
    private bool onCooldown;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource buttonPressSource;
    [SerializeField] private AudioSource empSFXSource;
    [SerializeField] private SoundEffectSO[] empSFX;

    [Header("VFX")]
    [SerializeField] private ParticleSystem electricBubbleVFX;

    private void Awake()
    {
        empColStartRadius = empCollider.radius;
        empCollider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EMPButtonPressedListener.OnEMPButtonPressed += SetOffEMP;
    }

    private void OnDisable()
    {
        EMPButtonPressedListener.OnEMPButtonPressed -= SetOffEMP;
    }

    /// <summary>
    ///     Set off an EMP.
    /// </summary>
    public void SetOffEMP()
    {
        buttonPressSource.Play();

        if (!onCooldown && empEnabled)
        {
            onCooldown = true;
            this.Invoke(() => onCooldown = false, cooldownDuration);
            OnEMP?.Invoke(cooldownDuration); // For displaying the cooldown bar in gameplay UI
            PlayEMPSounds();
            ExpandCollider();
        }
#if UNITY_EDITOR
        else
        {
            Debug.Log("Activating emp but currently on cooldown.");
        }
#endif
    }

    /// <summary>
    ///     Expand the EMP's capsule collider.
    /// </summary>
    public void ExpandCollider()
    {
        if (electricBubbleVFX != null)
        {
            electricBubbleVFX.Play();
        }

        empCollider.gameObject.SetActive(true);
        StartCoroutine(ExpandColliderRoutine());
    }

    /// <summary>
    ///     Handles expanding the capsule collider's radius to the max within the timeToMaxRadius duration.
    /// </summary>
    private IEnumerator ExpandColliderRoutine()
    {
        yield return null;
        
        float lerp = 0f;
        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / timeToMaxRadius);
            float radius = Mathf.Lerp(empColStartRadius, maxColRadius, lerp);
            empCollider.radius = radius;

            yield return null;
        }

        empCollider.radius = empColStartRadius;
        empCollider.gameObject.SetActive(false);
    }

    private void PlayEMPSounds()
    {
        for (int i = 0; i < empSFX.Length; i++)
        {
            if (empSFX[i] != null)
            {
                empSFX[i].PlayOneShot(empSFXSource);
            }
        }
    }
}
