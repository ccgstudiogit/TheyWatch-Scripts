using UnityEngine;

public class CellDoor : MonoBehaviour
{
    // This script is for dungeon HM, where the cell doors will be slightly open, giving the hint that the prisoners
    // have been let out

    [Tooltip("The rotation the cell door will have on Start() in Dungeon HardMode")]
    [SerializeField] private Quaternion startingRotation;

    private void Start()
    {
        if (LevelController.instance is IHMLevelController)
        {
            transform.rotation = startingRotation;
        }
    }
}
