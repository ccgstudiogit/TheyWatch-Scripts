using UnityEngine;

public class StitchAudioHandler : MonsterAudioHandler
{
    [Header("Audio Sources")]
    [field: SerializeField] public AudioSource sfxAudioSource { get; private set; }
    [field: SerializeField] public AudioSource footstepAudioSource { get; private set; }
}
