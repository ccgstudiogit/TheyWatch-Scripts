using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Entity : MonoBehaviour
{
    private EntityStateMachine _stateMachine;
    public EntityStateMachine stateMachine
    {
        get
        {
            if (_stateMachine == null)
            {
                _stateMachine = new EntityStateMachine();
            }

            return _stateMachine;
        }
    }

    // Make sure the inherited scripts are the ones selecting the starting state
    protected abstract EntityState _startingState { get; }

    [Header("Animations")]
    [SerializeField] protected Animator animator;
    [SerializeField] private float animDampenSpeed = 0.15f;
    private bool canPlayAnimations = false;
    private float startingAnimationSpeed;

    public NavMeshAgent agent { get; protected set; }
    private NavMeshPath navMeshPath;
    private NavMeshQueryFilter navMeshFilter;
    private float originalSpeed;
    private float originalAcceleration;

    // For calculating when this entity has reached its destination. If the entity is within this
    // distance to the destination, the destination is considered reached
    private float pathEndThreshold = 0.1f;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;
        originalAcceleration = agent.acceleration;

        navMeshPath = new NavMeshPath();
        navMeshFilter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = NavMesh.AllAreas
        };

        if (animator != null)
        {
            canPlayAnimations = true;
            startingAnimationSpeed = animator.speed;
        }
    }

    protected virtual void Start()
    {
        stateMachine.Initialize(_startingState);
    }

    protected virtual void Update()
    {
        if (stateMachine.currentEntityState != null)
        {
            stateMachine.currentEntityState.FrameUpdate();
        }

        HandleAnimations();
    }

    protected virtual void FixedUpdate()
    {
        if (stateMachine.currentEntityState != null)
        {
            stateMachine.currentEntityState.PhysicsUpdate();
        }
    }

    /// <summary>
    ///     Sets this agent's navmesh destination via a transform.
    /// </summary>
    public void SetDestination(Transform destination)
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        agent.SetDestination(destination.position);
    }

    /// <summary>
    ///     Sets this agent's navmesh destination via a Vector3 position.
    /// </summary>
    public void SetDestination(Vector3 position)
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        agent.SetDestination(position);
    }

    /// <summary>
    ///     Teleport this agent to a new position.
    /// </summary>
    /// <param name="position">The position to teleport to.</param>
    public void Teleport(Vector3 position)
    {
        agent.Warp(position);
    }

    /// <summary>
    ///     Checks if this agent is currently moving towards a destination.
    /// </summary>
    /// <returns>False if this agent is currently moving. Otherwise returns true.</returns>
    public bool AtEndOfPath()
    {
        if (agent.pathPending)
        {
            return false;
        }

        bool closeEnough = agent.remainingDistance <= agent.stoppingDistance + pathEndThreshold;
        bool isStopped = agent.velocity.sqrMagnitude < 0.1f;

        if (closeEnough && isStopped)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Get the overall distance of an agent's generated path on a navmesh surface (this takes into account various turns and
    ///     corners and is not a direct, straight path).
    /// </summary>
    /// <param name="targetPosition">The target position.</param>
    /// <returns>The total distance as a float.</returns>
    public float GetPathDistance(Vector3 targetPosition)
    {
        float distance = 0;

        if (NavMesh.CalculatePath(transform.position, targetPosition, navMeshFilter, navMeshPath))
        {
            if (navMeshPath.status != NavMeshPathStatus.PathComplete)
            {
                return distance;
            }

            // If a path is found, calculate the overall distance using the corners of the path. Note: since navMeshPath.corners[0]
            // is the starting point, i starts at 1
            for (int i = 1; i < navMeshPath.corners.Length; i++)
            {
                distance += Vector3.Distance(navMeshPath.corners[i - 1], navMeshPath.corners[i]);
            }
        }

        return distance;
    }

    /// <summary>
    ///     Set the current speed of this agent to a new speed.
    /// </summary>
    public void SetMovementSpeed(float speed)
    {
        if (speed >= 0)
        {
            agent.speed = speed;
        }
    }

    /// <summary>
    ///     Multiply the current speed of this agent by a multiplier.
    /// </summary>
    public void MovementSpeedMultiplier(float multiplier)
    {
        agent.speed *= multiplier;
    }

    /// <summary>
    ///     Resets the speed of this agent to its original speed set in the inspector upon entering playmode.
    /// </summary>
    public void ResetMovementSpeed()
    {
        agent.speed = originalSpeed;
    }

    /// <summary>
    ///     Multiply the current acceleration of this agent by a multiplier.
    /// </summary>
    public void AccelerationMultiplier(float multiplier)
    {
        agent.acceleration *= multiplier;
    }

    /// <summary>
    ///     Resets the acceleration of this agent to its original speed set in the inspector upon entering playmode.
    /// </summary>
    public void ResetAccelerationSpeed()
    {
        agent.acceleration = originalAcceleration;
    }

    private void HandleAnimations()
    {
        if (!canPlayAnimations)
        {
            return;
        }

        animator.SetFloat("speed", agent.velocity.magnitude, animDampenSpeed, Time.deltaTime);
    }

    /// <summary>
    ///     Set the current speed of this entity's animator component to a new speed.
    /// </summary>
    public void SetAnimatorSpeed(float speed)
    {
        animator.speed = speed;
    }

    /// <summary>
    ///     Multiply the current animation speed of this agent's animator by a multiplier.
    /// </summary>
    public void AnimatorSpeedMultiplier(float multiplier)
    {
        animator.speed *= multiplier;
    }

    /// <summary>
    ///     Resets the animation speed of this agent to its original speed set in the animator.
    /// </summary>
    public void ResetAnimatorSpeed()
    {
        animator.speed = startingAnimationSpeed;
    }

    public enum AnimationTriggerType
    {
        PlayFootstepSound
    }

    /// <summary>
    ///     Trigger an animation event for this entity's current state.
    /// </summary>
    public virtual void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        if (stateMachine.currentEntityState != null)
        {
            stateMachine.currentEntityState.AnimationTriggerEvent(triggerType);
        }
    }

    /// <summary>
    ///     Check whether this entity is in a specific state.
    /// </summary>
    /// <returns>True if inThisState is equal to this entity's current state.</returns>
    protected bool IsEntityInSpecificState(EntityState inThisState)
    {
        return inThisState != null && stateMachine.currentEntityState == inThisState;
    }

    /// <summary>
    ///     Get the navmesh agent's query filter, useful for managing multiple agent types on multiple navmesh surfaces.
    /// </summary>
    /// <returns>The NavMeshQueryFilter of this entity's navmesh agent component.</returns>
    public NavMeshQueryFilter GetNavMeshFilter()
    {
        return navMeshFilter;
    }
}
