using System;
using System.Collections;
using UnityEngine;

public class FactoryEndRoomDoor : MonoBehaviour
{
    // Lets ScraplingEndScene know when the door is closed so they can enter idle state (and prevent a bunch of footstep sfx)
    public event Action OnDoorClosed;

    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private Collider col;

    [Header("Settings")]
    [Tooltip("The time in seconds in takes to shut the door")]
    [SerializeField, Min(0f)] private float shutDuration = 0.4f;
    [Tooltip("The door will lerp to this local position and stop here")]
    [SerializeField] private Vector3 targetShutPos;

    [Header("Audio")]
    [SerializeField] private AudioSource shutSource;
    [SerializeField] private AudioSource slamSource;

    /// <summary>
    ///     Shut the door.
    /// </summary>
    public void ShutDoor()
    {
        StartCoroutine(ShutDoorRoutine());
    }
    
    private IEnumerator ShutDoorRoutine()
    {
        Vector3 startPos = door.transform.localPosition;
        float lerp = 0f;

        // Prevent the player from leaving once the door is starting to be shut
        col.gameObject.SetActive(true);

        if (shutSource != null)
        {
            shutSource.Play();
        }

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / shutDuration);
            door.transform.localPosition = Vector3.Lerp(startPos, targetShutPos, lerp);

            yield return null;
        }

        if (slamSource != null)
        {
            slamSource.Play();
        }

        OnDoorClosed?.Invoke();
    }
}
