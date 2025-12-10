using System.Collections;
using UnityEngine;

public class FactoryEndLevelController : LevelController
{
    [Header("End Sequence")]
    [SerializeField] private OverseerEndScene overseer;
    [Tooltip("After the player enters the final room, Overseer will wake up after this amount of time")]
    [SerializeField, Min(0f)] private float timeToWakeOverseer = 5f;

    [SerializeField] private SoundEffectSO caughtSFX;
    [Tooltip("The time it takes for the black cover screen to fade in and hide everything")]
    [SerializeField] private float coverScreenPanelFadeTime = 0.15f;
    [SerializeField] private float loadBackToMainMenuDelay = 0.65f;

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerCollisions.OnPlayerCollidedWithMonster += EndLevel;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PlayerCollisions.OnPlayerCollidedWithMonster -= EndLevel;
    }

    /// <summary>
    ///     Wake up Overseer.
    /// </summary>
    public void WakeUpOverseer()
    {
        StartCoroutine(WakeUpOverseerRoutine(timeToWakeOverseer));
    }

    private IEnumerator WakeUpOverseerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        overseer.WakeUp();
    }
    
    /// <summary>
    ///     End the level by playing a caught sfx, disabling player controls, and sending the player back to the main menu.
    /// </summary>
    private void EndLevel(PlayerReferences playerReferences, Monster monster)
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
