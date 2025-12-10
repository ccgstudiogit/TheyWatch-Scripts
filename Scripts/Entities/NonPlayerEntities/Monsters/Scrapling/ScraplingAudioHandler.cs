using UnityEngine;

public class ScraplingAudioHandler : MonsterAudioHandler
{
    [field: SerializeField] public AudioSource sfxAudioSource { get; private set; }
    [field: SerializeField] public AudioSource footstepAudioSource { get; private set; }

    [field: SerializeField] public AudioSource wakeUpSource { get; private set; }

    [field: SerializeField] public AudioSource jumpscareSource { get;  private set; }
}
