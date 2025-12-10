using System.Collections;
using UnityEngine;

public class HedgeMazeLevelController : LevelController
{
    [Header("Opening Messages")]
    [SerializeField] private string openingMessage = "Hello :)";
    [SerializeField] private float openingMessageDelay = 3f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " + 
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time
    
    [SerializeField] private string followupOpeningMessage = "Find the runestones and open the portal";
    [SerializeField] private float followupMessageDelay = 11f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " + 
        "the wrist device's script")]
    [SerializeField] private float followupMessageDuration = -1f;

    [Header("Inventory Full Message")]
    [SerializeField] private string inventoryFullMessage = "Bring those runestones back to the portal";
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " + 
        "the wrist device's script")]
    [SerializeField] private float inventoryFullMessageDuration = -1f;

    protected override void Start()
    {
        base.Start();

        if (SettingsManager.instance.AreDeviceTipsEnabled())
        {
            StartCoroutine(OpeningMessagesRoutine());
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InventoryUIController.OnFirstTimeInventoryFull += SendInventoryFullMessage;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InventoryUIController.OnFirstTimeInventoryFull -= SendInventoryFullMessage;
    }

    /// <summary>
    ///     Handles sending opening generic messages to the player's device.
    /// </summary>
    private IEnumerator OpeningMessagesRoutine()
    {
        yield return new WaitForSeconds(openingMessageDelay);
        SendMessageToWristDevice(openingMessage, openingMessageDuration);
        yield return new WaitForSeconds(followupMessageDelay);
        SendMessageToWristDevice(followupOpeningMessage, followupMessageDuration);
    }

    /// <summary>
    ///     Handles sending a message to the player to let them know their inventory is full and they should head back to
    ///     the portal. Since this is a generic tip it can be turned off in gameplay settings.
    /// </summary>
    private void SendInventoryFullMessage()
    {
        if (!SettingsManager.instance.AreDeviceTipsEnabled())
        {
            return;
        }

        SendMessageToWristDevice(inventoryFullMessage, inventoryFullMessageDuration);
    }
}
