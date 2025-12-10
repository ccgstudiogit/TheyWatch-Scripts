using UnityEngine;

[RequireComponent(typeof(MonsterSight))]
public class Glunny : Monster, IIdleStateUser, IPatrolStateUser, IChaseStateUser, ICaughtPlayerStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private PatrolState _patrolState;
    public PatrolState patrolState => _patrolState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    [Header("Behavior References")]
    [SerializeField] private IdleState idleStateBehavior;
    [SerializeField] private PatrolState basePatrolStateBehavior;
    [Tooltip("Glunny will utilize this behavior after a certain amount of time has passed without seeing the player")]
    [SerializeField] private PatrolState searchForPlayerPatrolStateBehavior;
    [SerializeField] private ChaseState chaseStateBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerStateBehavior;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behavior Settings")]
    [Tooltip("If Glunny hasn't seen the player in this amount of seconds, searchForPlayerBrain is activated")]
    [SerializeField] private float minTimeBeforeSwitchingToSearchForPlayerPatrol = 20f;
    private float timeSearchingForPlayer;

    [Header("Glunny Sound Effects")]
    [SerializeField] private SoundEffectSO footstepSoundEffect;
    [SerializeField] private AudioSource footstepAudioSource;

    private MonsterSight monsterSight;

    protected override void Awake()
    {
        base.Awake();

        monsterSight = GetComponent<MonsterSight>();

        _idleState = idleStateBehavior;
        _patrolState = basePatrolStateBehavior;
        _chaseState = chaseStateBehavior;
        _caughtPlayerState = caughtPlayerStateBehavior;

        startState = idleState;
        timeSearchingForPlayer = 0f;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        monsterSight.OnPlayerSeen += HandleOnPlayerSeen;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        monsterSight.OnPlayerSeen -= HandleOnPlayerSeen;
    }

    protected override void Update()
    {
        base.Update();

        MonitorTimeSearchingForPlayer();
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this)
        {
            return;
        }

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);
    }

    private void HandleOnPlayerSeen()
    {
        // Only go to chase state if the Monster has not already caught the player
        if (!IsEntityInSpecificState(caughtPlayerState))
        {   
            stateMachine.ChangeState(chaseState);
        }
    }

    public void FootstepEvent()
    {
        if (footstepSoundEffect != null && footstepAudioSource != null)
        {
            footstepSoundEffect.Play(footstepAudioSource);
        }
    }

    private void MonitorTimeSearchingForPlayer()
    {
        if (!monsterSight.isPlayerCurrentlySeen && patrolState != searchForPlayerPatrolStateBehavior)
        {
            timeSearchingForPlayer += Time.deltaTime;

            if (timeSearchingForPlayer >= minTimeBeforeSwitchingToSearchForPlayerPatrol)
            {
                SwitchStateBehavior(ref _patrolState, searchForPlayerPatrolStateBehavior);
            }
        }
        else if (monsterSight.isPlayerCurrentlySeen)
        {
            if (patrolState != basePatrolStateBehavior)
            {
                SwitchStateBehavior(ref _patrolState, basePatrolStateBehavior);
            }

            timeSearchingForPlayer = 0f;
        }
    }
}
