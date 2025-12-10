using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ListenForCancelInput : MonoBehaviour
{
    public UnityEvent onCancel;

    [SerializeField] private InputActionReference cancelActionRef;
    private InputAction cancelAction;

    private void Awake()
    {
        cancelAction = cancelActionRef?.action;

        if (cancelAction != null && !cancelAction.enabled)
        {
            cancelAction.Enable();
        }
#if UNITY_EDITOR
        else if (cancelAction == null)
        {
            Debug.LogWarning($"{gameObject.name}'s cancelAction null. Please assign a reference to cancelActionRef.");
        }
#endif
    }

    private void OnEnable()
    {
        if (cancelAction != null)
        {
            cancelAction.performed += OnCancel;
        }
    }

    private void OnDisable()
    {
        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancel;
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        onCancel?.Invoke();
    }
}
