using System.Collections;
using UnityEngine;

public class CollectableSightAudio : MonoBehaviour
{
    [Header("Active")]
    [Tooltip("The audio source that plays when collectable sight is active")]
    [SerializeField] private AudioSource collectableSightActiveSource;
    private float sourceStartVol;
    [SerializeField, Min(0f)] private float fadeDuration = 0.4f;

    [Header("Start & End")]
    [SerializeField] private SoundEffectSO startSFX;
    [SerializeField] private SoundEffectSO endSFX;

    private void Awake()
    {
        sourceStartVol = collectableSightActiveSource.volume;
    }

    private void OnEnable()
    {
        InputCollectableSight.OnCollectableSight += HandleCollectableSightAudio;
    }

    private void OnDisable()
    {
        InputCollectableSight.OnCollectableSight -= HandleCollectableSightAudio;
    }

    private void HandleCollectableSightAudio(bool active)
    {
        if (active)
        {
            startSFX.Play();
            collectableSightActiveSource.volume = 0f;
            collectableSightActiveSource.Play();
            StartCoroutine(FadeVolume(collectableSightActiveSource, sourceStartVol, fadeDuration));
        }
        else
        {
            endSFX.Play();
            StartCoroutine(FadeVolume(collectableSightActiveSource, 0f, fadeDuration));
            this.Invoke(() => collectableSightActiveSource.Pause(), fadeDuration);
        }
    }
    
    /// <summary>
    ///     Fade the volume from its current volume to a target volume.
    /// </summary>
    /// <param name="source">The audio source whos volume should change.</param>
    /// <param name="targetVolume">The volume that this audio source should be set to.</param>
    /// <param name="duration">How long it will take to go from the current volume to the target volume.</param>
    private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
    {
        float lerp = 0f;
        float startVolume = source.volume;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            source.volume = Mathf.Lerp(startVolume, targetVolume, lerp);

            yield return null;
        }

        source.volume = targetVolume;
    }
}
