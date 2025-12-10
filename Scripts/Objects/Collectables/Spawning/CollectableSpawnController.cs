using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The CollectableSpawnController game object will automatically be created if a CollectableSpawnLocation game object exists
public class CollectableSpawnController : MonoBehaviour
{
    private LevelDataSO data; // This script gets the current level's data from LevelController

    private List<CollectableSpawnLocation> spawnLocations = new List<CollectableSpawnLocation>();

    private static CollectableSpawnController _instance;
    public static CollectableSpawnController instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Singletons/CollectableSpawnController");
                if (prefab == null)
                {
#if UNITY_EDITOR
                    Debug.LogError("Could not instantiate CollectableSpawnController as the prefab could not be found in Resources/Singletons/.");
#endif
                    return null;
                }

                GameObject inScene = Instantiate(prefab);

                _instance = inScene.GetComponent<CollectableSpawnController>();
                if (_instance == null)
                {
                    inScene.AddComponent<CollectableSpawnController>();
                }
            }

            return _instance;
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnCollectablesProcess());
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    ///     Register a spawn location with CollectableSpawnController by adding it to the spawnLocations list.
    /// </summary>
    public void RegisterSpawnLocation(CollectableSpawnLocation spawnLocation)
    {
        spawnLocations.Add(spawnLocation);
    }

    /// <summary>
    ///     A coroutine that waits until data is not null, then starts the collectable spawn process.
    /// </summary>
    private IEnumerator SpawnCollectablesProcess()
    {
        // Wait until data is found before spawning
        while (data == null)
        {
            GetLevelData();
            yield return null;
        }

        StartCoroutine(SpawnCollectablesRoutine());
    }

    private void GetLevelData()
    {
        if (LevelController.instance != null)
        {
            data = LevelController.instance.GetLevelData();
        }
    }

    /// <summary>
    ///     A coroutine that initially waits for a tiny bit, then spawns in the collectables.
    /// </summary>
    private IEnumerator SpawnCollectablesRoutine()
    {
        yield return new WaitForSeconds(0.25f); // Wait a bit to be sure all spawn locations are registered

        int spawnLocationCount = spawnLocations.Count;
        int collectablesToSpawn = data.collectableCount;
        GameObject[] collectables;

        if (data.collectablePrefabs != null)
        {
            collectables = data.collectablePrefabs;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("LevelDataSO's collectablePrefab null. Unable to spawn collectables.");
#endif
            yield break;
        }

#if UNITY_EDITOR
        if (spawnLocationCount < collectablesToSpawn)
        {
            Debug.LogWarning("Spawn location count is less than the desired number of collectables to spawn. Unable " +
                "to spawn the specified number of collectables from LevelDataSO.collectableCount.");
        }
#endif

        // Randomize the spawns by shuffling the list
        HelperMethods.ShuffleList(spawnLocations);

        for (int i = 0; i < spawnLocationCount; i++)
        {
            if (i == collectablesToSpawn)
            {
                break;
            }

            Vector3 spawnPos = spawnLocations[i].GetPosition();
            GameObject instantiated = Instantiate(collectables[i % collectables.Length], spawnPos, Quaternion.identity);
            instantiated.transform.SetParent(spawnLocations[i].GetTransform(), true);
        }
    }
}
