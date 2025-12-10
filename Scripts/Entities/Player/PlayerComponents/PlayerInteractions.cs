using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : PlayerInput
{
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;
    private Vector3 interactionRayPoint = new Vector3(0.5f, 0.5f, 0f);

    [Header("Interactions")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactionLayer;

    private Interactable currentInteractable;

    private PlayerInventory playerInventory; // Currently used for portal interactions

    protected override void Awake()
    {
        base.Awake();

        playerInventory = GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"PlayerInteractions.cs was unable to find PlayerInventory component. Player unable to interact with portal.");
#endif
        }
    }

    protected override void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    protected override void OnEnable()
    {
        playerActions.Base.Interact.performed += OnInteractPerformed;

        // Makes sure the hover message text gets updated when Portal.cs experiences a change e.g. when
        // the player drops off the collectables into the portal
        Portal.OnMessageUpdate += CurrentInteractableUpdate;
    }

    protected override void OnDisable()
    {
        playerActions.Base.Interact.performed -= OnInteractPerformed;

        Portal.OnMessageUpdate -= CurrentInteractableUpdate;
    }

    private void FixedUpdate()
    {
        HandleInteractionCheck();
    }

    /// <summary>
    ///     This method should be in FixedUpdate() as it performs a raycast check to see if there are any colliders that have the interaction layer within
    ///     the specified interactionDistance to the player's camera. If there is a collider within this distance and it has the interaction layer, that object
    ///     is considered to be an interactable and currentInteractable is set to that object.
    /// </summary>
    private void HandleInteractionCheck()
    {
        if (mainCamera == null)
        {
            return;
        }

        if (Physics.Raycast(mainCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            // NOTE: this script expects the interactable collider to be a child of the collectable game object
            if (currentInteractable == null || currentInteractable.gameObject.GetInstanceID() != hit.collider.gameObject.transform.parent.gameObject.GetInstanceID())
            {
                // Check if the collider game object has Interactable script. If not, check parent game object
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable == null)
                {
                    hit.collider.gameObject.transform.parent.gameObject.TryGetComponent(out currentInteractable);

                    // Final check before returning out of this method
                    if (currentInteractable == null)
                    {
                        return;
                    }
                }

                CurrentInteractableUpdate();
            }
        }
        else if (currentInteractable != null)
        {
            currentInteractable.OffFocus();
            currentInteractable = null;
        }
    }

    /// <summary>
    ///     Updates the text that is shown when hovering over the current interactable.
    /// </summary>
    private void CurrentInteractableUpdate()
    {
        if (currentInteractable == null)
        {
            return;
        }

        string str;

        // Check if the current interactable is the portal. If so, do special stuff
        if (currentInteractable.TryGetComponent(out Portal portal) && playerInventory != null)
        {
            str = portal.GetInteractionMessage(
                playerInventory,
                playerActions.Base.Interact.controls[InputManager.instance.usingGamepad ? 1 : 0].displayName // Check if using gamepad and display correct binding
            );
        }
        else
        {
            str = playerActions.Base.Interact.controls[InputManager.instance.usingGamepad ? 1 : 0].displayName;
        }

        currentInteractable.OnFocus(str);
    }

    /// <summary>
    ///     Handles interaction by performing a raycast from the main camera to whatever interactable the player is currently looking at (if any).
    /// </summary>
    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (mainCamera == null)
        {
            return;
        }

        if (currentInteractable != null && Physics.Raycast(mainCamera.ViewportPointToRay(interactionRayPoint), out _, interactionDistance, interactionLayer))
        {
            // If the currentInteractable is a portal and the player has collectables in inventory, do special stuff
            if (playerInventory != null && currentInteractable.TryGetComponent(out Portal portal))
            {
                if (portal.IsOpened())
                {
                    portal.Escape();
                }
                else if (playerInventory.collectablesInInventoryCount > 0)
                {
                    portal.AddCollectables(playerInventory.RetrieveCollectablesAndEmptyInventory(), gameObject);
                }
            }

            currentInteractable.Interact();
        }
    }

#if UNITY_EDITOR
    // Leaving this here for future use (even if referencing in github)
    private void DebugRay()
    {
        Ray ray = mainCamera.ViewportPointToRay(interactionRayPoint);
        Color col = Color.red;

        if (Physics.Raycast(ray))
        {
            col = Color.green;
        }

        Debug.DrawRay(ray.origin, ray.direction * 100, col);
    }
#endif
}
