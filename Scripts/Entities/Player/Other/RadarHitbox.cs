using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RadarHitbox : MonoBehaviour
{
    [Tooltip("This reference is used to keep the collider rotated with the y-rotation of the camera")]
    [SerializeField] private GameObject cinemachineCam;

    private BoxCollider bCollider;

    private List<Collectable> collectablesInCollider = new List<Collectable>();

    private void Awake()
    {
        bCollider = GetComponent<BoxCollider>();

        if (cinemachineCam == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}'s cinemachineCam reference null. Disabling RadarHitbox script, radar may be unable to function correctly.");
#endif
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // Rotate the hitbox with the player's look direction
        transform.eulerAngles = new Vector3(0, cinemachineCam.transform.eulerAngles.y, 0);
    }

    /// <summary>
    ///     Add a collectable to collectablesInCollider list.
    /// </summary>
    public void AddCollectable(Collectable collectable)
    {
        collectablesInCollider.Add(collectable);
    }

    /// <summary>
    ///     Remove a collectable from collectablesInCollider list.
    /// </summary>
    public void RemoveCollectable(Collectable collectable)
    {
        if (collectablesInCollider.Contains(collectable))
        {
            collectablesInCollider.Remove(collectable);
        }
    }

    /// <summary>
    ///     Takes a List<Vector3> parameter. First this method clears the list, then adds each collectable's transform.position
    ///     that are currently within the radar's hitbox to the list.
    /// </summary>
    public void PutCollectableLocationsInList(List<Vector3> outputList)
    {
        if (outputList == null)
        {
            return;
        }

        outputList.Clear();

        for (int i = 0; i < collectablesInCollider.Count; i++)
        {
            outputList.Add(collectablesInCollider[i].transform.position);
        }
    }

    /// <summary>
    ///     Get the Box Collider component that is considered to be the radar hitbox.
    /// </summary>
    /// <returns>The radar hitbox's Box Collider.</returns>
    public BoxCollider GetCollider()
    {
        return bCollider;
    }
}
