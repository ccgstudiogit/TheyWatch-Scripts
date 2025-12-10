using UnityEngine;

public class SomnidEndScene : Monster, IIdleStateUser
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    private EntityState startState;
    protected override EntityState _startState => startState;

    [Header("Behavior References")]
    [SerializeField] private IdleState idleBehavior;

    protected override void Awake()
    {
        base.Awake();

        _idleState = idleBehavior;

        startState = idleState;
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster) { }
}
