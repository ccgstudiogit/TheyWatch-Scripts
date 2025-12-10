using UnityEngine;
using UnityEngine.Events;

public class CustomEventTrigger : TriggerCollider
{
    public UnityEvent onObjectEnterEvent;
    public UnityEvent onObjectExitEvent;

    protected override void OnObjectEntered()
    {
        onObjectEnterEvent?.Invoke();
    }

    protected override void OnObjectExit()
    {
        onObjectExitEvent?.Invoke();
    }
}
