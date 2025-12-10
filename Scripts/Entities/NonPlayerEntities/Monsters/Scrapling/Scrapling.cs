using UnityEngine;

public class Scrapling : Monster, IIdleStateUser, ISleepStateUser, ISearchStateUser, IFreezeStateUser, IChaseStateUser, IStunStateUser, ICaughtPlayerStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private SleepState _sleepState;
    public SleepState sleepState => _sleepState;

    private SearchState _searchState;
    public SearchState searchState => _searchState;

    private FreezeState _freezeState;
    public FreezeState freezeState => _freezeState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private StunState _stunState;
    public StunState stunState => _stunState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    // Animator variables
    private const string wakeUpStr = "wakeUp";
    private const string wakeUpAnimStr = "wakeUpAnim";

    [Header("Behavior References")]
    [SerializeField] private IdleState idleDoNothingBehavior;
    [SerializeField] private SleepState sleepBehavior;
    [SerializeField] private SearchState searchBehavior;
    [SerializeField] private FreezeState freezeBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private StunState stunBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;

    [Header("Component References")]
    [SerializeField] private Collider col;

    [Header("Awake")]
    [Tooltip("A wake up animation will be randomly selected, make sure the min/max matches the number of wake up anims in the animator")]
    [SerializeField] private Vector2Int wakeUpAnims = new Vector2Int(1, 4);
    [Tooltip("Once this Scrapling wakes up, the footstep detector will have this new range")]
    [SerializeField, Min(0f)] private float awakeFootstepMaxDist = 10f;
    [SerializeField, Min(0f)] private float investigateFootstepCooldown = 10f;
    private bool onInvestigateFootstepCooldown;

    [Header("Freeze")]
    [Tooltip("The minimum amount of time for this Scrapling to spend in the player's flashlight before freezing. This is used only when " +
        "this Scrapling is awake and moving around")]
    [SerializeField, Min(0f)] private float flashlightFreezeTime = 0.1f;

    [Header("Avoidance Priority")]
    [Tooltip("The nav mesh agent's priority is set to this amount while sleeping and frozen (makes sure that while this Scrapling is " +
        "frozen or sleeping it does not block other awake/moving Scraplings")]
    [SerializeField, Range(0, 99)] private int unmovingAvoidancePriority = 99;
    private int startingPriority;

    [Header("Look At Player On Freeze")]
    [Tooltip("Makes sure this Somnid looks at the player's camera position instead of the floor (since that is the player's origin, (0, 0, 0))")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);
    [SerializeField, Min(0f)] private float lookDelay = 0.075f;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO footstepSFX;
    [SerializeField] private SoundEffectSO freezeSFX;
    [SerializeField] private SoundEffectSO wakeUpSFX;

    public bool awake { get; private set; }

    private ScraplingAudioHandler audioHandler;
    private MonsterSight monsterSight;
    private FlashlightDetector flashlightDetector;
    private PlayerFootstepDetector playerFootstepDetector;
    private LookAtTargetOnce lookAtTargetOnce;
    private StunnableEntity stunnableEntity;

    private GameObject player = null;

    protected override void Awake()
    {
        base.Awake();

        audioHandler = GetComponent<ScraplingAudioHandler>();
        monsterSight = GetComponent<MonsterSight>();
        flashlightDetector = GetComponent<FlashlightDetector>();
        playerFootstepDetector = GetComponent<PlayerFootstepDetector>();
        lookAtTargetOnce = GetComponent<LookAtTargetOnce>();
        stunnableEntity = GetComponent<StunnableEntity>();

        _idleState = idleDoNothingBehavior;
        _sleepState = sleepBehavior;
        _searchState = searchBehavior;
        _freezeState = freezeBehavior;
        _chaseState = chaseBehavior;
        _stunState = stunBehavior;
        _caughtPlayerState = caughtPlayerBehavior;

        startState = sleepState;

        // Save this agent's priority and set back to this amount when woken up
        startingPriority = agent.avoidancePriority;

        // Since Scrapling starts off sleeping, update the avoidance priority so it doesn't block other moving Scraplings
        UpdateAgentPriority(unmovingAvoidancePriority);
    }

    protected override void Start()
    {
        // Disable monster sight while sleeping. Monster sight is re-enabled when this Scrapling wakes up
        if (monsterSight != null)
        {
            monsterSight.enabled = false;
        }

        if (stunnableEntity != null)
        {
            stunnableEntity.enabled = false;
        }

        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight += HandleMaxTimeReachedInFlashlight;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard += HandlePlayerFootstepHeard;
        }

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen += HandlePlayerSeen;
        }

        if (freezeBehavior != null)
        {
            freezeBehavior.OnFreezeEnded += HandleFreezeEnded;
        }

        if (stunnableEntity != null)
        {
            stunnableEntity.OnStunned += Stun;
            stunnableEntity.OnStunEnd += StunEnded;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight -= HandleMaxTimeReachedInFlashlight;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard -= HandlePlayerFootstepHeard;
        }

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen -= HandlePlayerSeen;
        }

        if (freezeBehavior != null)
        {
            freezeBehavior.OnFreezeEnded -= HandleFreezeEnded;
        }

        if (stunnableEntity != null)
        {
            stunnableEntity.OnStunned -= Stun;
            stunnableEntity.OnStunEnd -= StunEnded;
        }
    }

    /// <summary>
    ///     If this Scrapling is not awake, wake it up. If it already is awake, freeze it.
    /// </summary>
    private void HandleMaxTimeReachedInFlashlight()
    {
        if (IsEntityInSpecificState(stunState))
        {
            return;
        }

        if (!awake)
        {
            WakeUp();
        }
        // Makes sure of 2 things: 1) The freeze sfx is not played just after waking up and 2) The freeze sfx is only played once
        // when actually frozen, not while standing in the player's flashlight
        else if (!IsEntityInSpecificState(freezeState))
        {
            // The jumpscare audio source is used since it is a 2D source. This makes the subtle freeze sfx clear and obvious
            PlaySFX(freezeSFX, audioHandler.jumpscareSource);

            if (player != null && lookAtTargetOnce != null && !IsEntityInSpecificState(caughtPlayerState))
            {
                this.Invoke(() => lookAtTargetOnce.SetTargetAndLook(player.transform, offset), lookDelay);
            }
        }

        Freeze();
    }

    /// <summary>
    ///     If the player walks too close to this Scrapling without crouching, wake this Scrapling up.
    /// </summary>
    private void HandlePlayerFootstepHeard(Vector3 footstepOrigin)
    {
        if (IsEntityInSpecificState(freezeState) || IsEntityInSpecificState(stunState) || IsEntityInSpecificState(caughtPlayerState))
        {
            return;
        }

        if (awake)
        {
            if (!onInvestigateFootstepCooldown)
            {
                onInvestigateFootstepCooldown = true;
                SetDestination(footstepOrigin);
                this.Invoke(() => onInvestigateFootstepCooldown = false, investigateFootstepCooldown);
            }
        }
        else
        {
            WakeUp();
            Freeze();
        }
    }

    private void HandlePlayerSeen()
    {
        if (IsEntityInSpecificState(freezeState) || IsEntityInSpecificState(stunState) || IsEntityInSpecificState(caughtPlayerState))
        {
            return;
        }

        stateMachine.ChangeState(chaseState);
    }

    /// <summary>
    ///     Subscribes to freezeBehavior.OnFreezeEnded and handles necessary logic when this Scrapling should no longer be frozen.
    /// </summary>
    private void HandleFreezeEnded()
    {
        // Prevents an issue where if the Scrapling was frozen and the player does not leave the MonsterSight's radius, the
        // Scrapling would not "see" the player until the player left the radius
        if (monsterSight != null)
        {
            monsterSight.ResetIsPlayerCurrentlySeen();
        }

        // Make sure to stop looking at the player's direction (when the player first stopped this Scrapling)
        if (lookAtTargetOnce != null)
        {
            lookAtTargetOnce.Stop();
        }

        EnableColTrigger(true);
        UpdateAgentPriority(startingPriority);
        stateMachine.ChangeState(searchState);
    }

    /// <summary>
    ///     Freeze this Scrapling by stopping its movement.
    /// </summary>
    private void Freeze()
    {
        EnableColTrigger(false);

        // Make sure this Scrapling does not block other moving Scraplings while frozen
        if (agent.avoidancePriority != unmovingAvoidancePriority)
        {
            UpdateAgentPriority(unmovingAvoidancePriority);
        }

        stateMachine.ChangeState(freezeState);
    }

    /// <summary>
    ///     Wake this Scrapling up.
    /// </summary>
    public void WakeUp(bool playWakeUpSFX = true)
    {
        awake = true;
        flashlightDetector.SetMaxTimeInFlashlight(flashlightFreezeTime);

        int wakeUpAnim = Random.Range(wakeUpAnims.x, wakeUpAnims.y + 1);
        animator.SetInteger(wakeUpAnimStr, wakeUpAnim);
        animator.SetTrigger(wakeUpStr);

        if (playWakeUpSFX && wakeUpSFX != null)
        {
            wakeUpSFX.PlayOneShot(audioHandler.wakeUpSource, true, true);
        }

        if (monsterSight != null)
        {
            monsterSight.enabled = true;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.SetFootstepMaxDist(awakeFootstepMaxDist);
        }

        if (stunnableEntity != null)
        {
            stunnableEntity.enabled = true;
        }

        // Rotate towards the player's direction
        if (player == null)
        {
            player = LevelController.instance.GetPlayer();
        }

        RotateTowards(player.transform.position);
    }

    /// <summary>
    ///     Stun this Scrapling by entering stun state (basically IdleDoNothing).
    /// </summary>
    private void Stun()
    {
        if (IsEntityInSpecificState(sleepState))
        {
            return;
        }

        EnableColTrigger(false);

        // Make sure this Scrapling does not block other moving Scraplings while frozen
        if (agent.avoidancePriority != unmovingAvoidancePriority)
        {
            UpdateAgentPriority(unmovingAvoidancePriority);
        }

        if (lookAtTargetOnce != null)
        {
            lookAtTargetOnce.Stop();
        }

        stateMachine.ChangeState(stunState);
    }

    /// <summary>
    ///     Once the stun ends (subscribed to the event StunnableEntity.OnStunEnd), enter search state.
    /// </summary>
    private void StunEnded()
    {
        if (IsEntityInSpecificState(sleepState))
        {
            return;
        }

        // Prevents an issue where if the Scrapling was frozen and the player does not leave the MonsterSight's radius, the
        // Scrapling would not "see" the player until the player left the radius
        if (monsterSight != null)
        {
            monsterSight.ResetIsPlayerCurrentlySeen();
        }

        EnableColTrigger(true);
        UpdateAgentPriority(startingPriority);
        stateMachine.ChangeState(searchState);
    }

    /// <summary>
    ///     Rotate this Scrapling's entire body towards a Vector3 position.
    /// </summary>
    private void RotateTowards(Vector3 lookAtPos)
    {
        Vector3 targetDirection = (lookAtPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = lookRotation;
    }

    /// <summary>
    ///     Footstep event from the animator.
    /// </summary>
    private void FootstepEvent()
    {
        PlaySFX(footstepSFX, audioHandler.footstepAudioSource);
    }

    /// <summary>
    ///     Play a sound effect.
    /// </summary>
    /// <param name="sfx">The sound effect to play (Can be null. If it's null, the sound effect will not play).</param>
    /// <param name="source">The audio source of the sound effect.</param>
    private void PlaySFX(SoundEffectSO sfx, AudioSource source)
    {
        if (sfx != null)
        {
            sfx.Play(source);
        }
    }

    /// <summary>
    ///     Update this Scrapling's avoidance priority.
    /// </summary>
    private void UpdateAgentPriority(int newPriority)
    {
        agent.avoidancePriority = newPriority;
    }

    /// <summary>
    ///     Make the collider a trigger or not.
    /// </summary>
    private void EnableColTrigger(bool isTrigger)
    {
        col.isTrigger = isTrigger;
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);
    }
}
