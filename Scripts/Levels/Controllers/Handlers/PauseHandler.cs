using System;
using System.Collections;
using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    public static event Action OnGamePause;
    public static event Action OnGameResume;

    private bool currentlyPaused = false;

    // A slight delay from pausing and re-pausing to ensure no mixups occur with enabling/disabling action maps
    private bool pauseInputBlocked;
    private float timeToBlock = 0.2f;

    private void Awake()
    {
        pauseInputBlocked = false;
    }

    /// <summary>
    ///     Handles pause funtionality whenever the player presses the pause input.
    /// </summary>
    public void HandleOnPausePressed()
    {
        (currentlyPaused ? (Action)Resume : Pause)();
    }

    /// <summary>
    ///     Check if the game is paused.
    /// </summary>
    /// <returns>True if the game is paused, false if not.</returns>
    public bool IsPaused()
    {
        return currentlyPaused;
    }

    /// <summary>
    ///     Pause the game by setting Time.timeScale to 0. Also fires off an OnGamePause event.
    /// </summary>
    public void Pause()
    {
        if (pauseInputBlocked)
        {
            return;
        }

        InputManager.instance.EnableBaseMap(false);
        InputManager.instance.EnableUIMap(true);

        currentlyPaused = true;
        pauseInputBlocked = true;

        Time.timeScale = 0;
        OnGamePause?.Invoke();

        this.Invoke(() => pauseInputBlocked = false, timeToBlock);
    }

    /// <summary>
    ///     Resume the game by setting Time.timeScale to 1. Also fires off an OnGameResume event.
    /// </summary>
    public void Resume()
    {
        currentlyPaused = false;

        InputManager.instance.EnableBaseMap(true);
        InputManager.instance.EnableUIMap(false);

        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }

        OnGameResume?.Invoke();
    }
}
