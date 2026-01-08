using System;
using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    public static event Action<PlayerReferences, Monster> OnPlayerDeath; // Player's health reaches 0. PlayerReferences is sent so player input is disabled
    public static event Action<Monster> OnPlayerTakesDamage;

    private PlayerInventory playerInventory;
    private PlayerReferences playerReferences; // Used to pass references to the monster that collided with the player
    private PlayerHealth playerHealth;

    private bool escaping;

    private void Awake()
    {
        playerReferences = GetComponent<PlayerReferences>();
        playerInventory = GetComponent<PlayerInventory>();
        playerHealth = GetComponent<PlayerHealth>();
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

        // Don't take damage if the monster is retreating
        if (monster is IRetreatStateUser)
        {
            IRetreatStateUser retreatStateUser = monster as IRetreatStateUser;

            if (monster.IsEntityInSpecificState(retreatStateUser.retreatState))
            {
                return;
            }
        }

        int damageToTake = monster.GetDamage();
        playerHealth.TakeDamage(damageToTake);

        if (playerHealth.GetHealth() <= 0)
        {
            OnPlayerDeath?.Invoke(playerReferences, monster);
        }
        else
        {
            OnPlayerTakesDamage?.Invoke(monster);

            playerHealth.PlayDistortion();
            playerHealth.PlayImpactSFX();
            playerHealth.PlayScreenShake();
            playerHealth.ActivateBloodEffect();
        }
    }

    /// <summary>
    ///     Prevents edge-case scenarios where the player is escaping via the portal, but the maze's entity touches the player before
    ///     fully escaping via the portal causing the escape and jumpscare lose sequence to both play out, creating unexpected behavior.
    /// </summary>
    private void HandlePlayerEscaped()
    {
        escaping = true;
    }
}
