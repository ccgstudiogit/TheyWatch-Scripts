using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    /// <summary>
    ///     Unity invokes this method just before an object is serialized (right before the object is saved). Saves the dictionary to lists.
    /// </summary>
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    /// <summary>
    ///     Unity invokes this method after an object is deserialized (just after the object is loaded). Loads the dictionary to lists.
    /// </summary>
    public void OnAfterDeserialize()
    {
        // Clear is called to wipe old dictionary content, not the lists
        Clear();

#if UNITY_EDITOR
        if (keys.Count != values.Count)
        {
            Debug.LogError($"Tried to desrialize a SerializableDictionary, but the amount of keys {keys.Count} does not match the " +
                $"number of values {values.Count} which indicates that something went wrong.");
        }
#endif

        // Re-build the dictionary from the 2 lists
        for (int i = 0; i < keys.Count; i++)
        {
            Add(keys[i], values[i]);
        }
    }
}
