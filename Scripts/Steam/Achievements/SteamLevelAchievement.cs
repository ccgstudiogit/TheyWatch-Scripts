using UnityEngine;

public abstract class SteamLevelAchievement : MonoBehaviour
{
    [Tooltip("An achievement unlocked by completing a certain challenge in this level")]
    [SerializeField] protected SteamAchievement achievement;
}
