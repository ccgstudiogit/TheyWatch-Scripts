using UnityEngine;

public class StunnableEntity : Stunnable
{
    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Stun State")]
    [Tooltip("If this is not assigned in the inspector, this game object is searched and a reference is attempted to get that way")]
    [SerializeField] private Entity entity;

    private const string stunStr = "stun";

    protected override void Awake()
    {
        base.Awake();

        // Attempt to get an entity reference on the game object if it wasn't assigned in the inspector
        if (entity == null && TryGetComponent(out Entity e))
        {
            entity = e;
        }
    }

    public override void Stun()
    {
        if (!enabled)
        {
            return;
        }

        base.Stun();
        SetStunAnimation(1);
    }

    public override void StopStun()
    {
        if (!enabled)
        {
            return;
        }

        base.StopStun();
        SetStunAnimation(0);
    }
    
    /// <summary>
    ///     Set the stun animation to be active or inactive. 1 == active, 0 == inactive.
    /// </summary>
    protected virtual void SetStunAnimation(int active)
    {
        if (animator != null)
        {
            animator.SetInteger(stunStr, active);
        }
    }
}
