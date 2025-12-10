using UnityEngine;

public class Runestone : Collectable
{
    [Header("Audio Sources")]
    [Tooltip("These audio sources will pause/resume on game pause/resume")]
    [SerializeField] private AudioSource[] auraAudioSources;

    [Header("Sound Effects")]
    [Tooltip("No audio source is required for these sound effects to play")]
    [SerializeField] private SoundEffectSO[] onCollectedSFX;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (auraAudioSources.Length < 1)
        {
            Debug.LogWarning($"{gameObject.name} has no auraAudioSources.");
        }

        if (onCollectedSFX.Length < 1)
        {
            Debug.LogWarning($"{gameObject.name} has no onCollectedSFX.");
        }
#endif
    }

    // This is here due to a requirement through inheriting from Interactable. Currently there is no need to for
    // PlayerInteractions.cs to be able to interact with runestones since PlayerCollisions.cs handles automatically
    // collecting the runestone once the player collides with it.
    public override void Interact() { }

    /// <summary>
    ///     Collect this collectable by first playing the on collected SFX, then firing off the even OnCollected, then
    ///     finally destroying the collectable game object.
    /// </summary>
    public override void CollectThenDestroy()
    {
        // PlayCollectedSFX is called before base.CollectThenDestroy in order to make sure the SFX are played correctly
        PlayCollectedSFX();
        base.CollectThenDestroy();
    }

    /// <summary>
    ///     Loops through the on collected SFX array and plays each sound without an audio source (the SoundEffectSO creates)
    ///     a temporary 2D space audio source and plays the SFX that way).
    /// </summary>
    private void PlayCollectedSFX()
    {
        for (int i = 0; i < onCollectedSFX.Length; i++)
        {
            if (onCollectedSFX[i] != null)
            {
                onCollectedSFX[i].Play();
            }
        }
    }
}
