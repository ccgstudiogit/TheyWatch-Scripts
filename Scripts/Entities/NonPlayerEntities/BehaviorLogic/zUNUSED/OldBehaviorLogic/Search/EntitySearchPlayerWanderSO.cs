using UnityEngine;

[CreateAssetMenu(menuName = "Entity Logic/Search Logic/Wander Search For Player")]
public class EntitySearchPlayerWanderSO : EntitySearchSOBase
{
    [Tooltip("The area around the player that this entity will go to")]
    [SerializeField] private float searchRadiusAroundPlayer = 10f;

    private GameObject playerRef;

    public override void DoEnterLogic()
    {
        playerRef = GetPlayerReference();
    }

    public override void DoFrameUpdateLogic()
    {
        if (playerRef == null)
        {
            playerRef = GetPlayerReference();
            return;
        }

        if (!isMovingToPos)
        {
            Vector3 pos = GetRandomPointAroundObject(searchRadiusAroundPlayer, takeOtherObjectsIntoAccount: false, playerRef);
            entity.SetDestination(pos);
            isMovingToPos = true;
        }
        else if (IsEntityDoneMovingToPos())
        {
            //if (entity is IIdleBrainUser idleBrainUser)
                //entity.stateMachine.ChangeState(idleBrainUser.idleState);
        }
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}
