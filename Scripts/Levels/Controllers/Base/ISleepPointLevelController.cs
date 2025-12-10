using System.Collections.Generic;

public interface ISleepPointLevelController
{
    public abstract void RegisterSleepPoint(SleepPoint sleepPoint);

    public List<SleepPoint> sleepPoints { get; }
}
