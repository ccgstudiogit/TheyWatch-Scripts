using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiminalLevelController : LevelController, ISleepPointLevelController
{
    [Header("Opening Messages")]
    [SerializeField] private string openingMessage = "They sleep. Don't wake them.";
    [SerializeField] private float openingMessageDelay = 3.5f;
    [Tooltip("If greater than 0, this message will have this duration. Otherwise, it will use the duration set by " +
        "the wrist device's script")]
    [SerializeField] private float openingMessageDuration = -1f; // -1 is used to not override the wrist device's default time

    [Header("Awake Somnids")]
    [SerializeField, Min(0)] private int somnidsStartingAsAwake = 1;
    [Tooltip("Make one Somnid become awake after this many have registered (recommended to keep as the total number of) " +
        "Somnids that will be spawned on the map")]
    [SerializeField, Min(1)] private int wakeUpSomnidAfterXRegistered = 4;

    public List<SleepPoint> sleepPoints { get; private set; } = new List<SleepPoint>();
    public List<Somnid> somnids { get; private set; } = new List<Somnid>();

    protected override void Start()
    {
        base.Start();

        if (SettingsManager.instance.AreDeviceTipsEnabled())
        {
            StartCoroutine(OpeningMessagesRoutine());
        }
    }

    /// <summary>
    ///     Handles sending opening generic messages to the player's device.
    /// </summary>
    private IEnumerator OpeningMessagesRoutine()
    {
        yield return new WaitForSeconds(openingMessageDelay);
        SendMessageToWristDevice(openingMessage, openingMessageDuration);
    }

    /// <summary>
    ///     Register a sleep point.
    /// </summary>
    public void RegisterSleepPoint(SleepPoint sleepPoint)
    {
        sleepPoints.Add(sleepPoint);
    }

    /// <summary>
    ///     Register a Somnid.
    /// </summary>
    /// <param name="somnid"></param>
    public void RegisterSomnid(Somnid somnid)
    {
        somnids.Add(somnid);

        if (ShouldWakeUpSomnids())
        {
            WakeUpRandomSomnids();
        }
    }

    /// <summary>
    ///     Check if a Somnid should be woken up.
    /// </summary>
    /// <returns>True if a Somnid should be woken up, false if otherwise.</returns>
    private bool ShouldWakeUpSomnids()
    {
        return somnidsStartingAsAwake > 0 && somnids.Count >= wakeUpSomnidAfterXRegistered;
    }

    /// <summary>
    ///     Wake up a random Somnid from the somnids list.
    /// </summary>
    private void WakeUpRandomSomnids(bool shuffleSomnids = true)
    {
        if (somnids.Count < 1)
        {
            return;
        }

        if (shuffleSomnids)
        {
            HelperMethods.ShuffleList(somnids);
        }

        for (int i = 0; i < somnidsStartingAsAwake; i++)
        {
            Somnid somnid = somnids[i];
            somnid.WakeUp(false); // false makes it so that the wake up sfx doesn't play
        }
    }
}
