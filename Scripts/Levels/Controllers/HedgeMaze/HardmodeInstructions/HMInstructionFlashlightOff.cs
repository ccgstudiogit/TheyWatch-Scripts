using UnityEngine;

public class HMInstructionFlashlightOff : HedgeMazeHMInstruction
{
    private Flashlight flashlight;

    private void OnEnable()
    {
        SpawnPlayerHandler.OnPlayerLoaded += GetFlashlightReference;
    }

    private void OnDisable()
    {
        SpawnPlayerHandler.OnPlayerLoaded -= GetFlashlightReference;
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

        if (flashlight == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s flashlight reference null!");
#endif
            return;
        }

        if (flashlight.IsOn())
        {
            InvokeOnPlayerFailed();
        }
    }

    // Only attempt to get a reference to the flashlight once the player is fully loaded in
    private void GetFlashlightReference(GameObject player)
    {
        flashlight = FindFirstObjectByType<Flashlight>();
    }
}
