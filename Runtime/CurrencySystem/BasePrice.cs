using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class BasePrice
{
    public int value;
    public abstract void Earn();
    public abstract bool TrySpend();

#if UNITY_EDITOR
    private Texture2D icon;
    private string test;
    public virtual bool Editor_Draw()
    {
        icon ??= AssetDatabase.LoadAssetAtPath<Texture2D>(GetType().GetGenericArguments()[0].GetAttribute<IconAttribute>().path);
        EditorGUILayout.BeginHorizontal();
        var rect = EditorGUILayout.GetControlRect(GUILayout.Height(30));
        var boxRect = rect.TakeFromLeft(30);
        GUI.DrawTexture(boxRect, icon, ScaleMode.ScaleToFit);
        var textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = 25
        };
        EditorGUI.BeginChangeCheck();
        value = EditorGUI.IntField(rect, value, textAreaStyle);
        var isChanged = EditorGUI.EndChangeCheck();
        EditorGUILayout.EndHorizontal();
        return isChanged;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is BasePrice drawer)
        {
            return Equals(drawer);
        }

        return false;
    }
        
    public bool Equals(BasePrice other) => GetType() == other.GetType();

    public override int GetHashCode() => GetType().GetHashCode();
    
#endif
}