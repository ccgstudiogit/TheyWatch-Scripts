using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "ScriptableObjects/New PlayerConfig")]
public class PlayerConfigSO : ScriptableObject
{
    [Header("Arms")]
    [SerializeField] private GameObject _armsPrefab;
    public GameObject armsPrefab => _armsPrefab;

    [SerializeField] private RuntimeAnimatorController _armsAnimatorController;
    public RuntimeAnimatorController armsAnimatorController => _armsAnimatorController;

    [Header("Extra Inputs")]
    [SerializeField] private bool includeCollectableSight;
    [SerializeField] private bool includeFlashlight;
    [SerializeField] private bool includeCheckDevice;
    [SerializeField] private bool includeEMP;

    /// <summary>
    ///     Adds input components to the player based on the extra inputs settings.
    /// </summary>
    public void AddDesiredInputs(GameObject player)
    {
        if (includeCollectableSight && !player.GetComponent<InputCollectableSight>())
        {
            player.AddComponent<InputCollectableSight>();
        }

        if (includeFlashlight && !player.GetComponent<InputFlashlight>())
        {
            player.AddComponent<InputFlashlight>();
        }

        if (includeCheckDevice && !player.GetComponent<InputCheckDevice>())
        {
            player.AddComponent<InputCheckDevice>();
        }

        if (includeEMP && !player.GetComponent<InputEMP>())
        {
            player.AddComponent<InputEMP>();
        }
    }
}
