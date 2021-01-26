using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIToolKit))]
public class UIToolKitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UIToolKit uiToolKit = (UIToolKit)target;
        if (GUILayout.Button("Change Buttons Styles"))
        {
            uiToolKit.ChangeButtonsStyle();
            EditorUtility.SetDirty(uiToolKit);
        }

        if (GUILayout.Button("Change Texts Styles"))
        {
            uiToolKit.ChangeTextsStyle();
            EditorUtility.SetDirty(uiToolKit);
        }
        DrawDefaultInspector();
    }
}