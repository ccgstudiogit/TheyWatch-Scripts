using System.Collections.Generic;
using Michsky.MUIP;
using UnityEngine;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("Apply Settings Button")]
    [SerializeField] private ButtonManager applySettingsButton;

    // A HashSet is used instead of a list to prevent duplicate changes from being added
    private HashSet<GraphicsSetting> queuedGraphicsSettingsChanges = new HashSet<GraphicsSetting>();

#if UNITY_EDITOR
    private void Awake()
    {
        if (applySettingsButton == null)
        {
            Debug.LogWarning($"{gameObject.name}'s applySettingsButton null.");
        }
    }
#endif

    private void OnEnable()
    {
        if (queuedGraphicsSettingsChanges.Count > 0)
        {
            queuedGraphicsSettingsChanges.Clear();
        }

        UpdateApplySettingsButton();

        GraphicsSetting.OnSettingChanged += QueueChange;
    }

    private void OnDisable()
    {
        GraphicsSetting.OnSettingChanged -= QueueChange;
    }

    /// <summary>
    ///     Applies the current queued graphics setting changes.
    /// </summary>
    public void ApplyChanges()
    {
        foreach (var change in queuedGraphicsSettingsChanges)
        {
            change.Apply();
        }

        queuedGraphicsSettingsChanges.Clear();
        UpdateApplySettingsButton();
    }

    /// <summary>
    ///     Queues a change by adding it to the HashSet. Also checks if the change was reverted back to its previous state,
    ///     and if it was the GraphicsSetting is removed from the HashSet and won't be queued to change (since the user put
    ///     the graphics setting to its previous setting).
    /// </summary>
    private void QueueChange(GraphicsSetting graphicsSettingChange)
    {
        if (graphicsSettingChange == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} attempted to queue a change but the GraphicsSetting was null!");
#endif
            return;
        }

        if (graphicsSettingChange.Changed())
        {
            queuedGraphicsSettingsChanges.Add(graphicsSettingChange);
        }
        else if (queuedGraphicsSettingsChanges.Contains(graphicsSettingChange))
        {
            queuedGraphicsSettingsChanges.Remove(graphicsSettingChange);
        }

        UpdateApplySettingsButton();
    }

    /// <summary>
    ///     Used to update whether or not the Apply Settings button is interactable or not. The Apply Settings button is only
    ///     interactable if queuedGraphicsSettingsChanges has 1 or more changes queued up.
    /// </summary>
    private void UpdateApplySettingsButton()
    {
        applySettingsButton.Interactable(queuedGraphicsSettingsChanges.Count > 0);
    }
}
