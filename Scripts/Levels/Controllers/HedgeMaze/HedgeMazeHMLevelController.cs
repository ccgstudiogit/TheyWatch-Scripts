using System;
using System.Collections;
using UnityEngine;

public class HedgeMazeHMLevelController : LevelController, IHMLevelController
{
    // Lets ShadeHMHandler know that the player failed an instruction and that Shade should start the global chase. The
    // float is Shade's speed whilst globally chasing the player
    public static event Action<float> OnPlayerFailedInstruction;

    // These can be used to change Shade's speed while an instruction is active (such as lowering Shade's speed) to encourage
    // the player to follow the instructions by being safe if correctly following them
    public static event Action<float> OnInstructionStarted;
    public static event Action OnInstructionFinished;

    [Header("Opening Message")]
    [SerializeField] private string openingMessage = "Do as I say";
    [SerializeField] private float openingMessageDelay = 2.65f;
    [SerializeField] private float openingMessageDuration = 5.35f;

    [Header("Instructions")]
    [SerializeField] private HedgeMazeHMInstruction[] instructions;

    [Header("Instructions Settings")]
    [SerializeField] private bool shuffleOrder = true;

    [Tooltip("A random instruction is chosen every x seconds between this range")]
    [SerializeField] private Vector2 instructionEveryXSecondsRange = new Vector2(30f, 35f);

    [Tooltip("The first instruction will be sent after this many seconds on startup. After the initial instruction " +
        "is sent, the instructions will be sent every via instructionEveryXSecondsRange")]
    [SerializeField, Min(0)] private float firstInstructionDelay = 11.25f;

    [Tooltip("The time the player has to prepare for instructions (the player can see the instruction but not " +
        "following the instruction during prepTime does result in any negative consequences")]
    [SerializeField, Min(0)] private float prepTime = 3.25f;

    [Header("Failed Instruction Settings")]
    [Tooltip("When the player fails an instruction, Shade's movement speed will be set to this amount until the player scares " +
        "Shade away with the flashlight or Shade enters berserk")]
    [SerializeField, Min(0)] private float shadeSpeed = 16f;

    [Tooltip("If the player fails the instruction, the instruction's message will be replaced with this text instead")]
    [SerializeField] private string failedMessage = "Failed";
    [Tooltip("The text color of the failed message")]
    [SerializeField] private Color failedMessageColor = Color.red;

    // Shade references are used to determine if Shade is currently berserk or not. While Shade is berserk, no instructions should
    // be sent to the player's wrist device
    private Shade shade;
    private ShadeBerserkHandler shadeBerserkHandler;

    private bool keepSendingInstructions; // While true, this script will keep sending instructions to the player

    protected override void Start()
    {
        base.Start();

        // Send the opening message after a delay, assuming device tips are enabled
        if (SettingsManager.instance.AreDeviceTipsEnabled())
        {
            this.Invoke(() => SendMessageToWristDevice(openingMessage, openingMessageDuration), openingMessageDelay);
        }

#if UNITY_EDITOR
        // This is here to make testing a lot easier as I can have Shade already in the scene and still get a reference without
        // relying on MonsterSpawnController
        if (shadeBerserkHandler == null)
        {
            GetShadeReference();
        }
#endif
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SpawnPlayerHandler.OnPlayerLoaded += BeginInstructions;

        HedgeMazeHMInstruction.OnPlayerFailed += HandlePlayerFailedInstruction;
        HedgeMazeHMInstruction.OnInstructionCompleted += HandlePlayerCompletedInstruction;

        MonsterSpawnController.OnMonsterSpawnsComplete += GetShadeReference;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SpawnPlayerHandler.OnPlayerLoaded -= BeginInstructions;

        HedgeMazeHMInstruction.OnPlayerFailed -= HandlePlayerFailedInstruction;
        HedgeMazeHMInstruction.OnInstructionCompleted -= HandlePlayerCompletedInstruction;

        MonsterSpawnController.OnMonsterSpawnsComplete -= GetShadeReference;
    }

    private void HandlePlayerFailedInstruction()
    {
        OnInstructionFinished?.Invoke();

        // Let ShadeHMHandler.cs know the player failed the instruction
        OnPlayerFailedInstruction?.Invoke(shadeSpeed);

        // Update the message on the wrist device
        ChangeWristDeviceMessage(failedMessage);
        ChangeWristDeviceMessageColor(failedMessageColor);
    }

    private void HandlePlayerCompletedInstruction()
    {
        OnInstructionFinished?.Invoke();
    }

    /// <summary>
    ///     Begins the process of sending instructions to the player.
    /// </summary>
    private void BeginInstructions(GameObject player)
    {
        keepSendingInstructions = true;
        StartCoroutine(InstructionRoutine());
    }

    /// <summary>
    ///     Handles sending instructions to the player periodically.
    /// </summary>
    private IEnumerator InstructionRoutine()
    {
        int instructionIndex = 0;
        float timeElapsed = 0f;
        float waitTime;

        if (shuffleOrder && instructions.Length > 1)
        {
            System.Random rng = new System.Random();
            rng.ShuffleArray(instructions); // From HelperMethods.ShuffleArray()
        }

        // Send the first instruction after an initial delay
        yield return new WaitForSeconds(firstInstructionDelay);
        SendInstruction(instructions[0]);

        waitTime = UnityEngine.Random.Range(instructionEveryXSecondsRange.x, instructionEveryXSecondsRange.y);

        while (keepSendingInstructions)
        {
            if (timeElapsed > waitTime && ShouldSendInstruction())
            {
                // Get a new random wait time between the range of instructionEveryXSecondsRange
                waitTime = UnityEngine.Random.Range(instructionEveryXSecondsRange.x, instructionEveryXSecondsRange.y);

                timeElapsed = 0f;
                instructionIndex++; // This is increased first due to the first instruction being sent before the while loop

                SendInstruction(instructions[instructionIndex % instructions.Length]);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    ///     Sends an instruction to the player's wrist device and the instruction's InstructionRoutine monitors if
    ///     the player is following the instruction or not. If the player fails the instruction, HandlePlayerFailedInstruction()
    ///     is called as it is subscribed to the instruction's event OnPlayerFailed.
    /// </summary>
    /// <param name="instruction">The instruction that should be sent and used.</param>
    private void SendInstruction(HedgeMazeHMInstruction instruction)
    {
        if (spawnPlayerHandler != null && spawnPlayerHandler.player != null)
        {
            OnInstructionStarted?.Invoke(instruction.shadeSpeedDuringInstruction);

            // Start the coroutine after the preptime
            this.Invoke(() => StartCoroutine(instruction.InstructionRoutine(spawnPlayerHandler.player)), prepTime);

            // The message sent to the player has the duration of the instruction's monitoring time plus the prep time
            SendMessageToWristDevice(instruction.message, instruction.duration + prepTime);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"{gameObject.name} attempted to send an instruction but spawnPlayerHandler || spawnPlayerHandler.player was null.");
        }
#endif
    }

    /// <summary>
    ///     Makes sure that an instruction should be sent to the player. For example, while Shade is berserk and chasing
    ///     the player, an instruction should not be sent.
    /// </summary>
    /// <returns>True if an instruction should be sent, false if otherwise.</returns>
    private bool ShouldSendInstruction()
    {
        // Make sure Shade is not berserk
        if (shadeBerserkHandler != null && shadeBerserkHandler.currentlyBerserk)
        {
            return false;
        }
        // Fallback in-case the previous attempt at getting the reference failed
        else if (shadeBerserkHandler == null)
        {
            GetShadeReference();
        }

        // Make sure none of the instructions are active before sending another one
        for (int i = 0; i < instructions.Length; i++)
        {
            if (instructions[i].active)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Gets a reference to Shade and get the ShadeBerserkHandler component.
    /// </summary>
    private void GetShadeReference()
    {
        shade = FindFirstObjectByType<Shade>();

        if (shade != null)
        {
            shadeBerserkHandler = shade.GetComponent<ShadeBerserkHandler>();
        }
    }
}
