using UnityEngine;

public class HMInstructionStayCrouched : HedgeMazeHMInstruction
{
    [Header("Stay Crouched Settings")]
    [Tooltip("The maximum grace time the player is allowed to not crouch while this instruction is active")]
    [SerializeField] private float maxTimeNotCrouching = 0.35f;
    private float elapsedTimeNotCrouching;

    private PlayerMovement playerMovement;

    protected override void ResetValues()
    {
        elapsedTimeNotCrouching = 0;
    }

    protected override void MonitorPlayer(GameObject player)
    {
        if (player == null)
        {
#if UNITY_EDITOR
            LogPlayerNullError();
#endif
            return;
        }

        if (playerMovement == null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        if (!playerMovement.isCrouching)
        {
            elapsedTimeNotCrouching += Time.deltaTime;

            // Only trigger a fail if the max time spent not crouching is reached
            if (elapsedTimeNotCrouching > maxTimeNotCrouching)
            {
                InvokeOnPlayerFailed();
            }
        }
        else if (elapsedTimeNotCrouching > 0)
        {
            // Reset the timer if the player begins crouching quickly enough again
            elapsedTimeNotCrouching = 0;
        }
    }
}
