using UnityEngine;

public class DisableFlashlightTrigger : TriggerCollider
{
    [Header("Flashlight Flicker Settings")]

    [Tooltip("The total time to flicker before fading to being off")]
    [SerializeField] private float flickerTime = 2f;

    [Tooltip("The new base intensity of the flashlight that negative and positive intensity displacement will " +
        "base itself around")]
    [SerializeField] private float flashlightBaseIntensity = 13f;

    [Tooltip("The maximum negative intensity displacement that the flashlight will go down towards")]
    [SerializeField] private float negativeIntensityDisplacement = 8f;

    [Tooltip("The maximum positive intensity displacement that the flashlight will move up towards")]
    [SerializeField] private float positiveIntensityDisplacement = 5f;

    [Tooltip("The minimum interval between light intensity displacements")]
    [SerializeField] private float minInterval = 0.1f;

    [Tooltip("The maximum interval between light intensity displacements")]
    [SerializeField] private float maxInterval = 0.25f;

    [Tooltip("The time it will take for the flashlight's intensity to go to 0 after flickerTime has elapsed")]
    [SerializeField] private float finalFadeTime = 1.5f;

    protected override void OnObjectEntered()
    {
        if (LevelController.instance != null)
        {
            LevelController.instance.DisablePlayerFlashlight(
                flickerTime,
                flashlightBaseIntensity,
                new Vector2(negativeIntensityDisplacement, positiveIntensityDisplacement),
                new Vector2(minInterval, maxInterval),
                finalFadeTime
            );
        }
    }
}
