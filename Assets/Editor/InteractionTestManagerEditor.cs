using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractionTestManager))]
public class InteractionTestManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    
        if (GUILayout.Button(new GUIContent("Destroy Children")))
        {
            (target as InteractionTestManager).DestroyAllChildren();
        }
    }
}

