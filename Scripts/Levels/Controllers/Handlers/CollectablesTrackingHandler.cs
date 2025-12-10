using UnityEngine;

public class CollectablesTrackingHandler : MonoBehaviour
{
    public int remainingCollectables { get; private set; }

    /// <summary>
    ///     Set remainingCollectables to a specified amount.
    /// </summary>
    public void SetCollectablesRemaining(int newCollectablesRemaining)
    {
        remainingCollectables = newCollectablesRemaining;
    }

    /// <summary>
    ///     Should be called whenever a collectable is collected in order to update remainingCollectables
    ///     and check if remainingCollectables is less than 1.
    /// </summary>
    public void HandleCollectableCollected()
    {
        remainingCollectables--;

        if (remainingCollectables < 1)
        {
            OnAllCollectablesCollected();
        }
    }

    private void OnAllCollectablesCollected()
    {
#if UNITY_EDITOR
        Debug.Log("All collectables collected!");
#endif
    }
}
