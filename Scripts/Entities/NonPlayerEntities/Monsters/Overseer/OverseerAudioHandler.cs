using UnityEngine;

public class OverseerAudioHandler : MonsterAudioHandler
{
    [field: SerializeField] public AudioSource sfxAudioSource { get; private set; }
    [field: SerializeField] public AudioSource footstepAudioSource { get; private set; }

    [field: SerializeField] public AudioSource playerSpottedAudioSource { get; private set; }

    [field: SerializeField] public AudioSource auraAudioSource { get; private set; }

    [field: SerializeField] public AudioSource shutdownAudioSource { get; private set; }
    [field: SerializeField] public AudioSource powerUpAudioSource { get; private set; }
}
