public class EntityCaughtPlayerState : OldEntityState
{
    private OldICaughtPlayerUser caughtPlayerUser;

    public EntityCaughtPlayerState(Entity entity, EntityStateMachine entityStateMachine) : base(entity, entityStateMachine)
    {
        caughtPlayerUser = entity as OldICaughtPlayerUser;
    }

    public override void EnterState()
    {
        caughtPlayerUser?.entityCaughtPlayerInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        caughtPlayerUser?.entityCaughtPlayerInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        caughtPlayerUser?.entityCaughtPlayerInstance.DoFrameUpdateLogic();
    }

    public override void PhysicsUpdate()
    {
        caughtPlayerUser?.entityCaughtPlayerInstance.DoPhysicsUpdateLogic();
    }

    public override void AnimationTriggerEvent(Entity.AnimationTriggerType triggerType)
    {
        caughtPlayerUser?.entityCaughtPlayerInstance.DoAnimationTriggerEventLogic();
    }
}
