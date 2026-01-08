using UnityEngine;

public class Stitch : Monster, IIdleStateUser, IChaseStateUser, IStalkStateUser, IRetreatStateUser, ICaughtPlayerStateUser, IDisappearStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private StalkState _stalkState;
    public StalkState stalkState => _stalkState;

    private RetreatState _retreatState;
    public RetreatState retreatState => _retreatState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private DisappearState _disappearState;
    public DisappearState disappearState => _disappearState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    // Animator
    private const string canSprintStr = "canSprint";

    [Header("Behavior References")]
    [SerializeField] private IdleState idleBehavior;
    [SerializeField] private ChaseState chasePlayerBehavior;
    [SerializeField] private StalkState stalkPlayerBehavior;
    [SerializeField] private RetreatState retreatBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;
    [SerializeField] private DisappearState disappearBehavior;

    [Header("Flash Resistance Settings")]
    [Tooltip("If enabled, Stitch's flash resistence every [flashIncreaseInterval] times scared away")]
    [SerializeField] private bool includeFlashResistance = true;
    [Tooltip("The maximum amount of times that Stitch needs to be flashed before being scared away")]
    [SerializeField] private int maxFlashesNeeded = 7;
    [Tooltip("Everytime Stitch is scared away this many times, the flashes needed to scare Stitch increase by one (up to the maximum)")]
    [SerializeField] private int flashIncreaseInterval = 2;
    [Tooltip("If enabled, the flashes needed to scare Stitch increases by 1 the very first time Stitch is scared away rather than " +
        "relying on flashIncreaseInterval at first")]
    [SerializeField] private bool firstScareIncrease = true;
    private int timesScaredAway;

    [Header("Look At Player Settings")]
    [Tooltip("Turns off LookAtTarget.cs while Stitch is retreating so Stitch is not looking at the player while running away")]
    [SerializeField] private bool stopLookAtOnRetreat = true;
    [Tooltip("A delay before turning off LookAtTarget.cs (Felt unnatural for Stitch to stop looking as soon as entering retreat state)")]
    [SerializeField] private float turnOffLookAtTargetDelay = 0.33f;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO footstepSFX;
    [SerializeField] private float footstepSFXCooldownTime = 0.3f;
    private bool footstepSFXOnCooldown; // Prevents an issue where Stitch's footstep would play a lot during animation transitions

    [SerializeField] private SoundEffectSO playerSpottedSFX;
    [Tooltip("0 == Never play, 100 == Always play and any number in between is the percent change to play playerSpottedSFX")]
    [SerializeField, Range(0, 100)] private int chanceToPlayPlayerSpottedSFX = 100;

    private StitchAudioHandler audioHandler;

    private MonsterSight monsterSight;
    private FlashlightFlashingDetector flashlightFlashingDetector;
    private LookAtTarget lookAtTarget;

    protected override void Awake()
    {
        base.Awake();

        audioHandler = GetComponent<StitchAudioHandler>();
        monsterSight = GetComponent<MonsterSight>();
        flashlightFlashingDetector = GetComponent<FlashlightFlashingDetector>();
        lookAtTarget = GetComponent<LookAtTarget>();

        _idleState = idleBehavior;
        _chaseState = chasePlayerBehavior;
        _stalkState = stalkPlayerBehavior;
        _retreatState = retreatBehavior;
        _caughtPlayerState = caughtPlayerBehavior;
        _disappearState = disappearBehavior;

        startState = stalkState;
    }

    protected override void Start()
    {
        base.Start();

        timesScaredAway = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen += HandlePlayerSeen;
        }

        if (flashlightFlashingDetector != null)
        {
            flashlightFlashingDetector.OnMaxFlashesReached += HandleScaredAway;
        }

        if (retreatBehavior != null)
        {
            retreatBehavior.OnRetreatEnded += HandleRetreatEnded;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen -= HandlePlayerSeen;
        }

        if (flashlightFlashingDetector != null)
        {
            flashlightFlashingDetector.OnMaxFlashesReached -= HandleScaredAway;
        }

        if (retreatBehavior != null)
        {
            retreatBehavior.OnRetreatEnded -= HandleRetreatEnded;
        }
    }

    private void HandlePlayerSeen()
    {
        // IsEntityInSpecificState() prevents chaseState.EnterLogic() from running multiples times while chasing the player and
        // from Shade entering chase state whilst retreating
        if (IsEntityInSpecificState(chaseState) || IsEntityInSpecificState(retreatState) || IsEntityInSpecificState(caughtPlayerState))
        {
            return;
        }

        PlayerSpottedSFX(chanceToPlayPlayerSpottedSFX);

        stateMachine.ChangeState(chaseState);
    }

    private void HandleScaredAway()
    {
        if (flashlightFlashingDetector.enabled && !IsEntityInSpecificState(retreatState) && !IsEntityInSpecificState(caughtPlayerState))
        {
            // Make Stitch stop looking at the player when retreating, because I found it looks a little weird for Stitch to keep on looking
            // at the player when running away
            if (stopLookAtOnRetreat && lookAtTarget != null)
            {
                this.Invoke(() => lookAtTarget.enabled = false, turnOffLookAtTargetDelay);
            }

            if (includeFlashResistance && firstScareIncrease)
            {
                flashlightFlashingDetector.SetMinTimesFlashed(flashlightFlashingDetector.minTimesFlashed + 1);
                firstScareIncrease = false;
            }
            else
            {
                timesScaredAway++;

                // Slowly increase the flash requirements to scare Stitch away
                if (includeFlashResistance && timesScaredAway % flashIncreaseInterval == 0 && flashlightFlashingDetector.minTimesFlashed < maxFlashesNeeded)
                {
                    flashlightFlashingDetector.SetMinTimesFlashed(flashlightFlashingDetector.minTimesFlashed + 1);
                }
            }

            stateMachine.ChangeState(retreatState);
            animator.SetInteger(canSprintStr, 1); // Allow Stitch to use the sprint animation only when retreating
        }
    }

    private void HandleRetreatEnded()
    {
        if (lookAtTarget != null && !lookAtTarget.enabled)
        {
            lookAtTarget.enabled = true;
        }

        animator.SetInteger(canSprintStr, 0);
    }

    public void FootstepEvent()
    {
        if (!footstepSFXOnCooldown && footstepSFX != null)
        {
            footstepSFX.Play(audioHandler.footstepAudioSource);
            footstepSFXOnCooldown = true;
            this.Invoke(() => footstepSFXOnCooldown = false, footstepSFXCooldownTime);
        }
    }

    protected override void HandleKillPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this || IsEntityInSpecificState(retreatState))
        {
            return;
        }

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
    ///     Check if Stitch is currently in retreat state.
    /// </summary>
    /// <returns>True if Stitch is currently in retreat state, false if otherwise.</returns>
    public bool IsRetreating()
    {
        return IsEntityInSpecificState(retreatState);
    }

    /// <summary>
    ///     Plays the player spotted SFX via a percentage chance.
    /// </summary>
    /// <param name="chance">The chance to play the player spotted SFX.</param>
    private void PlayerSpottedSFX(int chance)
    {
        if (playerSpottedSFX == null)
        {
            return;
        }

        int random = Random.Range(1, 101);

        if (random <= chance)
        {
            playerSpottedSFX?.Play();
        }
    }
}
