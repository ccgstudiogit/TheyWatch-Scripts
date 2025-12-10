using TMPro;
using UnityEngine;

public class DisplayCollectablesRemaining : MonoBehaviour
{
    private LevelDataSO data;
    private TextMeshProUGUI remainingCountText;

    private int collectablesRemaining = 0;

    private void Awake()
    {
        remainingCountText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (LevelController.instance != null)
        {
            data = LevelController.instance.GetLevelData();
            collectablesRemaining = data.collectableCount;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("DisplayCollectablesRemaining was not able to get level data. Count set to 0.");
#endif
        }

        remainingCountText.text = collectablesRemaining.ToString();
    }

    private void OnEnable()
    {
        Collectable.OnCollected += HandleCollectableCollected;
    }

    private void OnDisable()
    {   
        Collectable.OnCollected -= HandleCollectableCollected;
    }

    private void HandleCollectableCollected()
    {
        collectablesRemaining--;

        // Make sure text does not go below 0 (in the off-chance this may happen in testing)
        if (collectablesRemaining < 1)
        {
            collectablesRemaining = 0;
        }

        remainingCountText.text = collectablesRemaining.ToString();
    }
}
