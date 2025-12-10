using UnityEngine;

public class SomnidEndSceneFrozen : MonoBehaviour
{
    [Header("Frozen Animation")]
    [Tooltip("The frozen animation that this Somnid will use (and not move)")]
    [SerializeField] private AnimationClip frozenAnim;

    private Animator animatorController;

    private void Awake()
    {
        animatorController = GetComponent<Animator>();
        
    }
}
