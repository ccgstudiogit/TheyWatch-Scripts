using UnityEngine;

public class PlaygroundLevelController : LevelController
{
    [Header("Opening Message")]
    [SerializeField] private bool sendMessages = false;
    [SerializeField] private string openingMessage = "Use this device as a radar and to check for messages";
    [SerializeField, Min(0)] private float openingMessageDelay = 4.5f;

    protected override void Start()
    {
        base.Start();

        if (sendMessages)
        {
            this.Invoke(() => SendMessageToWristDevice(openingMessage), openingMessageDelay);
        }
    }
}
