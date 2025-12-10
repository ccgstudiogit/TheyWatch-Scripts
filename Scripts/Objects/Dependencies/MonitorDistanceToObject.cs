using System.Collections;
using UnityEngine;

public abstract class MonitorDistanceToObject : MonoBehaviour
{
    private Transform otherTransform;
    protected abstract float _checkEveryXSeconds { get; }
    protected abstract string otherObjectTag { get; }

    private float delay; // Staggers the coroutines so that multiple objects arent running this at the exact same time

    protected virtual void Start()
    {
        StartCoroutine(FindOtherObject());
    }

    protected abstract void DoCheckDistanceLogic(float distance, Transform trans);
    
    /// <summary>
    ///     A coroutine that monitors this object's distance to another target object's transform.
    /// </summary>
    protected IEnumerator MonitorDistanceToTransformRoutine()
    {
        float distance;

        while (otherTransform != null)
        {
            distance = (transform.position - otherTransform.position).magnitude;
            DoCheckDistanceLogic(distance, otherTransform);
            yield return new WaitForSeconds(_checkEveryXSeconds);
        }

        StartCoroutine(FindOtherObject());
    }

    /// <summary>
    ///     This coroutine attempts to find another game object with a tag. Once found, it starts the coroutine
    ///     MonitorDistanceToObjectRoutine and begins monitoring the distance.
    /// </summary>
    protected IEnumerator FindOtherObject()
    {
        while (otherTransform == null)
        {
            if (!string.IsNullOrEmpty(otherObjectTag))
            {
                GameObject obj = GameObject.FindWithTag(otherObjectTag);

                if (obj != null)
                {
                    otherTransform = obj.transform;
                }
            }

            yield return new WaitForSeconds(_checkEveryXSeconds);
        }

        // Since this coroutine will likely be running an a decent amount of game objects, this is here to
        // offset the coroutines a bit so that they all don't run at exactly the same time
        delay = Random.Range(0.1f, 1.65f);
        yield return new WaitForSeconds(delay);

        StartCoroutine(MonitorDistanceToTransformRoutine());
    }
}
