using UnityEngine;

public class SleepPoint : MonoBehaviour
{
    // Makes sure an entity does not come to this location if it's already occupied.
    [Header("For Debug - Do Not Edit")]
    [SerializeField] private bool occupied;

    private void Start()
    {
        occupied = false;

        if (LevelController.instance != null && LevelController.instance is ISleepPointLevelController)
        {
            ISleepPointLevelController sleepPointLevelController = LevelController.instance as ISleepPointLevelController;
            sleepPointLevelController.RegisterSleepPoint(this);
        }
    }

    /// <summary>
    ///     Check if this SleepPoint is currently occupied.
    /// </summary>
    /// <returns>True if an entity is sleeping at this location, false if otherwise.</returns>
    public bool IsOccupied()
    {
        return occupied;
    }

    /// <summary>
    ///     Set this SleepPoint's occupied flag to be true or false.
    /// </summary>
    /// <param name="occupied">Whether or not this SleepPoint is occupied or not.</param>
    public void SetOccupied(bool occupied)
    {
        this.occupied = occupied;
    }
}
