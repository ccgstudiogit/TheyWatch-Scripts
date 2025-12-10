using UnityEngine;

public class PatrolTowardsPlayer : PatrolState
{
    [SerializeField] private float searchRadiusAroundPlayer = 10f;
    protected override float radius => searchRadiusAroundPlayer;

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (player == null)
        {
            player = GetPlayerReference();
            return;
        }

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
            MoveToPosition(FindNewRandomPositionInRange(searchRadiusAroundPlayer, takeOtherObjectsIntoAccount: false, player));
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
