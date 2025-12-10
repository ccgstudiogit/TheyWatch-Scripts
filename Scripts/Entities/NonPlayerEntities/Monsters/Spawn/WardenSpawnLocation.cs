using UnityEngine;

public class WardenSpawnLocation : MonoBehaviour
{
    private DungeonLevelController dungeonLevelController;

    private void Start()
    {
        if (LevelController.instance != null && LevelController.instance is DungeonLevelController)
        {
            dungeonLevelController = LevelController.instance as DungeonLevelController;
            dungeonLevelController.RegisterWardenSpawnLocation(this);
        }
    }
}
