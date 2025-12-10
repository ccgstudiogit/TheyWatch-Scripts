using System;
using System.Collections;
using UnityEngine;

public class Stunnable : MonoBehaviour
{
    public event Action OnStunned;
    public event Action OnStunEnd;

    [Header("Stun Duration")]
    [SerializeField, Min(0f)] protected float duration = 5f;

    [Header("VFX")]
    [Tooltip("Note: sparksVFX.Stop() is called in Awake()")]
    [SerializeField] protected ParticleSystem sparksVFX;

    [Header("Audio")]
    [SerializeField] protected AudioSource source;

    protected virtual void Awake()
    {
        PlayVFX(false);
        PlayAudio(false);
    }

    /// <summary>
    ///     Begin the stun. The stun will be stopped after the duration has elapsed.
    /// </summary>
    public virtual void Stun()
    {
        OnStunned?.Invoke();
        PlayVFX(true);
        PlayAudio(true);
        StartCoroutine(StunRoutine());
    }

    /// <summary>
    ///     After the duration of the stun has elapsed, StopStun() is called.
    /// </summary>
    private IEnumerator StunRoutine()
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        StopStun();
    }

    /// <summary>
    ///     Stops the stun and fires off the OnStunEnd event.
    /// </summary>
    public virtual void StopStun()
    {
        PlayVFX(false);
        PlayAudio(false);
        OnStunEnd?.Invoke();
    }

    /// <summary>
    ///     Set a new duration of the stun.
    /// </summary>
    /// <param name="duration">The new duration of the stun.</param>
    public virtual void SetDuration(float duration)
    {
        this.duration = duration;
    }

    /// <summary>
    ///     Set the particle system to be active or inactive.
    /// </summary>
    /// <param name="active">Whether or not the particle system should play.</param>
    public virtual void PlayVFX(bool active)
    {
        if (sparksVFX == null)
        {
            return;
        }

        if (active)
        {
            sparksVFX.Play();
        }
        else
        {
            sparksVFX.Stop();
        }
    }

    /// <summary>
    ///     Play the stun audio or stop the stun audio.
    /// </summary>
    /// <param name="active">Whether or not the audio should play.</param>
    public virtual void PlayAudio(bool active)
    {
        if (source == null)
        {
            return;
        }

        if (active)
        {
            source.Play();
        }
        else
        {
            source.Stop();
        }
    }
}
