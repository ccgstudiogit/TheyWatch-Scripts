using UnityEngine;

public class GuideButton : MonoBehaviour
{
    [Tooltip("The level this guide is tied to (should only be unlocked if this level is completed or has enough attempts)")]
    [field: SerializeField] public Level level { get; private set; }
}
