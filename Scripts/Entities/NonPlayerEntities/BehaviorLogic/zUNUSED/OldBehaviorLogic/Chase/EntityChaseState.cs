public class EntityChaseState : OldEntityState
{
    private OldIChaseBrainUser chaseBrainUser; // Cache the reference to avoid checking every frame

    public EntityChaseState(Entity entity, EntityStateMachine entityStateMachine) : base(entity, entityStateMachine) 
    {
        chaseBrainUser = entity as OldIChaseBrainUser;
    }

    public override void EnterState()
    {
        chaseBrainUser?.entityChaseInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        chaseBrainUser?.entityChaseInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        chaseBrainUser?.entityChaseInstance.DoFrameUpdateLogic();
    }

    public override void PhysicsUpdate()
    {
        chaseBrainUser?.entityChaseInstance.DoPhysicsUpdateLogic();
    }

    public override void AnimationTriggerEvent(Entity.AnimationTriggerType triggerType)
    {
        chaseBrainUser?.entityChaseInstance.DoAnimationTriggerEventLogic();
    }
}
