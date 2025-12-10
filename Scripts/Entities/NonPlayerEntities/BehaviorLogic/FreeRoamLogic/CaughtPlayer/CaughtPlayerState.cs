using UnityEngine;

public abstract class CaughtPlayerState : EntityState
{
    /// <summary>
    ///     Stop this entity's current movement by setting the destination to its own transform.position.
    /// </summary>
    protected void StopMovement()
    {
        entity.SetDestination(entity.transform.position);
    }
}
