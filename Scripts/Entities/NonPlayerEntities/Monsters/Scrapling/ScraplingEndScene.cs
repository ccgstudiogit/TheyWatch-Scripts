using UnityEngine;

public class ScraplingEndScene : Monster, IIdleStateUser, IChaseStateUser, ICaughtPlayerStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    private const string wakeUpStr = "wakeUp";

    [Header("Behavior References")]
    [SerializeField] private IdleState idleDoNothingBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;

    [Header("Collider")]
    [Tooltip("This collider is set to not be a trigger until this Scrapling wakes up")]
    [SerializeField] private Collider col;

    [Header("Chasing")]
    [Tooltip("If this Scrapling gets too close to the player, the Scrapling will get this new speed")]
    [SerializeField, Min(0f)] private float reducedSpeed = 3f;
    private float startSpeed;
    [Tooltip("If the Scrapling gets within this distance to the player, the speed will become the reduced speed")]
    [SerializeField, Min(0f)] private float minDisToPlayer = 7.5f;

    [Header("Door Reference")]
    [Tooltip("Optional reference where when used, once the door shuts this Scrapling enters idle state")]
    [SerializeField] private FactoryEndRoomDoor factoryEndRoomDoor;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private SoundEffectSO footstepSFX;

    private GameObject player;

    protected override void Awake()
    {
        base.Awake();

        _idleState = idleDoNothingBehavior;
        _chaseState = chaseBehavior;

        startState = idleState;
        col.isTrigger = false;
    }

    protected override void Start()
    {
        base.Start();

        startSpeed = agent.speed;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (factoryEndRoomDoor != null)
        {
            factoryEndRoomDoor.OnDoorClosed += StopChasing;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (factoryEndRoomDoor != null)
        {
            factoryEndRoomDoor.OnDoorClosed -= StopChasing;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player == null)
        {
            player = LevelController.instance.GetPlayer();
        }
        else if (IsEntityInSpecificState(chaseState))
        {
            float distanceToPlayer = (player.transform.position - transform.position).sqrMagnitude;

            if (distanceToPlayer < minDisToPlayer * minDisToPlayer)
            {
                if (agent.speed > reducedSpeed)
                {
                    agent.speed = reducedSpeed;
                }
            }
            else if (agent.speed < startSpeed)
            {
                agent.speed = startSpeed;
            }
        }
    }

    /// <summary>
    ///     Start chasing the player.
    /// </summary>
    public void ChasePlayer()
    {
        col.isTrigger = true;
        animator.SetTrigger(wakeUpStr);
        stateMachine.ChangeState(chaseState);
    }

    /// <summary>
    ///     Stop chasing the player and enter idle state.
    /// </summary>
    public void StopChasing()
    {
        stateMachine.ChangeState(idleState);   
    }

    /// <summary>
    ///     Footstep event from the animator.
    /// </summary>
    private void FootstepEvent()
    {
        footstepSFX.Play(footstepSource);
    }

    protected override void HandleKillPlayer(PlayerReferences playerReferences, Monster monster)
    {
        stateMachine.ChangeState(caughtPlayerState);
    }
}
