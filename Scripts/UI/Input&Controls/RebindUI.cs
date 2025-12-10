using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Michsky.MUIP;

public class RebindUI : MonoBehaviour
{
    [Header("Binding Settings")]
    [Tooltip("Selects a specified input action from an asset")]
    [SerializeField] private InputActionReference inputActionReference;

    [Tooltip("Excludes the mouse from being selected as an input binding while re-binding")]
    [SerializeField] private bool excludeMouse = true;

    [Tooltip("The index of the desired binding to swap (the range is how many bindings an input action may have)")]
    [SerializeField, Range(0, 10)] private int selectedBinding;
    
    // Composite settings
    [SerializeField] private bool isComposite = false;
    [Tooltip("The individual binding of the composite part. Starts at because 0 is considered the action itself, so " + 
        "Action == 0, Up == 1, Down == 2, Left == 3, Right == 4")]
    [SerializeField, Range(1, 4)] private int compositeIndex = 1;

    // Built in enum that allows the formatting of binding names
    [Tooltip("Format the binding names")]
    [SerializeField] private InputBinding.DisplayStringOptions displayStringOptions;

    // Helpful debug information
    [Header("Binding Info - Do Not Edit")]
    [SerializeField] private InputBinding inputBinding;

    // Makes sure that the selectedBinding does not cause an index out of range exception
    private int bindingIndex;

    // Stores the action name as a string so InputManager can search for the action name in the C# generated class
    private string actionName;

    [Header("UI Fields")]
    [Tooltip("The displayed text of the input action's name")]
    [SerializeField] private TextMeshProUGUI actionText;
    [Tooltip("If enabled, the displayed text will use a custom string instead of the input action's binding name")]
    [SerializeField] private bool overrideActionText;
    [SerializeField] private string actionTextOverride;

    [SerializeField] private ButtonManager rebindButton;
    [SerializeField] private ButtonManager resetButton;

    private bool initialized = false;

    private void Start()
    {
        GetBindingInfo();
        InputManager.instance.LoadBindingOverride(actionName);
        UpdateUI();

        initialized = true;
    }

    private void OnEnable()
    {
        InputManager.OnRebindComplete += UpdateUI;
        InputManager.OnRebindCanceled += UpdateUI;

        if (initialized)
        {
            GetBindingInfo();
            InputManager.instance.LoadBindingOverride(actionName);
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        InputManager.OnRebindComplete -= UpdateUI;
        InputManager.OnRebindCanceled -= UpdateUI;
    }

#if UNITY_EDITOR
    // Updates binding information even whilst not in playmode
    private void OnValidate()
    {
        if (inputActionReference == null)
        {
            return;
        }

        GetBindingInfo();
        UpdateUI();
    }
#endif

    /// <summary>
    ///     Get the binding information from this rebind UI's input action reference.
    /// </summary>
    private void GetBindingInfo()
    {
        if (inputActionReference.action == null)
        {
            return;
        }

        actionName = inputActionReference.action.name;

        // Make sure the bindingIndex does not exceed the action's total bindings count
        if (inputActionReference.action.bindings.Count > selectedBinding)
        {
            inputBinding = inputActionReference.action.bindings[selectedBinding];
            bindingIndex = selectedBinding;
        }
    }

    /// <summary>
    ///     Update the UI to make sure all of the current keybinding information is correct
    /// </summary>
    private void UpdateUI()
    {
        if (actionText != null)
        {
            actionText.text = overrideActionText ? actionTextOverride : actionName;
        }

        if (rebindButton != null)
        {
            // If the application is running, get the binding display information from InputManager. Otherwise, whilst in the
            // editor, get the binding display information directly from the scriptable object input action container
            if (Application.isPlaying)
            {
                // Grab info from InputManager
                rebindButton.buttonText = isComposite ?
                    rebindButton.buttonText = InputManager.instance.GetBindingName(actionName, compositeIndex) :
                    rebindButton.buttonText = InputManager.instance.GetBindingName(actionName, bindingIndex);
            }
            else
            {
                rebindButton.buttonText = isComposite ?
                    rebindButton.buttonText = inputActionReference.action.GetBindingDisplayString(compositeIndex) :
                    rebindButton.buttonText = inputActionReference.action.GetBindingDisplayString(bindingIndex);
            }   

            rebindButton.UpdateUI();
        }
    }

    /// <summary>
    ///     Start the rebind process with InputManager.
    /// </summary>
    public void DoRebind()
    {
        InputManager.instance.StartRebind(actionName, isComposite ? compositeIndex : bindingIndex, rebindButton, excludeMouse);
    }

    /// <summary>
    ///     Reset the this binding to its default set in PlayerActions.
    /// </summary>
    public void ResetBinding()
    {
        InputManager.instance.ResetBinding(actionName, isComposite ? compositeIndex : bindingIndex);
        UpdateUI();
    }
}
