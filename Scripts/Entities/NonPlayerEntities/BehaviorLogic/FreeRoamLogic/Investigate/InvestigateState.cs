using System;
using UnityEngine;

public abstract class InvestigateState : EntityState
{
    public event Action OnDoneInvestigating;

    protected GameObject emptyTarget { get; private set; } = null;

    protected override void Awake()
    {
        base.Awake();
        CreateEmptyTarget();
    }

    /// <summary>
    ///     Set the target position to investigate.
    /// </summary>
    public void SetInvestigateTargetPosition(Vector3 position)
    {
        emptyTarget.transform.position = position;
    }

    /// <summary>
    ///     Create the emptyTarget game object.
    /// </summary>
    private void CreateEmptyTarget()
    {
        emptyTarget = Instantiate(new GameObject());
    }

    /// <summary>
    ///     Fires off OnDoneInvestigating event.
    /// </summary>
    protected void DoneInvestigating()
    {
        OnDoneInvestigating?.Invoke();
    }
}
