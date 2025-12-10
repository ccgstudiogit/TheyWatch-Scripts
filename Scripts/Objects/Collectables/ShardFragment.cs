using UnityEngine;
using UnityEngine.Audio;

public class ShardFragment : Collectable
{
    [SerializeField] private SoundEffectSO onCollectedSFX;

    [SerializeField] private AudioResource auraSFX;

    public override void Interact() { }
}
