using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComputeTexture3D), true)]
public class ComputeTexture3DEditor: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ComputeTexture3D myScript = (ComputeTexture3D)target;
        if (GUILayout.Button("Save As 3D Texture Asset"))
        {
            myScript.Generate();
        }

        if (GUILayout.Button("Save Slice As 2D Texture Asset"))
        {
            myScript.GenerateSlice(0);
        }
    }
}
