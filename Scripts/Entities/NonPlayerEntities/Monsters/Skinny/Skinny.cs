using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MonsterSight))]
public class Skinny : Monster, IIdleStateUser, IPatrolStateUser, IInvestigateStateUser, IChaseStateUser, ICaughtPlayerStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private PatrolState _patrolState;
    public PatrolState patrolState => _patrolState;

    private InvestigateState _investigateState;
    public InvestigateState investigateState => _investigateState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    private MonsterSight monsterSight;

    [Header("Behavior References")]
    [SerializeField] private IdleState idleStateBehavior;
    [SerializeField] private PatrolState patrolStateBehavior;
    [Tooltip("If Skinny searches long enough without seeing the player, Skinny's patrol state will switch to this behavior")]
    [SerializeField] private PatrolState patrolAroundPlayerBehavior;
    [SerializeField] private InvestigateState investigateStateBehavior;
    [SerializeField] private ChaseState chaseStateBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerStateBehavior;

    [Header("Behavior Settings")]
    [SerializeField] private float minTimeBeforeSwitchingToSearchForPlayerPatrol = 45f;
    private float timeSearchingForPlayer;

    [Header("Skinny Audio & Sources")]
    [SerializeField] private SoundEffectSO footstepSoundEffect;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource footstepAudioSource;

    protected override void Awake()
    {
        base.Awake();

        _idleState = idleStateBehavior;
        _patrolState = patrolStateBehavior;
        _investigateState = investigateStateBehavior;
        _chaseState = chaseStateBehavior;
        _caughtPlayerState = caughtPlayerStateBehavior;

        startState = idleState;

        monsterSight = GetComponent<MonsterSight>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        PauseHandler.OnGamePause += HandleGamePaused;
        PauseHandler.OnGameResume += HandleGameResumed;

        monsterSight.OnPlayerSeen += HandlePlayerSeen;

        EntityState.OnPlayerReferenceNotFound += HandlePlayerReferenceNotFound;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        PauseHandler.OnGamePause -= HandleGamePaused;
        PauseHandler.OnGameResume -= HandleGameResumed;

        monsterSight.OnPlayerSeen -= HandlePlayerSeen;

        EntityState.OnPlayerReferenceNotFound -= HandlePlayerReferenceNotFound;
    }

    protected override void Update()
    {
        base.Update();

        HandleBehavior();
    }

    private void HandleBehavior()
    {
        if (!monsterSight.isPlayerCurrentlySeen && patrolState != patrolAroundPlayerBehavior)
        {
            timeSearchingForPlayer += Time.deltaTime;

            if (timeSearchingForPlayer >= minTimeBeforeSwitchingToSearchForPlayerPatrol)
            {
                SwitchStateBehavior(ref _patrolState, patrolAroundPlayerBehavior);
            }
        }
        else if (monsterSight.isPlayerCurrentlySeen)
        {
            timeSearchingForPlayer = 0f;

            if (patrolState != patrolStateBehavior)
            {
                SwitchStateBehavior(ref _patrolState, patrolStateBehavior);
            }
        }
    }

    protected override void HandleKillPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this)
        {
            return;
        }

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);
    }

    protected override void HandleDamagePlayer(Monster monster) { }

    private void HandlePlayerSeen()
    {
        if (IsEntityInSpecificState(caughtPlayerState))
        {
            return;
        }

        // Prevents chaseState.EnterLogic() from running multiples times while chasing the player
        if (IsEntityInSpecificState(chaseState))
        {
            return;
        }

        stateMachine.ChangeState(chaseState);
    }

    public void FootstepEvent()
    {
        if (footstepSoundEffect != null && footstepAudioSource != null)
        {
            footstepSoundEffect.Play(footstepAudioSource);
        }
    }

    private void HandleGamePaused()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Pause();
        }
    }

    private void HandleGameResumed()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Play();
        }
    }

    private void HandlePlayerReferenceNotFound()
    {
        // Reset timeSearchingForPlayer to avoid getting stuck in constantly switching back to patrolAroundPlayerBehavior
        timeSearchingForPlayer = 0f;

        SwitchStateBehavior(ref _patrolState, patrolStateBehavior);

        stateMachine.ChangeState(patrolState);
    }
}
