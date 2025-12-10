using System;
using UnityEngine;

public class ShadeEndScene : Monster, IIdleStateUser, IChaseStateUser, ICaughtPlayerStateUser
{
    // Lets HedgeMazeEndLevelController know when Shade caught the player
    public event Action OnShadeCollidedWithPlayer;

    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behaviors")]
    [SerializeField] private IdleState idleStateBehavior;
    [SerializeField] private ChaseState chaseStateBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerStateBehavior;

    [Header("SFX")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private SoundEffectSO footstepSFX;

    protected override void Awake()
    {
        base.Awake();

        _idleState = idleStateBehavior;
        _chaseState = chaseStateBehavior;
        _caughtPlayerState = caughtPlayerStateBehavior;

        startState = _idleState;
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this)
        {
            return;
        }

        stateMachine.ChangeState(caughtPlayerState);
        OnShadeCollidedWithPlayer?.Invoke();
    }

    public void FootstepEvent()
    {
        if (footstepAudioSource != null && footstepSFX != null)
        {
            footstepSFX.Play(footstepAudioSource);
        }
    }

    public void ChasePlayer()
    {
        stateMachine.ChangeState(chaseState);
    }
}
