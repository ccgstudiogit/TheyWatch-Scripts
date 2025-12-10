using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerReferences))]
public class PlayerHeadbob : MonoBehaviour
{
    private Transform camPosTransform;
    
    [Tooltip("The minimum speed needed for the headbob to take place")]
    [SerializeField] private float minMovementValue = 0.1f;
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    private float defaultYPos = 0; // The position at which the camera rests/resets to while the player is not moving
    private float timer;

    // Since floats are tricky to work with in getting exact comparisons, this tolerance is used for getting the camera back up to
    // the defaultYPos (technically very, very close but not exactly precise) when the player is no longer moving
    private const float defaultYPosTolerance = 0.0035f;

    private PlayerReferences playerReferences;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerReferences = GetComponent<PlayerReferences>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerReferences?.camPos == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"PlayerReferences camPos reference null. Disabling PlayerHeadbob.cs");
#endif
            enabled = false;
            return;
        }
        else
        {
            camPosTransform = playerReferences.camPos.transform;
        }
    }

    private void Start()
    {     
        defaultYPos = camPosTransform.localPosition.y;
    }

    private void OnDisable()
    {
        // Makes sure that the player camera transform.position.y is reset if headbob is disabled
        if (camPosTransform != null)
        {
            camPosTransform.position = new Vector3(camPosTransform.position.x, defaultYPos, camPosTransform.position.z);
        }
    }

    private void Update()
    {
        HandleHeadbob();
    }

    private void HandleHeadbob()
    {
        if (!playerMovement.enabled)
        {
            return;
        }

        // Handle the headbob while moving
        if (Mathf.Abs(playerMovement.moveDir.x) > minMovementValue || Mathf.Abs(playerMovement.moveDir.z) > minMovementValue)
        {
            timer += Time.deltaTime * (playerMovement.isSprinting ? sprintBobSpeed : walkBobSpeed);

            // Wrap the timer to keep it within 0 and 2 * pi -- realistically this doesn't need to be done but it can keep the timer from scaling
            // off into infinity (which in normal gameplay shouldn't matter)
            timer %= Mathf.PI * 2f;

            camPosTransform.localPosition = new Vector3(
                camPosTransform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (playerMovement.isSprinting ? sprintBobAmount : walkBobAmount),
                camPosTransform.localPosition.z
            );
        }
        // If the player is not moving and the current head position is not the resting position, move it back to the resting position (defaultYPos)
        else if (camPosTransform.localPosition.y < defaultYPos - defaultYPosTolerance || camPosTransform.localPosition.y > defaultYPos + defaultYPosTolerance)
        {
            camPosTransform.localPosition = new Vector3(camPosTransform.localPosition.x, defaultYPos, camPosTransform.localPosition.z);
        }
    }
}
