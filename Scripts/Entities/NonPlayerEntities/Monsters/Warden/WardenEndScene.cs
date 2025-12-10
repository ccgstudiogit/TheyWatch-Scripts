using UnityEngine;
using Unity.Cinemachine;

public class WardenEndScene : Monster, IIdleStateUser, IChaseStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private ChaseState _chaseState;
    public ChaseState chaseState => _chaseState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behaviors")]
    [SerializeField] private IdleState idleBehavior;
    [SerializeField] private ChaseState chaseBehavior;

    [Header("SFX")]
    [SerializeField] private SoundEffectSO footstepSFX;

    private WardenAudioHandler audioHandler;
    private CinemachineImpulseSource cameraShakeSource;

    protected override void Awake()
    {
        base.Awake();

        audioHandler = GetComponent<WardenAudioHandler>();
        cameraShakeSource = GetComponent<CinemachineImpulseSource>();

        _idleState = idleBehavior;
        _chaseState = chaseBehavior;

        startState = chaseState;
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        if (LevelController.instance != null && LevelController.instance is DungeonEndLevelController)
        {
            // If the Warden happens to catch up to the player or the player purposefully runs into Warden, end the scene
            DungeonEndLevelController dungeonEndLevelController = LevelController.instance as DungeonEndLevelController;
            dungeonEndLevelController.EndScene();
        }
    }

    public void FootstepEvent()
    {
        if (footstepSFX != null)
        {
            footstepSFX.Play(audioHandler.footstepAudioSource);
        }

        // Camera shake
        if (cameraShakeSource != null && SettingsManager.instance.IsCameraShakeEnabled())
        {
            cameraShakeSource.GenerateImpulse();
        }
    }

    /// <summary>
    ///     Make Warden exit chase state and also move Warden to a specific position.
    /// </summary>
    /// <param name="moveToPosition">The position Warden should move to.</param>
    public void StopChaseAndMoveToPos(Vector3 moveToPosition)
    {
        stateMachine.ChangeState(idleState);

        // Move to the position after a slight delay to make sure idle state doesn't interfere with the movement
        this.Invoke(() => SetDestination(moveToPosition), 0.15f);
    }
}
