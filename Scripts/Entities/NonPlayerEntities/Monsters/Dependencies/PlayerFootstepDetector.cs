using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerFootstepDetector : MonoBehaviour
{
    // Lets listeners know when a player footstep is within range and sends the Vector3 position of the player at the time
    // of the footstep
    public event Action<Vector3> OnPlayerFootstepHeard;

    [Header("Listening For Player Footsteps")]
    [Tooltip("This entity must be within this range of the player to be able to hear footsteps")]
    [SerializeField, Min(0)] private float _footstepMaxDist = 13.5f;
    public float footstepMaxDist => _footstepMaxDist;

#if UNITY_EDITOR
    [Header("Editor Only - Visualize Listening Range")]
    [SerializeField] private bool visualizeListeningRange = true;
    [Tooltip("The color of the arc that displays the range")]
    [SerializeField] private Color footstepArcColor = Color.green;
#endif

    // The PlayerFootstepAudio reference is acquired via 1 of 2 ways: 1) in Start(), if the player is already loaded in before
    // this listener is loaded in, the reference will be acquired in Start(). 2) in OnEnable(), if this listener is loaded in before
    // the player is, by subscribing to OnPlayerLoaded this makes sure that the reference is acquired when the player is loaded in
    private PlayerFootstepAudio playerFootstepAudio;

    private void Start()
    {
        GetPlayerFootstepAudioReference();
    }

    private void OnEnable()
    {
        SpawnPlayerHandler.OnPlayerLoaded += GetPlayerFootstepAudioReference;
    }

    private void OnDisable()
    {
        SpawnPlayerHandler.OnPlayerLoaded -= GetPlayerFootstepAudioReference;
    }

    private void OnDestroy()
    {
        if (playerFootstepAudio != null)
        {
            playerFootstepAudio.OnAudibleFootstep -= HandlePlayerFootstep;
        }
    }

    /// <summary>
    ///     Handles getting a reference to PlayerFootstepAudio.cs
    /// </summary>
    /// <param name="player">The player game object. This parameter is here because this method should be subscribed to
    ///     SpawnPlayerHandler.OnPlayerLoaded, in case the player is loaded after this entity's Start() method is called.</param>
    private void GetPlayerFootstepAudioReference(GameObject player = null)
    {
        // Makes sure that multiple subscriptions don't happen
        if (playerFootstepAudio != null)
        {
            return;
        }

        if (player == null)
        {
            player = LevelController.instance.GetPlayer();
        }

        if (player != null && player.TryGetComponent(out PlayerFootstepAudio pFA))
        {
            playerFootstepAudio = pFA;

            // This is unsubscribed from in OnDestroy()
            playerFootstepAudio.OnAudibleFootstep += HandlePlayerFootstep;
        }
    }

    /// <summary>
    ///     Handles firing off the OnPlayerFootstepHeard event if the player footstep was in range.
    /// </summary>
    /// <param name="footstepOrigin">The position of the player at the time of the footstep.</param>
    private void HandlePlayerFootstep(Vector3 footstepOrigin)
    {
        // Make sure the event is only fired off if this script is enabled on the game object
        if (enabled && Vector3.Distance(transform.position, footstepOrigin) < footstepMaxDist)
        {
            OnPlayerFootstepHeard?.Invoke(footstepOrigin);
        }
    }

    /// <summary>
    ///     Set footstep max distance.
    /// </summary>
    public void SetFootstepMaxDist(float footstepMaxDist)
    {
        _footstepMaxDist = footstepMaxDist;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!visualizeListeningRange)
        {
            return;
        }

        float angle = 360;
        Handles.color = footstepArcColor;
        Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, angle, footstepMaxDist);
    }
#endif
}
