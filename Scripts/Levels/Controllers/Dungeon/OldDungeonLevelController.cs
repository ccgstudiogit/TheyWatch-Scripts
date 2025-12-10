using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldDungeonLevelController : LevelController
{
    // Lets prisoners know when the Warden is on/off the map so they can manage being scared
    public static event Action<bool> OnWardenActive;

    [Header("Opening Messages")]
    [SerializeField] private string openingMessage = "The Warden watches over this place";
    [SerializeField] private float openingMessageDelay = 4f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " +
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time

    [Header("Warden Prefab")]
    [SerializeField] private GameObject wardenPrefab;
    private GameObject instantiatedWarden = null;

    [Header("Warden Spawn")]
    [SerializeField, Min(1)] private int spawnWardenEveryXRunestonesCollected = 3;
    [Tooltip("The Warden will be placed at a spawn location that is at least this distance from the player")]
    [SerializeField, Min(0)] private float minWardenSpawnDistance = 40f;
    private int runestonesCollected;

    private List<WardenSpawnLocation> wardenSpawnLocations = new List<WardenSpawnLocation>();

    [Header("Warden Settings")]
    [Tooltip("The time in seconds that the Warden is active around the dungeon, searching for the player")]
    [SerializeField] private float wardenActiveTime = 65f;

    public bool wardenActive { get; private set; }

    private Coroutine wardenActiveRoutine;

    protected override void Start()
    {
        base.Start();

        runestonesCollected = 0;

        StartCoroutine(OpeningMessagesRoutine());
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

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            EnterWarden();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            ExitWarden();
        }
    }
#endif

    /// <summary>
    ///     Register a warden spawn location with this controller.
    /// </summary>
    /// <param name="wardenSpawnLocation">The WardenSpawnLocation to be registered.</param>
    public void RegisterWardenSpawnLocation(WardenSpawnLocation wardenSpawnLocation)
    {
        wardenSpawnLocations.Add(wardenSpawnLocation);
    }

    /// <summary>
    ///     Handles instantiating/setting the Warden active when the Warden should enter the dungeon.
    /// </summary>
    private void EnterWarden()
    {
        Debug.Log("EnterWarden()");
        // Reset the warden active timer if the Warden is already active
        if (wardenActiveRoutine != null)
        {
            Debug.Log("Stopping current coroutine and starting a new one");
            StopCoroutine(wardenActiveRoutine);
            wardenActiveRoutine = null;
            wardenActiveRoutine = StartCoroutine(WardenActiveRoutine());
            return;
        }

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
            Debug.Log("Picking a random spawn location since the for loop failed");
            targetSpawnLocation = wardenSpawnLocations[UnityEngine.Random.Range(0, wardenSpawnLocations.Count)];
        }

        // This is mainly here for testing purposes, as I don't imagine the targetSpawnLocation would be null in an actual build
        if (targetSpawnLocation == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s targetSpawnLocation null. Unable to spawn Warden.");
#endif
            return;
        }

        if (instantiatedWarden == null)
        {
            Debug.Log("Instantiating warden");
            instantiatedWarden = Instantiate(wardenPrefab, targetSpawnLocation.gameObject.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log($"Warden already exists, moving Warden to {targetSpawnLocation.name} at {targetSpawnLocation.transform.position}");
            instantiatedWarden.transform.position = targetSpawnLocation.gameObject.transform.position;
            instantiatedWarden.SetActive(true);
        }

        wardenActive = true;
        wardenActiveRoutine = StartCoroutine(WardenActiveRoutine());

        OnWardenActive?.Invoke(true);
    }

    /// <summary>
    ///     This coroutine handles managing the amount of time the Warden needs to be active in the dungeon. Once the time is over, ExitWarden()
    ///     is called.
    /// </summary>
    private IEnumerator WardenActiveRoutine()
    {
        Debug.Log("WardenActiveRoutine started");
        yield return new WaitForSeconds(wardenActiveTime);
        wardenActiveRoutine = null;
        ExitWarden();
        Debug.Log("WardenActiveRoutine completely finished");
    }

    /// <summary>
    ///     Handles setting the Warden to be inactive.
    /// </summary>
    private void ExitWarden()
    {
        Debug.Log("ExitWarden()");
        if (wardenActiveRoutine != null)
        {
            Debug.Log("wardenActiveRoutine was not null when calling ExitWarden(), but now the coroutine is null");
            StopCoroutine(wardenActiveRoutine);
            wardenActiveRoutine = null;
        }

        instantiatedWarden.SetActive(false);
        wardenActive = false;

        OnWardenActive?.Invoke(false);
    }

    private void HandleCollectableCollected()
    {
        runestonesCollected++;

        if (runestonesCollected % spawnWardenEveryXRunestonesCollected == 0)
        {
            EnterWarden();
        }
    }
}
