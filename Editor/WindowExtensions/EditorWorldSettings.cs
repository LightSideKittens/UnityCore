using LSCore;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class EditorWorldSettings
{
    static EditorWorldSettings()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnGUI);
    }

    private static void OnGUI()
    {
        if (World.IsEditMode)
        { 
            EditorWorld.TargetFps = EditorGUILayout.FloatField("Editor Target FPS", EditorWorld.TargetFps, GUILayout.MaxWidth(200));
        }
        else
        {
            Application.targetFrameRate = EditorGUILayout.IntField("Target FPS", Application.targetFrameRate, GUILayout.MaxWidth(200));
        }
    }
}