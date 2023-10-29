using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class BasePrice
{
    [CustomValueDrawer("Editor_Draw")] public int value;
    public abstract void Earn();
    public abstract bool Spend(out Action spend);

#if UNITY_EDITOR
    private Texture2D icon;
    [NonSerialized] public bool isControls;
    protected abstract void SetIcon(ref Texture2D icon);
    protected virtual int Editor_Draw(int value, GUIContent _)
    {
        SetIcon(ref icon);
        EditorGUILayout.BeginHorizontal();
        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(30));
        var boxRect = rect.TakeFromLeft(30);
        GUI.DrawTexture(boxRect, icon, ScaleMode.ScaleToFit);
        var textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = 25
        };
        value = EditorGUI.IntField(rect, value, textAreaStyle);
        EditorGUILayout.EndHorizontal();
        return value;
    }
    
#endif
}