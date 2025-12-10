using UnityEngine;

public class OverseerSpawnLocation : MonoBehaviour
{
    private FactoryLevelController factoryLevelController;

    private void Start()
    {
        if (LevelController.instance != null && LevelController.instance is FactoryLevelController)
        {
            factoryLevelController = LevelController.instance as FactoryLevelController;
            factoryLevelController.RegisterOverseerSpawnLocation(this);
        }
    }
}
