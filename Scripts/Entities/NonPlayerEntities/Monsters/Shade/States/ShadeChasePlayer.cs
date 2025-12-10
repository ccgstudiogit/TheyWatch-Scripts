using UnityEngine;

public class ShadeChasePlayer : ChasePlayer
{
    [Header("Animation Settings")]
    [Tooltip("Used to control Shade's animations while chasing the player")]
    [SerializeField] private Animator animator;

    [Header("Arms Up Animation")]
    [SerializeField] private string layerName = "Arms";
    [Tooltip("The weight of this layer when chasing the player")]
    [SerializeField, Range(0f, 1f)] private float armsLayerWeight = 1f;

    [Header("Run Animation")]
    [Tooltip("The chance for Shade to use the run animation rather than continuing to crawl when entering chase. Set " + 
        "this value to -1 to gaurantee to not use the run anim or 101 to gaurantee to use the run anim")]
    [SerializeField, Range(-1, 101)] private int chanceToUseRunAnim = 50;
    private bool useRunAnim;

    public override void EnterState()
    {
        base.EnterState();

        useRunAnim = false;
        int chance = Random.Range(0, 100);

        if (chance <= chanceToUseRunAnim)
        {
            useRunAnim = true;
            AttemptToSetLayerWeight(armsLayerWeight);
            animator.SetInteger("run", 1);
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void ExitState()
    {
        base.ExitState();

        if (useRunAnim)
        {
            AttemptToSetLayerWeight(0);
            animator.SetInteger("run", 0);
        }
    }

    private void AttemptToSetLayerWeight(float weight)
    {
        int layerIndex = animator.GetLayerIndex(layerName);

        if (layerIndex != -1)
        {
            animator.SetLayerWeight(layerIndex, weight);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"{gameObject.name} has an animator reference but the animator does not have a {layerName} layer.");
        }
#endif
    }
}
