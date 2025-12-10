using UnityEngine;

public class CaughtPlayer : CaughtPlayerState
{
    public override void EnterState()
    {
        base.EnterState();
        StopMovement();
    }
}
