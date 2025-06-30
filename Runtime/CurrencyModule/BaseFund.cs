﻿using System;
using LSCore;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class BaseFund
{
    public abstract Id Id { get; }
    public abstract int Value { get; set; }
    public bool CanSpend => Currencies.Spend(Id, Value, out _);
    
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
        rect.TakeFromLeft(EditorGUI.indentLevel * 15);
        var boxRect = rect.TakeFromLeft(30);
        GUI.DrawTexture(boxRect, icon, ScaleMode.ScaleToFit);
        var style = SirenixGUIStyles.BoldLabel;
        var fs = style.fontSize;
        style.fontSize = 25;
        rect.TakeFromLeft(10);
        GUI.Label(rect, Value.ToString(), style); 
        style.fontSize = fs;
        rect.TakeFromLeft(80);
        return rect;
    }

#endif
}