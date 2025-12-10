using System.Collections;
using UnityEngine;

public class PortalCollectableLocation : MonoBehaviour
{
    public bool occupied { get; private set; }

    private void Awake()
    {
        occupied = false;
    }

    /// <summary>
    ///     Place a collectable with an animation at this location.
    /// </summary>
    /// <param name="collectable">The collectable to be placed.</param>
    /// <param name="animationSpeed">The speed of the animation.</param>
    /// <param name="animationDistance">The distance from this location that the collectable should start at.</param>
    /// <param name="animationDelay">A delay that occurs after the collectable is instantiated but before the animation begins.</param>
    public void PlaceCollectable(CollectableDataSO collectable, float animationSpeed, float animationDistance, float animationDelay, SoundEffectSO sfx, AudioSource source)
    {
        if (occupied)
        {
            return;
        }

        StartCoroutine(PlaceCollectableRoutine(collectable.placedInPortalPrefab, animationSpeed, animationDistance, animationDelay, sfx, source));
    }

    private IEnumerator PlaceCollectableRoutine(GameObject collectablePrefab, float speed, float distance, float delay, SoundEffectSO sfx, AudioSource source)
    {
        if (collectablePrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} attempted to place a collectable in the portal but the placedInPortalPrefab from CollectableDataSO was null.");
#endif
            yield break;
        }

        occupied = true;

        // Instantiate the prefab and set the prefab's parent to be this transform
        GameObject instantiated = Instantiate(collectablePrefab);
        instantiated.transform.SetParent(gameObject.transform, false);

        // Calculate the animation start position and move the instantiated prefab to that position
        Vector3 animationStartPos = transform.position + transform.TransformDirection(Vector3.forward) * distance;
        instantiated.transform.position = animationStartPos;

        // Begin the animation process after the specified delay
        yield return new WaitForSeconds(delay);
        float duration = distance / speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // Increment elapsed based on framerate
            float t = Mathf.Clamp01(elapsed / duration); // Get a clamped value based on targeted duration
            float easedT = HelperMethods.EaseOutCubic(t); // As t increases, this method decreases t which gives a smoother feeling

            instantiated.transform.position = Vector3.Lerp(animationStartPos, transform.position, easedT);

            yield return null;
        }

        // Play the sound effect once the runestone finishes its animation
        if (sfx != null && source != null)
        {
            sfx.PlayOneShot(source);
        }

        instantiated.transform.position = transform.position;
    }

    /// <summary>
    ///     Places a collectable at this location immediately with no animation.
    /// </summary>
    public void PlaceCollectableNoAnimation(CollectableDataSO collectable)
    {
        if (collectable.placedInPortalPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name} attempted to place a collectable in the portal but the placedInPortalPrefab from CollectableDataSO was null.");
#endif
            return;
        }

        occupied = true;

        // Instantiate the prefab and set the prefab's parent to be this transform
        GameObject instantiated = Instantiate(collectable.placedInPortalPrefab);
        instantiated.transform.SetParent(gameObject.transform, false);
    }
}
