using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Overseer : Monster, IIdleStateUser, ISearchStateUser, IStalkStateUser, IChaseStateUser, IStunStateUser, IInvestigateStateUser, ICaughtPlayerStateUser, IDisappearStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private SearchState _searchState;
    public SearchState searchState => _searchState;

    private StalkState _stalkState;
    public StalkState stalkState => _stalkState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private StunState _stunState;
    public StunState stunState => _stunState;

    private InvestigateState _investigateState;
    public InvestigateState investigateState => _investigateState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private DisappearState _disappearState;
    public DisappearState disappearState => _disappearState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    // Animation values
    private const string shutdownStr = "shutdown";

    [Header("Behavior References")]
    [SerializeField] private IdleState idleDoNothingBehavior;
    [SerializeField] private SearchState searchBehavior;
    [SerializeField] private StalkState stalkBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private StunState stunBehavior;
    [SerializeField] private InvestigateState investigateBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;
    [SerializeField] private DisappearState disappearBehavior;

    [Header("Component References")]
    [SerializeField] private MonsterSight monsterSight;
    [SerializeField] private Collider col;

    [Header("Start Stalking")]
    [Tooltip("The time Overseer spends in search state before switching to stalk state")]
    [SerializeField, Min(0f)] private float maxTimeSearching = 30f;
    private float timeElapsedSearching = 0f;
    [Tooltip("Overseer will only start stalking if the player is at least this distance away (prevents cases where the player " +
        "is able to hear/see the Overseer's lights but then they just disappear as Overseer enters stalk state)")]
    [SerializeField, Min(0f)] private float minDisForStalk = 20f;

    [Header("Searching")]
    [Tooltip("Overseer will re-enter search if the Overseer gets within this distance to the player (similar to Warden)")]
    [SerializeField, Min(0f)] private float resetSearchAtDistanceToPlayer = 10f;
    private float searchResetCooldownLength = 10f; // After search is reset, it cannot be reset for this many seconds
    private bool onSearchResetCooldown;

    [Header("Investigating")]
    [Tooltip("Overseer will enter investigate state at most once every this amount of seconds")]
    [SerializeField, Min(0f)] private float investigateCooldown = 12.5f;
    private bool onInvestigateCooldown;

    [Header("Player Spotted")]
    [SerializeField, Min(0f)] private float timeToWaitBeforeChase = 1.5f;
    [SerializeField] private SoundEffectSO playerSpottedSFX;

    [Header("Lights")]
    [Tooltip("These lights will turn to the playerSpottedColor once Overseer spots the player")]
    [SerializeField] private Light[] lights;
    private Color lightDefaultColor; // The color of the lights set in the inspector
    [Tooltip("This material's color will match that of the lights' color when the player is spotted")]
    [SerializeField] private Material bulbMat;
    [Tooltip("Makes sure that bulbMat will always reset to this material's color")]
    [SerializeField] private Material baseBulbMat;
    [SerializeField] private Color playerSpottedColor = Color.red;
    [SerializeField, Min(0f)] private float playerSpottedIntensity = 800f;
    private float lightStartingIntensity;
    [SerializeField, Range(0f, 180f)] private float playerSpottedAngle = 180f;
    private float lightStartingAngle;

    [Header("Smoke VFX")]
    [SerializeField] private ParticleSystem[] smokeVFX;

    [Header("Shut Down")]
    [SerializeField, Min(0f)] private float shutdownDuration = 7.25f;
    [SerializeField, Min(0f)] private float lightFlickerDuration = 3.25f;
    [Tooltip("The min/max time between flickers. A random float between this range is picked.")]
    [SerializeField] private Vector2 timeBetweenFlickers = new Vector2(0.05f, 0.4f);

    [Header("Start Up")]
    [Tooltip("Before waking back up after being shut down, Overseer's lights will flicker for this amount of time before turning on")]
    [SerializeField, Min(0f)] private float lightStartUpFlickerDuration = 1.5f;

    [Header("Footsteps")]
    [SerializeField, Min(0f)] private float maxImpuleDistance = 10f;
    [SerializeField] private SoundEffectSO footstepSFX;

    [Header("Aura")]
    [SerializeField, Min(0f)] private float auraFadeTime = 0.5f;
    private float auraStartVolume;

    private OverseerAudioHandler audioHandler;
    private FlashlightDetector flashlightDetector;
    private PlayerFootstepDetector playerFootstepDetector;
    private StunnableEntity stunnableEntity;
    private LookAtTarget lookAtTarget;
    private CinemachineImpulseSource cameraShakeSource;

    private GameObject player;

    protected override void Awake()
    {
        base.Awake();

        audioHandler = GetComponent<OverseerAudioHandler>();
        flashlightDetector = GetComponent<FlashlightDetector>();
        playerFootstepDetector = GetComponent<PlayerFootstepDetector>();
        stunnableEntity = GetComponent<StunnableEntity>();
        lookAtTarget = GetComponent<LookAtTarget>();
        cameraShakeSource = GetComponent<CinemachineImpulseSource>();

        _idleState = idleDoNothingBehavior;
        _searchState = searchBehavior;
        _stalkState = stalkBehavior;
        _chaseState = chaseBehavior;
        _stunState = stunBehavior;
        _investigateState = investigateBehavior;
        _caughtPlayerState = caughtPlayerBehavior;
        _disappearState = disappearBehavior;

        startState = stalkState;

        // Save the lights starting color and material starting color so they can switch back after chasing
        lightDefaultColor = lights[0].color;
        lightStartingIntensity = lights[0].intensity;
        lightStartingAngle = lights[0].spotAngle;
    }

    protected override void Start()
    {
        base.Start();
        EnableLook(false);
        auraStartVolume = audioHandler.auraAudioSource.volume;

        if (IsEntityInSpecificState(stalkState))
        {
            SetSmokeActive(false);
            TurnOnLights(false);
            PlayAura(false);
            audioHandler.auraAudioSource.volume = 0f;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen += HandlePlayerSeen;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard += HandlePlayerFootstepHeard;
        }

        if (stunnableEntity != null)
        {
            stunnableEntity.OnStunned += Stun;
            stunnableEntity.OnStunEnd += StunEnded;
        }

        if (investigateBehavior != null)
        {
            investigateBehavior.OnDoneInvestigating += HandleDoneInvestigating;
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight += HandleMaxTimeInFlashlight;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (monsterSight != null)
        {
            monsterSight.OnPlayerSeen -= HandlePlayerSeen;
        }

        if (playerFootstepDetector != null)
        {
            playerFootstepDetector.OnPlayerFootstepHeard -= HandlePlayerFootstepHeard;
        }

        if (stunnableEntity != null)
        {
            stunnableEntity.OnStunned -= Stun;
            stunnableEntity.OnStunEnd -= StunEnded;
        }

        if (investigateBehavior != null)
        {
            investigateBehavior.OnDoneInvestigating -= HandleDoneInvestigating;
        }

        if (flashlightDetector != null)
        {
            flashlightDetector.OnMaxTimeInFlashlight -= HandleMaxTimeInFlashlight;
        }

        // Reset bulb material in case game was stopped while Overseer is chasing/stunned
        SetBulbColor(baseBulbMat.color);
    }

    protected override void Update()
    {
        base.Update();

        // After searching for the player for a specified duration, switch to teleport stalking
        if (IsEntityInSpecificState(searchState) || IsEntityInSpecificState(investigateState))
        {
            timeElapsedSearching += Time.deltaTime;
            float distanceToPlayer = GetDistanceToPlayer();

            if (timeElapsedSearching > maxTimeSearching && distanceToPlayer > minDisForStalk)
            {
                StartTeleportStalking();
            }
            else if (IsEntityInSpecificState(searchState) && distanceToPlayer < resetSearchAtDistanceToPlayer && !onSearchResetCooldown)
            {
                onSearchResetCooldown = true;
                stateMachine.ChangeState(searchState);
                this.Invoke(() => onSearchResetCooldown = false, searchResetCooldownLength);
            }
        }
        else
        {
            timeElapsedSearching = 0f;
        }
    }

    private void HandlePlayerSeen()
    {
        if (IsEntityInSpecificState(searchState) || IsEntityInSpecificState(stalkState) || IsEntityInSpecificState(investigateState))
        {
            if (timeToWaitBeforeChase > 0.1f)
            {
                StartCoroutine(PreChaseRoutine(timeToWaitBeforeChase));
            }
            else
            {
                stateMachine.ChangeState(chaseState);
            }
        }
    }

    private void HandlePlayerFootstepHeard(Vector3 footstepOrigin)
    {
        if (IsEntityInSpecificState(searchState) && !onInvestigateCooldown)
        {
            onInvestigateCooldown = true;
            investigateBehavior.SetInvestigateTargetPosition(footstepOrigin);
            stateMachine.ChangeState(investigateState);
            this.Invoke(() => onInvestigateCooldown = false, investigateCooldown);
        }
    }

    /// <summary>
    ///     After Overseer is finished investigating (the duration in InvestigateState has elapsed), enter search state.
    /// </summary>
    private void HandleDoneInvestigating()
    {
        if (IsEntityInSpecificState(investigateState))
        {
            stateMachine.ChangeState(searchState);
        }
    }

    private void HandleMaxTimeInFlashlight()
    {
        if (IsEntityInSpecificState(searchState) && !onInvestigateCooldown)
        {
            if (player == null)
            {
                player = LevelController.instance.GetPlayer();
            }
            
            onInvestigateCooldown = true;
            investigateBehavior.SetInvestigateTargetPosition(player.transform.position);
            stateMachine.ChangeState(investigateState);
            this.Invoke(() => onInvestigateCooldown = false, investigateCooldown);
        }
    }

    /// <summary>
    ///     Turn off lights and become really quiet. Stalk the player and teleport around the maze, waiting around corners.
    /// </summary>
    private void StartTeleportStalking()
    {
        PlayAura(false);
        TurnOnLights(false); // Turn off the lights to surprise the player once the player comes across Overseer
        SetSmokeActive(false);
        SetDestination(transform.position); // Makes sure the current waypoint destination doesn't interfere with entering stalk state.
        stateMachine.ChangeState(stalkState);
    }

    /// <summary>
    ///     Do stuff and wait a certain duration before entering chase state.
    /// </summary>
    /// <param name="timeToWait">The time to wait before entering chase state.</param>
    private IEnumerator PreChaseRoutine(float timeToWait)
    {
        stateMachine.ChangeState(idleState);

        SetSmokeActive(true);
        PlayAura(false, true); // True makes the aura stop playing instantly
        PlaySFX(playerSpottedSFX, audioHandler.playerSpottedAudioSource);

        // Turn the lights red
        SetChaseLightsActive(true);

        // Start tracking the player
        EnableLook(true);

        yield return new WaitForSeconds(timeToWait);

        if (!IsEntityInSpecificState(stunState))
        {
            stateMachine.ChangeState(chaseState);
        }
    }

    /// <summary>
    ///     Stun Overseer by entering stun state (basically IdleDoNothing).
    /// </summary>
    private void Stun()
    {
        if (IsEntityInSpecificState(stunState))
        {
            return;
        }

        EnableColTrigger(false);
        PlayAura(false);
        StartCoroutine(FlickerLights(lightFlickerDuration, timeBetweenFlickers, false));
        EnableLook(false);
        stateMachine.ChangeState(stunState);
    }

    /// <summary>
    ///     Once the stun ends (subscribed to the event StunnableEntity.OnStunEnd), enter search state.
    /// </summary>
    private void StunEnded()
    {
        ResetMonsterSight();
        StartCoroutine(ShutdownRoutine(shutdownDuration));
    }

    /// <summary>
    ///     Shut down happens after being stunned.
    /// </summary>
    private IEnumerator ShutdownRoutine(float duration)
    {
        SetSmokeActive(false);

        animator.SetInteger(shutdownStr, 1);

        audioHandler.shutdownAudioSource.Play();

        SetChaseLightsActive(false);
        TurnOnLights(false);

        // Turns on lights and starts flickering them a little before fully waking up
        this.Invoke(() =>
        {
            TurnOnLights(true);
            StartCoroutine(FlickerLights(lightStartUpFlickerDuration, timeBetweenFlickers, true));
        }, duration - lightStartUpFlickerDuration - 0.1f); // 0.1f as a slight buffer

        // Play the power up sfx right before powering up
        this.Invoke(() => audioHandler.powerUpAudioSource.Play(), duration - audioHandler.powerUpAudioSource.clip.length);

        yield return new WaitForSeconds(duration);

        ResetMonsterSight();

        animator.SetInteger(shutdownStr, 0);
        stateMachine.ChangeState(searchState);

        SetSmokeActive(true);
        PlayAura(true);

        EnableColTrigger(true);
    }

    /// <summary>
    ///     Footstep event from the animator.
    /// </summary>
    private void FootstepEvent()
    {
        PlaySFX(footstepSFX, audioHandler.footstepAudioSource);

        if (cameraShakeSource != null && SettingsManager.instance.IsCameraShakeEnabled() && GetDistanceToPlayer() < maxImpuleDistance)
        {
            cameraShakeSource.GenerateImpulse();
        }
    }

    /// <summary>
    ///     Get the distance to the player.
    /// </summary>
    /// <returns>A float distance to the player. If the player is not found, -1 is returned.</returns>
    private float GetDistanceToPlayer()
    {
        if (player == null)
        {
            player = LevelController.instance.GetPlayer();
        }

        return player != null ? (transform.position - player.transform.position).magnitude : -1f;
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
    ///     Enable/disable LookAtTarget's look or enable/disable the actual LookAtTarget component.
    /// </summary>
    /// <param name="look">Set LookAtTarget to look or not look.</param>
    private void EnableLook(bool look)
    {
        if (lookAtTarget != null)
        {
            lookAtTarget.SetLooking(look);
        }
    }

    /// <summary>
    ///     Resets MonsterSight's isPlayerCurrentlySeen flag.
    /// </summary>
    private void ResetMonsterSight()
    {
        if (monsterSight != null)
        {
            monsterSight.ResetIsPlayerCurrentlySeen();
        }
    }

    /// <summary>
    ///     Set the chase lights and light emissive material to the chase color. If false, return them back to normal.
    /// </summary>
    private void SetChaseLightsActive(bool setChaseColorActive)
    {
        // Change the color of the lights
        SetLightsColor(setChaseColorActive ? playerSpottedColor : lightDefaultColor);

        // Change the intensity of the spotlight
        lights[0].intensity = setChaseColorActive ? playerSpottedIntensity : lightStartingIntensity;
        lights[0].spotAngle = setChaseColorActive ? playerSpottedAngle : lightStartingAngle;

        // Change the color of the emissive material
        SetBulbColor(setChaseColorActive ? playerSpottedColor : baseBulbMat.color);
    }

    /// <summary>
    ///     Turn on/off lights by setting the lights' colors to their defaults or set to black.
    /// </summary>
    /// <param name="enable">Whether or not the lights should be enabled.</param>
    private void TurnOnLights(bool enable)
    {
        SetLightsColor(enable ? lightDefaultColor : Color.black);
        SetBulbColor(enable ? baseBulbMat.color : Color.black);
    }

    /// <summary>
    ///     Turn on/off lights by setting the lights' colors to a new colore or set to black.
    /// </summary>
    /// <param name="enable">Whether or not the lights should be enabled.</param>
    /// <param name="turnedOnColor">The color of the lights if they are turning on.</param>
    private void TurnOnLights(bool enable, Color turnedOnColor)
    {
        SetLightsColor(enable ? turnedOnColor : Color.black);
        SetBulbColor(enable ? turnedOnColor : Color.black);
    }

    /// <summary>
    ///     Set Overseer's lights to a new color.
    /// </summary>
    private void SetLightsColor(Color color)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].color = color;
        }
    }

    /// <summary>
    ///     Set Overseer's light bulbs to a new color.
    /// </summary>
    private void SetBulbColor(Color color)
    {
        bulbMat.color = color;
    }

    /// <summary>
    ///     Flicker Overseer's lights for a specified duration.
    /// </summary>
    private IEnumerator FlickerLights(float duration, Vector2 timeBetweenFlicker, bool keepLightsOnWhenFinished)
    {
        float timeElapsed = 0f;
        float timeTillNextFlicker = Random.Range(timeBetweenFlicker.x, timeBetweenFlicker.y);
        float timeSinceLastFlicker = 0f;
        bool lightsOn = true;
        Color startingColor = lights[0].color;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            timeSinceLastFlicker += Time.deltaTime;

            if (timeSinceLastFlicker > timeTillNextFlicker)
            {
                if (lightsOn)
                {
                    lightsOn = false;
                    TurnOnLights(false);
                }
                else
                {
                    lightsOn = true;
                    TurnOnLights(true, startingColor);
                }

                timeTillNextFlicker = Random.Range(timeBetweenFlicker.x, timeBetweenFlicker.y);
                timeSinceLastFlicker = 0f;
            }

            yield return null;
        }

        if (keepLightsOnWhenFinished)
        {
            TurnOnLights(true, startingColor);
        }
        else
        {
            TurnOnLights(false);
        }
    }

    /// <summary>
    ///     Set Overseer's smoke to be active or inactive.
    /// </summary>
    /// <param name="active"></param>
    private void SetSmokeActive(bool active)
    {
        for (int i = 0; i < smokeVFX.Length; i++)
        {
            if (active && !smokeVFX[i].isPlaying)
            {
                smokeVFX[i].Play();
            }
            else if (!active && smokeVFX[i].isPlaying)
            {
                smokeVFX[i].Stop();
            }
        }
    }

    /// <summary>
    ///     Play or stop the aura sound effects.
    /// </summary>
    private void PlayAura(bool play, bool ignoreFadeTime = false)
    {
        if (play)
        {
            audioHandler.auraAudioSource.Play();

            if (ignoreFadeTime)
            {
                audioHandler.auraAudioSource.volume = auraStartVolume;
            }
            else
            {
                StartCoroutine(FadeAudio(audioHandler.auraAudioSource, auraStartVolume, auraFadeTime));
            }
        }
        else
        {
            if (ignoreFadeTime)
            {
                audioHandler.auraAudioSource.volume = 0f;
                audioHandler.auraAudioSource.Pause();
            }
            else
            {
                StartCoroutine(FadeAudio(audioHandler.auraAudioSource, 0f, auraFadeTime));
                this.Invoke(() => audioHandler.auraAudioSource.Pause(), auraFadeTime);
            }
        }
    }

    /// <summary>
    ///     Fade an audio source's volume.
    /// </summary>
    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        float lerp = 0f;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            source.volume = Mathf.Lerp(source.volume, targetVolume, lerp);

            yield return null;
        }

        source.volume = targetVolume;
    }

    /// <summary>
    ///     Make the collider a trigger or not.
    /// </summary>
    private void EnableColTrigger(bool isTrigger)
    {
        col.isTrigger = isTrigger;
    }
    
    protected override void HandleKillPlayer(PlayerReferences playerReferences, Monster monster)
    {
        PlayAura(false, true);
        EnableLook(false);

        // This only stops movement. Deathscreen jumpscare sfx and logic is handled by DeathscreenJumpscare
        stateMachine.ChangeState(caughtPlayerState);
    }

    protected override void HandleDamagePlayer(Monster monster)
    {
        if (monster != this)
        {
            return;
        }

        stateMachine.ChangeState(disappearState);

        EnableLook(false);
        SetChaseLightsActive(false);
        PlayAura(true);
    }
}
