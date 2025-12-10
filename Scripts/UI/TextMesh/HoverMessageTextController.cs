using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HoverMessageTextController : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Color visibleColor;
    private Color invisibleColor;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        visibleColor = new Color(text.color.r, text.color.g, text.color.b, 1.0f);
        invisibleColor = new Color(text.color.r, text.color.g, text.color.b, 0f);

        text.color = invisibleColor;
    }

    private void OnEnable()
    {
        Interactable.OnInteractableFocus += ShowMessage;
        Interactable.OffInteractableFocus += HideMessage;
    }

    private void OnDisable()
    {
        Interactable.OnInteractableFocus -= ShowMessage;
        Interactable.OffInteractableFocus -= HideMessage;
    }

    private void ShowMessage(string message)
    {
        text.text = message;
        text.color = visibleColor;
    }

    private void HideMessage()
    {
        text.color = invisibleColor;
    }
}
