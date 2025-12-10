using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedButton : MonoBehaviour
{
    public bool isSelected { get; private set; }

    [SerializeField] private Color selectedColor = Color.yellow;

    [SerializeField] private TextMeshProUGUI normalText;
    [SerializeField] private TextMeshProUGUI highlightedText;

    private Color startingNormalTextColor;
    private Color startingHighlightedTextColor;

    [SerializeField] private Image normalBackground;
    [SerializeField] private Image highlightedBackground;

    private Color startingNormalBackgroundColor;
    private Color startingHighlightedBackgroundColor;

    private void Awake()
    {
        // Get starting colors for text
        if (normalText != null)
        {
            startingNormalTextColor = normalText.color;
        }

        if (highlightedText != null)
        {
            startingHighlightedTextColor = highlightedText.color;
        }

        // Get starting colors for background images
        if (normalBackground != null)
        {
            startingNormalBackgroundColor = normalBackground.color;
        }

        if (highlightedBackground != null)
        {
            startingHighlightedBackgroundColor = highlightedBackground.color;
        }
    }

    /// <summary>
    ///     If this button is considered to be selected, change the color to its selected color. Otherwise, the
    ///     color will be set to its starting color.
    /// </summary>
    /// <param name="selected">Whether or not this button should be considered currently selected.</param>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (normalText != null)
        {
            normalText.color = isSelected ? selectedColor : startingNormalTextColor;
        }

        if (highlightedText != null)
        {
            highlightedText.color = isSelected ? selectedColor : startingHighlightedTextColor;
        }

        if (normalBackground != null)
        {
            normalBackground.color = isSelected ? selectedColor : startingNormalBackgroundColor;
        }

        if (highlightedBackground != null)
        {
            highlightedBackground.color = isSelected ? selectedColor : startingHighlightedBackgroundColor;
        }   
    }
}
