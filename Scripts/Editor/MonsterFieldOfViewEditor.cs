using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterSight), true)]
public class MonsterFieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        MonsterSight monster = (MonsterSight)target;
        Handles.color = Color.white;
        Vector3 pos = monster.GetPositionPlusOffset();
        Handles.DrawWireArc(pos, Vector3.up, Vector3.forward, 360, monster.fovRadius);

        Vector3 viewAngle01 = DirectionFromAngle(monster.transform.eulerAngles.y, -monster.fovAngle / 2); // Left side of vision
        Vector3 viewAngle02 = DirectionFromAngle(monster.transform.eulerAngles.y, monster.fovAngle / 2); // Right side of vision

        Handles.color = Color.yellow;
        Handles.DrawLine(pos, pos + viewAngle01 * monster.fovRadius);
        Handles.DrawLine(pos, pos + viewAngle02 * monster.fovRadius);
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
