using UnityEngine;

public class HMInstructionDontSprint : HedgeMazeHMInstruction
{
    private PlayerMovement playerMovement;

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

        if (playerMovement != null && playerMovement.isSprinting)
        {
            InvokeOnPlayerFailed();
        }
    }
}
