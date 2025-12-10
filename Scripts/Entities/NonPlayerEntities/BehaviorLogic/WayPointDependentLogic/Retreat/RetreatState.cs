using System;
using UnityEngine;

public abstract class RetreatState : EntityWayPointState
{
    // Currently used by Shade to turn the mesh renderers back on when finished retreating and for Stitch to turn LookAtTarget.cs back on
    public event Action OnRetreatEnded;

    protected WayPoint wayPointToMoveTo;

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
        wayPointToMoveTo = null;
        OnRetreatEnded?.Invoke();
    }

    /// <summary>
    ///     Retreat to a random distant waypoint, using the given number of waypoints to consider.
    /// </summary>
    protected void Retreat(int numberOfWayPointsToConsider)
    {
        wayPointToMoveTo = GetRandomDistantWayPoint(numberOfWayPointsToConsider);

        if (wayPointToMoveTo == null)
        {
#if UNITY_EDITOR
            Debug.LogError("RetreatToDistantWayPoint attempted to find a distant way point but returned null. Moving to random waypoint.");
#endif
            wayPointToMoveTo = GetRandomWayPoint(LevelController.instance.wayPoints);
        }
    
        MoveToWayPoint(wayPointToMoveTo, makeSureWayPointHasValidPath: false);
    }

    /// <summary>
    ///     Checks if this entity is done retreating by calling IsEntityMovingToPos(). If this entity is done moving,
    ///     it will attempt to switch a valid state in this order: Idle -> Stalk -> Patrol. Note: this entity must be
    ///     a valid IStateUser in order to successfully switch to that state.
    /// </summary>
    protected void CheckIfDoneRetreatingAndSwitchStates()
    {
        if (!IsEntityMovingToPos())
        {
            if (entity is IIdleStateUser idleStateUser)
            {
                entity.stateMachine.ChangeState(idleStateUser.idleState);
            }
            else if (entity is IStalkStateUser stalkStateUser)
            {
                entity.stateMachine.ChangeState(stalkStateUser.stalkState);
            }
            else if (entity is IPatrolStateUser patrolStateUser)
            {
                entity.stateMachine.ChangeState(patrolStateUser.patrolState);
            }
        }
    }
}
