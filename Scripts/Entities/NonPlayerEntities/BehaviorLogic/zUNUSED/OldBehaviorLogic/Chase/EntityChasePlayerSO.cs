using UnityEngine;

[CreateAssetMenu(menuName = "Entity Logic/Chase Logic/Chase Player")]
public class EntityChasePlayerSO : EntityChaseSOBase
{
    [SerializeField] private float chaseSpeedMultiplier = 1.15f;
    [SerializeField] private float minTimeInChaseBrain = 5f;

    private GameObject chaseTarget;
    private MonsterSight monsterSight;

    public override void DoEnterLogic()
    {
        elapsedTimeInChase = 0f;

        monsterSight = entity.GetComponent<MonsterSight>();

        chaseTarget = GetPlayerReference();

        entity.AnimatorSpeedMultiplier(chaseSpeedMultiplier);
        entity.MovementSpeedMultiplier(chaseSpeedMultiplier);
    }

    public override void DoFrameUpdateLogic()
    {
        elapsedTimeInChase += Time.deltaTime;

        if (chaseTarget == null)
        {
            chaseTarget = GetPlayerReference();
            return;
        }
        
        // Makes sure that if the monster is still looking at the player, the monster stays in chase state
        // *TODO may want to change this later to be less dependent on the entity being a monster
        if (monsterSight != null && monsterSight.isPlayerCurrentlySeen)
            elapsedTimeInChase = 0f;

        if (elapsedTimeInChase <= minTimeInChaseBrain)
            entity.SetDestination(chaseTarget.transform);
        //else if (entity is ISearchBrainUser searchBrainUser)
            //entity.stateMachine.ChangeState(searchBrainUser.searchState);
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();

        entity.ResetAnimatorSpeed();
        entity.ResetMovementSpeed();
    }
}
