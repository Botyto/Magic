using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnergyManifestation))]
public class EnergyManifestationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (Application.isPlaying)
        {
            if (GUILayout.Button(new GUIContent("Smash")))
            {
                (target as EnergyManifestation).Smash();
            }
        }
    }
}
