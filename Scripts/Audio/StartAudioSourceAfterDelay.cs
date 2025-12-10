using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StartAudioSourceAfterDelay : MonoBehaviour
{
    [Header("Fade In Settings")]
    [SerializeField, Min(0)] private float initialDelay = 1f;
    [SerializeField, Min(0)] private float fadeInTime = 1f;

    private AudioSource audioSource;

    private float startingVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Cache the starting volume before setting the volume to be 0 and pausing the audio source
        startingVolume = audioSource.volume;
        audioSource.volume = 0f;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn(audioSource, startingVolume, initialDelay));
    }

    private IEnumerator FadeIn(AudioSource source, float targetVolume, float delay)
    {
        float lerp = 0f;

        yield return new WaitForSeconds(delay);

        source.Play();

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / fadeInTime);
            source.volume = Mathf.Lerp(0, targetVolume, lerp);

            yield return null;
        }

        source.volume = targetVolume;
    }
}
