using System.Collections;
using UnityEngine;

public class HedgeMazeHMEndLevelController : LevelController
{
    [Header("Opening Messages")]
    [SerializeField] private string openingMessage = "You may rest now";
    [SerializeField] private float openingMessageDelay = 4f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " +
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time

    [SerializeField] private string followupOpeningMessage = "Stay here as long as you like";
    [SerializeField] private float followupMessageDelay = 11.25f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " + 
        "the wrist device's script")]
    [SerializeField] private float followupMessageDuration = -1f;

    protected override void Start()
    {
        base.Start();

        if (SettingsManager.instance.AreDeviceTipsEnabled())
        {
            StartCoroutine(OpeningMessagesRoutine());
        }
    }

    private IEnumerator OpeningMessagesRoutine()
    {
        yield return new WaitForSeconds(openingMessageDelay);
        SendMessageToWristDevice(openingMessage, openingMessageDuration);
        yield return new WaitForSeconds(followupMessageDelay);
        SendMessageToWristDevice(followupOpeningMessage, followupMessageDuration);
    }
}
