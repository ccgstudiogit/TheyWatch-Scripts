using UnityEngine;

public class WardenAudioHandler : MonsterAudioHandler
{
    [Header("Audio Sources")]
    [field: SerializeField] public AudioSource sfxAudioSource { get; private set; }
    [field: SerializeField] public AudioSource footstepAudioSource { get; private set; }

    [field: SerializeField] public AudioSource drumbeatAudioSource { get; private set; }
    [field: SerializeField] public AudioSource whistleAudioSource { get; private set; }

    [field: SerializeField] public AudioSource chaseMusicAudioSource { get; private set; }

    [field: SerializeField] public AudioSource breathingAudioSource { get; private set; }
}
