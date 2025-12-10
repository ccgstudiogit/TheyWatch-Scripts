using UnityEngine;

public class InvestigateLookThenMove : InvestigateState
{
    [Header("Look Duration")]
    [SerializeField, Min(0f)] private float lookDuration = 1f;
    private float lookTimeElapsed;
    [Tooltip("How long the entity should keep looking at the investigate spot after lookDuration has elapsed and the entity " +
        "has started to move")]
    [SerializeField, Min(0f)] private float keepLookingAfterMovingDuration = 0.33f;

    [Header("References")]
    [Tooltip("lookAtTarget will be enabled in EnterState() and disabled in ExitState()")]
    [SerializeField] private LookAtTarget lookAtTarget;

    // For remembering LookAtTarget's state
    private bool lookAtTargetEnabled;
    private bool lookAtTargetLookingEnabled;
    private bool lookAtTargetTrackingPlayer;

    // Helpers
    private bool looking;

    public override void EnterState()
    {
        base.EnterState();

        lookTimeElapsed = 0f;
        entity.SetDestination(entity.transform.position);

        lookAtTargetEnabled = lookAtTarget.enabled;
        lookAtTargetLookingEnabled = lookAtTarget.CanLook();
        lookAtTargetTrackingPlayer = lookAtTarget.IsTrackingPlayer();

        // Have the entity's LookAtTarget look in the general direction
        lookAtTarget.SetObjectToBeTracked(emptyTarget);
        looking = true;

        if (!lookAtTarget.enabled)
        {
            lookAtTarget.enabled = true;
        }

        if (lookAtTarget.IsTrackingPlayer())
        {
            lookAtTarget.SetTrackPlayer(false);
        }

        if (!lookAtTarget.CanLook())
        {
            lookAtTarget.SetLooking(true);
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        lookTimeElapsed += Time.deltaTime;

        if (looking && lookTimeElapsed > lookDuration)
        {
            this.Invoke(() => ResetLookAtTarget(), keepLookingAfterMovingDuration);
            looking = false;
            entity.SetDestination(emptyTarget.transform);
        }
        else if (!looking && entity.AtEndOfPath())
        {
            DoneInvestigating();
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        // Make sure to restore LookAtTarget's previous settings/state if needed
        ResetLookAtTarget();
    }

    private void ResetLookAtTarget()
    {
        if (!lookAtTargetEnabled)
        {
            lookAtTarget.enabled = false;
        }

        if (!lookAtTargetLookingEnabled)
        {
            lookAtTarget.SetLooking(false);
        }

        if (lookAtTargetTrackingPlayer)
        {
            lookAtTarget.SetTrackPlayer(true);
        }
    }
}
