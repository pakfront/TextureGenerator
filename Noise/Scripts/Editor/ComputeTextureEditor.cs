using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComputeTexture), true)]
public class ComputeTextureEditor: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ComputeTexture myScript = (ComputeTexture)target;
        if (GUILayout.Button("Save As 2D Texture Asset"))
        {
            myScript.Generate();
        }
    }
}
