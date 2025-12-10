using System.Collections;
using UnityEngine;

public class OverseerDeathscreenJumpscare : DeathscreenJumpscareBase
{
    [Header("Look At Overseer Settings")]
    [Tooltip("The speed at which the player looks at Overseer's head")]
    [SerializeField] private float rotationSpeed = 615f;
    [SerializeField] private bool useLookDownAnim = true;

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

        if (useLookDownAnim && animator != null)
        {
            animator.SetTrigger("lookDown");
        }

        if (impactSFX != null)
        {
            impactSFX.PlayOneShot(jumpscareSFXAudioSource);
        }

        // Fullscreen distortion effect
        if (LevelController.instance != null)
        {
            LevelController.instance.BeginFullscreenDistortionOnPlayerDeath();
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
