using UnityEngine;

public class Munty : Monster
{
    private EntityState startState;
    protected override EntityState _startState => startState;

    protected override void Awake()
    {
        base.Awake();

        //startState = idleState;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void HandleOnMonsterCollidedWithPlayer(PlayerReferences playerReferences, Monster monster)
    {
        
    }
}
