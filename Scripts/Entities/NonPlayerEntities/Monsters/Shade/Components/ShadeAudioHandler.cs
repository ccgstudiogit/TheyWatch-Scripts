using UnityEngine;

public class ShadeAudioHandler : MonsterAudioHandler
{
    // This class holds all of the audio sources needed for Shade

    [Header("Audio Sources")]
    [Tooltip("This audio source handles PlayOneShot SFX, such as berserkBuildUp, berserkWhisperBuildUp, etc.")]
    [field: SerializeField] public AudioSource sfxAudioSource { get; private set; }
    [field: SerializeField] public AudioSource footstepAudioSource { get; private set; }
    [field: SerializeField] public AudioSource berserkFootstepAudioSource { get; private set; }
    [Tooltip("This audio source is only played while Shade is chasing the player while berserk")]
    [field: SerializeField] public AudioSource whisperChaseAudioSource { get; private set; }
}
