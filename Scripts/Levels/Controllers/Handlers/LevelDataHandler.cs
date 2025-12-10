using UnityEngine;

[RequireComponent(typeof(LevelData))]
public class LevelDataHandler : MonoBehaviour
{
    private LevelData levelDataScript;
    private LevelDataSO data;

    private void Awake()
    {
        levelDataScript = GetComponent<LevelData>();

        data = levelDataScript.GetData();
        if (data == null)
        {   
            GetGenericData(); // If levelDataScript's levelDataSO is null, generic data is used instead
        }
    }

    /// <summary>
    ///     Sets data to the generic data found in Assets/Resources/LevelData/.
    /// </summary>
    private void GetGenericData()
    {
        LevelDataSO genericData = Resources.Load<LevelDataSO>("LevelData/GenericData");
        if (genericData == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name} attempted to load GenericData from Resources/LevelData/ because LevelData's " +
                $"levelDataSO was null, but {name} could not find GenericData in Resources/LevelData/.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.LogWarning("The game object containing level data was not found, so GenericData was loaded from " +
            "Resources/LevelData/ and will be used as this level's data.");
#endif

        data = genericData;
    }   

    /// <summary>
    ///     Get this level's data.
    /// </summary>
    /// <returns>LevelDataSO.</returns>
    public LevelDataSO GetLevelData()
    {
        if (data != null)
        {
            return data;
        }
        else
        {
            GetGenericData();
            return data;
        }
    }
}
