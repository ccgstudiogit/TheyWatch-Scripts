using System.Collections;
using UnityEngine;

public class TrailerLerpMove : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private GameObject startPoint;
    [SerializeField] private GameObject endPoint;

    [Header("Duration")]
    [Tooltip("The delay before starting the movement")]
    [SerializeField, Min(0f)] private float delay = 5f;
    [Tooltip("The duration, in seconds, for how long it will take to move through the start and end points")]
    [SerializeField, Min(0f)] private float duration = 5f;

    private void Start()
    {
        transform.position = startPoint.transform.position;
        this.Invoke(() => StartCoroutine(MoveLerp()), delay);
    }

    private IEnumerator MoveLerp()
    {
        Vector3 startingPos, endingPos;
        float lerp = 0f, smoothLerp;

        startingPos = startPoint.transform.position;
        endingPos = endPoint.transform.position;

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            smoothLerp = Mathf.SmoothStep(0, 1, lerp);

            transform.position = Vector3.Lerp(startingPos, endingPos, smoothLerp);

            yield return null;
        }

        transform.position = endPoint.transform.position;
    }
}
