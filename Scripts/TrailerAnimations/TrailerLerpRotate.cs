using System.Collections;
using UnityEngine;

public class TrailerLerpRotate : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Quaternion targetRotation;

    [Header("Duration")]
    [Tooltip("The delay before starting the rotation")]
    [SerializeField, Min(0f)] private float delay = 5f;
    [Tooltip("The duration, in seconds, for how long it will take for this object to get to the target rotation")]
    [SerializeField, Min(0f)] private float duration = 5f;

    private void Start()
    {
        this.Invoke(() => StartCoroutine(RotateLerp()), delay);
    }

    private IEnumerator RotateLerp()
    {
        float lerp = 0f, smoothLerp;
        Quaternion startingRotation = transform.rotation;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            smoothLerp = Mathf.SmoothStep(0, 1, lerp);

            transform.rotation = Quaternion.Lerp(startingRotation, targetRotation, smoothLerp);

            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
