using UnityEngine;

public abstract class StunState : EntityState
{
    [Header("Stun Animations")]
    [SerializeField] private Animator animator;

    [Tooltip("The animator's stun int. When this entity is stunned, this is set to 1. When unstunned, this is set to 0")]
    [SerializeField] private string stunStr = "stun";
    
    // The animator considers 1 to be stunned and 0 to be not stunned
    private const int stun = 1; 
    private const int unStunned = 0;

    [Tooltip("The animator's stunAnim int. This can be used to randomly select a stun animation")]
    [SerializeField] private string stunAnimStr = "stunAnim";

    [Tooltip("The possible stun animations to use. A random number will be selected from this, with x and y being inclusive." +
        "This should match how many stun animations there are for this entity")]
    [SerializeField] private Vector2Int anims = new Vector2Int(1, 3);

    public override void EnterState()
    {
        base.EnterState();
        entity.SetDestination(entity.transform.position);

        if (animator != null)
        {
            // Select a random stun animation
            int stunAnim = Random.Range(anims.x, anims.y + 1);
            animator.SetInteger(stunAnimStr, stunAnim);

            // Actually call the animator to use the stun animation
            animator.SetInteger(stunStr, stun);
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        if (animator != null)
        {
            // Call the animator to stop using the stunned animation
            animator.SetInteger(stunStr, unStunned);
        }
    }
}
