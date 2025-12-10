using UnityEngine;

public class ChasePlayer : ChaseState
{
    [Header("Chase Settings")]
    [Tooltip("The minimum time this entity will spend chasing the player, whether the player is seen or not (if the " +
        "player is currently seen, this timer resets to 0 ensuring that the monster keeps chasing if the player is seen)")]
    [SerializeField] private float minTimeInChase = 5f;

    private MonsterSight monsterSight;

    public override void EnterState()
    {
        base.EnterState();

        if (monsterSight == null)
        {
            monsterSight = entity.GetComponent<MonsterSight>();
        }

#if UNITY_EDITOR
        // Checks to see if monsterSight is still null even after getting the component
        if (monsterSight == null)
        {
            Debug.LogWarning($"{gameObject.name}'s ChasePlayer state behavior was not able to find a MonsterSight component.");
        }
#endif
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
        // Makes sure that if the monster is still looking at the player, the monster stays in chase state and
        // remembers the player's last known position
        if (monsterSight != null && monsterSight.isPlayerCurrentlySeen)
        {
            elapsedTimeInChase = 0f;
            playerLastKnownPosition = player.transform.position;
        }

        // If the path is not valid (i.e. from player crouching in a hiding spot), wait at player's last known
        // position for [timeToWaitAtPlayersLastKnownLocation] seconds before giving up and moving on
        if (!IsPathValid(player.transform.position))
        {
            MoveToPosition(playerLastKnownPosition);

            // Get close to the player before incrementing time spent waiting
            if (Vector3.Distance(playerLastKnownPosition, transform.position) < 1.5f)
            {
                elapsedTimeWaiting += Time.deltaTime;

                if (elapsedTimeWaiting > timeToWaitAtPlayersLastKnownLocation)
                {
                    StopChaseAndSwitchStates();
                }
            }
        }
        else if (elapsedTimeInChase <= minTimeInChase)
        {
            MoveToTransform(player.transform);
        }
        else
        {
            StopChaseAndSwitchStates();
        }
    }
}
