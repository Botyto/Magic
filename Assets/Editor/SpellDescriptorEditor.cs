using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpellDescriptor))]
public class SpellDescsriptorEditor : Editor
{
    public void OnSceneGUI()
    {
        GUILayout.Label("GUID: " + (target as SpellDescriptor).ToString());
        DrawDefaultInspector();
    }
}
