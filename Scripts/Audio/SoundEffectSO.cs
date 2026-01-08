using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "ScriptableObjects/New Sound Effect")]
public class SoundEffectSO : ScriptableObject
{
    [Header("Sound Effect Settings")]
    [SerializeField] private AudioClip[] clips;
    [Tooltip("Will play the volume at a random level between x and y")]
    [SerializeField] private Vector2 volume = new Vector2(1f, 1f);
    [Tooltip("Will change the pitch randomly between the values of x and y")]
    [SerializeField] private Vector2 pitch = new Vector2(0.9f, 1.1f);
    [SerializeField] private SoundClipPlayOrder playOrder;
    private int playIndex; // Keeps track of the play order

    [Header("Mixer Reference")]
    [Tooltip("This mixer is used when either PlayOneShot(useSeparateSource = true) and Play(audioSourceParam = null) are called")]
    [SerializeField] private AudioMixerGroup mixer;

    private enum SoundClipPlayOrder
    {
        Random,
        In_Order,
        Reverse
    }

    /// <summary>
    ///     Plays this sound effects audio clip. Note: if audioSourceParam is null, this sound effect will temporarily create a
    ///     game object with an audio source, play the sound effect with that source, and then delete that created source.
    /// </summary>
    /// <param name="audioSourceParam">Optional audio source that can be used as this sound's source.</param>
    /// <returns>The audio source that was used to play the clip.</returns>
    public AudioSource Play(AudioSource audioSourceParam = null)
    {
        return PlayClip(volume, audioSourceParam);
    }

    /// <summary>
    ///     Plays this sound effects audio clip with a volume override.
    /// </summary>
    /// <param name="volumeOverride">A volume override that can be used to override the target volume set in the inspector.</param>
    /// <param name="audioSourceParam">Optional audio source that can be used as this sound's source.</param>
    /// <returns>The audio source that was used to play the clip.</returns>
    public AudioSource Play(Vector2 volumeOverride, AudioSource audioSourceParam = null)
    {
        return PlayClip(volumeOverride, audioSourceParam);
    }

    private AudioSource PlayClip(Vector2 clipVolume, AudioSource audioSource)
    {
        if (clips.Length == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name} has no audio clips.");
#endif
            return null;
        }

        if (audioSource == null)
        {
            GameObject obj = new GameObject("SoundSource", typeof(AudioSource));
            audioSource = obj.GetComponent<AudioSource>();

            if (mixer != null)
            {
                audioSource.outputAudioMixerGroup = mixer;
            }
        }

        audioSource.clip = GetAudioClip();
        audioSource.volume = Random.Range(clipVolume.x, clipVolume.y);
        audioSource.pitch = Random.Range(pitch.x, pitch.y);
        audioSource.dopplerLevel = 0;

        audioSource.Play();

        if (audioSource.name == "SoundSource")
        {
            Destroy(audioSource.gameObject, audioSource.clip.length / audioSource.pitch);
        }

        return audioSource;
    }

    /// <summary>
    ///     Plays this sound effect via audioSourceParam.PlayOneShot(). If useSeparateSource is enabled, a game object with another
    ///     audio source will be created and added under the game object that has the audioSourceParam and PlayOneShot() will
    ///     be called through that new audio source. After the clip has ended, the created audio source is destroyed.
    /// </summary>
    /// <returns>The clip that was played.</returns>
    //
    // Note: useSeparateSource is automatically set to true to make it so that the clip being played does not interfere with the audio
    // source's original volume/pitch
    public AudioClip PlayOneShot(AudioSource audioSourceParam, bool useSeparateSource = true, bool separate3DAudio = false)
    {
        if (audioSourceParam == null || clips.Length == 0)
        {
            return null;
        }

        AudioSource source;

        if (useSeparateSource)
        {
            // Creates the new game object and makes sure its parent is the game object with the original audio source
            GameObject obj = new GameObject("SoundSource", typeof(AudioSource));
            obj.transform.SetParent(audioSourceParam.gameObject.transform, false);
            source = obj.GetComponent<AudioSource>();

            if (separate3DAudio)
            {
                source.spatialBlend = 1; // Make sure the audio is set to use 3D space
            }

            if (mixer != null)
            {
                source.outputAudioMixerGroup = mixer;
            }

            // This is the reason why a new game object is created, to freely change volume/pitch without affecting other sfx
            source.volume = Random.Range(volume.x, volume.y);
            source.pitch = Random.Range(pitch.x, pitch.y);
        }
        else
        {
            source = audioSourceParam;
        }

        AudioClip clip = GetAudioClip();
        source.PlayOneShot(clip);

        if (source.gameObject.name == "SoundSource")
        {
            Destroy(source.gameObject, clip.length / source.pitch);
        }

        return clip;
    }

    private AudioClip GetAudioClip()
    {
        // Get current clip
        AudioClip clip = clips[playIndex >= clips.Length ? 0 : playIndex];

        // Find next clip
        switch (playOrder)
        {
            case SoundClipPlayOrder.Random:
                playIndex = Random.Range(0, clips.Length);
                break;
            case SoundClipPlayOrder.In_Order:
                playIndex = (playIndex + 1) % clips.Length;
                break;
            case SoundClipPlayOrder.Reverse:
                playIndex = (playIndex + clips.Length - 1) % clips.Length;
                break;
        }

        return clip;
    }
}
