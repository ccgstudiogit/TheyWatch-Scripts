using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonLevelController : LevelController
{   
    // Lets prisoners know when the Warden is spawned and also sends a reference to the Warden game object
    public static event Action<GameObject> OnWardenSpawned;

    [Header("Opening Messages")]
    [Tooltip("This option is here because the DungeonHMLevelController inherits from DungeonLevelController, and I don't " + 
        "want the HM level controller to use opening messages")]
    [SerializeField] private string openingMessage = "The Warden watches over this place";
    [SerializeField] private float openingMessageDelay = 4f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " +
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time

    [Header("Warden Is Here Message")]
    [SerializeField] private string wardenIsHereMessage = "He's coming...";
    [SerializeField] private float wardenIsHereMessageDuration = 3.5f;

    [SerializeField] private bool sendHintMessage = true;
    [SerializeField] private float hintMessageDelay = 4.25f;
    [SerializeField] private string hintMessage = "Crouching is quieter";
    [SerializeField] private float hintMessageDuration = 5f;

    [Header("Warden Prefab")]
    [SerializeField] private GameObject wardenPrefab;
    private GameObject instantiatedWarden = null;

    [Header("Warden Spawn")]
    [Tooltip("The Warden will only spawn once this many runestones have been collected")]
    [SerializeField, Min(0)] private int minRunestones = 2;
    [SerializeField] private Vector2 spawnDelay = new Vector2(0.25f, 6f);

    [Tooltip("Spawn the Warden after this many seconds, regardless of whether or not the minRunestones requirement was met")]
    [SerializeField, Min(0f)] private float spawnWardenOverride = 60f;
    protected int runestonesCollected;

    [Tooltip("The Warden will be placed at a spawn location that is at least this distance from the player")]
    [SerializeField, Min(0)] private float minWardenSpawnDistance = 40f;

    [Tooltip("The Warden's spawn delay. This is mainly here to give extra time (if needed) to wardenIsHereMessage")]
    [SerializeField, Min(0f)] private float wardenSpawnDelay = 5.75f;

    private List<WardenSpawnLocation> wardenSpawnLocations = new List<WardenSpawnLocation>();

    private Coroutine spawnWardenRoutine = null;

    protected override void Start()
    {
        base.Start();

        runestonesCollected = 0;

        // Instantiate the Warden before gameplay starts to avoid lag spikes when instantiating the Warden
        instantiatedWarden = Instantiate(wardenPrefab, Vector3.zero, Quaternion.identity);
        instantiatedWarden.SetActive(false);

        if (SettingsManager.instance.AreDeviceTipsEnabled())
        {
            StartCoroutine(OpeningMessagesRoutine());
        }

        StartCoroutine(WardenSpawnTimer());
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Collectable.OnCollected += HandleCollectableCollected;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Collectable.OnCollected -= HandleCollectableCollected;
    }

    /// <summary>
    ///     Handles sending opening generic messages to the player's device.
    /// </summary>
    private IEnumerator OpeningMessagesRoutine()
    {
        yield return new WaitForSeconds(openingMessageDelay);
        SendMessageToWristDevice(openingMessage, openingMessageDuration);
    }

    /// <summary>
    ///     If the Warden is not spawned after a certain amount of time has passed, spawn the Warden regardless of whether or
    ///     not the minimum runestones required to spawn the Warden have been met.
    /// </summary>
    protected IEnumerator WardenSpawnTimer()
    {
        yield return new WaitForSeconds(spawnWardenOverride);
        SpawnWarden();
    }

    /// <summary>
    ///     Register a warden spawn location with this controller.
    /// </summary>
    /// <param name="wardenSpawnLocation">The WardenSpawnLocation to be registered.</param>
    public void RegisterWardenSpawnLocation(WardenSpawnLocation wardenSpawnLocation)
    {
        wardenSpawnLocations.Add(wardenSpawnLocation);
    }

    protected void HandleCollectableCollected()
    {
        runestonesCollected++;

        if (runestonesCollected >= minRunestones)
        {
            SpawnWarden();
        }
    }

    /// <summary>
    ///     Starts the coroutine to spawn the Warden. If the Warden is already instantiated, nothing happens.
    /// </summary>
    protected void SpawnWarden()
    {
        if (spawnWardenRoutine == null && (instantiatedWarden == null || (instantiatedWarden != null && !instantiatedWarden.activeSelf)))
        {
            spawnWardenRoutine = StartCoroutine(SpawnWardenRoutine());
        }
    }

    /// <summary>
    ///     Handles the necessary steps in spawning the Warden.
    /// </summary>
    private IEnumerator SpawnWardenRoutine()
    {
        float delay = UnityEngine.Random.Range(spawnDelay.x, spawnDelay.y);
        yield return new WaitForSeconds(delay);
        
        SendMessageToWristDevice(wardenIsHereMessage, wardenIsHereMessageDuration);

        // Send the tip message after a specified delay, so the message does not get overridden when the warden is here message is sent
        if (sendHintMessage && SettingsManager.instance.AreDeviceTipsEnabled())
        {
            this.Invoke(() => SendMessageToWristDevice(hintMessage, hintMessageDuration), hintMessageDelay);
        }

        yield return new WaitForSeconds(wardenSpawnDelay);

        // Get a spawn position
        WardenSpawnLocation targetSpawnLocation = null;
        for (int i = 0; i < wardenSpawnLocations.Count; i++)
        {
            if (Vector3.Distance(wardenSpawnLocations[i].transform.position, GetPlayer().transform.position) > minWardenSpawnDistance)
            {
                targetSpawnLocation = wardenSpawnLocations[i];
                break;
            }
        }

        // If a spawn location was not found in the above check, just pick a random one
        if (targetSpawnLocation == null && wardenSpawnLocations.Count > 0)
        {
            targetSpawnLocation = wardenSpawnLocations[UnityEngine.Random.Range(0, wardenSpawnLocations.Count)];

            // This is mainly here for testing purposes, as I don't imagine the targetSpawnLocation would be null in an actual build
            if (targetSpawnLocation == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{gameObject.name}'s targetSpawnLocation null. Unable to spawn Warden.");
#endif
                yield break;
            }
        }

        if (instantiatedWarden == null)
        {
            instantiatedWarden = Instantiate(wardenPrefab, targetSpawnLocation.gameObject.transform.position, Quaternion.identity);
        }
        else
        {
            instantiatedWarden.transform.position = targetSpawnLocation.gameObject.transform.position;
            instantiatedWarden.SetActive(true);
        }

        OnWardenSpawned?.Invoke(instantiatedWarden);
    }
}
