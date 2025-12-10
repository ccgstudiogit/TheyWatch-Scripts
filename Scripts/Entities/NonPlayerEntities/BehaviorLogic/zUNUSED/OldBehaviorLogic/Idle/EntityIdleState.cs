public class EntityIdleState : OldEntityState
{
    private OldIIdleBrainUser idleBrainUser;

    public EntityIdleState(Entity entity, EntityStateMachine entityStateMachine) : base(entity, entityStateMachine) 
    {
        idleBrainUser = entity as OldIIdleBrainUser;
    }

    public override void EnterState()
    {
        idleBrainUser?.entityIdleInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        idleBrainUser?.entityIdleInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        idleBrainUser?.entityIdleInstance.DoFrameUpdateLogic();
    }

    public override void PhysicsUpdate()
    {
        idleBrainUser?.entityIdleInstance.DoPhysicsUpdateLogic();
    }

    public override void AnimationTriggerEvent(Entity.AnimationTriggerType triggerType)
    { 
        idleBrainUser?.entityIdleInstance.DoAnimationTriggerEventLogic();
    }
}
