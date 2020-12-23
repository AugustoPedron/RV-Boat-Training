using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PathManager pathManager = (PathManager)target;
        if (GUILayout.Button("Create Path"))
        {
            pathManager.Create();
            EditorUtility.SetDirty(pathManager);
        }
        DrawDefaultInspector();
    }
}
