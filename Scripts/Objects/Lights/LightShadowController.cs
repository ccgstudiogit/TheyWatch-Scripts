using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif

[RequireComponent(typeof(Light), typeof(MonitorDistanceToPlayer))]
public class LightShadowController : MonoBehaviour
{
    [Tooltip("The reference that enables this light's shadows to be enabled/disabled depending " +
        "on if the mesh renderer is within the camera's view frustrum")]
    [SerializeField] private MeshRenderer meshRenderer;

    [Tooltip("If the meshrenderer is visible and within this distance to the player, shadows will be turned on")]
    [SerializeField, Min(0)] private float maxDistanceToPlayer = 40f;

    [Tooltip("If the light source is within this distance to the player, shadows will be turned on no matter what")]
    [SerializeField, Min(0)] private float distanceOverride = 18.5f;
    
    [SerializeField] private LightShadows lightShadows = LightShadows.Hard;

    private float distanceToPlayer = 0f;

#if UNITY_EDITOR
    [Header("Editor Only Settings")]
    [SerializeField] private bool disableShadowsInEditMode = true;

    // For keeping track of how many lights are in the scene
    private static int totalLightCount = 0;
    private static bool totalLightCountLogged = false;
#endif

    private Light thisLight;
    private MonitorDistanceToPlayer monitorDistanceToPlayer;

    private void Awake()
    {
        thisLight = GetComponent<Light>();
        monitorDistanceToPlayer = GetComponent<MonitorDistanceToPlayer>();

#if UNITY_EDITOR
        totalLightCount++;
#endif
    }

    private void Start()
    {
        if (meshRenderer == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{gameObject.name}'s LightShadowController does not have MeshRenderer reference. " +
                    "Please assign a reference. Unable to dynamically change shadow settings on this light.");
#endif
            enabled = false;
            return;
        }

#if UNITY_EDITOR
        if (Application.isPlaying && !totalLightCountLogged)
        {
            totalLightCountLogged = true;
            Debug.Log("Current light count with ShadowController = " + totalLightCount);
        }
#endif
    }

    private void OnEnable()
    {
        monitorDistanceToPlayer.DistanceToPlayer += GetDistanceToPlayer;
    }

    private void OnDisable()
    {
        monitorDistanceToPlayer.DistanceToPlayer -= GetDistanceToPlayer;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Prevent affecting playmode and prefabs in project
        if (!Application.isPlaying && !PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            // Disabling shadows in the editor helps reduce debug.log statements stating there are too many lights with shadows
            if (disableShadowsInEditMode && thisLight != null && thisLight.shadows != LightShadows.None)
            {
                DisableShadows();
            }

            return;
        }
#endif
        // Note: distance is calculated via MonitorDistanceToPlayer
        if (distanceToPlayer <= distanceOverride)
        {
            // This is a separate if-statement so that if the distanceToPlayer is less than distanceOverride, the other else-if aren't checked
            if (thisLight.shadows == LightShadows.None)
            {
                EnableShadows(lightShadows);
            }
        }
        else if (thisLight.shadows == LightShadows.None && meshRenderer.isVisible && distanceToPlayer <= maxDistanceToPlayer)
        {
            EnableShadows(lightShadows);
        }
        else if (thisLight.shadows != LightShadows.None && (!meshRenderer.isVisible || distanceToPlayer > maxDistanceToPlayer))
        {
            DisableShadows();
        }
    }

    private void GetDistanceToPlayer(float distance)
    {
        // Save the distance to be used in Update()
        distanceToPlayer = distance;
    }

    private void EnableShadows(LightShadows shadowType)
    {
        thisLight.shadows = shadowType;
    }

    private void DisableShadows()
    {
        thisLight.shadows = LightShadows.None;
    }
}
