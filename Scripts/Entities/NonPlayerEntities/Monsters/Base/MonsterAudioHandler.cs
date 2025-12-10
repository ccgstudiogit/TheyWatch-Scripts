using System.Collections;
using UnityEngine;

public abstract class MonsterAudioHandler : MonoBehaviour
{
    [Header("Audio Source Prefab")]
    [Tooltip("Can be used to instantiate an audio source in the scene and play an SFX via that source")]
    [field: SerializeField] public GameObject audioSourcePrefab { get; private set; }

    /// <summary>
    ///     Instantiate an audioSourcePrefab game object and play an SFX from that game object.
    /// </summary>
    /// <param name="sfx">The sound effect to play.</param>
    /// <param name="position">The position the audioSourcePrefab will be spawn at.</param>
    public void PlaySFXFromPrefab(SoundEffectSO sfx, Vector3 position)
    {
        MonsterHelper.CreateAudioSourceAndPlaySFX(audioSourcePrefab, position, sfx);
    }

    /// <summary>
    ///     Plays a given SFX after a delay.
    /// </summary>
    public void PlaySFXWithDelay(SoundEffectSO sfx, AudioSource source, float delay)
    {
        if (sfx != null && source != null)
        {
            StartCoroutine(PlaySFXAfterDelay(sfx, source, delay));
        }
    }

    private IEnumerator PlaySFXAfterDelay(SoundEffectSO sfx, AudioSource source, float d)
    {
        float timeElapsed = 0f;

        while (timeElapsed < d)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        sfx.Play(source);
    }

    /// <summary>
    ///     Starts playing an audio source.
    /// </summary>
    public void PlaySource(AudioSource source)
    {
        source.Play();
    }

    /// <summary>
    ///     Pauses an audio source.
    /// </summary>
    public void PauseSource(AudioSource source)
    {
        source.Pause();
    }

    /// <summary>
    ///     Stops an audio source.
    /// </summary>
    public void StopSource(AudioSource source)
    {
        source.Stop();
    }
}
