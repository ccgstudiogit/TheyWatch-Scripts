using System;
using UnityEngine;

public abstract class GraphicsSetting : MonoBehaviour
{
    // This event is used to send change requests to GraphicsSettingsController.cs
    public static event Action<GraphicsSetting> OnSettingChanged;

    public abstract bool Changed();
    public abstract void Apply();

    protected void QueueSettingChange()
    {
        OnSettingChanged?.Invoke(this);
    }

#if UNITY_EDITOR
    protected void LogApplyNoChangeWarning()
    {
        Debug.LogWarning($"{gameObject.name} was told to apply a change but there are no changes to be made!");
    }
#endif
}
