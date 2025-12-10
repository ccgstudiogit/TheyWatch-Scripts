using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuController : MonoBehaviour
{
    // Let LevelController know when the player exits the pause menu
    public static event Action OnExitPauseMenu;

    private CanvasGroup canvasGroup;
    [SerializeField] private bool hidePauseMenuOnStart = true;

    private bool active = true;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (hidePauseMenuOnStart)
        {
            canvasGroup.alpha = 0f;
            active = false;
        }
    }

    private void OnEnable()
    {
        PauseHandler.OnGamePause += ShowMenu;
    }

    private void OnDisable()
    {
        PauseHandler.OnGamePause -= ShowMenu;
    }

    public void ShowMenu()
    {
        if (active)
        {
            return;
        }

        active = true;
        canvasGroup.alpha = 1f;
    }

    // Should be called through a unity event by a resume game button
    public void HideMenu()
    {
        // Checking if level controller's instance is null makes it easier to debug/test
        if (!active || LevelController.instance == null)
        {
            return;
        }

        active = false;
        OnExitPauseMenu?.Invoke();
        canvasGroup.alpha = 0f;
    }
}
