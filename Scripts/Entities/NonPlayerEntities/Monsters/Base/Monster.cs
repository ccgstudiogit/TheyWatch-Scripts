using UnityEngine;

public abstract class Monster : Entity
{
    [Header("Damage Dealt To Player")]
    [Tooltip("The damage this monster does to the player on contact")]
    [SerializeField] private int damage = 50;

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
        PlayerCollisions.OnPlayerDeath += HandleKillPlayer;
        PlayerCollisions.OnPlayerTakesDamage += HandleDamagePlayer;
    }

    protected virtual void OnDisable()
    {
        PlayerCollisions.OnPlayerDeath -= HandleKillPlayer;
        PlayerCollisions.OnPlayerTakesDamage -= HandleDamagePlayer;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected abstract void HandleKillPlayer(PlayerReferences playerReferences, Monster monster);
    protected virtual void HandleDamagePlayer(Monster monster) { }

    /// <summary>
    ///     Switch a state's behavior.
    /// </summary>
    protected void SwitchStateBehavior<T>(ref T currentState, T newBehavior) where T : EntityState
    {
        currentState = newBehavior;
    }

    /// <summary>
    ///     Get this monster's damage.
    /// </summary>
    public int GetDamage()
    {
        return damage;
    }
}
