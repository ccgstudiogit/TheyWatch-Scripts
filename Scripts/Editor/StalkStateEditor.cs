using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StalkState), true)]
public class StalkStateEditor : Editor
{
    private void OnSceneGUI()
    {
        StalkState stalk = (StalkState)target;
        Vector3 pos = stalk.transform.position;

        Handles.color = Color.red;
        Handles.DrawWireArc(pos, Vector3.up, Vector3.forward, 360, stalk.stopAtPlayerRange);

        Handles.color = Color.blue;
        Handles.DrawWireArc(pos, Vector3.up, Vector3.forward, 360, stalk.strategicWayPointRange);
    }
}