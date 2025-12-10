using UnityEngine;

public class FreezeForRandomTime : FreezeState
{
    [SerializeField] private float minTimeFrozen = 4f;
    [SerializeField] private float maxTimeFrozen = 8f;

    private float timeToFreeze;
    private float timeElapsed;

    public override void EnterState()
    {
        base.EnterState();

        timeToFreeze = Random.Range(minTimeFrozen, maxTimeFrozen);
        timeElapsed = 0f;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        timeElapsed += Time.deltaTime;

        if (timeElapsed > timeToFreeze)
        {
            EndFreeze();
        }
    }
}
