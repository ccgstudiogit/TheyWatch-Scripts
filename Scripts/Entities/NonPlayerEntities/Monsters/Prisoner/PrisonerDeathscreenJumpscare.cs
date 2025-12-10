using System.Collections;
using UnityEngine;

public class PrisonerDeathscreenJumpscare : DeathscreenJumpscareBase
{
    [Header("Animator")]
    [SerializeField] private Animator animatorController;

    private const string animatorCowering = "cowering";
    private const string animatorScared = "scared";
    
    [Header("Look At Prisoner Settings")]
    [Tooltip("The speed at which the player looks at Prisoner's head")]
    [SerializeField] private float rotationSpeed = 615f;

    [Header("Additional Audio")]
    [SerializeField] private SoundEffectSO impactSFX;

    protected override void HandleDeathscreenJumpscare(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this.monster || caughtPlayer)
        {
            return;
        }

        caughtPlayer = true;

        StartCoroutine(DeathscreenJumpscareRoutine(playerReferences, monster));
    }

    private IEnumerator DeathscreenJumpscareRoutine(PlayerReferences playerReferences, Monster monster)
    {
        GameObject player = playerReferences.gameObject;

        DisablePlayerMovement(player);

        if (impactSFX != null)
        {
            impactSFX.PlayOneShot(jumpscareSFXAudioSource);
        }

        // Fullscreen distortion effect
        if (LevelController.instance != null)
        {
            LevelController.instance.BeginFullscreenDistortionOnPlayerDeath();
        }

        // Make sure the prisoner is not scared/cowering
        if (animatorController != null)
        {
            animatorController.SetInteger(animatorCowering, 0);
            animatorController.SetInteger(animatorScared, 0);
        }

        RotateMonsterTowardsTarget(player.transform, monster);
        yield return null; // Wait one frame to make sure that the rotation does not affect MakePlayerLookAtMonster

        if (headPos != null)
        {
            yield return StartCoroutine(MakePlayerLookAtMonster(playerReferences, headPos.transform, rotationSpeed));
        }

        // Play the jumpscare SFX and load the player back into the main menu after the audio clip ends
        AudioSource source = PlayJumpscareSFX();
        LoadPlayerBackIntoMainMenu(source.clip.length / source.pitch + 0.25f); // The extra 1/4 second adds an extra slight delay
    }
}
