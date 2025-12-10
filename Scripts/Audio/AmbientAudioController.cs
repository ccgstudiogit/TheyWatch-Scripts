using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioController : MonoBehaviour
{
    [Header("Respond To LevelController Events")]
    [Tooltip("Any audio source within this array will respond to LevelController's events OnReduceAmbientSFX and " +
        "OnRestoreAmbientSFX")]
    [SerializeField] private AudioSource[] sourcesToRespondToLevelController;

    [Tooltip("The time it takes to fade the audio source's volume from its starting volume to 0 (and 0 back to " +
        "its starting volume)")]
    [SerializeField] private float volumeFadeTime = 1f;

    [Header("Scene Start Fade In")]
    [Tooltip("If enabled, the audio sources audio will be set to 0 in awake and they will fade in on Start()")]
    [SerializeField] private bool fadeInOnStart = true;
    [Tooltip("If fadeInOnStart is enabled, this is the time it will take for the sources' volumes to go from 0 to " + 
        "their starting volume")]
    [SerializeField] private float fadeInOnStartTime = 2.25f;

    // A dictionary is used to keep track of each audio source's starting volume
    private Dictionary<AudioSource, float> sourcesAndStartingVolumes = new Dictionary<AudioSource, float>();

    private void Awake()
    {
        for (int i = 0; i < sourcesToRespondToLevelController.Length; i++)
        {
            float startingVolume = sourcesToRespondToLevelController[i].volume;
            sourcesAndStartingVolumes.TryAdd(sourcesToRespondToLevelController[i], startingVolume);

            if (fadeInOnStart)
            {
                sourcesToRespondToLevelController[i].volume = 0;
            }
        }
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            foreach (KeyValuePair<AudioSource, float> kvp in sourcesAndStartingVolumes)
            {
                StartCoroutine(FadeVolume(kvp.Key, kvp.Value, fadeInOnStartTime));
            }
        }
    }

    private void OnEnable()
    {
        LevelController.OnReduceAmbientSFX += HandleAmbientVolumeChange;
    }

    private void OnDisable()
    {
        LevelController.OnReduceAmbientSFX -= HandleAmbientVolumeChange;
    }

    private void HandleAmbientVolumeChange(bool reduceAmbientSFX)
    {
        foreach (KeyValuePair<AudioSource, float> kvp in sourcesAndStartingVolumes)
        {
            if (reduceAmbientSFX)
            {
                // Reduce this audio source's volume to 0
                StartCoroutine(FadeVolume(kvp.Key, 0f, volumeFadeTime));
            }
            else
            {
                // Bring this audio source's volume back to its starting volume
                StartCoroutine(FadeVolume(kvp.Key, kvp.Value, volumeFadeTime));
            }
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

        // Make sure duration is above 0
        if (duration <= 0)
        {
            source.volume = targetVolume;
            yield break;
        }

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            source.volume = Mathf.Lerp(startVolume, targetVolume, lerp);

            yield return null;
        }

        source.volume = targetVolume;
    }
}
