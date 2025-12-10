public class EntitySearchState : OldEntityState
{
    private OldISearchBrainUser searchBrainUser;

    public EntitySearchState(Entity entity, EntityStateMachine entityStateMachine) : base(entity, entityStateMachine)
    {
        searchBrainUser = entity as OldISearchBrainUser;
    }

    public override void EnterState()
    {
        searchBrainUser?.entitySearchInstance.DoEnterLogic();
    }

    public override void ExitState()
    {
        searchBrainUser?.entitySearchInstance.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        searchBrainUser?.entitySearchInstance.DoFrameUpdateLogic();
    }

    public override void PhysicsUpdate()
    {
        searchBrainUser?.entitySearchInstance.DoPhysicsUpdateLogic();
    }

    public override void AnimationTriggerEvent(Entity.AnimationTriggerType triggerType)
    { 
        searchBrainUser?.entitySearchInstance.DoAnimationTriggerEventLogic();
    }
}
