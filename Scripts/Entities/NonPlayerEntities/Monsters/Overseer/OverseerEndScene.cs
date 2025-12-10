using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class OverseerEndScene : Monster, IIdleStateUser, IChaseStateUser, ICaughtPlayerStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private CaughtPlayerState _caughtPlayerState;
    public CaughtPlayerState caughtPlayerState => _caughtPlayerState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behavior References")]
    [SerializeField] private IdleState idleDoNothingBehavior;
    [SerializeField] private ChaseState chaseBehavior;
    [SerializeField] private CaughtPlayerState caughtPlayerBehavior;

    [Header("Waking Up")]
    [Tooltip("Before turning on the lights and playing the player spotted sfx, wait this amount of time and play the power up sfx")]
    [SerializeField, Min(0f)] private float wakeUpDelay = 2.3f;
    [Tooltip("Overseer's lights that will turn on once Overseer wakes up")]
    [SerializeField] private Light[] lights;
    [SerializeField] private GameObject lightBulbs;
    [SerializeField] private ParticleSystem[] smokeVFX;
    [Tooltip("Overseer will chase the player after this amount of seconds after waking up")]
    [SerializeField, Min(0f)] private float chaseDelay = 2.3f;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource powerUpSource;
    [SerializeField] private AudioSource playerSpottedSource;

    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private SoundEffectSO footstepSFX;

    private LookAtTarget lookAtTarget;
    private CinemachineImpulseSource cameraShakeSource;

    protected override void Awake()
    {
        base.Awake();

        lookAtTarget = GetComponent<LookAtTarget>();
        cameraShakeSource = GetComponent<CinemachineImpulseSource>();

        _idleState = idleDoNothingBehavior;
        _chaseState = chaseBehavior;
        _caughtPlayerState = caughtPlayerBehavior;

        startState = idleState;

        SetLightsActive(false);
        SetSmokeActive(false);
        EnableLookingAtTarget(false);
    }

    /// <summary>
    ///     Wake up and begin chasing the player.
    /// </summary>
    public void WakeUp()
    {
        StartCoroutine(WakeUpRoutine());
    }

    private IEnumerator WakeUpRoutine()
    {
        EnableLookingAtTarget(true);
        powerUpSource.Play();
        yield return new WaitForSeconds(wakeUpDelay);

        SetSmokeActive(true);
        SetLightsActive(true);
        playerSpottedSource.Play();
        yield return new WaitForSeconds(chaseDelay);

        stateMachine.ChangeState(chaseState);
    }

    /// <summary>
    ///     Set Overseer's lights to be active or inactive.
    /// </summary>
    private void SetLightsActive(bool active)
    {
        lightBulbs.SetActive(active);

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].gameObject.SetActive(active);
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
            if (active)
            {
                smokeVFX[i].Play();
            }
            else
            {
                smokeVFX[i].Stop();
            }
        }
    }

    /// <summary>
    ///     Footstep event from the animator.
    /// </summary>
    private void FootstepEvent()
    {
        footstepSFX.Play(footstepSource);

        if (cameraShakeSource != null && SettingsManager.instance.IsCameraShakeEnabled())
        {
            cameraShakeSource.GenerateImpulse();
        }
    }

    /// <summary>
    ///     Enable/disable LookAtTarget's look or enable/disable the actual LookAtTarget component.
    /// </summary>
    /// <param name="look">Set LookAtTarget to look or not look.</param>
    /// <param name="enabled">Enable or disable LookAtTarget.cs</param>
    private void EnableLookingAtTarget(bool look, bool enabled = true)
    {
        if (lookAtTarget != null)
        {
            lookAtTarget.SetLooking(look);
            lookAtTarget.enabled = enabled;
        }
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        stateMachine.ChangeState(caughtPlayerState);
    }
}
