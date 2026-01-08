using UnityEngine;

public class StitchEndScene : Monster, IIdleStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behaviors")]
    [SerializeField] private IdleState idleBehavior;

    [Header("SFX")]
    [SerializeField] private SoundEffectSO footstepSFX;

    private StitchAudioHandler audioHandler;

    protected override void Awake()
    {
        base.Awake();

        audioHandler = GetComponent<StitchAudioHandler>();

        _idleState = idleBehavior;

        startState = idleState;
    }

    protected override void HandleKillPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this)
        {
            return;
        }
    }

    public void FootstepEvent()
    {
        if (footstepSFX != null)
        {
            footstepSFX.Play(audioHandler.footstepAudioSource);
        }
    }
}
