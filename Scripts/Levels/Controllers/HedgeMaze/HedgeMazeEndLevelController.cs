using System.Collections;
using UnityEngine;

public class HedgeMazeEndLevelController : LevelController
{
    [Header("Shade")]
    [SerializeField] private GameObject shadeEndScenePrefab;
    private ShadeEndScene shade;

    [Tooltip("This controller will attempt to spawn shade at this location, but if this location is in view then the hidden " +
        "location will be used instead")]
    [SerializeField] private GameObject shadeSpawnOpen;
    [SerializeField] private GameObject shadeSpawnHidden;

    [Header("Shade Chase")]
    [SerializeField] private float chaseAfterXSeconds = 4.5f;
    private Coroutine countdownRoutine = null;

    [Header("Shade Reaches Player")]
    [Tooltip("The sound effect that will play once shade reaches the player")]
    [SerializeField] private SoundEffectSO caughtSFX;
    [Tooltip("The time it takes for the black cover screen to fade in and hide everything")]
    [SerializeField] private float coverScreenPanelFadeTime = 0.15f;
    [SerializeField] private float loadBackToMainMenuDelay = 0.65f;

    private Camera playerCamera;

    private bool spawnShadeStarted;

    protected override void Awake()
    {
        base.Awake();
        spawnShadeStarted = false;
    }

    /// <summary>
    ///     Handles spawning shade and starting the countdown until Shade lunges at the player.
    /// </summary>
    public void SpawnShade()
    {
        if (spawnShadeStarted)
        {
            return;
        }

        spawnShadeStarted = true;

        if (spawnPlayerHandler.player != null && spawnPlayerHandler.player.TryGetComponent(out PlayerReferences playerReferences))
        {
            playerCamera = playerReferences.playerCamera;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"{gameObject.name} had trouble getting the playerCamera reference. Unable to continue with SpawnShade().");
#endif
            return;
        }

        // Make sure the player is facing away from shadeSpawnOpen. If the player is facing towards shadeSpawnOpen, use
        // shadeSpawnHidden instead and have shade move towards shadeSpawnOpen
        GameObject instantiatedShade;
        if (HelperMethods.IsVisible(playerCamera, shadeSpawnOpen))
        {
            instantiatedShade = Instantiate(shadeEndScenePrefab, shadeSpawnHidden.transform.position, Quaternion.identity);
        }
        else
        {
            instantiatedShade = Instantiate(shadeEndScenePrefab, shadeSpawnOpen.transform.position, Quaternion.identity);
        }

        shade = instantiatedShade.GetComponent<ShadeEndScene>();
        shade.OnShadeCollidedWithPlayer += HandleShadeReachedPlayer;

        this.Invoke(() => shade.SetDestination(shadeSpawnOpen.transform.position), 0.5f);

        countdownRoutine = StartCoroutine(CountdownUntilChase(chaseAfterXSeconds));
    }

    /// <summary>
    ///     If the player exits the trigger too early, just start the chase.
    /// </summary>
    public void PlayerExitTrigger()
    {
        ShadeChasePlayer();

        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }
    }

    private IEnumerator CountdownUntilChase(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ShadeChasePlayer();
    }

    private void ShadeChasePlayer()
    {
        if (shade != null)
        {
            shade.ChasePlayer();
        }
    }

    /// <summary>
    ///     Handles finishing the level.
    /// </summary>
    private void HandleShadeReachedPlayer()
    {
        if (caughtSFX != null)
        {
            caughtSFX.Play();
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
