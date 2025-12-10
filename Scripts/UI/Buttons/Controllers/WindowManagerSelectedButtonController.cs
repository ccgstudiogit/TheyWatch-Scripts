using Michsky.MUIP;
using UnityEngine;

[RequireComponent(typeof(WindowManager))]
public class WindowManagerSelectedButtonController : MonoBehaviour
{
    [SerializeField] private ButtonController[] buttons;

    private WindowManager windowManager;

    private bool initialized = false;

    private void Awake()
    {
        windowManager = GetComponent<WindowManager>();

#if UNITY_EDITOR
        if (buttons.Length < 1)
        {
            Debug.LogWarning($"{gameObject.name}'s buttons has no elements.");
        }
#endif
    }

    private void Start()
    {
        initialized = true;

        if (buttons.Length > 0)
        {
            SetSelectedButton(buttons[windowManager.currentWindowIndex]);
        }
    }

    private void OnEnable()
    {
        // Make sure to wait for the colors to be cached in SelectedButton.cs, otherwise there is a chance OnEnable will call
        // before SelectedButton.cs's Awake() is called, causing issues with the button text color
        if (!initialized)
        {
            return;
        }

        if (buttons.Length > 0)
        {
            SetSelectedButton(buttons[windowManager.currentWindowIndex]);
        }
    }

    /// <summary>
    ///     Set a button to be set as the currently selected button. All other buttons' isSelected will be set to false.
    /// </summary>
    public void SetSelectedButton(ButtonController buttonToBeSelected)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == buttonToBeSelected)
            {
                buttons[i].SetHighlightedActive();
                buttons[i].stayHighlighted = true;
            }
            else
            {
                buttons[i].SetNormalActive();
                buttons[i].stayHighlighted = false;
            }
        }
    }
}
