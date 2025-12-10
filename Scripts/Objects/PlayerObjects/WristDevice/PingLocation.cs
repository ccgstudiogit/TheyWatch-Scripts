using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PingLocation : MonoBehaviour
{
    // Currently used by Radar.cs to play a ping sound effect
    public event Action OnPing;

    [HideInInspector] public bool useLocation;

    [Header("Icon Prefab")]
    [SerializeField] private PingIcon iconPrefab;

    [Tooltip("The maximum amount of icons this ping location will instantiate in Start() and have access to")]
    [SerializeField] private int maxIcons = 3;

    private PingIcon[] icons;
    public RectTransform rectTransform { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        GenerateIcons(maxIcons);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useLocation)
        {
            PingIcon iconToUse = GetNonActivePingIcon();
            iconToUse.Ping(rectTransform.localPosition);
            OnPing?.Invoke();
        }
    }

    /// <summary>
    ///     Instantiate and add the ping icons to the icons array.
    /// </summary>
    private void GenerateIcons(int iconsToGenerate)
    {
        icons = new PingIcon[iconsToGenerate];

        for (int i = 0; i < iconsToGenerate; i++)
        {
            // The ping icons are children of the radar object since the PingLocation will be moving around a lot, and if the ping
            // icon is a child of this location then the ping icon will also move with it, so having the icon be a child of the radar
            // (which should be the parent of this location, as set in Radar.cs when instantating these locations) prevents that issue
            icons[i] = Instantiate(iconPrefab.gameObject, transform.parent).GetComponent<PingIcon>();
            icons[i].Hide();
        }
    }

    /// <summary>
    ///     Get an icon that is not currently already being used (prevents an icon from moving across the radar).
    /// </summary>
    /// <returns>An icon that is not actively pinged. If none are found, the first icon in the icons array is returned.</returns>
    private PingIcon GetNonActivePingIcon()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null && !icons[i].activelyPinged)
            {
                return icons[i];
            }
        }

        return icons[0];
    }
}
