using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryLevelController : LevelController, ISleepPointLevelController
{
    [Header("Opening Messages")]
    [SerializeField] private string openingMessage = "Though ancient, the Overseer remains operational";
    [SerializeField] private float openingMessageDelay = 4f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " +
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time

    [Header("Overseer")]
    [SerializeField] private GameObject overseerPrefab;
    private GameObject instantiatedOverseer;

    [Header("Overseer Spawn")]
    [Tooltip("Overseer will only spawn once this many runestones have been collected")]
    [SerializeField, Min(0)] private int minRunestones = 1;

    [Tooltip("Spawn Overseer after this many seconds, regardless of whether or not the minRunestones requirement was met")]
    [SerializeField, Min(0f)] private float spawnOverseerOverride = 60f;
    protected int runestonesCollected;

    [Tooltip("Overseer will be placed at a spawn location that is at least this distance from the player")]
    [SerializeField, Min(0)] private float minOverseerSpawnDistance = 40f;

    [Tooltip("Overseer's min/max spawn delay after the player collects minRunestones")]
    [SerializeField] private Vector2 overseerSpawnDelay = new Vector2(5f, 20f);

    public List<SleepPoint> sleepPoints { get; private set; } = new List<SleepPoint>();
    private List<OverseerSpawnLocation> overseerSpawnLocations = new List<OverseerSpawnLocation>();

    private Coroutine spawnOverseerRoutine = null;

    protected override void Start()
    {
        base.Start();

        // Instantiate Overseer before gameplay starts to avoid lag spikes when instantiating Overseer
        if (overseerPrefab != null)
        {
            instantiatedOverseer = Instantiate(overseerPrefab, Vector3.zero, Quaternion.identity);
            instantiatedOverseer.SetActive(false);
        }
        
        if (SettingsManager.instance.AreDeviceTipsEnabled())
        {
            StartCoroutine(OpeningMessagesRoutine());
        }

        StartCoroutine(OverseerSpawnTimer());
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
    ///     Register a sleep point.
    /// </summary>
    public void RegisterSleepPoint(SleepPoint sleepPoint)
    {
        sleepPoints.Add(sleepPoint);
    }

    /// <summary>
    ///     Register an Overseer spawn location.
    /// </summary>
    public void RegisterOverseerSpawnLocation(OverseerSpawnLocation spawnLocation)
    {
        overseerSpawnLocations.Add(spawnLocation);
    }

    protected void HandleCollectableCollected()
    {
        runestonesCollected++;

        if (runestonesCollected >= minRunestones)
        {
            SpawnOverseer();
        }
    }

    /// <summary>
    ///     Starts the coroutine to spawn Overseer.
    /// </summary>
    protected void SpawnOverseer()
    {
        if (overseerPrefab != null && spawnOverseerRoutine == null && (instantiatedOverseer == null || (instantiatedOverseer != null && !instantiatedOverseer.activeSelf)))
        {
            spawnOverseerRoutine = StartCoroutine(SpawnOverseerRoutine());
        }
    }

    /// <summary>
    ///     Handles the necessary steps in spawning Overseer.
    /// </summary>
    private IEnumerator SpawnOverseerRoutine()
    {
        yield return new WaitForSeconds(Random.Range(overseerSpawnDelay.x, overseerSpawnDelay.y));

        // Get a spawn position
        OverseerSpawnLocation targetSpawnLocation = null;
        for (int i = 0; i < overseerSpawnLocations.Count; i++)
        {
            if (Vector3.Distance(overseerSpawnLocations[i].transform.position, GetPlayer().transform.position) > minOverseerSpawnDistance)
            {
                targetSpawnLocation = overseerSpawnLocations[i];
                break;
            }
        }

        // If a spawn location was not found in the above check, just pick a random one
        if (targetSpawnLocation == null && overseerSpawnLocations.Count > 0)
        {
            targetSpawnLocation = overseerSpawnLocations[Random.Range(0, overseerSpawnLocations.Count)];

            // This is mainly here for testing purposes, as I don't imagine the targetSpawnLocation would be null in an actual build
            if (targetSpawnLocation == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{gameObject.name}'s targetSpawnLocation null. Unable to spawn Warden.");
#endif
                yield break;
            }
        }

        if (instantiatedOverseer == null)
        {
            instantiatedOverseer = Instantiate(overseerPrefab, targetSpawnLocation.gameObject.transform.position, Quaternion.identity);
        }
        else
        {
            instantiatedOverseer.transform.position = targetSpawnLocation.gameObject.transform.position;
            instantiatedOverseer.SetActive(true);
        }
    }

    /// <summary>
    ///     Begin a countdown using spawnOverseerOverride. If the Overseer hasn't been spawned before the time has elapsed, spawn Overseer.
    /// </summary>
    private IEnumerator OverseerSpawnTimer()
    {
        yield return new WaitForSeconds(spawnOverseerOverride);
        SpawnOverseer();
    }
}
