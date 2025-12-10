using UnityEngine;
using UnityEngine.AI;

public abstract class EntitySearchSOBase : EntityStateSOBase
{
    protected bool isMovingToPos = false;

    public override void ResetValues()
    {
        isMovingToPos = false;
    }

    protected Vector3 GetRandomPoint(float range, bool takeOtherObjectsIntoAccount = false)
    {
        Vector3 point;

        if (RandomPoint(range, out point, takeOtherObjectsIntoAccount))
        {
#if UNITY_EDITOR
            Debug.DrawRay(point, Vector3.up, Color.black, 3);
#endif
            return point;
        }

        return Vector3.zero;
    }

    protected Vector3 GetRandomPointAroundObject(float range, bool takeOtherObjectsIntoAccount = false, GameObject center = null)
    {
        Vector3 point;

        if (RandomPoint(range, out point, takeOtherObjectsIntoAccount, center))
        {
#if UNITY_EDITOR
            Debug.DrawRay(point, Vector3.up, Color.black, 3);
#endif
            return point;
        }

        return Vector3.zero;
    }

    private bool RandomPoint(float range, out Vector3 result, bool checkForOtherObjects, GameObject center = null)
    {
        int maxAttempts = 60;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint;

            if (center == null)
                randomPoint = transform.position + Random.insideUnitSphere * range;
            else
                randomPoint = center.transform.position + Random.insideUnitSphere * range;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                if (!checkForOtherObjects)
                {
                    result = hit.position;
                    return true;
                }

                Vector3 direction = hit.position - transform.position;
                if (!Physics.Raycast(transform.position, direction.normalized, direction.magnitude, LayerMask.GetMask("Default")))
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = Vector3.zero;
        return false;
    }

    protected bool IsEntityDoneMovingToPos()
    {
        bool leavingThisHereBecauseItDoesntMatter = true;
        /*
        return isMovingToPos && !entity.IsPathPending() && entity.GetAgentRemainingDistance() <= entity.GetAgentStoppingDistance() &&
                (!entity.DoesHavePath() || entity.GetAgentVelocity().sqrMagnitude == 0f);
        */
        return leavingThisHereBecauseItDoesntMatter;
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
