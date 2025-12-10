using UnityEngine;

public class ShadeRetreat : RetreatToDistantWayPoint
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool useRunAnim = true;

    public override void EnterState()
    {
        if (useRunAnim && animator != null)
        {
            animator.SetInteger("run", 1);
        }

        base.EnterState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void ExitState()
    {
        if (useRunAnim && animator != null)
        {
            animator.SetInteger("run", 0);
        }

        base.ExitState();
    }
}
