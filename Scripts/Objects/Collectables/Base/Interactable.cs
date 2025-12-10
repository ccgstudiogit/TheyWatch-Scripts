using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public static event Action<string> OnInteractableFocus;
    public static event Action OffInteractableFocus;

    [Tooltip("Note: PlayerInteractions.cs expects this collider to be a child of the Interactable game object")]
    [SerializeField] private Collider[] interactableColliders;

    protected virtual void Awake()
    {
        if (interactableColliders.Length > 0)
        {
            for (int i = 0; i < interactableColliders.Length; i++)
            {
                interactableColliders[i].gameObject.layer = 9;
            }
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"{gameObject.name} does not have interactableCollider reference. Unable to interact with {gameObject.name}");
        }
#endif
    }

    /// <summary>
    ///     Interact with this Interactable.
    /// </summary>
    public abstract void Interact();
    
    public virtual void OnFocus(string message)
    {
        if (message != null)
        {
            OnInteractableFocus?.Invoke(message);
        }
    }

    public virtual void OffFocus()
    {
        OffInteractableFocus?.Invoke();
    }
}
