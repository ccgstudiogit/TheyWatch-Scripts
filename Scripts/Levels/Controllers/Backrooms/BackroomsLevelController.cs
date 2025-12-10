using System.Collections;
using UnityEngine;

public class BackroomsLevelController : LevelController
{
    [Header("Opening Messages")]
    [SerializeField] private string openingMessage = "It doesn't like flashing lights";
    [SerializeField] private float openingMessageDelay = 2.85f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " +
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time

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
    }
}
