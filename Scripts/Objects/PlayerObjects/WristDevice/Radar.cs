using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Radar : MonoBehaviour
{
    private RectTransform radarRect;

    [Header("Wrist Device Reference")]
    [Tooltip("This reference is used to prevent radar SFX from playing when there is a message on the device")]
    [SerializeField] private WristDevice wristDevice;

    [Header("Scan Settings")]
    [SerializeField, Min(0f)] private float scanEveryXSeconds = 1f;
#if UNITY_EDITOR
    // Since I have not used this variable at all but leaving this here just in case
    [Tooltip("The radar will only scan for collectables only whilst the player is looking at the wrist device")]
    [SerializeField] private bool onlyScanWhenLookingAtRadar = true;
#endif

    [Header("Ping Icon")]
    [SerializeField] private PingLocation pingLocationPrefab;
    [Tooltip("This amount of icon locations will be instantiated in Start() and these will be used to ping locations of collectables")]
    [SerializeField, Min(0)] private int pingLocationsToGenerateAtStart = 12;
    private PingLocation[] pingLocations;

    [Header("Radar Sweep")]
    [Tooltip("The capsule collider expands with the sweep icon and reveals the ping icons on the radar")]
    [SerializeField] private CapsuleCollider sweepCollider;
    [Tooltip("The target radius of the sweep's collider (this target radius should match up with the sweep's target scale)")]
    [SerializeField, Min(0f)] private float colTargetRadius = 1f;
    private float colStartingRadius;
    [Tooltip("The rect transform of the sweep icon")]
    [SerializeField] private RectTransform sweepRect;
    [Tooltip("How long it will take for the sweep icon to go across the radar's screen")]
    [SerializeField, Min(0f)] private float sweepTime;
    [Tooltip("The sweep rect transform's scale will increase to this amount (it should be large enough to cover the device's entire screen)")]
    [SerializeField] private Vector2 sweepTargetScale = new Vector2(1f, 1f);
    private Vector2 sweepStartingScale;
    private bool sweeping;

    [Header("Radar SFX")]
    // 2 audio sources are used since PlayOneShot creates separate audio sources, I decided to just create 2 audio sources to play each sfx so their
    // pitch/volume changes don't impact the other and GC is reduced since there won't be a bunch of audio sources created and destroyed
    [SerializeField] private AudioSource sweepAudioSource;
    [SerializeField] private AudioSource pingAudioSource;
    [Tooltip("The sound effect that will be played when a radar sweep begins")]
    [SerializeField] private SoundEffectSO sweepSFX;
    [Tooltip("The sound effect that will be played whenever a collectable's location is revealed on the radar")]
    [SerializeField] private SoundEffectSO pingSFX;

    private Coroutine radarRoutine;
    private RadarHitbox radarHitbox;
    private BoxCollider radarCollider;

    private List<Vector3> cachedLocations = new List<Vector3>();

    private void Awake()
    {
        radarRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        sweeping = false;

        if (pingLocationPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Radar.cs does not have a pingLocation. Unable to display collections locations. Disabling Radar.cs");
#endif
            enabled = false;
            return;
        }

        if (sweepRect == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Radar.cs' sweepIcon reference null. Disabling Radar.cs");
#endif
            enabled = false;
            return;
        }
        else
        {
            sweepStartingScale = sweepRect.localScale;
        }

        if (sweepCollider == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Radar.cs' capsule collider reference null. Disabling Radar.cs");
#endif
            enabled = false;
            return;
        }
        else
        {
            colStartingRadius = sweepCollider.radius;
        }

        GeneratePingLocations(pingLocationsToGenerateAtStart);
        StartCoroutine(GetRadarHitboxRef());
    }

    private void OnEnable()
    {
        InputCheckDevice.OnCheckDevice += HandleOnCheckDevice;
    }

    private void OnDisable()
    {
        InputCheckDevice.OnCheckDevice -= HandleOnCheckDevice;
    }

    private void OnDestroy()
    {
        if (pingLocations != null)
        {
            for (int i = 0; i < pingLocations.Length; i++)
            {
                if (pingLocations[i] != null)
                {
                    pingLocations[i].OnPing -= PlayPingSFX;
                }
            }
        }
    }

    private void HandleOnCheckDevice(bool checkingDevice)
    {
        if (checkingDevice && !wristDevice.deviceDisabled)
        {
            StartRadarRoutine();
        }
        else
        {
            StopRadarRoutine();
        }
    }

    /// <summary>
    ///     A coroutine that handles getting the relative positions of each collectable that is currently within the 
    ///     radar's Box Collider and maps the positions to the RectTransform.
    /// </summary>
    private IEnumerator RadarRoutine()
    {
        while (true)
        {
            radarHitbox.PutCollectableLocationsInList(cachedLocations);

            for (int i = 0; i < cachedLocations.Count; i++)
            {
                Vector3 localPos = radarCollider.transform.InverseTransformPoint(cachedLocations[i]);
                Vector3 center = radarCollider.center;
                Vector3 size = radarCollider.size;

                float xNorm = (localPos.x - center.x) / size.x;
                float zNorm = (localPos.z - center.z) / size.z;

                float canvasWidth = radarRect.rect.width;
                float canvasHeight = radarRect.rect.height;

                float xCanvas = xNorm * canvasWidth;
                float yCanvas = zNorm * canvasHeight;

                pingLocations[i].useLocation = true;
                pingLocations[i].rectTransform.localPosition = new Vector2(xCanvas, yCanvas);
            }

            // Make sure the other locations are not pinged since they are not needed
            for (int i = cachedLocations.Count; i < pingLocations.Length; i++)
            {
                pingLocations[i].useLocation = false;
            }

            if (!sweeping)
            {
                StartCoroutine(SweepRoutine());
            }

            yield return new WaitForSeconds(scanEveryXSeconds);
        }
    }

    /// <summary>
    ///     Handles the sweep motion of the sweep icon.
    /// </summary>
    private IEnumerator SweepRoutine()
    {
        sweeping = true;
        float lerp = 0f;

        PlaySweepSFX();

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / sweepTime);

            sweepRect.localScale = Vector2.Lerp(sweepStartingScale, sweepTargetScale, lerp);
            sweepCollider.radius = Mathf.Lerp(colStartingRadius, colTargetRadius, lerp);

            yield return null;
        }

        // Reset the sweep's rect transform and the capsule collider's radius in preparation for the next sweep
        sweepRect.localScale = sweepStartingScale;
        sweepCollider.radius = colStartingRadius;

        sweeping = false;
    }

    private void StartRadarRoutine()
    {
        radarRoutine = StartCoroutine(RadarRoutine());
    }

    private void StopRadarRoutine()
    {
        if (radarRoutine != null)
        {
            StopCoroutine(radarRoutine);
            radarRoutine = null;
        }
    }

    /// <summary>
    ///     Generate the ping locations to use for the radar.
    /// </summary>
    /// <param name="amountToGenerate">The number of ping locations to generate.</param>
    private void GeneratePingLocations(int amountToGenerate)
    {
        pingLocations = new PingLocation[amountToGenerate];

        for (int i = 0; i < amountToGenerate; i++)
        {
            pingLocations[i] = Instantiate(pingLocationPrefab.gameObject, radarRect).GetComponent<PingLocation>();
            pingLocations[i].OnPing += PlayPingSFX;
        }
    }

    /// <summary>
    ///     Plays the ping sound effect.
    /// </summary>
    private void PlayPingSFX()
    {
        if (wristDevice.messageIsDisplayed || !wristDevice.playerLookingAtDevice)
        {
            return;
        }

        if (pingAudioSource != null && pingSFX != null)
        {
            pingSFX.Play(pingAudioSource);
        }
    }

    /// <summary>
    ///     Plays the sweep sound effect.
    /// </summary>
    private void PlaySweepSFX()
    {
        if (wristDevice.messageIsDisplayed || !wristDevice.playerLookingAtDevice)
        {
            return;
        }

        if (sweepSFX != null && sweepAudioSource != null)
        {
            sweepSFX.Play(sweepAudioSource);
        }
    }

    /// <summary>
    ///     A coroutine that handles getting the RadarHitbox reference. This only runs while radarHitbox is null.
    /// </summary>
    private IEnumerator GetRadarHitboxRef()
    {
        while (radarHitbox == null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                radarHitbox = player.GetComponentInChildren<RadarHitbox>();
                radarCollider = radarHitbox.GetCollider();
            }

            yield return null;
        }
    }
}
