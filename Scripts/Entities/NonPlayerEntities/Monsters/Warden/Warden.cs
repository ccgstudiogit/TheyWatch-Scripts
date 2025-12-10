using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Warden : Monster, IIdleStateUser, ISearchStateUser, IChaseStateUser, ICaughtPlayerStateUser
{
    // Currently used by SteamDungeonHMAchievement to keep track of how many times the Warden spots the player
    public static event Action OnWardenSpottedPlayer;

    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private SearchState _searchState;
    public SearchState searchState => _searchState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    // Animator variables
    private const string animatorKneel = "kneel"; // Trigger
    private const string animatorKneelExit = "kneelExit"; // Trigger

    [Header("Behavior References")]
    [SerializeField] private IdleState idleDoNothingBehavior;
    [SerializeField] private IdleState idleTrackPlayerBehavior;
    [SerializeField] private SearchState searchBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;

    [Header("Listening For Player Footsteps")]
    [Tooltip("If the path to the footstep audio cue is longer than this distance, the Warden will not move to the footstep audio position")]
    [SerializeField, Min(0)] private float footstepMaxMoveDist = 35f;

    [Tooltip("The Warden can only investigate footstep audio positions every X amount of seconds")]
    [SerializeField, Min(0)] private float investigateFootstepCooldown = 5f;
    private bool onInvestigateFootstepCooldown;

    [Header("Bonus Speed")]
    [Tooltip("If enabled, the Warden will get a speed boost (speedWhenFarAway) when far enough away from the player. This is implemented to create " +
        "more tension and to make sure that the Warden is able to catch up to the player if far enough away")]
    [SerializeField] private bool enableSpeedBoostWhenFarAway = true;
    private float distance = 0f;

#if UNITY_EDITOR
    [SerializeField] private Color speedBoostArcColor = Color.magenta;
#endif

    [Tooltip("The speed that Warden will have when far enough away from the player")]
    [SerializeField, Min(0)] private float speedWhenFarAway = 15f;

    [SerializeField, Min(0)] private float minDistForSpeedBoost = 30f;
    private float startingSpeed = 0;

    [Header("Reset Search Radius")]
    [Tooltip("If enabled, the search radius of search state will be reset when Warden gets close enough to the player")]
    [SerializeField] private bool enableResetSearchRadius = true;

#if UNITY_EDITOR
    [SerializeField] private Color resetSearchRadiusArcColor = Color.yellow;
#endif

    [Tooltip("If the Warden gets within this distance to the player, the search radius is reset. This is used to make the searching feel more fair, " +
        "as if the Warden gets really, really close to the player but the player manages to remain hidden, the player should be rewarded")]
    [SerializeField] private float minDistForSearchReset = 7.5f;

    [Tooltip("The search radius can only be reset every X amount of seconds at most")]
    [SerializeField] private float resetSearchRadiusCooldown = 25f;
    private bool onResetSearchRadiusCooldown;

    [Header("Begin Chase Settings")]
    [Tooltip("Gives the player a headstart")]
    [SerializeField] private float timeToWaitBeforeChase = 2.9f;

    [Tooltip("The SFX that will play once the Warden finds the player")]
    [SerializeField] private SoundEffectSO foundPlayerSFX;
    [SerializeField] private SoundEffectSO growlSFX;
    [SerializeField] private float growlSFXDelay = 0.65f;

    private Coroutine beginChaseRoutine = null;

    [Header("Lights")]
    [SerializeField] private Light[] lights;
    [Tooltip("The color of the lights when Warden chases the player")]
    [SerializeField] private Color chaseLightColor = Color.red;
    private Color lightStartingColor; // A reference to the lights' starting color so that once the chase ends, they will go back to this color

    [Header("End Chase Settings")]
    [Tooltip("The time Warden will spend kneeling after a chase, giving the player a chance to get away")]
    [SerializeField] private float timeToSpendResting = 10f;
    private Coroutine endChaseRoutine = null;

    [Header("Footstep SFX")]
    [SerializeField] private SoundEffectSO footstepSFX;
    [SerializeField] private float footstepSFXCooldownTime = 0.225f;
    private bool footstepSFXOnCooldown;

    [Header("Footstep Impulse")]
    [Tooltip("The cinemachine camera shake will only fire off if the player is within this distance to the Warden")]
    [SerializeField] private float maxDistanceForImpulse = 9f;

    [Header("Breathing SFX")]
    [Tooltip("The audio clip that will play while the Warden is resting after a chase")]
    [SerializeField] private AudioClip lightBreathing;
    [Tooltip("The audio clip that will play while the Warden is searching for the player")]
    [SerializeField] private AudioClip searchBreathing;
    [Tooltip("The audio clip that will play while the Warden is chasing the player")]
    [SerializeField] private AudioClip chaseBreathing;

    private MonsterSight monsterSight;
    private LookAtTarget lookAtTarget;
    private FlashlightDetector flashlightDetector; // Makes sure if the player shines their light on Warden, Warden finds out player location
    private PlayerFootstepDetector playerFootstepDetector;

    private WardenAudioHandler audioHandler;

    // A player reference is used mainly for InvestigateLocation() to work with FlashlightDetector. Instead of the Warden instantly chasing the
    // player when the player shines their flashlight onto Warden, I want Warden to instead first investigate the location to give the player a
    // chance of escaping and not being seen, but since OnMaxTimeInFlashlight does not send a Vector3 position, a reference to the player is
    // necessary to get the player's location at the time of when OnMaxTimeInFlashlight is invoked.
    private GameObject player;

    private CinemachineImpulseSource cameraShakeSource;

    protected override void Awake()
    {
        base.Awake();

        lookAtTarget = GetComponent<LookAtTarget>();
        monsterSight = GetComponent<MonsterSight>();
        flashlightDetector = GetComponent<FlashlightDetector>();
        playerFootstepDetector = GetComponent<PlayerFootstepDetector>();

        audioHandler = GetComponent<WardenAudioHandler>();

        cameraShakeSource = GetComponent<CinemachineImpulseSource>();

        _idleState = idleDoNothingBehavior;
        _searchState = searchBehavior;
        _chaseState = chaseBehavior;
        _caughtPlayerState = caughtPlayerBehavior;

        startState = searchState;
    }

    protected override void Start()
    {
        base.Start();

        startingSpeed = agent.speed;
        onResetSearchRadiusCooldown = false;

        EnableLookAtTarget(false);

        onInvestigateFootstepCooldown = false;
        footstepSFXOnCooldown = false;

        if (lights[0] != null)
        {
            lightStartingColor = lights[0].color;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight += InvestigateFlashlight;
        }

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen += ChasePlayer;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard += InvestigatePlayerFootstep;
        }

        if (chaseBehavior != null)
        {
            chaseBehavior.OnStopChase += HandleOnStopChase;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight -= InvestigateFlashlight;
        }

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen -= ChasePlayer;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard -= InvestigatePlayerFootstep;
        }

        if (chaseBehavior != null)
        {
            chaseBehavior.OnStopChase -= HandleOnStopChase;
        }
    }

    protected override void Update()
    {
        base.Update();
        HandleDistanceLogic();
    }

    /// <summary>
    ///     Handles monitoring the distance between the Warden and the player and taking actions based on the distance, such
    ///     as increasing Warden's movement speed or resetting Warden's search radius.
    /// </summary>
    private void HandleDistanceLogic()
    {
        if (player == null)
        {
            player = LevelController.instance.GetPlayer();
            return;
        }

        distance = (transform.position - player.transform.position).magnitude;

        // Handle the speed boosts based on distance
        if (enableSpeedBoostWhenFarAway)
        {
            if (agent.speed < speedWhenFarAway && distance > minDistForSpeedBoost)
            {
                agent.speed = speedWhenFarAway;
#if UNITY_EDITOR
                Debug.Log("Warden's speed set to: " + agent.speed);
#endif
            }
            else if (agent.speed > startingSpeed && distance <= minDistForSpeedBoost)
            {
                agent.speed = startingSpeed;
#if UNITY_EDITOR
                Debug.Log("Warden's speed set to: " + agent.speed);
#endif
            }
        }

        // Handle the search radius being reset
        bool shouldReset = (
            !IsEntityInSpecificState(chaseState) &&
            !IsEntityInSpecificState(caughtPlayerState) &&
            !IsEntityInSpecificState(idleState) &&
            enableResetSearchRadius &&
            !onResetSearchRadiusCooldown && distance < minDistForSearchReset
        );

        if (shouldReset)
        {
            onResetSearchRadiusCooldown = true;
            stateMachine.ChangeState(searchState);
            this.Invoke(() => onResetSearchRadiusCooldown = false, resetSearchRadiusCooldown);
        }
    }

    private void ChasePlayer()
    {
        if (IsEntityInSpecificState(chaseState) || IsEntityInSpecificState(caughtPlayerState) || beginChaseRoutine != null)
        {
            return;
        }

        beginChaseRoutine = StartCoroutine(BeginChasePlayerRoutine());
    }

    /// <summary>
    ///     Handles the necessary steps when beginning chasing the player.
    /// </summary>
    private IEnumerator BeginChasePlayerRoutine()
    {
        OnWardenSpottedPlayer?.Invoke();

        ChangeLightColors(chaseLightColor);

        audioHandler.whistleAudioSource.Stop();
        audioHandler.drumbeatAudioSource.Stop();

        if (foundPlayerSFX != null)
        {
            foundPlayerSFX.Play();
        }

        if (growlSFX != null)
        {
            audioHandler.PlaySFXWithDelay(growlSFX, audioHandler.sfxAudioSource, growlSFXDelay);
        }

        // Disabling the detectors while entering chase prevents an issue where one of the detectors may cause the Warden
        // to move forward while waiting
        EnableDetectors(false);

        // Enter the idle track player state
        SwitchStateBehavior(ref _idleState, idleTrackPlayerBehavior);
        stateMachine.ChangeState(idleState);

        yield return new WaitForSeconds(timeToWaitBeforeChase);

        audioHandler.chaseMusicAudioSource.Play();

        // Re-enable the detectors
        EnableDetectors(true);

        // Handle audio changes
        UpdateBreathingAudio(chaseBreathing);

        // Have Warden look at the player whilst chasing
        EnableLookAtTarget(true);

        stateMachine.ChangeState(chaseState);

        // Change the idle state behavior back to the idle-do nothing in preparation for Warden's rest phase
        SwitchStateBehavior(ref _idleState, idleDoNothingBehavior);

        beginChaseRoutine = null;
    }

    private void HandleOnStopChase()
    {
        if (endChaseRoutine == null)
        {
            endChaseRoutine = StartCoroutine(EndChaseRoutine());
        }
    }

    /// <summary>
    ///     Handles the necessary steps when ending a chase.
    /// </summary>
    private IEnumerator EndChaseRoutine()
    {
        ChangeLightColors(lightStartingColor);

        // Disable other scripts so Warden does not move out of the kneeling position
        EnableDetectors(false);

        // Turn off LookAtTarget so that while kneeling and searching, the Warden isn't looking at the player
        EnableLookAtTarget(false);

        audioHandler.chaseMusicAudioSource.Stop();
        UpdateBreathingAudio(lightBreathing);

        // Enter idle state and begin kneeling animation
        stateMachine.ChangeState(idleState);
        animator.SetTrigger(animatorKneel);

        yield return new WaitForSeconds(timeToSpendResting);

        // Re-enable the previously disabled scripts
        EnableDetectors(true);

        animator.SetTrigger(animatorKneelExit);
        stateMachine.ChangeState(searchState);

        audioHandler.whistleAudioSource.Play();
        audioHandler.drumbeatAudioSource.Play();
        UpdateBreathingAudio(searchBreathing);

        endChaseRoutine = null;
    }

    /// <summary>
    ///     Investigate a player footstep audio cue.
    /// </summary>
    /// <param name="footstepOrigin">The origin of the footstep audio cue.</param>
    private void InvestigatePlayerFootstep(Vector3 footstepOrigin)
    {
        // Make sure the footstep is within a reasonable walking distance to investigate
        if (!onInvestigateFootstepCooldown && GetPathDistance(footstepOrigin) < footstepMaxMoveDist)
        {
            onInvestigateFootstepCooldown = true;
            SetDestination(footstepOrigin);
            this.Invoke(() => onInvestigateFootstepCooldown = false, investigateFootstepCooldown);
        }
    }

    /// <summary>
    ///     Investigates the player's transform.position whenever OnMaxTimeInFlashlight is invoked on the Warden.
    /// </summary>
    private void InvestigateFlashlight()
    {
        if (player == null)
        {
            player = LevelController.instance.GetPlayer();
        }

        if (player != null)
        {
            SetDestination(player.transform.position);
        }
    }

    /// <summary>
    ///     Enable or disable LookAtTarget.cs, so Warden is looking/not looking at the player.
    /// </summary>
    public void EnableLookAtTarget(bool enabled)
    {
        if (lookAtTarget != null)
        {
            lookAtTarget.enabled = enabled;
        }
    }

    /// <summary>
    ///     Plays a footstep sound effect when not on cooldown.
    /// </summary>
    public void FootstepEvent()
    {
        if (!footstepSFXOnCooldown && footstepSFX != null)
        {
            footstepSFX.Play(audioHandler.footstepAudioSource);
            footstepSFXOnCooldown = true;
            this.Invoke(() => footstepSFXOnCooldown = false, footstepSFXCooldownTime);

            // Camera shake
            if (cameraShakeSource != null && SettingsManager.instance.IsCameraShakeEnabled() && distance < maxDistanceForImpulse)
            {
                cameraShakeSource.GenerateImpulse();
            }
        }
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        EnableDetectors(false);

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);

        // Stop playing the musical aura and breathing sfx
        if (audioHandler != null)
        {
            audioHandler.drumbeatAudioSource.Stop();
            audioHandler.whistleAudioSource.Stop();
            audioHandler.breathingAudioSource.Stop();

            if (audioHandler.chaseMusicAudioSource.isPlaying)
            {
                audioHandler.chaseMusicAudioSource.Stop();
            }
        }
    }

    /// <summary>
    ///     Update the Warden's breathing audio source with a new audio clip.
    /// </summary>
    /// <param name="newBreathingClip">The new audio clip to use as the Warden's breathing.</param>
    private void UpdateBreathingAudio(AudioClip newBreathingClip)
    {
        audioHandler.breathingAudioSource.clip = newBreathingClip;
        audioHandler.breathingAudioSource.Play();
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
    ///     Changes the lights' colors from lights array to a new color.
    /// </summary>
    /// <param name="newColor">The new color of the lights.</param>
    private void ChangeLightColors(Color newColor)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].color = newColor;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (enableSpeedBoostWhenFarAway)
        {
            DrawArc(transform.position, minDistForSpeedBoost, speedBoostArcColor);
        }

        if (enableResetSearchRadius)
        {
            DrawArc(transform.position, minDistForSearchReset, resetSearchRadiusArcColor);
        }
    }

    private void DrawArc(Vector3 center, float radius, Color color)
    {
        float angle = 360;
        Handles.color = color;
        Handles.DrawWireArc(center, Vector3.up, Vector3.forward, angle, radius);
    }
#endif
}
