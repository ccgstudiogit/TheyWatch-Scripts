using System;
using UnityEngine;

[RequireComponent(typeof(CollectableCollisions))]
public abstract class Collectable : Interactable // TODO: Decide what I want to do with inherting from Interactable
{
    public static event Action OnCollected;

    [Tooltip("This data is necessary in order to pass this collectable's information to player's inventory")]
    [SerializeField] private CollectableDataSO collectableData;

    [Tooltip("Enable this to display a hover message when looking at the collectable (Such as: [keybinding]: Collect)")]
    [SerializeField] private bool enableHoverMessage = false;

    protected override void Awake()
    {
        base.Awake();

        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.gameObject.layer = 8;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name} was not able to find a MeshRenderer component in children. Unable to assign Collectable layer.");
#endif
        }

#if UNITY_EDITOR
        if (collectableData == null)
        {
            Debug.LogWarning($"{name} does not have collectableData. Unable to send this collectable to player inventory once collected.");
        }
        else if (collectableData.prefab == null)
        {
            Debug.LogWarning($"{name} does have collectableData, but the prefab reference in collectableData is null.");
        }
#endif
    }

    /// <summary>
    ///     When the player is looking at this object, display a string.
    /// </summary>
    /// <param name="keybinding">Can be used to show the player what key to use to interact with.</param>
    public override void OnFocus(string keybinding)
    {
        if (keybinding == "" || !enableHoverMessage)
        {
            return;
        }

        string message = $"[{keybinding}]: Collect";
        base.OnFocus(message);
    }

    /// <summary>
    ///     Get this collectable's data.
    /// </summary>
    /// <returns>CollectableDataSO.</returns>
    public CollectableDataSO GetData()
    {
        if (collectableData != null)
        {
            return collectableData;
        }

        return null;
    }

    /// <summary>
    ///     Collect this collectable by firing off OnCollected and then destroying the game object.
    /// </summary>
    public virtual void CollectThenDestroy()
    {
        OnCollected?.Invoke();
        Destroy(gameObject);
    }
}
