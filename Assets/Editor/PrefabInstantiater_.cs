using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabInitializer))]
public class PrefabInstantiater_ : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PrefabInitializer script = (PrefabInitializer)target;
        if(GUILayout.Button("Build Object"))
        {
            script.BuildPrefabs();
        }
        if(GUILayout.Button("Clear Objects"))
        {
            script.ClearPrefabs();
        }
    }
}
