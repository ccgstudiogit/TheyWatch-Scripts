using UnityEngine;

public class HMInstructionDontMove : HedgeMazeHMInstruction
{
    [Header("Don't Move Settings")]
    [Tooltip("If the player's move speed goes beyond this amount the player will have failed the instruction")]
    [SerializeField] private float playerMaxVelocity = 0.1f;

    private PlayerMovement playerMovement;

    /// <summary>
    ///     Handles checking if the player is meeting the instruction's demands.
    /// </summary>
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

        // Make sure the player's velocity does not exceed the maximum
        if (playerMovement != null && playerMovement.characterController.velocity.magnitude > playerMaxVelocity)
        {
            InvokeOnPlayerFailed();
        }
    }
}
