using UnityEngine;

[CreateAssetMenu(fileName = "CollectableData", menuName = "ScriptableObjects/New Collectable Data")]
public class CollectableDataSO : ScriptableObject
{
    [SerializeField] private GameObject _prefab;
    public GameObject prefab => _prefab;

    [SerializeField] private GameObject _placedInPortalPrefab;
    public GameObject placedInPortalPrefab => _placedInPortalPrefab;
}
