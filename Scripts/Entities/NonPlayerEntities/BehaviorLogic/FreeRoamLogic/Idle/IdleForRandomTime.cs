using UnityEngine;

public class IdleForRandomTime : IdleState
{
    [Header("Idle Time Settings")]
    [SerializeField] private float minIdleTime = 0.5f;
    [SerializeField] private float maxIdleTime = 2f;

    [SerializeField] private bool useAnimationCurveInstead;
    [SerializeField] private AnimationCurve idleCurve;

    [Header("Look At Player Settings")]
    [Tooltip("Optional reference to make this entity look at the player's direction when idling")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float rotationSpeed = 10f;

    private float idleTime;
    private float elapsedTime;

    public override void EnterState()
    {
        base.EnterState();

        idleTime = useAnimationCurveInstead ? HelperMethods.GetRandomValueFromAnimationCurve(idleCurve) : Random.Range(minIdleTime, maxIdleTime);

#if UNITY_EDITOR
        Debug.Log("Random idle time = " + idleTime);
#endif

        elapsedTime = 0f;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        elapsedTime += Time.deltaTime;

        // It's helpful to remain looking at the player even when far away so that when this entity enters a different state (most likely
        // stalk state), the entity is looking towards the player's direction which is helpful for GetSequentialWayPointTowardsTarget()
        if (player != null && pivot != null && !IsEntityMovingToPos())
        {
            LookAtTarget(player.transform, pivot, rotationSpeed);
        }

        if (elapsedTime > idleTime)
        {
            if (entity is IStalkStateUser stalkStateUser)
            {
                entity.stateMachine.ChangeState(stalkStateUser.stalkState);
            }
            else if (entity is ISearchStateUser searchStateUser)
            {
                entity.stateMachine.ChangeState(searchStateUser.searchState);
            }
            else if (entity is IPatrolStateUser patrolStateUser)
            {
                entity.stateMachine.ChangeState(patrolStateUser.patrolState);
            }
        }
    }
}
