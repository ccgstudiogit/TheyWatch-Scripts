using UnityEngine;

public class ChasePlayerTimed : ChaseState
{
    [Header("Chase Settings")]
    [Tooltip("The time this entity will chase the player")]
    [SerializeField] private float chaseTime = 10f;

    public override void EnterState()
    {
        base.EnterState();

        if (player != null)
        {
            playerLastKnownPosition = player.transform.position;
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (player == null)
        {
            player = GetPlayerReference();
            return;
        }

        HandleChase();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    private void HandleChase()
    {
        if (elapsedTimeInChase < chaseTime)
        {
            if (IsPathValid(player.transform.position))
            {
                playerLastKnownPosition = player.transform.position;
                MoveToTransform(player.transform);
            }
            else
            {
                MoveToPosition(playerLastKnownPosition);

                // Get close to the player before incrementing time spent waiting
                if (Vector3.Distance(playerLastKnownPosition, transform.position) < 1.5f)
                {
                    elapsedTimeWaiting += Time.deltaTime;

                    if (elapsedTimeWaiting > timeToWaitAtPlayersLastKnownLocation)
                    {
                        StopChase();
                    }
                }
            }
        }
        else
        {
            StopChase();
        }
    }
}
