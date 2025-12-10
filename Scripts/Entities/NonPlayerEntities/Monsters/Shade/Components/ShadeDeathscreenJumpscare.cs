using System.Collections;
using UnityEngine;

public class ShadeDeathscreenJumpscare : DeathscreenJumpscareBase
{
    [Header("Look At Shade Settings")]
    [Tooltip("The speed at which the player looks at Shade's head")]
    [SerializeField] private float rotationSpeed = 250f;

    [Header("Additional Audio")]
    [SerializeField] private SoundEffectSO impactSFX;

    private Shade shade;
    private ShadeBerserkHandler berserkHandler;

    protected override void Awake()
    {
        base.Awake();

        shade = GetComponent<Shade>();
        berserkHandler = GetComponent<ShadeBerserkHandler>();
    }

    protected override void HandleDeathscreenJumpscare(PlayerReferences playerReferences, Monster monster)
    {
        // If Shade is retreating, don't continue
        if (monster != this.monster || shade.IsRetreating() || caughtPlayer)
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

        // If Shade catches the player while berserk, keep the berserk body material active but turn off berserk-related
        // game objects and audio (also don't play laughing SFX and don't re-enable ambient audio)
        if (berserkHandler != null && berserkHandler.currentlyBerserk)
        {
            berserkHandler.CaughtPlayerWhileBerserk();
        }

        // Fullscreen distortion effect
        if (LevelController.instance != null)
        {
            LevelController.instance.BeginFullscreenDistortionOnPlayerDeath();
        }

        RotateMonsterTowardsTarget(player.transform, monster);
        yield return null; // Wait one frame to make sure that the rotation does not affect MakePlayerLookAtMonster

        if (animator != null)
        {
            animator.SetTrigger("lookDown");
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
