using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public abstract class DisappearState : EntityWayPointState
{
    /// <summary>
    ///     Get the farthest waypoint and teleport to it.
    /// </summary>
    protected void Disappear()
    {
        TeleportToWayPoint(GetRandomDistantWayPoint(1));
    }

    /// <summary>
    ///     Checks if this entity is done waiting after disappearing by calling IsEntityMovingToPos(). If this entity is done moving,
    ///     it will attempt to switch a valid state in this order: Idle -> Stalk -> Patrol.
    /// </summary>
    protected void CheckIfDoneWaitingAndSwitchStates()
    {
        if (!IsEntityMovingToPos())
        {
            if (entity is ISearchStateUser searchStateUser)
            {
                entity.stateMachine.ChangeState(searchStateUser.searchState);
            }
            else if (entity is IIdleStateUser idleStateUser)
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
