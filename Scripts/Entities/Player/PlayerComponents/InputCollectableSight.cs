using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputCollectableSight : PlayerInput
{
    public static event Action<bool> OnCollectableSight;

    private ICollectableSightLevel collectableSightLevel;

    protected override void OnEnable()
    {
        if (playerActions != null)
        {
            playerActions.Base.CollectableSight.started += OnCollectableSightStarted;
            playerActions.Base.CollectableSight.canceled += OnCollectableSightCanceled;
        }

        PlayerCollisions.OnPlayerCollidedWithMonster += HandlePlayerCollidedWithMonster;
    }

    protected override void OnDisable()
    {
        if (playerActions != null)
        {
            playerActions.Base.CollectableSight.started -= OnCollectableSightStarted;
            playerActions.Base.CollectableSight.canceled -= OnCollectableSightCanceled;
        }

        PlayerCollisions.OnPlayerCollidedWithMonster -= HandlePlayerCollidedWithMonster;
    }

    protected override void Start()
    {
        base.Start();

        if (LevelController.instance != null && LevelController.instance is ICollectableSightLevel)
        {
            collectableSightLevel = LevelController.instance as ICollectableSightLevel;
        }
    }

    private void OnCollectableSightStarted(InputAction.CallbackContext ctx)
    {
        if (AreInputsEnabled())
        {
            OnCollectableSight?.Invoke(true);

            if (collectableSightLevel != null)
            {
                collectableSightLevel.SetFeatureActive(true);
            }
        }
    }

    private void OnCollectableSightCanceled(InputAction.CallbackContext ctx)
    {
        if (AreInputsEnabled())
        {
            OnCollectableSight?.Invoke(false);

            if (collectableSightLevel != null)
            {
                collectableSightLevel.SetFeatureActive(false);
            }
        }
    }

    /// <summary>
    ///     Handles turning off collectable sight if the player was using it and collided with a monster.
    /// </summary>
    private void HandlePlayerCollidedWithMonster(PlayerReferences playerReferences, Monster monster)
    {
        OnCollectableSight?.Invoke(false);

        if (collectableSightLevel != null)
        {
            collectableSightLevel.SetFeatureActive(false);
        }
    }
}
