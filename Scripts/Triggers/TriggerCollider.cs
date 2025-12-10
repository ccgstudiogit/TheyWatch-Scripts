using UnityEngine;

public abstract class TriggerCollider : MonoBehaviour
{
    [Header("Collider Reference")]
    [SerializeField] private Collider triggerCollider;

    [Header("Object Tag")]
    [Tooltip("This trigger will only work if it comes across objects with this tag")]
    [SerializeField] private string objectTag = "Player";

    [Header("General Settings")]
    [Tooltip("The maximum amount of times the player can trigger this collider by entering (can be used to prevent " + 
        "doubling up on unique events")]
    [SerializeField] private int enterMaxTimes = 1;
    private int timesEntered;

    [Tooltip("The maximum amount of times the player can trigger this collider by exiting (can be used to prevent " +
        "doubling up on unique events")]
    [SerializeField] private int exitMaxTimes = 1;
    private int timesExited;

    protected virtual void Awake()
    {
        if (triggerCollider == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s triggerCollider reference null. Disabling TriggerCollider.cs");
#endif
            enabled = false;
            return;
        }

        timesEntered = 0;
        timesExited = 0;
    }

    protected virtual void OnObjectEntered() { }
    protected virtual void OnObjectExit() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(objectTag) && timesEntered < enterMaxTimes)
        {
            OnObjectEntered();
            timesEntered++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(objectTag) && timesExited < exitMaxTimes)
        {
            OnObjectExit();
            timesExited++;
        }
    }
}
