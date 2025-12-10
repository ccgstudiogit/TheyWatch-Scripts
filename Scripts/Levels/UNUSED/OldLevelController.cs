using System;
using System.Collections;
using UnityEngine;

public class OldLevelController : MonoBehaviour //DestroyWhenBackInMainMenu Old script that I deleted from the project
{
    public static event Action<bool> OnPause;

    [Header("Scenes")]
    [Tooltip("The scenes that should be loaded additively once LevelController is instantiated (This will be universal for ALL levels)")]
    [SerializeField] private SceneName[] scenesToLoad;

    [Header("Pause Settings")]
    [Tooltip("Sets Time.timeScale to 0 when paused. Sets Time.timeScale back to 1 when un-paused")]
    [SerializeField] private bool stopTimeWhenPaused = true;
    private bool currentlyPaused = false;

    private LevelData levelData; // The object that holds a reference to LevelDataSO
    private LevelDataSO data; // The actual data that the level needs

    private int collectablesRemaining;
    /*
    private static LevelController _instance;
    public static LevelController instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Singletons/LevelController");
                if (prefab == null)
                {
                    Debug.LogError("Could not instantiate LevelController as the prefab could not be found in Resources/Singletons/.");
                    return null;
                }

                GameObject inScene = Instantiate(prefab);

                _instance = inScene.GetComponent<LevelController>();
                if (_instance == null)
                    inScene.AddComponent<LevelController>();

                DontDestroyOnLoad(_instance.transform.root.gameObject);
            }

            return _instance;
        }
    }
    */
    private void Start()
    {
        AdditivelyLoadScenes(scenesToLoad);
        FindLevelData(); // Searches for the LevelData gameobject that holds this scene's specific LevelDataSO
        InitLevelData();
    }
    /*
    protected override void OnEnable()
    {
        base.OnEnable();

        //UniversalPlayerInput.OnPause += HandleOnPausePressed;

        Collectable.OnCollected += CollectableCollected;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        //UniversalPlayerInput.OnPause -= HandleOnPausePressed;

        Collectable.OnCollected -= CollectableCollected;
    }

    protected override void OnDestroy()
    {
        // Set LevelController instance to null (this should only happen when leaving the game scenes to go to the main menu) as
        // LevelController will be destroyed if the player loads back into the main menu (as this inherits from DestroyWhenBackInMainMenu)
        //_instance = null;
    }
    */
    private void AdditivelyLoadScenes(SceneName[] scenes)
    {
        if (scenes.Length < 1)
            return;

        foreach (SceneName scene in scenes)
        {
            string sN = scene.ToString();

            if (SceneHandler.IsSceneLoaded(sN))
                continue;

            SceneHandler.LoadSceneImmediate(sN, additive: true);
        }
    }

    private void FindLevelData()
    {
        GameObject levelDataGO = GameObject.FindWithTag("LevelData");
        if (levelDataGO == null)
        {
            GetGenericData();
            return;
        }
        else
            levelData = levelDataGO.GetComponent<LevelData>();

        data = levelData.GetData();
        if (data == null)
            GetGenericData();
    }

    private void GetGenericData()
    {
        LevelDataSO genericData = Resources.Load<LevelDataSO>("LevelData/GenericData");
        if (genericData == null)
        {
            Debug.LogWarning($"{name} attempted to load GenericData from Resources/LevelData/ because a game object " +
                $"containing level data was not found, but {name} could not find GenericData in Resources/LevelData/.");
            return;
        }

        Debug.LogWarning("The game object containing level data was not found, so GenericData was loaded from " +
            "Resources/LevelData/ and will be used as this level's data.");

        data = genericData;
    }

    private void InitLevelData()
    {
        collectablesRemaining = data.collectableCount;
    }

    public LevelDataSO GetLevelData()
    {
        if (data != null)
            return data;
        else
            return null;
    }

    public void SpawnPlayer()
    {
        Vector3 defaultSpawn = Vector3.zero;
        SpawnPlayer(defaultSpawn);
    }

    public void SpawnPlayer(Vector3 spawnPos)
    {
        StartCoroutine(SpawnPlayerRoutine(spawnPos));
    }

    private IEnumerator SpawnPlayerRoutine(Vector3 spawnPos)
    {
        if (!SceneHandler.IsSceneLoaded(SceneName.Player.ToString()))
        {
            yield return SceneHandler.LoadSceneCoroutine(SceneName.Player.ToString(), additive: true);

            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                Transform playerRoot = player.transform.root;
                playerRoot.position = spawnPos;

                ConfigurePlayerGameObject(player);
            }
            else
                Debug.LogWarning("Player game object not found after loading " + SceneName.Player.ToString() + " scene!");
        }
        else
            Debug.LogWarning("LevelController attempting to spawn player while player already exists!");
    }

    // Configure player game object using playerConfigSO data
    private void ConfigurePlayerGameObject(GameObject player)
    {
        PlayerConfigSO playerConfig = data.playerConfig;
        
        if (playerConfig == null)
            return;

        /*
        // Add necessary input scripts
        if (playerConfig.inputFlashlight)
            player.AddComponent<InputFlashlight>();
        if (playerConfig.inputCollectableSight)
            player.AddComponent<InputCollectableSight>();
        */
    }

    public bool IsPaused()
    {
        return currentlyPaused;
    }

    private void HandleOnPausePressed()
    {
        // Check if the game is not currently paused. If it's not, pause the game. Else, resume game
        if (!currentlyPaused)
            PauseGame();
        else
            ResumeGame();
    }

    public void PauseGame()
    {
        currentlyPaused = true;
        OnPause?.Invoke(currentlyPaused);

        if (stopTimeWhenPaused)
            Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        currentlyPaused = false;
        OnPause?.Invoke(currentlyPaused);

        if (Time.timeScale != 1)
            Time.timeScale = 1;
    }

    private void CollectableCollected()
    {
        collectablesRemaining--;

        if (collectablesRemaining < 1)
            AllCollectablesCollected();
    }

    private void AllCollectablesCollected()
    {
        Debug.Log("All collectables collected!");
    }
}
