using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Tooltip("The maximum amount of collectables that can be contained within the inventory")]
    [SerializeField] private int inventorySize = 4;

    // This is serialized for debug reasons
    [SerializeField] private List<CollectableDataSO> collectablesInInventory = new List<CollectableDataSO>();
    public int collectablesInInventoryCount => collectablesInInventory.Count;

    /// <summary>
    ///     Check if the player inventory is currently full.
    /// </summary>
    /// <returns>True if collectablesInInventory.Count >= inventorySize, False if otherwise.</returns>
    public bool IsInventoryFull()
    {
        return collectablesInInventory.Count >= inventorySize;
    }

    /// <summary>
    ///     Add a collectable's data to the player inventory.
    /// </summary>
    public void AddToInventory(CollectableDataSO collectableData)
    {
        collectablesInInventory.Add(collectableData);
    }

    /// <summary>
    ///     Retrieve the player's collectables and empty the inventory by clearing the list collectablesInInventory.
    /// </summary>
    /// <returns>An array of CollectableDataSO that were found within the player's inventory.</returns>
    public CollectableDataSO[] RetrieveCollectablesAndEmptyInventory()
    {
        CollectableDataSO[] collectables = new CollectableDataSO[collectablesInInventoryCount];

        for (int i = 0; i < collectablesInInventoryCount; i++)
        {
            collectables[i] = collectablesInInventory[i];
        }

        collectablesInInventory.Clear();

        return collectables;
    }

    /// <summary>
    ///     Get the player's inventory size.
    /// </summary>
    /// <returns>An int specifying the inventory size.</returns>
    public int GetInventorySize()
    {
        return inventorySize;
    }
}
