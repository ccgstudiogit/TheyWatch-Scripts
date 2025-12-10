using System;
using UnityEngine;

public class MonitorDistanceToPlayer : MonitorDistanceToObject
{
    public event Action<float> DistanceToPlayer;

    public event Action<float> PlayerWithinRange;
    public event Action PlayerNotWithinRange;

    [SerializeField] private float minDistanceToPlayer = 40f;

    [SerializeField] private float checkEveryXSeconds = 1.5f;
    protected override float _checkEveryXSeconds => checkEveryXSeconds;

    protected override string otherObjectTag => "Player";

    /// <summary>
    ///     Checks the distance every x amount of seconds. If the player is within the distance, fire off
    ///     PlayerWithinRange<float> event. If the player is not in range, fire off PlayerNotWithinRange instead.
    /// </summary>
    protected override void DoCheckDistanceLogic(float distance, Transform playerTransform)
    {
        DistanceToPlayer?.Invoke(distance);

        if (distance <= minDistanceToPlayer)
        {
            PlayerWithinRange?.Invoke(distance);
        }
        else
        {
            PlayerNotWithinRange?.Invoke();
        }
    }

    /// <summary>
    ///     Set the minimum distance to the player.
    /// </summary>
    public void SetMinDistance(float minDistanceToPlayer)
    {
        this.minDistanceToPlayer = minDistanceToPlayer;
    }

    /// <summary>
    ///     Set how often the distance to the player will be calculated.
    /// </summary>
    public void SetCheckEveryXSeconds(float checkEveryXSeconds)
    {
        this.checkEveryXSeconds = checkEveryXSeconds;
    }
}
