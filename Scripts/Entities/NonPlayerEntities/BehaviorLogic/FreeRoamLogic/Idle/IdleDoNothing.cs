using UnityEngine;

public class IdleDoNothing : IdleState
{
    public override void EnterState()
    {
        base.EnterState();
        entity.SetDestination(entity.transform.position);
    }
}
