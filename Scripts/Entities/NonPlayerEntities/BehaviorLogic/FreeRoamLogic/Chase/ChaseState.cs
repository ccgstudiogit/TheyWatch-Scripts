using System;
using UnityEngine;

public abstract class ChaseState : EntityState
{
    public event Action OnStartChase; // Invoked at the start of the chase in EnterState()
    public event Action OnStopChase; // Can be used to stop the chase prematurely
    public event Action OnEndChase; // Invoked at the end of the chase in ExitState()

    [Header("- Speed Settings -")]
    [Header("Multipliers")]
    [Tooltip("Multiplies the movement speed of this entity by this amount (set to 1 if no speed increase is desired)")]
    [SerializeField] private float chaseSpeedMultiplier = 1.15f;
    [Tooltip("Multiplies the animator speed of this entity by this amount (set to 1 if no speed increase is desired)")]
    [SerializeField] private float AnimatorSpeedMultiplier = 1.15f;

    [Header("Static")]
    [Tooltip("Instead of using multipliers, if this is enabled, the agent's speed and animation speed will be set to " +
        "these values (overrides useMultipliers)")]
    [SerializeField] private bool useStaticSpeeds;
    [Tooltip("Set the agent's movement speed to this amount")]
    [SerializeField] private float agentSpeed = 1f;
    [Tooltip("Set this entity's animator speed to this amount")]
    [SerializeField] private float animatorSpeed = 1f;

    [Header("Waiting For Player")]
    [Tooltip("The time this entity will wait at the player's last known location before switching to an investigative/patrol state.")]
    [SerializeField] protected float timeToWaitAtPlayersLastKnownLocation = 7f;
    protected Vector3 playerLastKnownPosition;
    protected float elapsedTimeWaiting;

    protected float elapsedTimeInChase;

    // Flag for StopChase() to make sure OnStopChase is not invoked more than once since StopChase will likely be called somewhere in
    // FrameUpdate()
    private bool chaseStopped; 

    public override void EnterState()
    {
        if (useStaticSpeeds)
        {
            entity.SetMovementSpeed(agentSpeed);
            entity.SetAnimatorSpeed(animatorSpeed);
        }
        else
        {
            entity.MovementSpeedMultiplier(chaseSpeedMultiplier);
            entity.AnimatorSpeedMultiplier(AnimatorSpeedMultiplier);
        }

        elapsedTimeInChase = 0f;
        elapsedTimeWaiting = 0f;
        chaseStopped = false;

        base.EnterState();

        OnStartChase?.Invoke();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        elapsedTimeInChase += Time.deltaTime;
    }

    public override void ExitState()
    {
        base.ExitState();

        entity.ResetMovementSpeed();
        entity.ResetAnimatorSpeed();

        OnEndChase?.Invoke();
    }

    /// <summary>
    ///     Stops the chase by switching to a different state (prioritizes stalk state, then patrol state, then idle state).
    /// </summary>
    protected virtual void StopChaseAndSwitchStates()
    {
        if (entity is IStalkStateUser stalkStateUser)
        {
            entity.stateMachine.ChangeState(stalkStateUser.stalkState);
        }
        else if (entity is ISearchStateUser searchStateUser)
        {
            entity.stateMachine.ChangeState(searchStateUser.searchState);
        }
        else if (entity is IPatrolStateUser patrolStateUser)
        {
            entity.stateMachine.ChangeState(patrolStateUser.patrolState);
        }
        else if (entity is IIdleStateUser idleStateUser)
        {
            entity.stateMachine.ChangeState(idleStateUser.idleState);
        }
    }

    /// <summary>
    ///     Stop the chase by firing off an event and letting the monster know it should switch states manually.
    /// </summary>
    protected virtual void StopChase()
    {
        // Prevents calling this method more than once after a chase has finished
        if (chaseStopped)
        {
            return;
        }

#if UNITY_EDITOR
        Debug.Log("StopChase() called and chase was stopped manually.");
#endif

        OnStopChase?.Invoke();
        chaseStopped = true;
    }

    /// <summary>
    ///     Multiplies the agent's chase speed by a set amount. Note: Since this method multiplies ChaseState's agent speed
    ///     variable, this multiplication is permanent.
    /// </summary>
    public void AgentSpeedMultiplier(float multiplier)
    {
        agentSpeed *= multiplier;
    }
}
