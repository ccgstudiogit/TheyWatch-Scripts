using System.Collections;
using UnityEngine;

public class Shade : Monster, IStalkStateUser, IRetreatStateUser, IChaseStateUser, ICaughtPlayerStateUser, IIdleStateUser
{
    private StalkState _stalkState;
    public StalkState stalkState => _stalkState;

    private RetreatState _retreatState;
    public RetreatState retreatState => _retreatState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behavior References")]
    [SerializeField] private StalkState stalkStateBehavior;
    [SerializeField] private RetreatState retreatStateBehavior;
    [SerializeField] private ChaseState chaseStateBehavior;
    [SerializeField] private ChasePlayerTimed berserkChaseBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;
    [SerializeField] private IdleState idleStateBehavior;
    [SerializeField] private IdleDoNothing idleDoNothingBehavior;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectSO footstepSFX;
    [Tooltip("The SFX that will play when entering retreat state")]
    [SerializeField] private SoundEffectSO fleeNoiseSFX;
    [Tooltip("The SFX that will play when Shade spots the player")]
    [SerializeField] private SoundEffectSO playerSpottedNoiseSFX;
    [Tooltip("The chance that Shade will make a noise when spotting the player")]
    [SerializeField, Range(0, 1)] private float playerSpottedNoiseChance = 0.65f;

    [Tooltip("Flee and player spotted SFX will have this cooldown so the SFX don't stack")]
    [SerializeField] private float noiseSFXCooldownTime = 1.5f;
    private bool noiseSFXOnCooldown; // fleeNoiseSFx & playerSpottedSFX will only play if this is false

    [Header("Berserk")]
    [Tooltip("After the player scares away Shade this many times, the next time Shade would retreat, go berserk instead")]
    [SerializeField] private int timesUntilBerserk = 2;
    private int retreatCounter; // Keeps track of how many times Shade has retreated so after a certain amount of times Shade can go berserk
    [Tooltip("Every time Shade enters berserk chase, Shade's speed is multiplied by this amount. This creates an increasing " +
        "difficulty in that every time Shade goes berserk, shade's berserk chase speed gets faster each time")]
    [SerializeField] private float chaseSpeedMultiplier = 1.1f;

    [Header("Visibility")]
    [SerializeField] private bool turnOffRenderersWhenRetreating = true;
    [Tooltip("A delay before the meshrenderers are turned off")]
    [SerializeField] private float meshRendererDisabledDelay = 0.25f;

    private FlashlightDetector flashlightDetector;
    private MonsterSight monsterSight;

    private ShadeAudioHandler audioHandler;
    private ShadeBerserkHandler berserkHandler;
    private ShadeRendererHandler rendererHandler;

    private ShadeDeathscreenJumpscare shadeDeathscreenJumpscare;

    // Flag to keep track of BeginGlobalChase() since that method technically uses berserkChaseBehavior. If the player successfully
    // scares Shade away with the flashlight, Shade will check if this flag is true and then switch the chase behavior back to
    // the base chase behavior
    public bool isGloballyChasing { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        flashlightDetector = GetComponent<FlashlightDetector>();
#if UNITY_EDITOR
        if (flashlightDetector == null)
        {
            Debug.LogWarning($"{gameObject.name} does not have a FlashlightDetector component.");
        }
#endif

        monsterSight = GetComponent<MonsterSight>();
#if UNITY_EDITOR
        if (monsterSight == null)
        {
            Debug.LogWarning($"{gameObject.name} does not have a MonsterSight component.");
        }
#endif

        audioHandler = GetComponent<ShadeAudioHandler>();
        berserkHandler = GetComponent<ShadeBerserkHandler>();
        rendererHandler = GetComponent<ShadeRendererHandler>();
        shadeDeathscreenJumpscare = GetComponent<ShadeDeathscreenJumpscare>();

        _stalkState = stalkStateBehavior;
        _retreatState = retreatStateBehavior;
        _chaseState = chaseStateBehavior;
        _caughtPlayerState = caughtPlayerBehavior;
        _idleState = idleStateBehavior;

        startState = stalkState;
    }

    protected override void Start()
    {
        base.Start();

        retreatCounter = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight += HandleReachedMaxTimeInFlashlight;
        }

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen += HandlePlayerSeen;
        }

        if (berserkChaseBehavior != null)
        {
            berserkChaseBehavior.OnStopChase += ExitBerserk;
        }

        if (berserkHandler != null)
        {
            berserkHandler.OnBerserkReady += StartBerserkChase;
        }

        if (retreatStateBehavior != null)
        {
            retreatState.OnRetreatEnded += EnableRenderers;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight -= HandleReachedMaxTimeInFlashlight;
        }

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen -= HandlePlayerSeen;
        }

        if (berserkChaseBehavior != null)
        {
            berserkChaseBehavior.OnStopChase -= ExitBerserk;
        }

        if (berserkHandler != null)
        {
            berserkHandler.OnBerserkReady -= StartBerserkChase;
        }

        if (retreatStateBehavior != null)
        {
            retreatState.OnRetreatEnded -= EnableRenderers;
        }
    }

    private void HandleReachedMaxTimeInFlashlight()
    {
        // Prevents an unlikely but possible issue where Shade will become berserk after catching the player (if the right conditions are met)
        if (shadeDeathscreenJumpscare != null && shadeDeathscreenJumpscare.caughtPlayer)
        {
            return;
        }

        if (flashlightDetector.enabled && !IsEntityInSpecificState(retreatState) && !IsEntityInSpecificState(caughtPlayerState))
        {
            // If Shade chased the player from BeginGlobalChase(), once Shade is scared away from the flashlight (or enters berserk) make sure
            // Shade's chase state behavior is swapped back to the base chase behavior and that the movement speed is reset
            if (isGloballyChasing)
            {
                ResetChaseBehaviorFromGlobalToBase();
            }

            // If scared away too many times, instead of going into retreat become berserk and chase the player
            if (HelperMethods.NotNullAndEnabled(berserkHandler) && retreatCounter >= timesUntilBerserk)
            {
                BecomeBerserk();
            }
            else
            {
                // Only play the flee noise if currently not on the noise SFX cooldown
                if (!noiseSFXOnCooldown)
                {
                    noiseSFXOnCooldown = true;
                    audioHandler.PlaySFXFromPrefab(fleeNoiseSFX, transform.position);
                    Invoke(nameof(ResetNoiseSFXCooldown), noiseSFXCooldownTime);
                }

                retreatCounter++;
                stateMachine.ChangeState(retreatState);

                if (turnOffRenderersWhenRetreating)
                {
                    Invoke(nameof(DisableRenderers), meshRendererDisabledDelay); // Shade becomes invisible when retreating
                }
            }
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

        // Make sure that before chasing the player, the chase behavior is the base chase behavior
        if (isGloballyChasing)
        {
            ResetChaseBehaviorFromGlobalToBase();
        }

        // Only play player spotted noise SFX if not on cooldown and if the random chance is met
        if (!noiseSFXOnCooldown && playerSpottedNoiseChance > Random.value)
        {
            noiseSFXOnCooldown = true;
            playerSpottedNoiseSFX.PlayOneShot(audioHandler.sfxAudioSource);
            Invoke(nameof(ResetNoiseSFXCooldown), noiseSFXCooldownTime);
        }

        stateMachine.ChangeState(chaseState);
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        // If Shade is retreating or invisible, don't do anything when colliding with the player
        if (monster != this || IsEntityInSpecificState(retreatState) || !rendererHandler.bodyMeshRenderer.enabled)
        {
            return;
        }

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);
    }

    public void FootstepEvent()
    {
        // Only play sfx when the mesh renderers are enabled
        if (rendererHandler.bodyMeshRenderer.enabled && rendererHandler.eyesMeshRenderer.enabled)
        {
            footstepSFX.Play(audioHandler.footstepAudioSource);
        }
    }

    /// <summary>
    ///     While berserk, Shade relentlessly chases the player and cannot be scared away from the player's flashlight.
    /// </summary>
    private void BecomeBerserk()
    {
        retreatCounter = 0;

        // Disable both monster sight and flashlight detector to prevent them from interfering with berserk chase
        if (monsterSight != null)
        {
            monsterSight.enabled = false;
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.enabled = false;
        }

        // Change idle state behavior to "do nothing" in preparation for the stun animation
        SwitchStateBehavior(ref _idleState, idleDoNothingBehavior);
        stateMachine.ChangeState(idleState);

        berserkHandler.BecomeBerserk();
    }

    /// <summary>
    ///     Listens for berserkHandler to fire off an event to let this method know when to officially begin the chase.
    /// </summary>
    private void StartBerserkChase()
    {
        // Increase Shade's chase speed each time Shade becomes berserk
        berserkChaseBehavior.AgentSpeedMultiplier(chaseSpeedMultiplier);

        // Start the chase
        SwitchStateBehavior(ref _chaseState, berserkChaseBehavior);
        stateMachine.ChangeState(chaseState);

        // Swap the idle state behavior from do nothing back to the base behavior
        SwitchStateBehavior(ref _idleState, idleStateBehavior);
    }

    /// <summary>
    ///     Listens for berserkChaseBehavior to fire off an event and officially stop that chase after a set amount of time.
    /// </summary>
    private void ExitBerserk()
    {
        if (HelperMethods.NotNullAndEnabled(berserkHandler) && !berserkHandler.currentlyBerserk)
        {
#if UNITY_EDITOR
            Debug.LogWarning("EndBerserkChase() called but Shade is not currently berserk!");
#endif
            return;
        }

        StartCoroutine(ExitBerserkRoutine());
    }

    private IEnumerator ExitBerserkRoutine()
    {
        // Swap chase state back to the base chase state behavior and retreat
        SwitchStateBehavior(ref _chaseState, chaseStateBehavior);
        stateMachine.ChangeState(retreatState);

        berserkHandler.ExitBerserk();

        // Wait a bit before becoming invisible using meshRendererDisabledDelay but shorter
        yield return new WaitForSeconds(meshRendererDisabledDelay / 2);

        // Hide the mesh renderers whilst retreating
        if (turnOffRenderersWhenRetreating && HelperMethods.NotNullAndEnabled(rendererHandler))
        {
            rendererHandler.SetVisible(false);
        }

        // Re-enable monster sight and flashlight detector after a delay (gives time for Shade to retreat before accidentally
        // immediately re-chasing the player if monster sight detects the player)
        yield return new WaitForSeconds(2f);

        if (monsterSight != null)
        {
            monsterSight.enabled = true;
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.enabled = true;
        }
    }

    /// <summary>
    ///     Sets Shade's body and eye mesh renderers to be enabled.
    /// </summary>
    // Note: This method is currently only used to subscribe to retreat state's event OnRetreatEnded to set the renderers
    // to be visible again
    private void EnableRenderers()
    {
        if (HelperMethods.NotNullAndEnabled(rendererHandler))
        {
            rendererHandler.SetVisible(true);
        }
    }

    /// <summary>
    ///     Sets Shade's body and eye mesh renderers to be disabled.
    /// </summary>
    private void DisableRenderers()
    {
        if (HelperMethods.NotNullAndEnabled(rendererHandler))
        {
            rendererHandler.SetVisible(false);
        }
    }

    private void ResetNoiseSFXCooldown()
    {
        noiseSFXOnCooldown = false;
    }

    /// <summary>
    ///     Check whether Shade is currently in retreat state.
    /// </summary>
    /// <returns>True if Shade is in retreat state, false if otherwise.</returns>
    // Note: This is so that ShadeDeathscreenJumpscare doesn't trigger when Shade is retreating
    public bool IsRetreating()
    {
        return IsEntityInSpecificState(retreatState);
    }

    /// <summary>
    ///     Check whether Shade is currently in chase state.
    /// </summary>
    /// <returns>True if Shade is in chase state, false if otherwise.</returns>
    // Note: This is for ShadeHMHandler.cs, so it does not set Shade's speed to 0 if Shade is currently chasing
    public bool IsChasing()
    {
        return IsEntityInSpecificState(chaseState);
    }

    /// <summary>
    ///     When this method is called, no matter where Shade is on the map Shade will move towards the player.
    /// </summary>
    public void BeginGlobalChase(float globalChaseSpeed)
    {
        // Don't begin global chase if Shader is already berserk, since berserk uses berserkChaseBehavior
        if (berserkHandler.currentlyBerserk)
        {
#if UNITY_EDITOR
            Debug.LogWarning("BeginGlobalChase() was called while Shade is berserk!");
#endif
            return;
        }

        // Make sure that if Shade was retreating and BeginGlobalChase is called, the mesh renderers are turned back on. This shouldn't
        // need to be called since OnRetreatEnded turns the mesh renderers back on but this is here for redudancy
        if (!rendererHandler.IsVisible())
        {
            EnableRenderers();
        }

        isGloballyChasing = true;
        SwitchStateBehavior(ref _chaseState, berserkChaseBehavior);
        stateMachine.ChangeState(chaseState);

        // Increase the speed, it's also important to call this after ChangeState so ChaseState.EnterState() does not override the changed speed
        agent.speed = globalChaseSpeed;

#if UNITY_EDITOR
        // Keeping these logs here for future debugging
        Debug.Log("BeginGlobalChase() called for Shade, with Shade's new speed being: " + agent.speed);
#endif
    }

    /// <summary>
    ///     Makes sure that after the global chase (started by BeginGlobalChase()) the chase behavior is changed
    ///     back to the base chase state behavior.
    /// </summary>
    private void ResetChaseBehaviorFromGlobalToBase()
    {
        SwitchStateBehavior(ref _chaseState, chaseStateBehavior);
        ResetMovementSpeed(); // Make sure to reset the increased speed
        isGloballyChasing = false;

#if UNITY_EDITOR
        // Keeping these logs here for future debugging
        Debug.Log("ResetChaseBehaviorFromGlobalToBase() called for Shade, with Shade's speed being reset back to " + agent.speed);
#endif
    }
}
