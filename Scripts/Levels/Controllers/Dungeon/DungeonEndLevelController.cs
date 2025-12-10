using System.Collections;
using UnityEngine;

public class DungeonEndLevelController : LevelController
{
    [Header("Warden")]
    [SerializeField] private GameObject wardenEndPrefab;
    private GameObject instantiatedWarden;
    private WardenEndScene wardenEndScene;

    [Header("Warden Spawn")]
    [SerializeField] private GameObject wardenSpawn;

    [Header("Warden Move To Position")]
    [SerializeField] private Transform wardenMoveToPosition;
    [SerializeField] private float moveToPosThreshold = 0.5f;

    [Header("Cell Door")]
    [SerializeField] private GameObject cellDoor;
    [Tooltip("The cell door will close after X amount of seconds once the Warden reaches his final position")]
    [SerializeField] private float closeCellDoorAfterXSeconds = 3.15f;
    [Tooltip("The time in seconds it will take for the cell door to close")]
    [SerializeField] private float cellDoorCloseTime = 1f;
    [Tooltip("The end rotation of the door. Once the door reaches this rotation, it will be considered close and this scene " +
        "will end")]
    [SerializeField] private Quaternion cellDoorClosedRotation;
    [SerializeField] private SoundEffectSO cellDoorCloseSFX;
    [Tooltip("The collider will be turned on once the player reaches the cell, preventing the player from escaping once inside")]
    [SerializeField] private GameObject cellBlockCollider;

    [Header("End Scene Settings")]
    [Tooltip("The sound effect that will play once the scene ends, with either the player getting locked in the cell or " +
        "Warden reaching the player")]
    [SerializeField] private SoundEffectSO endSFX;
    [Tooltip("The time it takes for the black cover screen to fade in and hide everything")]
    [SerializeField] private float coverScreenPanelFadeTime = 0.15f;
    [SerializeField] private float loadBackToMainMenuDelay = 0.65f;

    private Coroutine finalPhaseRoutine = null;

    protected override void Start()
    {
        base.Start();

        if (cellBlockCollider != null)
        {
            cellBlockCollider.SetActive(false);
        }
    }

    /// <summary>
    ///     Spawn the Warden at the wardenSpawn location.
    /// </summary>
    public void SpawnWarden()
    {
        instantiatedWarden = Instantiate(wardenEndPrefab, wardenSpawn.transform.position, Quaternion.identity);

        if (instantiatedWarden != null)
        {
            wardenEndScene = instantiatedWarden.GetComponent<WardenEndScene>();
        }
    }

    /// <summary>
    ///     Have Warden exit chase state and begin the final phase of dungeon end scene (where the player gets locked in the cell)
    /// </summary>
    public void StopChaseAndBeginFinalPhase()
    {
        if (wardenEndScene == null && instantiatedWarden != null)
        {
            wardenEndScene = instantiatedWarden.GetComponentInParent<WardenEndScene>();
        }

        // Turn on the cell block collider to prevent the player from leaving once player has entered the cell
        if (cellBlockCollider != null)
        {
            cellBlockCollider.SetActive(true);
        }

        wardenEndScene.StopChaseAndMoveToPos(wardenMoveToPosition.position);
        finalPhaseRoutine = StartCoroutine(FinalPhase());
    }

    /// <summary>
    ///     Handles the necessary steps for the closing of the cell door.
    /// </summary>
    private IEnumerator FinalPhase()
    {
        float wardenDistance;

        // Wait until the Warden is (mostly) finished moving to the position before closing the cell door
        do
        {
            wardenDistance = (wardenMoveToPosition.position - instantiatedWarden.transform.position).magnitude;
            yield return null;

        } while (wardenDistance > moveToPosThreshold);

        // Wait a few moments before closing the cell door
        yield return new WaitForSeconds(closeCellDoorAfterXSeconds);

        if (cellDoorCloseSFX != null)
        {
            cellDoorCloseSFX.Play();
        }   

        // Lerp the rotation of the door until it's fully closed
        float lerp = 0f;
        Quaternion cellDoorStartingRotation = cellDoor.transform.rotation;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / cellDoorCloseTime);
            cellDoor.transform.rotation = Quaternion.Slerp(cellDoorStartingRotation, cellDoorClosedRotation, lerp);

            yield return null;
        }

        finalPhaseRoutine = null;
        EndScene();
    }

    /// <summary>
    ///     End the scene by playing the endSFX, fading the screen to black, and sending the player back to the main menu.
    /// </summary>
    public void EndScene()
    {
        // Makes sure that if the player exits the cell and runs into the Warden that the coroutine does not interfere with EndScene()
        if (finalPhaseRoutine != null)
        {
            StopCoroutine(finalPhaseRoutine);
        }

        if (endSFX != null)
        {
            endSFX.Play();
        }

        // Disable player controls/input
        if (spawnPlayerHandler.player.TryGetComponent(out UniversalPlayerInput uPI))
        {
            uPI.enabled = false;
        }

        if (spawnPlayerHandler.player.TryGetComponent(out PlayerLook pL))
        {
            pL.enabled = false;
        }

        if (spawnPlayerHandler.player.TryGetComponent(out PlayerMovement pM))
        {
            pM.enabled = false;
        }

        SetScreenCoverPanelAlpha(1, coverScreenPanelFadeTime);
        SceneSwapManager.instance.LoadSceneWithFade(levelCompleteScene, loadBackToMainMenuDelay);
    }
}
