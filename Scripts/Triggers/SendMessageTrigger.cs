using UnityEngine;

public class SendMessageTrigger : TriggerCollider
{
    [Header("Message Settings")]
    [SerializeField] private string message;
    [Tooltip("If enabled, this message is considered to be a generic tip message. What this means is that if the player " + 
        "has turned off generic tip messages in gameplay settings, this message will not appear. Otherwise, this message " + 
        "will show up as normal")]
    [SerializeField] private bool isGenericTip;

    protected override void OnObjectEntered()
    {
        // Do not send the message if it is a generic tip and the player turned off generic tips in gameplay settings
        if (isGenericTip && !SettingsManager.instance.AreDeviceTipsEnabled())
        {
            return;
        }
        
        if (LevelController.instance != null)
        {
            LevelController.instance.SendMessageToWristDevice(message);
        }
    }
}
