using UnityEngine;

public class DisappearToFarthestWayPoint : DisappearState
{
    private float minTime = 1f; // After teleporting to the farthest waypoint, wait this amount of time before resuming normal behavior
    private float timeElapsed;
    
    public override void EnterState()
    {
        base.EnterState();
        Disappear();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        timeElapsed += Time.deltaTime;

        if (timeElapsed > minTime)
        {
            CheckIfDoneWaitingAndSwitchStates();
        }
    }
}
