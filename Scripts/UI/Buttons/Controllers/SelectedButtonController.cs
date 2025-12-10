using System.Collections.Generic;

public class SelectedButtonController
{
    private HashSet<ButtonController> buttons;

    public SelectedButtonController()
    {
        buttons = new HashSet<ButtonController>();
    }

    public SelectedButtonController(HashSet<ButtonController> buttons)
    {
        this.buttons = buttons;
    }

    /// <summary>
    ///     Add a SelectedButton to the HashSet.
    /// </summary>
    /// <param name="button">The button to be added.</param>
    public void Add(ButtonController button)
    {
        buttons.Add(button);
    }

    /// <summary>
    ///     Remove a SelectedButton from the HashSet.
    /// </summary>
    /// <param name="button">The button to be removed.</param>
    public void Remove(ButtonController button)
    {
        if (buttons.Contains(button))
        {
            buttons.Remove(button);
        }
    }

    /// <summary>
    ///     Set a button to be set as the currently selected button. All other buttons' isSelected will be set to false.
    /// </summary>
    public void SetSelectedButton(ButtonController buttonToBeSelected)
    {
        foreach (var button in buttons)
        {
            // If this button matches buttonToBeSelected, send true. Otherwise, send false
            if (button == buttonToBeSelected)
            {
                button.SetHighlightedActive();
                button.stayHighlighted = true;
            }
            else
            {
                button.SetNormalActive();
                button.stayHighlighted = false;
            }
        }
    }

    /// <summary>
    ///     Get the SelectedButton HashSet's current count.
    /// </summary>
    /// <returns>An int of the current count.</returns>
    public int CurrentCount()
    {
        return buttons.Count;
    }
}
