using UnityEngine;

public abstract class PatrolState : EntityState
{
    protected abstract float radius { get; }
#if UNITY_EDITOR
    public float Radius => radius; // Used for custom editor script to visualize searchRadius
#endif

    protected bool isMovingToPatrolPoint;

    public override void EnterState()
    {
        base.EnterState();

        isMovingToPatrolPoint = false;
    }
}
