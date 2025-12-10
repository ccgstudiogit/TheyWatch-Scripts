using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableHashSet<TValue> : HashSet<TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TValue> values = new List<TValue>();

    /// <summary>
    ///     Unity invokes this method just before an object is serialized (right before the object is saved). Saves the HashSet to a list.
    /// </summary>
    public void OnBeforeSerialize()
    {
        // Clear the values list before adding the elements of this SerializableHashSet to the values list
        values.Clear();

        // Add the elements of this SerializableHashSet to the values list
        foreach (TValue value in this)
        {
            values.Add(value);
        }
    }

    /// <summary>
    ///     Unity invokes this method after an object is deserialized (just after the object is loaded). Loads the Hashset from a list.
    /// </summary>
    public void OnAfterDeserialize()
    {
        // Clear is called to wipe old hashset content before rebuilding the hashset from the values list
        Clear();

        // Rebuild the hashset from the values list
        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i]);
        }
    }
}
