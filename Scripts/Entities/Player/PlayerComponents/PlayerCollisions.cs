using System;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    public static event Action<PlayerReferences, Monster> OnPlayerCollidedWithMonster;

    private PlayerInventory playerInventory;
    private PlayerReferences playerReferences; // Used to pass references to the monster that collided with the player

    private bool escaping;

    private void Awake()
    {
        playerReferences = GetComponent<PlayerReferences>();
#if UNITY_EDITOR
        if (playerReferences == null)
        {
            Debug.LogWarning("PlayerCollisions.cs is unable to find PlayerReferences component. " +
                "Unable to invoke OnPlayerCollidedWithMonster event action.");
        }
#endif

        playerInventory = GetComponent<PlayerInventory>();
#if UNITY_EDITOR
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerCollisions.cs is unable to find PlayerInventory component. " +
                "Unable to add collectables to inventory.");
        }
#endif
    }

    private void OnEnable()
    {
        LevelController.OnPlayerEscaped += HandlePlayerEscaped;
    }

    private void OnDisable()
    {
        LevelController.OnPlayerEscaped -= HandlePlayerEscaped;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Monster monster))
        {
            CollidedWithMonster(monster);
        }
        else if (other.TryGetComponent(out Collectable collectable))
        {
            CollidedWithCollectable(collectable);
        }
        // Check if the collectable component is apart of the collider's parent object
        else if (other.transform.parent != null && other.transform.parent.TryGetComponent(out collectable))
        {
            CollidedWithCollectable(collectable);
        }
    }

    private void CollidedWithCollectable(Collectable collectable)
    {
        if (playerInventory == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("PlayerInventory null, unable to collect.");
#endif
            return;
        }

        // If the player's inventory is currently full, do not do anything to the collectable
        if (playerInventory.IsInventoryFull())
        {
            return;
        }

        CollectableDataSO data = collectable.GetData();
        if (data != null)
        {
            playerInventory.AddToInventory(data);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("PlayerCollisions.cs detected a collectable collision but that collectable did not " +
                "have a CollectableDataSO. Unable to send this collectable's data to PlayerInventory.");
#endif
        }

        collectable.CollectThenDestroy();
    }

    private void CollidedWithMonster(Monster monster)
    {
        if (playerReferences == null || escaping)
        {
            return;
        }

        OnPlayerCollidedWithMonster?.Invoke(playerReferences, monster);
    }

    private void HandlePlayerEscaped()
    {
        escaping = true;
    }
}
