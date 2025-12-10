using System.Collections;
using UnityEngine;

/// <summary>
///     This script can be used to periodically flicker the player's flashlight. Requires a LevelController reference.
/// </summary>
public class FlickerFlashlightHandler : MonoBehaviour
{
    [Header("Flicker Player Flashlight")]

    [Tooltip("A random time is chosen between this range to flicker the light every x seconds")]
    [SerializeField] private Vector2 flickerEveryXSecondsRange = new Vector2(25f, 42.5f);

    [Tooltip("The total time to flicker")]
    [SerializeField] private float flickerTime = 1.65f;

    [Tooltip("The base intensity of the light while flickering")]
    [SerializeField] private float baseIntensity = 15f;

    [Tooltip("The maximum negative intensity displacement that the flashlight will go down towards")]
    [SerializeField] private float negativeIntensityDisplacement = 8f;

    [Tooltip("The maximum positive intensity displacement that the flashlight will move up towards")]
    [SerializeField] private float positiveIntensityDisplacement = 5f;

    [Tooltip("The minimum interval between light intensity displacements")]
    [SerializeField] private float minInterval = 0.035f;

    [Tooltip("The maximum interval between light intensity displacements")]
    [SerializeField] private float maxInterval = 0.085f;

    private Coroutine flickerRoutine;
    private LevelController levelController;

    private void Awake()
    {
        levelController = GetComponent<LevelController>();
    }

    private void OnEnable()
    {
        if (levelController != null && flickerRoutine == null)
        {
            flickerRoutine = StartCoroutine(FlickeringFlashlight());
        }
    }

    private void OnDisable()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }
    }

    /// <summary>
    ///     Handles calling LevelController to flicker the flashlight every x random seconds between the flickerEveryXSecondsRange.
    /// </summary>
    private IEnumerator FlickeringFlashlight()
    {
        while (enabled)
        {
            // Get a new random time between the flickerEveryXSecondsRange
            float randomTime = Random.Range(flickerEveryXSecondsRange.x, flickerEveryXSecondsRange.y);
            yield return new WaitForSeconds(randomTime);

            if (levelController != null)
            {
                levelController.FlickerPlayerFlashlight(
                    flickerTime,
                    baseIntensity,
                    new Vector2(negativeIntensityDisplacement, positiveIntensityDisplacement),
                    new Vector2(minInterval, maxInterval)
                );
            }
        }
    }
}
