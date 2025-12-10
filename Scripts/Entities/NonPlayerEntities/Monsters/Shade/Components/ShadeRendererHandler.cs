using UnityEngine;

public class ShadeRendererHandler : MonoBehaviour
{
    [field: SerializeField] public SkinnedMeshRenderer bodyMeshRenderer { get; private set; }
    [field: SerializeField] public SkinnedMeshRenderer eyesMeshRenderer { get; private set; }

    /// <summary>
    ///     Can be used to enable/disable Shade's body and eye mesh renderers.
    /// </summary>
    public void SetVisible(bool visible)
    {
        bodyMeshRenderer.enabled = visible;
        eyesMeshRenderer.enabled = visible;
    }

    /// <summary>
    ///     Set Shade's body material to a new material.
    /// </summary>
    public void SetBodyMaterial(Material material)
    {
        bodyMeshRenderer.material = material;
    }

    /// <summary>
    ///     Set Shade's eye material to a new material.
    /// </summary>
    public void SetEyesMaterial(Material material)
    {
        eyesMeshRenderer.material = material;
    }

    /// <summary>
    ///     Check if Shade is currently visible (if the mesh renderers are enabled or disabled).
    /// </summary>
    /// <returns>True if the mesh renderers are enabled, false if otherwise.</returns>
    public bool IsVisible()
    {
        return bodyMeshRenderer.enabled && eyesMeshRenderer.enabled;
    }
}
