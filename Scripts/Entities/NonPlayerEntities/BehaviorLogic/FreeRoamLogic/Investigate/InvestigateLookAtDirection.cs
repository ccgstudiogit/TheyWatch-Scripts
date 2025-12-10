using UnityEngine;

public class InvestigateLookAtDirection : InvestigateState
{
    [Header("Investigate Duration")]
    [SerializeField, Min(0f)] private float duration = 4f;
    private float timeElapsed;

    [Header("References")]
    [Tooltip("lookAtTarget will be enabled in EnterState() and disabled in ExitState()")]
    [SerializeField] private LookAtTarget lookAtTarget;

    // For remembering LookAtTarget's state
    private bool lookAtTargetEnabled;
    private bool lookAtTargetLookingEnabled;
    private bool lookAtTargetTrackingPlayer;

    public override void EnterState()
    {
        base.EnterState();

        timeElapsed = 0f;
        entity.SetDestination(entity.transform.position);

        lookAtTargetEnabled = lookAtTarget.enabled;
        lookAtTargetLookingEnabled = lookAtTarget.CanLook();
        lookAtTargetTrackingPlayer = lookAtTarget.IsTrackingPlayer();

        // Have the entity's LookAtTarget look in the general direction
        lookAtTarget.SetObjectToBeTracked(emptyTarget);

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
        timeElapsed += Time.deltaTime;

        if (timeElapsed > duration)
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
