using UnityEngine;

public class RetreatToDistantWayPoint : RetreatState
{
    [Header("Retreat Settings")]
    [Tooltip("This entity will consider this amount of distant waypoints to head to and randomly pick one")]
    [SerializeField] private int wayPointsToConsiderCount = 5;

    [SerializeField] private float movementSpeedMultiplier = 3f;
    [SerializeField] private float accelerationMultiplier = 11f;
    [Tooltip("Note: This changes the entire animator's speed, not just a specific animation")]
    [SerializeField] private float animatorSpeed = 1.5f;

    private float minTime = 1f; // Bare minimum time in retreat (helps prevent monstersight from getting the entity out of retreat too early)
    private float timeElapsed;

    public override void EnterState()
    {
        base.EnterState();

        timeElapsed = 0f;

        entity.MovementSpeedMultiplier(movementSpeedMultiplier);
        entity.AccelerationMultiplier(accelerationMultiplier);
        entity.SetAnimatorSpeed(animatorSpeed);

        Retreat(wayPointsToConsiderCount);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        timeElapsed += Time.deltaTime;

        if (timeElapsed > minTime)
        {
            CheckIfDoneRetreatingAndSwitchStates();
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        entity.ResetMovementSpeed();
        entity.ResetAccelerationSpeed();
        entity.ResetAnimatorSpeed();
    }
}
