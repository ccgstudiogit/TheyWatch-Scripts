using UnityEngine;

public class PlayerReferences : MonoBehaviour
{
    [field: SerializeField] public Camera playerCamera { get; private set; }
    [field: SerializeField] public GameObject cinemachineCam { get; private set; }
    [field: SerializeField] public GameObject camPos { get; private set; }
    [field: SerializeField] public GameObject armsPos { get; private set; }
    public AudioSource audioSource { get; private set; }

    private void Awake()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning($"PlayerReferences.cs' playerCamera reference null. Other scripts may not be able to function correctly.");
        }

        if (cinemachineCam == null)
        {
            Debug.LogWarning($"PlayerReferences.cs' cinemachineCam reference null. Other scripts may not be able to function correctly.");
        }

        if (camPos == null)
        {
            Debug.LogWarning($"PlayerReferences.cs' camPos reference null. Other scripts may not be able to function correctly.");
        }

        if (armsPos == null)
        {
            Debug.LogWarning($"PlayerReferences.cs' armsPos reference null. Other scripts may not be able to function correctly.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning($"PlayerReferences.cs could not find an audio source component. Other scripts may not be able to perform audio functions.");
        }
    }
}
