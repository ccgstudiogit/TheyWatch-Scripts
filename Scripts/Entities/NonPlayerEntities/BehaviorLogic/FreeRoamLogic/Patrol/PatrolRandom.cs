using UnityEngine;

public class PatrolRandom : PatrolState
{
    [SerializeField] private float minimumTravelDistance = 17.5f;
    protected override float radius => minimumTravelDistance;

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        HandleMovement();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    private void HandleMovement()
    {
        if (!isMovingToPatrolPoint)
        {
            isMovingToPatrolPoint = true;
            MoveToPosition(FindNewRandomPositionOnNavMesh(minimumTravelDistance));
        }
        else if (!IsEntityMovingToPos())
        {
            if (entity is IIdleStateUser idleStateUser)
            {
                entity.stateMachine.ChangeState(idleStateUser.idleState);
            }
        }
    }
}
