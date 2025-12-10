using System.Collections;
using UnityEngine;

public class BackroomsEndLevelController : LevelController
{
    [Header("Stitch")]
    [SerializeField] private GameObject stitchEndPrefab;
    private StitchEndScene stitchEndScene;
    private StitchHandGrabAnimation stitchHandGrabAnimation;

    [SerializeField] private GameObject stitchSpawn;
    [Tooltip("Stitch will move to this location after being spawned in")]
    [SerializeField] private GameObject stitchTargetLocation;

    [Tooltip("If the player spends this amount of seconds without entering the trigger collider, Stitch will spawn")]
    [SerializeField] private float forceSpawnAfterXSeconds = 20f;

    [Tooltip("The delay that takes place after Stitch is spawned before starting to run to the player")]
    // This is mainly here so that when Stitch is instantiated, SetDestination is not overridden by Stitch entering idle state
    [SerializeField, Min(0.5f)] private float initialChaseDelay = 1f;

    [Header("Stitch Grab Animation Settings")]
    [SerializeField] private float grabAnimationDelay = 5f;

    [Header("Stitch Grabbed Player")]
    [Tooltip("The sound effect that will play once Stitch grabs the player")]
    [SerializeField] private SoundEffectSO grabbedSFX;
    [Tooltip("The time it takes for the black cover screen to fade in and hide everything")]
    [SerializeField] private float coverScreenPanelFadeTime = 0.15f;
    [SerializeField] private float loadBackToMainMenuDelay = 0.65f;

    // Makes sure that if the player spends long enough without entering the trigger collider to spawn Stitch, Stitch will
    // spawn anyways and start the chase
    private float timeSinceSpawn;
    private bool startedStitchChase;

    protected override void Start()
    {
        base.Start();

        timeSinceSpawn = 0f;
        startedStitchChase = false;

        StartCoroutine(MonitorTimeSinceSpawn());
    }

    public void StartStitchChase()
    {
        if (startedStitchChase)
        {
            return;
        }

        startedStitchChase = true;

        stitchEndScene = Instantiate(stitchEndPrefab, stitchSpawn.transform.position, Quaternion.identity).GetComponent<StitchEndScene>();

        if (stitchEndScene.gameObject.TryGetComponent(out StitchHandGrabAnimation handGrabAnimation))
        {
            stitchHandGrabAnimation = handGrabAnimation;
            stitchHandGrabAnimation.OnStitchGrabbedPlayer += HandleStitchGrabbedPlayer;
        }

        if (stitchEndScene != null)
        {
            StartCoroutine(StitchChase(stitchEndScene));
        }
    }

    private IEnumerator StitchChase(StitchEndScene stitch)
    {
        // Makes sure SetDestination is not overridden by idleState.EnterState()
        yield return new WaitForSeconds(initialChaseDelay);
        stitch.SetDestination(stitchTargetLocation.transform.position);
    }

    /// <summary>
    ///     Monitors the time its been since spawn, and if the time exceeds forceSpawnAfterXSeconds Stitch is spawned.
    /// </summary>
    private IEnumerator MonitorTimeSinceSpawn()
    {
        while (!startedStitchChase)
        {
            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn > forceSpawnAfterXSeconds)
            {
                StartStitchChase();
            }

            yield return null;
        }
    }

    /// <summary>
    ///     Start Stitch's hand grab animation, where Stitch reaches his hand out to the player's camera.
    /// </summary>
    public void StartStitchHandGrabAnimation()
    {
        if (stitchHandGrabAnimation == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} attempted to start the hand grab animation but stitchHandGrabAnimation was null!");
#endif
            return;
        }

        this.Invoke(() => stitchHandGrabAnimation.StartGrabAnimation(), grabAnimationDelay);
    }

    private void HandleStitchGrabbedPlayer()
    {
        if (grabbedSFX != null)
        {
            grabbedSFX.Play();
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
