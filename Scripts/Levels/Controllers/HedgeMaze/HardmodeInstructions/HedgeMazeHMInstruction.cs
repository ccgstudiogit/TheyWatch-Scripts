using System;
using System.Collections;
using UnityEngine;

public abstract class HedgeMazeHMInstruction : MonoBehaviour
{
    // Lets HedgeMazeHMLevelController know if the player failed an instruction
    public static event Action OnPlayerFailed;

    // Lets HedgeMazeHMLevelController know if the player succesfully completed an instruction
    public static event Action OnInstructionCompleted;

    // active is set to true once the instruction begins and set to false either when the instruction ends
    // naturally or the player fails the instruction
    public bool active { get; protected set; }

    [Header("Instruction Settings")]
    [Tooltip("The total duration of this instruction")]
    [field: SerializeField, Min(0)] public float duration { get; private set; } = 8f;

    [Tooltip("The message that will be sent to the player's device")]
    [field: SerializeField] public string message { get; private set; }

    [Header("Shade Speed Settings")]
    [Tooltip("Sets shade's speed while this instruction is active")]
    [field: SerializeField, Min(0)] public float shadeSpeedDuringInstruction = 6f;

    protected abstract void MonitorPlayer(GameObject player);
    protected virtual void ResetValues() { }

    /// <summary>
    ///     Handles monitoring the player every frame.
    /// </summary>
    public IEnumerator InstructionRoutine(GameObject player)
    {
        active = true;
        float timeElapsed = 0f;

        ResetValues();

        while (active && timeElapsed < duration)
        {
            MonitorPlayer(player);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        active = false;
        InvokeOnInstructionCompleted();
    }

    /// <summary>
    ///     Override the length set in the inspector.
    /// </summary>
    /// <param name="newDuration">The new length of the instruction.</param>
    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    /// <summary>
    ///     Fires off the OnPlayerFailed event.
    /// </summary>
    protected void InvokeOnPlayerFailed()
    {
        if (active)
        {
            active = false;
            OnPlayerFailed?.Invoke();
        }
    }

    /// <summary>
    ///     Fires off the OnInstructionCompleted event.
    /// </summary>
    protected void InvokeOnInstructionCompleted()
    {
        OnInstructionCompleted?.Invoke();
    }

#if UNITY_EDITOR
    protected void LogPlayerNullError()
    {
        Debug.LogError($"{gameObject.name} does not have a reference to the player!");
    }
#endif
}
