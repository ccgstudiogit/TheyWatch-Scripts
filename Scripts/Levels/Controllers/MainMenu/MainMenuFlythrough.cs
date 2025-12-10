using System.Collections;
using UnityEngine;

public class MainMenuFlythrough : MonoBehaviour
{
    [Header("Environment")]
    [Tooltip("These references are used to turn environments on/off. These objects will be turned on once the flythrough is started " +
        "and then turned back off once the flythrough is finished")]
    [SerializeField] private GameObject[] environmentObjects;

    [Header("Camera Movement Points")]
    [Tooltip("The location the camera will start at")]
    [SerializeField] private GameObject startingLocation;
    [Tooltip("The location the camera will end at")]
    [SerializeField] private GameObject endingLocation;

    [Header("Camera Rotation")]
    [Tooltip("Optional reference where the camera's transform will always be looking at this object")]
    [SerializeField] private Transform cameraFocalPoint = null;

    private void Awake()
    {
        // Set this flythrough's environment to be in-active so it doesn't potentially interfere with other flythrough environments
        SetEnvironmentActive(false);
    }

    /// <summary>
    ///     Have the camera do a flythrough.
    /// </summary>
    /// <param name="cameraTransform">The camera's transform.</param>
    public IEnumerator FlythroughRoutine(Transform cameraTransform)
    {
        if (MainMenuController.instance == null)
        {
            yield break;
        }

        SetEnvironmentActive(true);

        Vector3 startingPos, endingPos;
        float flythroughDuration = MainMenuController.instance.flythroughDuration;

        // Makes sure the fade out happens when the camera's position reaches far enough. For example, if the backgroundFadeOutPercentage
        // is 80, that means the fade panel will fade back in when the camera is 80% of the way done. This is converted to a decimal so that
        // lerp can easily be compared to this percentage value
        float fadeOutStartPercentage = MainMenuController.instance.backgroundFadeOutPercentage / 100f;

        startingPos = startingLocation.transform.position;
        endingPos = endingLocation.transform.position;

        // Move the camera to the starting position and match the camera's rotation to that of the starting location's rotation
        cameraTransform.position = startingPos;
        cameraTransform.rotation = startingLocation.transform.rotation;

        MainMenuController.instance.FadePanel(0, MainMenuController.instance.backgroundFadeInTime);

        float lerp = 0f, smoothLerp;
        bool fadeOutStarted = false;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / flythroughDuration);
            smoothLerp = Mathf.SmoothStep(0, 1, lerp);

            cameraTransform.position = Vector3.Lerp(startingPos, endingPos, smoothLerp);

            if (lerp >= fadeOutStartPercentage && !fadeOutStarted && MainMenuController.instance != null)
            {
                fadeOutStarted = true;
                MainMenuController.instance.FadePanel(1, MainMenuController.instance.backgroundFadeOutTime);
            }

            // NOTE: If I notice the camera becomes jittery, using transform.LookAt() may cause jittery/un-smooth movements if the
            // camera happens to get too close to the focal point
            if (cameraFocalPoint != null)
            {
                cameraTransform.LookAt(cameraFocalPoint);
            }

            yield return null;
        }

        SetEnvironmentActive(false);
    }

    /// <summary>
    ///     Set this flythrough's environment to be active or in-active.
    /// </summary>
    /// <param name="setActive">Whether the game objects of this flythrough's environment should be active or not.</param>
    public void SetEnvironmentActive(bool setActive)
    {
        HelperMethods.SetGameObjectsActive(environmentObjects, setActive);
    }
}
