using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void OnEnable()
    {
        LockCursor();

        PauseHandler.OnGamePause += UnlockCursor;
        PauseHandler.OnGameResume += LockCursor;
    }

    private void OnDisable()
    {
        UnlockCursor(); // Makes sure the cursor is unlocked if CursorManager is disabled or destroyed

        PauseHandler.OnGamePause -= UnlockCursor;
        PauseHandler.OnGameResume -= LockCursor;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursor()
    {
        // Hides cursor during gameplay, ESC/Pause can be pressed to reveal it
        Cursor.lockState = CursorLockMode.Locked;
    }
}
