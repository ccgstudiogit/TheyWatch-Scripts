using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : Interactable
{
    // public static instance is currently only used for the wrist device to easily get a reference to Portal
    public static Portal instance { get; private set; }

    // ***CURRENTLY UNUSED***
    // I plan to use this to let monsters know when the portal is opened as once the portal is opened, I want
    // the player to be safe from losing. 
    public static event Action OnPortalOpened;

    // Currently used to send a message to PlayerInteractions, letting PlayerInteractions know when to update 
    // the hover message text (e.g. portal message text should update when player drops off runes)
    public static event Action OnMessageUpdate;

    private int collectablesNeededToOpenPortal;
    private int defaultAmount = 5; // Only used if levelController reference is null

    [Header("VFX")]
    [SerializeField] private GameObject portalVFXObject;
    [SerializeField] private bool disableVFXOnStart = true;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource humAudioSource;
    [Tooltip("This sound effect is also played whenever a runestone is placed in the portal")]
    [SerializeField] private SoundEffectSO portalOpenSFX; // The portalOpenSFX uses humAudioSource as its source
    [SerializeField] private SoundEffectSO runestonePlacedInPortalSFX; // Also uses humAudioSource as its source

    [Header("Runestones")]
    [Tooltip("When runestones are added to the portal, they will be added to these locations")]
    [SerializeField] private PortalCollectableLocation[] runestoneLocations;

    [Tooltip("This runestone will be put in locations that won't be necessary to open the portal (determined by LevelController)")]
    [SerializeField] private CollectableDataSO blankRunestone;

    [Tooltip("The distance at which the runestones will spawn from an unoccupied portal location")]
    [SerializeField] private float animationDistance = 0.5f;
    [Tooltip("The time it takes for the runestone to move from its spawn position to the location's position within the portal")]
    [SerializeField] private float animationTime = 1f;
    [Tooltip("The delay in which the animation will begin after the runestone is instantiated")]
    [SerializeField] private float animationDelay = 0.25f;

    // TODO: Remove once finished playing around with portal
    [Header("- Testing Purposes Only -")]
    [Tooltip("Using this override will make the collectable required amount be the test count and not the " +
        "actual amount of collectables that were spawned in")]
    [SerializeField] private bool overrideCollectablesNeeded;
    [SerializeField] private int collectablesNeededTestCount = 8;

    // This is serialized for debug purposes
    [SerializeField] private List<CollectableDataSO> collectablesInPortal = new List<CollectableDataSO>();

    private bool isOpened;

    protected override void Awake()
    {
        base.Awake();

        isOpened = false;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"More than 1 instances of Portal detected. Destroying {gameObject.name}.");
#endif
            Destroy(gameObject);
        }

        // Make sure the hum audio source isn't accidentally turned on
        if (humAudioSource != null && humAudioSource.isPlaying)
        {
            humAudioSource.Stop();
        }
    }

    private void Start()
    {
        if (portalVFXObject == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Portal's portalVFXObject reference null. Unable to play portal VFX.");
#endif
        }
        else if (disableVFXOnStart)
        {
            portalVFXObject.SetActive(false);
        }

        if (overrideCollectablesNeeded)
        {
#if UNITY_EDITOR
            Debug.Log("*Overriding collectablesNeededToOpenPortal to collectablesNeededTestCount*");
#endif
            collectablesNeededToOpenPortal = collectablesNeededTestCount;

            // This is here mainly for testing purposes
            if (collectablesNeededToOpenPortal < 1)
            {
                OpenPortal();
            }
        }
        else if (LevelController.instance != null)
        {
            collectablesNeededToOpenPortal = LevelController.instance.GetLevelData().collectableCount;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"LevelController.instance is null. Portal defaulting to {defaultAmount} collectables required.");
#endif
            collectablesNeededToOpenPortal = defaultAmount;
        }

        AddBlankRunestones();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    ///     Interact with the portal.
    /// </summary>
    public override void Interact()
    {
        OnMessageUpdate?.Invoke();
    }

    /// <summary>
    ///     Adds collectables to the portal's list collectablesInPortal, then checks if the necessary requirements
    ///     have been met in order to open the portal. A player reference is used for adding the collectables to the
    ///     portal and trying to add them on the same side of the portal as the player if able.
    /// </summary>
    public void AddCollectables(CollectableDataSO[] collectables, GameObject player)
    {
        for (int i = 0; i < collectables.Length; i++)
        {
            collectablesInPortal.Add(collectables[i]);
            AddCollectableToALocation(collectables[i], player, runestonePlacedInPortalSFX, humAudioSource);
        }

        if (portalOpenSFX != null && humAudioSource != null)
        {
            portalOpenSFX.PlayOneShot(humAudioSource);
        }

        // Check if the requirements have been met in order to open the portal
        CheckIfRequirementsMet();
    }

    /// <summary>
    ///     Add a collectable to a location.
    /// </summary>
    /// <param name="collectable">The collectable that should be added.</param>
    /// <param name="player">This reference is used to attempt to add the collectable to the same side of the portal as the player.</param>
    private void AddCollectableToALocation(CollectableDataSO collectable, GameObject player, SoundEffectSO sfx, AudioSource audioSource)
    {
        // Attempt to add the collectable to a location on the same relative side to the portal as the player is
        for (int i = 0; i < runestoneLocations.Length; i++)
        {
            if (!runestoneLocations[i].occupied && LocationOnSameSideAsPlayer(runestoneLocations[i].transform, player.transform))
            {
                runestoneLocations[i].PlaceCollectable(collectable, animationTime, animationDistance, animationDelay, sfx, audioSource);
                return;
            }
        }

        // If the above attempt did not work (all locations on the same side of the player are full), just add it to the other side
        for (int i = 0; i < runestoneLocations.Length; i++)
        {
            if (!runestoneLocations[i].occupied)
            {
                runestoneLocations[i].PlaceCollectable(collectable, animationTime, animationDistance, animationDelay, sfx, audioSource);
                return;
            }
        }

#if UNITY_EDITOR
        Debug.LogWarning("Attempted to add a collectable to a location but all locations are occupied!");
#endif
    }

    /// <summary>
    ///     Check if a runestone location is on the same relative side of the portal as the player.
    /// </summary>
    /// <returns>True if the location is on the same side of the portal as the player, false if otherwise.</returns>
    private bool LocationOnSameSideAsPlayer(Transform locationTransform, Transform playerTransform)
    {
        return Vector3.Dot(locationTransform.TransformDirection(Vector3.forward), Vector3.Normalize(locationTransform.position - playerTransform.position)) < 0;
    }

    /// <summary>
    ///     Checks how many runestones are required to open the portal. Based on the requirement, a number of blank runestones will be
    ///     randomly placed in runestone locations. For example, if 10 runestones are needed to open the portal and since there are 12 
    ///     available locations, 2 blank runestones will take up unnecessary locations.
    /// </summary>
    private void AddBlankRunestones()
    {
        if (collectablesNeededToOpenPortal < runestoneLocations.Length)
        {
            int blankRunestonesNeeded = runestoneLocations.Length - collectablesNeededToOpenPortal;

            for (int i = 0; i < blankRunestonesNeeded; i++)
            {
                int failSafeCounter = 65; // Prevents an infinite loop by limiting iterations to this amount
                PortalCollectableLocation randomLocation;

                do
                {
                    // Spawn blank runestones back and forth from one side of the portal to the other to get an even distribution across both
                    // sides of the portal. Note: This assumes the runestonesLocation array will have the first half be one side of the portal
                    // and the other half be the other side.
                    randomLocation = HelperMethods.IsEven(i) ?
                        runestoneLocations[UnityEngine.Random.Range(0, runestoneLocations.Length / 2)] :
                        runestoneLocations[UnityEngine.Random.Range(runestoneLocations.Length / 2, runestoneLocations.Length)];

                } while (randomLocation.occupied && --failSafeCounter > 0);

                // If the failsafe counter triggers, just loop through all of the locations and get a suitable location
                if (failSafeCounter <= 1)
                {
                    for (int j = 0; j < runestoneLocations.Length; j++)
                    {
                        if (!runestoneLocations[j].occupied)
                        {
                            randomLocation = runestoneLocations[j];
                            break;
                        }
                    }
                }

                randomLocation.PlaceCollectableNoAnimation(blankRunestone);
            }
        }
    }

    /// <summary>
    ///     Check if enough collectables were added to the portal in order to open the portal.
    /// </summary>
    private void CheckIfRequirementsMet()
    {
        if (collectablesInPortal.Count >= collectablesNeededToOpenPortal)
        {
            OpenPortal();
        }
    }

    private void OpenPortal()
    {
        isOpened = true;

        // Force a hover message update when the portal has all of the needed collectables
        OnMessageUpdate?.Invoke();

        OnPortalOpened?.Invoke();

        if (humAudioSource != null)
        {
            // Play the hum audio source
            humAudioSource.Play();
        }

        if (portalVFXObject != null && !portalVFXObject.activeSelf)
        {
            portalVFXObject.SetActive(true);
        }
    }

    /// <summary>
    ///     Check if the portal is opened.
    /// </summary>
    /// <returns>True if opened, false if not.</returns>
    public bool IsOpened()
    {
        return isOpened;
    }

    /// <summary>
    ///     Get the special interaction message that should appear when looking to interact with the portal.
    /// </summary>
    /// <param name="playerInventory">A reference to the player inventory.</param>
    /// <param name="interactKeybinding">The interaction keybinding in order to interact with the portal.</param>
    /// <returns>A string with a specified message based upon the player's inventory, portal needs, etc.</returns>
    public string GetInteractionMessage(PlayerInventory playerInventory, string interactKeybinding)
    {
        if (isOpened)
        {
            return $"[{interactKeybinding}]: Escape";
        }
        else if (playerInventory.collectablesInInventoryCount > 0)
        {
            return $"[{interactKeybinding}]: Place {playerInventory.collectablesInInventoryCount} runestones";
        }
        else if (GetAmountOfCollectablesNeeded() > 0)
        {
            return $"Portal needs {GetAmountOfCollectablesNeeded()} more runestones";
        }
        else
        {
            // Fallback is used to display no message (for example while portal is in the process of opening)
            return " ";
        }
    }

    /// <summary>
    ///     Enter the portal and escape from the level by calling this method. This method calls LevelController.instance.EnterLevelCompleteScene().
    /// </summary>
    public void Escape()
    {
        if (LevelController.instance != null)
        {
            LevelController.instance.EnterLevelCompleteScene();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Portal.cs attempted to escape but LevelController.instance was null. Player unable to leave.");
#endif
        }
    }

    private int GetAmountOfCollectablesNeeded()
    {
        int amountNeeded = collectablesNeededToOpenPortal - collectablesInPortal.Count;

        if (amountNeeded < 1)
        {
            amountNeeded = 0;
        }

        return amountNeeded;
    }
}
