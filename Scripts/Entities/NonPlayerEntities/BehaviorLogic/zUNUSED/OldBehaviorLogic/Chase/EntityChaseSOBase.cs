using UnityEngine;

public abstract class EntityChaseSOBase : EntityStateSOBase
{
    protected float elapsedTimeInChase;

    public override void ResetValues() 
    {
        elapsedTimeInChase = 0f;
    }

    protected GameObject GetPlayerReference()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
            return playerObj;
        else
            return null;
    }
}
