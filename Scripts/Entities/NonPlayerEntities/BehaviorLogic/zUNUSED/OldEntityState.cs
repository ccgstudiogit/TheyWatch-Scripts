public abstract class OldEntityState
{
    protected Entity entity;
    protected EntityStateMachine entityStateMachine;

    public OldEntityState(Entity entity, EntityStateMachine entityStateMachine)
    {
        this.entity = entity;
        this.entityStateMachine = entityStateMachine;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(Entity.AnimationTriggerType triggerType) { }
}
