using System;
using UnityEngine;

public abstract class FreezeState : EntityState
{
    public event Action OnFreezeEnded;

    [Header("Freeze Animations")]
    [SerializeField] private Animator animator;

    [Tooltip("The animator's frozen int. When this entity is frozen, this is set to 1. When unfrozen, this is set to 0")]
    [SerializeField] private string frozenStr = "frozen";
    
    // The animator considers 1 to be frozen and 0 to be not frozen
    private const int frozen = 1; 
    private const int unFrozen = 0;

    [Tooltip("The animator's frozenAnim int. This can be used to randomly select a freeze animation")]
    [SerializeField] private string frozenAnimStr = "frozenAnim";

    [Tooltip("The possible freeze animations to use. A random number will be selected from this, with x and y being inclusive." + 
        "This should match how many frozen animations there are for this entity")]
    [SerializeField] private Vector2Int anims = new Vector2Int(1, 4);

    public override void EnterState()
    {
        base.EnterState();
        entity.SetDestination(entity.transform.position);

        if (animator != null)
        {
            // Select a random freeze animation
            int frozenAnim = UnityEngine.Random.Range(anims.x, anims.y + 1);
            animator.SetInteger(frozenAnimStr, frozenAnim);

            // Actually call the animator to use the frozen animation
            animator.SetInteger(frozenStr, frozen);
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        if (animator != null)
        {
            // Call the animator to stop using the frozen animation
            animator.SetInteger(frozenStr, unFrozen);
        }
    }

    /// <summary>
    ///     Fire off OnFreezeEnded event letting subscribers know to stop being frozen.
    /// </summary>
    protected void EndFreeze()
    {
        OnFreezeEnded?.Invoke();
    }
}
