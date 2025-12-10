using System.Collections;
using UnityEngine;

public class StitchDeathscreenJumpscare : DeathscreenJumpscareBase
{
    [Header("Look At Stitch Settings")]
    [Tooltip("The speed at which the player looks at Stitch's head")]
    [SerializeField] private float rotationSpeed = 615f;

    [Header("Additional Audio")]
    [SerializeField] private SoundEffectSO impactSFX;

    private Stitch stitch;

    protected override void Awake()
    {
        base.Awake();

        stitch = GetComponent<Stitch>();
    }

    protected override void HandleDeathscreenJumpscare(PlayerReferences playerReferences, Monster monster)
    {
        // If Stitch is retreating, don't continue
        if (monster != this.monster || stitch.IsRetreating() || caughtPlayer)
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

        RotateMonsterTowardsTarget(player.transform, monster);
        yield return null; // Wait one frame to make sure that the rotation does not affect MakePlayerLookAtMonster
        
        // Currently not needed since LookAtTarget makes sure Stitch's head is looking at the player
        if (animator != null)
        {
            //animator.SetTrigger("lookDown");
        }
        
        if (headPos != null)
        {
            yield return StartCoroutine(MakePlayerLookAtMonster(playerReferences, headPos.transform, rotationSpeed));
        }

        // Play the jumpscare SFX and load the player back into the main menu after the audio clip ends
        AudioSource source = PlayJumpscareSFX();
        LoadPlayerBackIntoMainMenu(source.clip.length / source.pitch + 0.25f); // The extra 1/4 second adds an extra slight delay
    }
}
