using UnityEngine;

public class CryingPrisoner : MonoBehaviour
{
    private IdleState _idleState;
    public IdleState idleState => _idleState;

    [SerializeField] private Animator animator;
    private const string animatorScared = "scared";

    [SerializeField] private bool turnOffDuringHM = true;

    private void Start()
    {
        if (turnOffDuringHM && LevelController.instance is IHMLevelController)
        {
            gameObject.SetActive(false);
            return;
        }

        animator.SetInteger(animatorScared, 1);
    }
}
