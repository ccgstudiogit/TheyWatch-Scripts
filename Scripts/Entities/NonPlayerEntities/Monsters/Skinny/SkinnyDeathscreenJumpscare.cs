using System.Collections;
using UnityEngine;

public class SkinnyDeathscreenJumpscare : DeathscreenJumpscareBase
{
    [Header("Look At Skinny Settings")]
    [Tooltip("The speed at which the player looks at Skinny's head")]
    [SerializeField] private float rotationSpeed = 250f;

    [Header("Skinny Audio Settings")]
    [SerializeField] private bool turnOffMusic = true;
    [SerializeField] private AudioSource musicAudioSource;

    protected override void HandleDeathscreenJumpscare(PlayerReferences playerReferences, Monster monster)
    {
        if (monster != this.monster)
        {
            return;
        }

        StartCoroutine(DeathscreenJumpscareRoutine(playerReferences, monster));
    }

    private IEnumerator DeathscreenJumpscareRoutine(PlayerReferences playerReferences, Monster monster)
    {
        InvokeOnDeathscreenJumpscareAction();
        
        GameObject player = playerReferences.gameObject;

        DisableScript<UniversalPlayerInput>(player);
        DisableScript<PlayerLook>(player);
        DisableScript<PlayerMovement>(player);

        if (turnOffMusic)
        {
            TurnOffMusic();
        }

        RotateMonsterTowardsTarget(player.transform, monster);
        yield return null; // Wait one frame to make sure that the rotation does not affect MakePlayerLookAtMonster

        PlayJumpscareSFX();

        if (animator != null)
        {   
            animator.SetTrigger("lookDown");
        }

        if (headPos != null)
        {
            yield return StartCoroutine(MakePlayerLookAtMonster(playerReferences, headPos.transform, rotationSpeed));
        }

        LoadPlayerBackIntoMainMenu(2f);
    }

    private void TurnOffMusic()
    {
        if (musicAudioSource == null)
        {
            return;
        }

        musicAudioSource.Pause();
    }
}
