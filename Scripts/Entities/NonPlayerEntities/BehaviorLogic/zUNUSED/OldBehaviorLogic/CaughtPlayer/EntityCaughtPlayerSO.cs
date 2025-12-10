using UnityEngine;

[CreateAssetMenu(menuName = "Entity Logic/Caught Player Logic/Caught Player")]
public class EntityCaughtPlayerSO : EntityCaughtPlayerSOBase
{
    public override void DoEnterLogic()
    {
        entity.SetDestination(transform.position);
    }
}
