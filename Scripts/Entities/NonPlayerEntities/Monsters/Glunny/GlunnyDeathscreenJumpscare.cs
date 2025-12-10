using System.Collections;
using UnityEngine;

public class GlunnyDeathscreenJumpscare : DeathscreenJumpscareBase
{
    [Header("Look At Glunny Settings")]
    [Tooltip("The speed at which the player looks at Glunny's head")]
    [SerializeField] private float rotationSpeed = 250f;

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
}
