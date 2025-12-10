using UnityEngine;

public abstract class Monster : Entity
{
    protected abstract EntityState _startState { get; }
    protected override EntityState _startingState => _startState;

    protected override void Awake()
    {
        base.Awake();

        // Make sure that this game object and its children have the Monster layermask
        HelperMethods.SetLayerRecursive(transform, 7);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected virtual void OnEnable()
    {
        PlayerCollisions.OnPlayerCollidedWithMonster += HandleOnMonsterCollidedWithPlayer;
    }

    protected virtual void OnDisable()
    {
        PlayerCollisions.OnPlayerCollidedWithMonster -= HandleOnMonsterCollidedWithPlayer;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected abstract void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster);

    /// <summary>
    ///     Switch a state's behavior.
    /// </summary>
    protected void SwitchStateBehavior<T>(ref T currentState, T newBehavior) where T : EntityState
    {
        currentState = newBehavior;
    }
}
