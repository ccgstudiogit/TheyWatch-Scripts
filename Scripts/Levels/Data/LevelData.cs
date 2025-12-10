using UnityEngine;

// This script holds LevelDataSO so that LevelController is able to get the current level's specific data
public class LevelData : MonoBehaviour
{
    [SerializeField] private LevelDataSO data;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogWarning($"{name}'s data is null.");
        }

        if (gameObject.tag != "LevelData")
        {
            gameObject.tag = "LevelData";
        }
    }

    /// <summary>
    ///     Get this level's data.
    /// </summary>
    /// <returns>LevelDataSO.</returns>
    public LevelDataSO GetData()
    {
        return data != null ? data : null;
    }
}
