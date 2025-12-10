using UnityEngine;

[CreateAssetMenu(menuName = "Entity Logic/Search Logic/Wander Around Randomly")]
public class EntitySearchWanderSO : EntitySearchSOBase
{
    [Tooltip("The entity will select a random point on the nav mesh within this search radius (walls and other objects don't interfere)")]
    [SerializeField] private float searchRadius = 15f;

    private Vector3 pos = Vector3.zero;

    public override void DoFrameUpdateLogic()
    {
        if (!isMovingToPos)
        {
            pos = GetRandomPoint(searchRadius);
            entity.SetDestination(pos);
            isMovingToPos = true;
        }
        else if (IsEntityDoneMovingToPos())
        {
            isMovingToPos = false;

            //if (entity is IIdleBrainUser idleBrainUser)
                //entity.stateMachine.ChangeState(idleBrainUser.idleState);
        }
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
