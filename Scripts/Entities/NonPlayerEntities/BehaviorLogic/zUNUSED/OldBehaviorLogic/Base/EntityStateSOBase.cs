using UnityEngine;

public abstract class EntityStateSOBase : ScriptableObject
{
    protected Entity entity;
    protected Transform transform;
    protected GameObject gameObject;

    public virtual void Initialize(GameObject gameObject, Entity entity)
    {
        this.gameObject = gameObject;
        this.entity = entity;
        transform = gameObject.transform;
    }

    public virtual void DoEnterLogic() { }
    public virtual void DoExitLogic() { ResetValues(); }
    public virtual void DoFrameUpdateLogic() { }
    public virtual void DoPhysicsUpdateLogic() { }
    public virtual void DoAnimationTriggerEventLogic() { }
    public virtual void ResetValues() { }
}
