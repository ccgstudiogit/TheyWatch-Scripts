using UnityEngine;

public class WayPoint : MonoBehaviour
{
#if UNITY_EDITOR
    // Helpful for debugging the current score
    private float currentScore = 0f;
#endif

    private void Start()
    {
        if (LevelController.instance != null)
        {
            LevelController.instance.RegisterWayPoint(this);
        }
    }

#if UNITY_EDITOR
    public void SetScore(float score)
    {
        currentScore = score;
    }

    public float GetScore()
    {
        return currentScore;
    }
#endif
}
