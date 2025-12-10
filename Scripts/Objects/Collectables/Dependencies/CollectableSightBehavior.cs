using UnityEngine;

public class CollectableSightBehavior : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Light lightAura;
    [SerializeField, Min(0f)] private float maxDistance = 37.5f;

    private bool collectableSightActive;
    private Transform player;

    private void OnEnable()
    {
        InputCollectableSight.OnCollectableSight += HandlePlayerUsingCS;
    }

    private void OnDisable()
    {
        InputCollectableSight.OnCollectableSight -= HandlePlayerUsingCS;
    }

    private void Update()
    {
        if (collectableSightActive)
        {
            // If the player is within the maxDistance, set it visible. Otherwise, make it invisible.
            SetVisible(GetDistanceToPlayerSqred() < maxDistance * maxDistance);
        }
        else if (!meshRenderer.enabled)
        {
            SetVisible(true);
        }
    }

    /// <summary>
    ///     Handles whenever the player uses collectable sight. If collectable sight is active, disable the mesh renderers and only
    ///     re-enable them when the player gets close enough.
    /// </summary>
    /// <param name="collectableSightActive">Whether or not collectable sight is active</param>
    private void HandlePlayerUsingCS(bool collectableSightActive)
    {
        this.collectableSightActive = collectableSightActive;
    }

    /// <summary>
    ///     Get the distance to the player's transform.
    /// </summary>
    /// <returns>A float distance.</returns>
    private float GetDistanceToPlayerSqred()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        return (transform.position - player.transform.position).sqrMagnitude;
    }

    /// <summary>
    ///     Set this collectable to be visible or not visible.
    /// </summary>
    private void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;

        if (lightAura != null)
        {
            lightAura.enabled = visible;
        }
    }
}
