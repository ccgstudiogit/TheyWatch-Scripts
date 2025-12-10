using UnityEngine;

[CreateAssetMenu(menuName = "Entity Logic/Idle Logic/Stay Put")]
public class EntityIdleStayPutSO : EntityIdleSOBase
{
    [SerializeField] private float maxTimeInIdle = 4f;
    [SerializeField] private float minTimeInIdle = 0.5f;
    private float idleTime;
    private float elapsedTime;

    public override void DoEnterLogic()
    {
        idleTime = Random.Range(minTimeInIdle, maxTimeInIdle);
        elapsedTime = 0f;
    }

    public override void DoFrameUpdateLogic()
    {
        elapsedTime += Time.deltaTime;
        
        //if (elapsedTime >= idleTime && entity is ISearchBrainUser searchBrainUser)
        //   entity.stateMachine.ChangeState(searchBrainUser.searchState);
    }
}
