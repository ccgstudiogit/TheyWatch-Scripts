using UnityEngine;

public class IdleDoNothingLookAtPlayer : IdleState
{
    [Header("Look At Player Settings")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float rotationSpeed = 10f;

    public override void EnterState()
    {
        base.EnterState();
        entity.SetDestination(entity.transform.position);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (player == null)
        {
            player = GetPlayerReference();
        }
        else
        {
            LookAtTarget(player.transform, pivot, rotationSpeed);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
