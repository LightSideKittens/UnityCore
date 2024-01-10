using System;
using LSCore;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class BaseFund
{
    public abstract Id Id { get; }
    public abstract int Value { get; set; }
    public virtual void Earn()
    {
        Currencies.Earn(Id, Value);
    }

    public virtual bool Spend(out Action spend)
    {
        return Currencies.Spend(Id, Value, out spend);
    }

#if UNITY_EDITOR
    private Texture2D icon;
    protected abstract void SetIcon(ref Texture2D icon);
    private int Editor_Draw(int value, GUIContent _)
    {
        EditorGUILayout.BeginHorizontal();
        var rect = DrawIcon();
        var textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = 25
        };
        value = EditorGUI.IntField(rect, value, textAreaStyle);
        EditorGUILayout.EndHorizontal();
        return value;
    }

    protected Rect DrawIcon()
    {
        SetIcon(ref icon);
        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(30));
        var boxRect = rect.TakeFromLeft(60);
        GUI.DrawTexture(boxRect, icon, ScaleMode.ScaleToFit);
        return rect;
    }

#endif
}