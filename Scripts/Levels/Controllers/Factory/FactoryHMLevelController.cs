using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FactoryHMLevelController : FactoryLevelController, IHMLevelController, ICollectableSightLevel
{
    [Header("Collectable Sight Effects")]
    [SerializeField] private ScriptableRendererFeature collectableSightFeature;

    [SerializeField] private Material collectableSightMat;
    [Tooltip("The vignette effect's power while collectable sight is not active (a higher power will make the effect go away)")]
    [SerializeField, Min(0f)] private float inactiveVignettePower = 25f;
    private const string vignettePower = "_VignettePower"; // The variable name in the shader

    [SerializeField, Min(0.05f)] private float csFadeTime = 0.5f;
    private float activeVignettePower; // What the vignette power should be while collectable sight is active

    protected override void Start()
    {
        base.Start();
        activeVignettePower = collectableSightMat.GetFloat(vignettePower);
        collectableSightMat.SetFloat(vignettePower, inactiveVignettePower);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        collectableSightMat.SetFloat(vignettePower, activeVignettePower);
    }

    /// <summary>
    ///     Set the collectable sight feature to be active or in-active.
    /// </summary>
    public void SetFeatureActive(bool active)
    {
        this.Invoke(() => collectableSightFeature.SetActive(active), active ? 0f : csFadeTime);
        LerpVignette(active ? activeVignettePower : inactiveVignettePower);
    }

    /// <summary>
    ///     Lerp the vignette of the collectableSightMat to the target vignette.
    /// </summary>
    private void LerpVignette(float targetVignette)
    {
        StartCoroutine(LerpVignetteRoutine(collectableSightMat.GetFloat(vignettePower), targetVignette, csFadeTime));
    }
    
    /// <summary>
    ///     Handles lerping the vignette from its current vignette to the target vignette.
    /// </summary>
    private IEnumerator LerpVignetteRoutine(float startVignette, float targetVignette, float duration)
    {
        float lerp = 0f;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            float vignette = Mathf.Lerp(startVignette, targetVignette, lerp);
            collectableSightMat.SetFloat(vignettePower, vignette);

            yield return null;
        }

        collectableSightMat.SetFloat(vignettePower, targetVignette);
    }
}
