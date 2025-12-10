public class EntityStateMachine
{
    private EntityState activeState = null;
    public EntityState currentEntityState
    {
        get
        {
            if (HelperMethods.FullyActive(activeState))
            {
                return activeState;
            }

            return null;
        }
    }

    /// <summary>
    ///     Initialize the state machine by setting currentEntityState to the startingState, and calling state.EnterState().
    /// </summary>
    /// <param name="startingState">The starting state of this entity.</param>
    public void Initialize(EntityState startingState)
    {
        if (!HelperMethods.FullyActive(startingState))
        {
            return;
        }

        activeState = startingState;
        activeState.EnterState();
    }

    /// <summary>
    ///     Changes this entity's state to a new state. The old state's ExitState() is called, currentEntityState is set
    ///     to the new state, and then the new state's EnterState() is called.
    /// </summary>
    /// <param name="newState">Change this entity's state to this new state.</param>
    public void ChangeState(EntityState newState)
    {
        if (!HelperMethods.FullyActive(newState))
        {
            return;
        }

        // Make sure that, in the extremely unlikely chance, if activeState is null, do not call ExitState()
        if (HelperMethods.FullyActive(activeState))
        {
            activeState.ExitState();
        }

        activeState = newState;
        activeState.EnterState();
    }
}
