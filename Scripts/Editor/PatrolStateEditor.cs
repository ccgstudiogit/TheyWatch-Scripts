using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolState), true)]
public class PatrolStateEditor : Editor
{
    private void OnSceneGUI()
    {
        PatrolState patrol = (PatrolState)target;
        Handles.color = Color.blue;
        Vector3 pos = patrol.transform.position;
        Handles.DrawWireArc(pos, Vector3.up, Vector3.forward, 360, patrol.Radius);
    }
}
