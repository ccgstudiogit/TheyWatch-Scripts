using UnityEngine;

public class OffPortal : Interactable
{
    [Header("Focus Message")]
    [SerializeField] private string message = "Escape?";

    /// <summary>
    ///     Interact with the portal and enter the level complete scene set by LevelController.instance
    /// </summary>
    public override void Interact()
    {
        LevelController.instance.EnterLevelCompleteScene();
    }

    /// <summary>
    ///     Show the portal hover message.
    /// </summary>
    /// <param name="keybinding">The interact keybinding.</param>
    public override void OnFocus(string keybinding)
    {
        string m = $"[{keybinding}]: {message}";
        base.OnFocus(m);
    }
}
