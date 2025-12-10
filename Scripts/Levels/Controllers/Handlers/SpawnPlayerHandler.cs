using System;
using System.Collections;
using UnityEngine;

public class SpawnPlayerHandler : MonoBehaviour
{
    // Lets listeners know when the player is loaded (e.g. currently used by LevelController to play the load in SFX)
    public static event Action<GameObject> OnPlayerLoaded;

    public GameObject player { get; private set; }

    /// <summary>
    ///     Spawn the player in with the specified player configuration settings.
    /// </summary>
    /// <param name="spawnPlayerTransform">The transform to spawn the player at.</param>
    /// <param name="playerConfig">Player configuration settings.</param>
    public void SpawnPlayer(Transform spawnPlayerTransform, PlayerConfigSO playerConfig)
    {
        StartCoroutine(SpawnPlayerRoutine(spawnPlayerTransform, playerConfig));
    }

    /// <summary>
    ///     A coroutine that is used to spawn the player. This is a coroutine because this method first loads the Player scene
    ///     and then waits until the Player scene is loaded, then gets a reference to the player game object.
    /// </summary>
    private IEnumerator SpawnPlayerRoutine(Transform spawnPlayerTransform, PlayerConfigSO playerConfig)
    {
        if (!SceneHandler.IsSceneLoaded(SceneName.Player.ToString()))
        {
            yield return SceneHandler.LoadSceneCoroutine(SceneName.Player.ToString(), additive: true);
            yield return new WaitForSeconds(0.1f); // Slight buffer before searching for player

            player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                // Disable player's CharacterController component because I noticed an inconsistent bug in that the player
                // would occasionally spawn in and stay at world origin instead of being moved to the spawnPos. Disabling
                // CharacterController seems to fix this issue
                if (player.TryGetComponent(out CharacterController characterController))
                {
                    characterController.enabled = false;
                }

                // Move root to spawn position
                Transform playerRoot = player.transform.root;
                playerRoot.position = spawnPlayerTransform.position;

                // Make the player face the same direction as the spawn position
                playerRoot.rotation = spawnPlayerTransform.rotation;

                // Re-enable the character controller and main camera audio listener if able
                if (characterController != null)
                {
                    characterController.enabled = true;
                }

                ConfigurePlayerGameObject(player, playerConfig);

                // Makes sure the player camera's audio listener component is enabled (it is disabled to try and prevent an
                // issue where abrupt SFX would play when loading into a level)
                if (player.TryGetComponent(out PlayerReferences playerReferences) && playerReferences.playerCamera != null)
                {
                    if (playerReferences.playerCamera.TryGetComponent(out AudioListener listener))
                    {
                        listener.enabled = true;
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("Player game object not found after loading " + SceneName.Player.ToString() + " scene!");
#endif
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("LevelController attempting to spawn player while player already exists!");
#endif
        }
    }

    /// <summary>
    ///     Configures the player game object with PlayerConfigSO settings found in this level's LevelDataSO.
    /// </summary>
    private void ConfigurePlayerGameObject(GameObject player, PlayerConfigSO playerConfig)
    {
        playerConfig.AddDesiredInputs(player);

        // Add player arms
        if (player.TryGetComponent(out PlayerReferences playerReferences))
        {
            // Make sure player arms are a child of the armsPos game object
            GameObject armsPos = playerReferences.armsPos;

            GameObject instantiatedArms = Instantiate(playerConfig.armsPrefab, armsPos.transform);
            instantiatedArms.transform.SetParent(armsPos.transform);

            if (player.TryGetComponent(out PlayerAnimations playerAnimations) && instantiatedArms.TryGetComponent(out Animator animator))
            {
                playerAnimations.SetAnimatorAndAnimatorController(animator, playerConfig.armsAnimatorController);
            }
        }

        // Let other scripts know the player is configured and ready
        OnPlayerLoaded?.Invoke(player);
    }
}
