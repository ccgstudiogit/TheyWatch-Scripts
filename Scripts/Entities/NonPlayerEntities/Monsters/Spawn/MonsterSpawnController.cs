using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MonsterSpawnController : MonoBehaviour
{
    // Lets listeners know when the monster(s) have been instantiated
    public static event Action OnMonsterSpawnsComplete;

    private LevelDataSO data;

    private List<MonsterSpawnLocation> spawnLocations = new List<MonsterSpawnLocation>();

    private static MonsterSpawnController _instance;
    public static MonsterSpawnController instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Singletons/MonsterSpawnController");
                if (prefab == null)
                {
                    Debug.LogError("Could not instantiate MonsterSpawnController as the prefab could not be found in Resources/Singletons/.");
                    return null;
                }

                GameObject inScene = Instantiate(prefab);

                _instance = inScene.GetComponent<MonsterSpawnController>();
                if (_instance == null)
                {
                    inScene.AddComponent<MonsterSpawnController>();
                }
            }

            return _instance;
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnMonstersProcess());
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    ///     Registers a SpawnLocation with MonsterSpawnController by adding to the spawn locations list.
    /// </summary>
    public void RegisterSpawnLocation(MonsterSpawnLocation spawnLocation)
    {
        spawnLocations.Add(spawnLocation);
    }

    private void GetLevelData()
    {
        if (LevelController.instance != null)
        {
            data = LevelController.instance.GetLevelData();
        }
    }

    private IEnumerator SpawnMonstersProcess()
    {
        while (data == null)
        {
            GetLevelData();
            yield return null;
        }

        StartCoroutine(SpawnMonsterRoutine());
    }

    private IEnumerator SpawnMonsterRoutine()
    {
        int spawnLocationCount = spawnLocations.Count;
        int numOfMonstersToSpawn = data.monsterPrefabs.Length;

        if (numOfMonstersToSpawn < 1)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} attempting to spawn monsters but there are no monsters found in this level's LevelData.");
#endif
            yield break;
        }

        yield return new WaitForSeconds(0.135f); // Wait a bit to make sure all spawn locations are registered (**most likely not necessary to do this)

        if (LevelController.instance != null && LevelController.instance.includeMonsterSpawnDelay)
        {
            yield return new WaitForSeconds(LevelController.instance.monsterSpawnDelay);
        }

        HelperMethods.ShuffleList(spawnLocations);

        for (int i = 0; i < spawnLocationCount; i++)
        {
            if (i == numOfMonstersToSpawn)
            {
                break;
            }

            Vector3 spawnPos = spawnLocations[i].GetPosition();
            Instantiate(data.monsterPrefabs[i], spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.65f); // Slight delay to make sure monsters are fully loaded in/initialized

        OnMonsterSpawnsComplete?.Invoke();
    }
}
