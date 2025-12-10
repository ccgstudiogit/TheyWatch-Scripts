using UnityEngine;

public class BobUpDown : MonoBehaviour
{   
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float height = 0.2f;
    [Tooltip("If enabled, this script will randomly select a float between min and max and use that as a start delay")]
    [SerializeField] private bool includeRandomPhaseOffset = true;
    [Tooltip("If a random start delay is included, this is the minimum delay")]
    [SerializeField] private float min = 0;
    [Tooltip("If a random start delay is included, this is the maximum delay")]
    [SerializeField] private float max = 5f;
    private float phaseOffset = 0;
    private Vector3 startPos;
    private float newY;

    private void Awake()
    {
        startPos = transform.position;

        if (includeRandomPhaseOffset)
        {
            phaseOffset = Random.Range(min, max);
        }
    }

    private void Update()
    {
        newY = startPos.y + Mathf.Sin(Time.time * speed + phaseOffset) * height;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
