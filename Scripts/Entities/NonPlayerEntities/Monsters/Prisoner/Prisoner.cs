using System.Collections;
using UnityEngine;

public class Prisoner : Monster, IIdleStateUser, IStalkStateUser, ISearchStateUser, IChaseStateUser, ICaughtPlayerStateUser, IDisappearStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private StalkState _stalkState;
    public StalkState stalkState => _stalkState;

    private SearchState _searchState;
    public SearchState searchState => _searchState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private DisappearState _disappearState;
    public DisappearState disappearState => _disappearState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    private enum Behavior
    {
        Search,
        Stalk
    }

    // Animator variables
    private const string animatorCowering = "cowering"; // Int, where 1 == cowering, 0 == not cowering
    private const string animatorScared = "scared"; // Int, where 1 == scared, 0 == not scared
    private const string animatorRunning = "running"; // Int, where 1 == running, 0 == not running

    [Header("Behavior Setting")]
    [Tooltip("The behavior of this prisoner")]
    [SerializeField] private Behavior behavior = Behavior.Stalk;

    [Header("Behavior References")]
    [SerializeField] private IdleState idleBehavior;
    [SerializeField] private StalkState stalkBehavior;
    [SerializeField] private SearchState searchBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;
    [SerializeField] private DisappearState disappearBehavior;

    [Header("Cowering")]
    [Tooltip("When cowering, this prisoner will remain cowering between this amount of time")]
    [SerializeField] private Vector2 cowerTimeRange = new Vector2(8f, 12f);
    private Coroutine cowerRoutine = null;
    private bool cowering;
    private bool wasCowering;

    [Header("Scared")]
    [Tooltip("The prisoner will become scared when the Warden is this distance or closer")]
    [SerializeField] private float scaredDistance = 15f;
    [Tooltip("This prisoner will calculate its distance to Warden and act accordingly every X amount of seconds")]
    [SerializeField] private float monitorDistanceEveryXSeconds = 1f;
    private bool scared;

    [Header("Collider")]
    [Tooltip("If enabled, the prisoner will be unable to kill the player if cowering or scared. It does this by turning off " +
        "the trigger on the collider")]
    [SerializeField] private bool turnOffColWhenCoweringOrScared = true;
    [SerializeField] private CapsuleCollider col;
    [Tooltip("The radius is reduced to make sure that, while scared or cowering, the prisoner does not block paths")]
    [SerializeField] private float scaredColRadius = 0.065f;
    private float startingColRadius;

    [Header("Listening For Player Footsteps")]
    [Tooltip("If the path to the footstep audio cue is longer than this distance, this Prisoner will not move to the footstep audio position")]
    [SerializeField, Min(0)] private float footstepMaxMoveDist = 50f;

    [Tooltip("This Prisoner can only investigate footstep audio positions every X amount of seconds")]
    [SerializeField, Min(0)] private float investigateFootstepCooldown = 5f;
    private bool onInvestigateFootstepCooldown;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO footstepSFX;
    [SerializeField] private SoundEffectSO hissSFX;
    [SerializeField] private SoundEffectSO playerSpottedSFX;

    private GameObject instantiatedWarden;

    private MonsterSight monsterSight;
    private FlashlightDetector flashlightDetector;
    private PlayerFootstepDetector playerFootstepDetector;

    private PrisonerAudioHandler audioHandler;

    protected override void Awake()
    {
        base.Awake();

        monsterSight = GetComponent<MonsterSight>();
        flashlightDetector = GetComponent<FlashlightDetector>();
        playerFootstepDetector = GetComponent<PlayerFootstepDetector>();

        audioHandler = GetComponent<PrisonerAudioHandler>();

        _idleState = idleBehavior;
        _stalkState = stalkBehavior;
        _searchState = searchBehavior;
        _chaseState = chaseBehavior;
        _caughtPlayerState = caughtPlayerBehavior;
        _disappearState = disappearBehavior;

        startState = behavior switch
        {
            Behavior.Search => searchState,
            Behavior.Stalk => stalkState,
            _ => stalkState // Default to stalk state
        };
    }

    protected override void Start()
    {
        base.Start();

        onInvestigateFootstepCooldown = false;
        startingColRadius = col.radius;

        StartCoroutine(MonitorDistanceToWarden());
#if UNITY_EDITOR
        GetWardenReference(null);
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen += ChasePlayer;
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight += Cower;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard += InvestigatePlayerFootstep;
        }

        if (chaseBehavior != null)
        {
            chaseBehavior.OnEndChase += HandleEndChase;
        }

        DungeonLevelController.OnWardenSpawned += GetWardenReference;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen -= ChasePlayer;
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight -= Cower;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard -= InvestigatePlayerFootstep;
        }

        if (chaseBehavior != null)
        {
            chaseBehavior.OnEndChase -= HandleEndChase;
        }

        DungeonLevelController.OnWardenSpawned -= GetWardenReference;
    }

    /// <summary>
    ///     Monitors the distance to the Warden and becomes scared if close enough.
    /// </summary>
    private IEnumerator MonitorDistanceToWarden()
    {
        while (true)
        {
            yield return new WaitForSeconds(monitorDistanceEveryXSeconds);

            if (instantiatedWarden != null)
            {
                float distanceToWarden = (transform.position - instantiatedWarden.transform.position).magnitude;

                if (!scared && distanceToWarden <= scaredDistance)
                {
                    Scared();
                }
                else if (!cowering && scared && distanceToWarden > scaredDistance)
                {
                    NotScared();
                }
                else if (IsEntityInSpecificState(caughtPlayerState) && scared)
                {
                    NotScared();
                }
            }
        }
    }

    /// <summary>
    ///     Get a reference to the Warden. This method should be subscribed to DungeonLevelController.OnWardenSpawned
    /// </summary>
    /// <param name="warden">The instantiated Warden game object.</param>
    private void GetWardenReference(GameObject warden)
    {
        instantiatedWarden = warden;

        // Fallback
        if (instantiatedWarden == null)
        {
            Warden wardenCS = FindFirstObjectByType<Warden>();

            if (wardenCS != null)
            {
                instantiatedWarden = wardenCS.gameObject;
#if UNITY_EDITOR
                Debug.Log("instantiatedWarden reference found using FindFirstObjectByType");
#endif
            }
        }
    }

    /// <summary>
    ///     Begin chasing the player. This prisoner will not chase the player if scared or cowering, however.
    /// </summary>
    public void ChasePlayer()
    {
        if (IsEntityInSpecificState(chaseState) || IsEntityInSpecificState(caughtPlayerState) || scared || cowering)
        {
            return;
        }

        PlayPlayerSpottedSFX();

        animator.SetInteger(animatorRunning, 1); // Have the prisoner use the running animation when chasing the player
        stateMachine.ChangeState(chaseState);
    }

    /// <summary>
    ///     Subscribes to the ChaseState's OnEndChase to handle any logic that should be used when no longer chasing the player.
    /// </summary>
    private void HandleEndChase()
    {
        animator.SetInteger(animatorRunning, 0);
    }

    /// <summary>
    ///     Begin cowering.
    /// </summary>
    public void Cower()
    {
        if (scared || IsEntityInSpecificState(caughtPlayerState))
        {
            return;
        }

        CanKillPlayer(false);
        cowering = true;

        // Reset the coroutine so that this prisoner stays cowering if the player keeps their flashlight shining on it
        if (cowerRoutine != null)
        {
            StopCoroutine(cowerRoutine);
            cowerRoutine = null;
        }
        // Only play the hiss SFX if the prisoner is not already cowering
        else
        {
            PlayHissSFX();
        }

        cowerRoutine = StartCoroutine(CowerRoutine());
    }

    /// <summary>
    ///     Handles cowering/idling for a specified period of time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CowerRoutine()
    {
        wasCowering = true;
        stateMachine.ChangeState(idleState);
        animator.SetInteger(animatorCowering, 1);

        float timeToCower = UnityEngine.Random.Range(cowerTimeRange.x, cowerTimeRange.y);
        yield return new WaitForSeconds(timeToCower);

        animator.SetInteger(animatorCowering, 0);

        cowering = false;
        wasCowering = false;
        CanKillPlayer(true);

        // Prevents an issue where if the prisoner starts cowering and the player does not leave the prisoner's monster sight, the prisoner
        // will not start chasing the player since MonsterSight requires the player to exit the sight at least once before re-firing off the
        // OnPlayerSeen event
        if (monsterSight != null)
        {
            monsterSight.ResetIsPlayerCurrentlySeen();
        }

        ChangeStateToBehavior(behavior);
        cowerRoutine = null;
    }

    /// <summary>
    ///     Make this prisoner become scared: this prisoner will enter a idle state and begin a scared animation and will not do anything.
    /// </summary>
    public void Scared()
    {
        if (cowerRoutine != null)
        {
            StopCoroutine(cowerRoutine);
            animator.SetInteger(animatorCowering, 0);
            cowering = false;
        }

        CanKillPlayer(false);
        scared = true;

        stateMachine.ChangeState(idleState);
        animator.SetInteger(animatorScared, 1);
    }

    /// <summary>
    ///     Make this prisoner no longer scared.
    /// </summary>
    public void NotScared()
    {
        animator.SetInteger(animatorScared, 0);
        scared = false;

        if (wasCowering)
        {
            Cower();
        }
        else
        {
            CanKillPlayer(true);

            if (monsterSight != null)
            {
                monsterSight.ResetIsPlayerCurrentlySeen();
            }

            ChangeStateToBehavior(behavior);
        }
    }

    /// <summary>
    ///     Play a footstep sound effect.
    /// </summary>
    public void FootstepEvent()
    {
        if (footstepSFX != null)
        {
            footstepSFX.Play(audioHandler.footstepAudioSource);
        }
    }

    /// <summary>
    ///     Change whether or not this prisoner can kill the player by setting the collider's trigger to be active or inactive.
    /// </summary>
    /// <param name="canKillPlayer">Whether or not this prisoner can kill the player.</param>
    private void CanKillPlayer(bool canKillPlayer)
    {
        if (turnOffColWhenCoweringOrScared && col != null)
        {
            col.isTrigger = canKillPlayer;
            col.radius = canKillPlayer ? startingColRadius : scaredColRadius;
        }
    }

    /// <summary>
    ///     Investigate a player footstep audio cue.
    /// </summary>
    /// <param name="footstepOrigin">The origin of the footstep audio cue.</param>
    private void InvestigatePlayerFootstep(Vector3 footstepOrigin)
    {
        // Make sure prisoner is not scared and the footstep is within a reasonable walking distance to investigate
        if (!cowering && !scared && !onInvestigateFootstepCooldown && GetPathDistance(footstepOrigin) < footstepMaxMoveDist)
        {
            onInvestigateFootstepCooldown = true;
            SetDestination(footstepOrigin);
            this.Invoke(() => onInvestigateFootstepCooldown = false, investigateFootstepCooldown);
        }
    }

    /// <summary>
    ///     Change this prisoner's state to the target behavior (stalk or search).
    /// </summary>
    /// <param name="behavior">The targeted behavior.</param>
    private void ChangeStateToBehavior(Behavior behavior)
    {
        switch (behavior)
        {
            case Behavior.Search:
                stateMachine.ChangeState(searchState);
                break;
            case Behavior.Stalk:
                stateMachine.ChangeState(stalkState);
                break;
            default:
                stateMachine.ChangeState(stalkState);
                break;
        }
    }

    protected override void HandleKillPlayer(PlayerReferences playerReferences, Monster monster)
    {
        /// The reason Prisoner doesn't check if the monster that caught the player is this prisoner is because since there will be
        /// multiple prisoners in Dungeon HM, all prisoners should stop moving if one catches the player to make sure that another
        /// deathscreen jumpscare does not play again if another prisoner runs into the player

        EnableDetectors(false);

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);
    }

    protected override void HandleDamagePlayer(Monster monster)
    {
        if (monster == this)
        {
            stateMachine.ChangeState(disappearState);
        }
    }

    /// <summary>
    ///     Enable or disable player detectors: MonsterSight, PlayerFootstepDetector, and FlashlightDetectors
    /// </summary>
    private void EnableDetectors(bool enabled)
    {
        if (monsterSight != null)
        {
            monsterSight.enabled = enabled;
            monsterSight.ResetIsPlayerCurrentlySeen();
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.enabled = enabled;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.enabled = enabled;
        }
    }

    /// <summary>
    ///     Plays the hiss sound effect.
    /// </summary>
    private void PlayHissSFX()
    {
        if (hissSFX == null)
        {
            return;
        }

        hissSFX.PlayOneShot(audioHandler.sfxAudioSource);
    }

    /// <summary>
    ///     Plays the player spotted sound effect.
    /// </summary>
    private void PlayPlayerSpottedSFX()
    {
        if (playerSpottedSFX == null)
        {
            return;
        }

        playerSpottedSFX.Play(audioHandler.sfxAudioSource);
    }
}
