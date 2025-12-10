using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/New Level Data")]
public class LevelDataSO : ScriptableObject
{
    [SerializeField] private PlayerConfigSO _playerConfig;
    public PlayerConfigSO playerConfig => _playerConfig;

    [SerializeField] private GameObject[] _collectablePrefabs;
    public GameObject[] collectablePrefabs => _collectablePrefabs;

    [SerializeField] private int _collectableCount;
    public int collectableCount => _collectableCount;

    [SerializeField] GameObject[] _monsterPrefabs;
    public GameObject[] monsterPrefabs => _monsterPrefabs;
}
