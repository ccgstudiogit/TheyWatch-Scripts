using UnityEngine;

public abstract class PlayerInput : MonoBehaviour
{
    protected PlayerActions playerActions;

    protected virtual void Awake()
    {
        if (InputManager.instance != null)
        {
            playerActions = InputManager.instance.Actions;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("InputManager.instance is null in Awake().");
#endif
        }
    }

    protected virtual void Start()
    {
        // Fallback if InputManager.instance was null in Awake()
        if (playerActions == null && InputManager.instance != null)
        {
            playerActions = InputManager.instance.Actions;
        }
    }


    protected abstract void OnEnable();
    protected abstract void OnDisable();

    /// <summary>
    ///     Check whether InputManager's inputsEnabled is true or false.
    /// </summary>
    /// <returns>True if InputManager.instance.inputsEnabled is true, false if otherwise.</returns>
    protected virtual bool AreInputsEnabled()
    {
        return InputManager.instance.inputsEnabled;
    }
}
