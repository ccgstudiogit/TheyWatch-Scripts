using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    private static SpawnPlayer instance; // Only one SpawnPlayer script will be allowed, prevents the creation of multiple Player scenes

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name} is an excess SpawnPlayer instance. Destroying this spawn point at " + transform.position);
#endif
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Spawn();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Spawn()
    {
        if (LevelController.instance != null)
        {
            LevelController.instance.SpawnPlayer(transform);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("LevelController.instance null.");
#endif
        }
    }
}
