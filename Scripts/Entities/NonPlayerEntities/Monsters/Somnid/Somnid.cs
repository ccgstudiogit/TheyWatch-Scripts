using System.Collections;
using UnityEngine;

public class Somnid : Monster, ISleepStateUser, IIdleStateUser, IChaseStateUser, ISearchStateUser, ICaughtPlayerStateUser
{
    private SleepState _sleepState;
    public SleepState sleepState => _sleepState;

    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private SearchState _searchState;
    public SearchState searchState => _searchState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    // Animator variables
    private const string frozenStr = "frozen";
    private const string stopAnimStr = "stopAnim"; // Random step animation that should be used when frozen

    [Header("Behavior References")]
    [SerializeField] private SleepState sleepBehavior;
    [SerializeField] private IdleState idleBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private SearchState searchBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;

    [Header("Sleep")]
    [Tooltip("The nav mesh agent's priority is set to this amount while sleeping (recommended to set as a higher value, so awake " +
        "Somnids are able to push sleeping Somnids out of the way if necessary")]
    [SerializeField, Range(0, 99)] private int sleepAvoidancePriority = 99;
    private int startingPriority;

    [Header("Awake")]
    [Tooltip("Since the PlayerFootstepDetector is only used while this Somnid is sleeping, enable this to make sure that " +
        "when this Somnid wakes up that component is disabled to eliminate unnecessary calculations")]
    [SerializeField] private bool disableFootstepDetectorOnAwake = true;
    [Tooltip("If enabled, if the player wakes up a Somnid by stepping too close, this Somnid will be frozen regardless " + 
        "of whether or not the player shined their light on it")]
    [SerializeField] private bool freezeOnFootstepAwake = true;

    [Header("Freeze")]
    [SerializeField, Min(0f)] private float minTimeFrozen = 4f;
    [SerializeField, Min(0f)] private float maxTimeFrozen = 8f;
    [Tooltip("The minimum amount of time for this Somnid to spend in the player's flashlight before freezing. This is used only when " +
        "this Somnid is awake and moving around")]
    [SerializeField, Min(0f)] private float flashlightFreezeTime = 0.1f;
    [Tooltip("The possible freeze animations to use. A random number will be selected from this, with x and y being inclusive")]
    [SerializeField] private Vector2Int anims = new Vector2Int(1, 2);
    [Tooltip("Set this Somnid's avoidance (in the navmesh component) to this amount when frozen (prevents other moving " + 
        "Somnids from getting stuck on this frozen one if they have a higher value")]
    [SerializeField, Range(0, 99)] private int frozenAvoidancePriority = 99;

    [Header("Look At Player On Freeze")]
    [Tooltip("Makes sure this Somnid looks at the player's camera position instead of the floor (since that is the player's origin, (0, 0, 0))")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.6f, 0f);
    [SerializeField, Min(0f)] private float lookDelay = 0.1f;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private SoundEffectSO footstepSFX;
    [SerializeField] private AudioSource jumpscareSource;
    [SerializeField] private SoundEffectSO freezeSFX;
    [SerializeField] private AudioSource wakeUpSource;
    [SerializeField] private SoundEffectSO wakeUpSFX;

    private FlashlightDetector flashlightDetector;
    private PlayerFootstepDetector playerFootstepDetector;
    private MonsterSight monsterSight;
    private LookAtTargetOnce lookAtTargetOnce;

    private Coroutine unfreezeRoutine = null;
    public bool awake { get; private set; }
    public bool frozen { get; private set; }

    private GameObject player = null;
    private LiminalLevelController liminalLevelController = null;

    protected override void Awake()
    {
        base.Awake();

        flashlightDetector = GetComponent<FlashlightDetector>();
        playerFootstepDetector = GetComponent<PlayerFootstepDetector>();
        monsterSight = GetComponent<MonsterSight>();
        lookAtTargetOnce = GetComponent<LookAtTargetOnce>();

        _sleepState = sleepBehavior;
        _idleState = idleBehavior;
        _chaseState = chaseBehavior;
        _searchState = searchBehavior;
        _caughtPlayerState = caughtPlayerBehavior;

        startState = sleepState;

        // Save this agent's priority and set back to this amount when woken up
        startingPriority = agent.avoidancePriority;
        agent.avoidancePriority = sleepAvoidancePriority;
    }

    protected override void Start()
    {
        // Disable monster sight while sleeping. Monster sight is re-enabled when this Somnid wakes up
        if (monsterSight != null)
        {
            monsterSight.enabled = false;
        }

        base.Start();

        if (LevelController.instance != null && LevelController.instance is LiminalLevelController)
        {
            liminalLevelController = LevelController.instance as LiminalLevelController;
            liminalLevelController.RegisterSomnid(this);
        }
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
    }

    /// <summary>
    ///     If this Somnid is not awake, wake it up. If it already is awake, freeze it.
    /// </summary>
    private void HandleMaxTimeReachedInFlashlight()
    {
        if (!awake)
        {
            WakeUp();
        }
        // This is its own if statement to make sure these sound effects don't play when this Somnid wakes up, but rather only
        // plays once it has been frozen by the player
        else if (!frozen)
        {
            freezeSFX.PlayOneShot(jumpscareSource);
            PlayFootstep();

            if (player != null && lookAtTargetOnce != null && stateMachine.currentEntityState != caughtPlayerState)
            {
                this.Invoke(() => lookAtTargetOnce.SetTargetAndLook(player.transform, offset), lookDelay);
            }
        }

        if (!frozen)
        {
            Freeze();
        }

        if (unfreezeRoutine != null)
        {
            StopCoroutine(unfreezeRoutine);
            unfreezeRoutine = null;
        }

        unfreezeRoutine = StartCoroutine(UnFreezeRoutine());
    }

    /// <summary>
    ///     If the player walks too close to this Somnid without crouching, wake this Somnid up.
    /// </summary>
    private void HandlePlayerFootstepHeard(Vector3 footstepOrigin)
    {
        if (!awake)
        {
            WakeUp();

            if (!freezeOnFootstepAwake)
            {
                return;
            }

            // If freezeOnFootstepAwake is true, then this Somnid will be frozen if the player wakes it up by stepping too close
            // (even if the player's flashlight is off or this Somnid was not in the flashlight's collider)
            if (freezeOnFootstepAwake && !frozen)
            {
                Freeze();
            }

            if (unfreezeRoutine != null)
            {
                StopCoroutine(unfreezeRoutine);
                unfreezeRoutine = null;
            }

            unfreezeRoutine = StartCoroutine(UnFreezeRoutine());
        }
    }

    /// <summary>
    ///     Face a Vector3 position.
    /// </summary>
    private void RotateTowards(Vector3 lookAtPos)
    {
        Vector3 targetDirection = (lookAtPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = lookRotation;
    }

    /// <summary>
    ///     Freeze this Somnid by stopping its movement.
    /// </summary>
    private void Freeze()
    {
        if (stateMachine.currentEntityState != searchState)
        {
            stateMachine.ChangeState(searchState);
        }

        // Select a random freeze animation
        int randomFreeze = Random.Range(anims.x, anims.y + 1); // + 1 since exclusive
        animator.SetInteger(stopAnimStr, randomFreeze);

        frozen = true;
        agent.speed = 0f;
        animator.SetInteger(frozenStr, 1);

        agent.avoidancePriority = frozenAvoidancePriority;
    }

    /// <summary>
    ///     Handle unfreezing this Somnid after a random duration (set in the inspector).
    /// </summary>
    /// <returns></returns>
    private IEnumerator UnFreezeRoutine()
    {
        float frozenTime = Random.Range(minTimeFrozen, maxTimeFrozen);
        yield return new WaitForSeconds(frozenTime);
        UnFreeze();
        unfreezeRoutine = null;
    }

    /// <summary>
    ///     Unfreeze this Somnid by resetting its movement speed back to what it started with.
    /// </summary>
    private void UnFreeze()
    {
        agent.avoidancePriority = startingPriority;

        ResetMovementSpeed();
        animator.SetInteger(frozenStr, 0);
        frozen = false;

        if (monsterSight != null)
        {
            monsterSight.ResetIsPlayerCurrentlySeen();
        }

        // Make sure to stop looking at the player's direction (when the player first stopped this Somnid)
        if (lookAtTargetOnce != null)
        {
            lookAtTargetOnce.Stop();
        }
    }

    /// <summary>
    ///     Wake this Somnid up.
    /// </summary>
    public void WakeUp(bool playWakeUpSFX = true)
    {
        awake = true;
        flashlightDetector.SetMaxTimeInFlashlight(flashlightFreezeTime);

        animator.SetTrigger("wakeUp");
        stateMachine.ChangeState(searchState);

        agent.avoidancePriority = startingPriority;

        if (playWakeUpSFX && wakeUpSFX != null)
        {
            wakeUpSFX.PlayOneShot(wakeUpSource, true, true);
        }

        if (monsterSight != null)
        {
            monsterSight.enabled = true;
        }

        if (disableFootstepDetectorOnAwake && playerFootstepDetector != null)
        {
            playerFootstepDetector.enabled = false;
        }

        // Look toward the player's direction
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        RotateTowards(player.transform.position);
    }

    private void HandlePlayerSeen()
    {
        if (frozen)
        {
            return;
        }

        stateMachine.ChangeState(chaseState);
    }

    private void FootstepEvent()
    {
        PlayFootstep();
    }

    private void PlayFootstep()
    {
        if (footstepSFX != null)
        {
            footstepSFX.Play(footstepSource);
        }
    }

#if UNITY_EDITOR
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            WakeUp();
        }

        base.Update();
    }
#endif

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (lookAtTargetOnce != null)
        {
            lookAtTargetOnce.Stop();
        }

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);

        // If this Somnid did not catch the player, freeze it so that the footsteps are not heard if it happens to be close by
        // the player and Somnid that actually caught the player
        if (awake && monster != this)
        {
            Freeze();
        }
    }
}
