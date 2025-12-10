using UnityEngine;

public class ShadeHMHandler : MonoBehaviour
{
    private Shade shade;
    private ShadeBerserkHandler shadeBerserkHandler;

    private void Awake()
    {
        shade = GetComponent<Shade>();
        shadeBerserkHandler = GetComponent<ShadeBerserkHandler>();

        if (shade == null || shadeBerserkHandler == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{name} attempted to get a reference to Shade/ShadeBerserkHandler but one of them was null!");
#endif
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        HedgeMazeHMLevelController.OnInstructionStarted += SetShadeSpeed;
        HedgeMazeHMLevelController.OnPlayerFailedInstruction += HandlePlayerFailedInstruction;
        HedgeMazeHMLevelController.OnInstructionFinished += ResetShadeSpeed;
    }

    private void OnDisable()
    {
        HedgeMazeHMLevelController.OnInstructionStarted -= SetShadeSpeed;
        HedgeMazeHMLevelController.OnPlayerFailedInstruction -= HandlePlayerFailedInstruction;
        HedgeMazeHMLevelController.OnInstructionFinished -= ResetShadeSpeed;
    }

    /// <summary>
    ///     When the player fails an instruction, Shade.BeginGlobalChase() is called and chaseSpeed is used as the chase
    ///     state's speed.
    /// </summary>
    private void HandlePlayerFailedInstruction(float chaseSpeed)
    {
        // Only begin the global chase if Shade is not berserk
        if (!shadeBerserkHandler.currentlyBerserk)
        {
            shade.BeginGlobalChase(chaseSpeed);
        }
    }

    /// <summary>
    ///     Set Shade's agent movement speed to a new speed.
    /// </summary>
    /// <param name="newSpeed">The new speed to be used.</param>
    private void SetShadeSpeed(float newSpeed)
    {
        // Only change the movement speed if Shade is not currently chasing the player nor retreating
        if (!shade.IsChasing() && !shade.IsRetreating())
        {
            shade.SetMovementSpeed(newSpeed);
        }
    }

    /// <summary>
    ///     Reset Shade's agent movement speed back to its default starting speed.
    /// </summary>
    private void ResetShadeSpeed()
    {
        // Only reset the movement speed if Shade is not globally chasing the player and Shade is not berserk
        if (!shade.isGloballyChasing && !shadeBerserkHandler.currentlyBerserk)
        {
            shade.ResetMovementSpeed();
        }
    }
}
