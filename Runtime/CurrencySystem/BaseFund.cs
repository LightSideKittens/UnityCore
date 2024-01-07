using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class BaseFund
{
    [HideIf("isControls")] 
    [ValueDropdown("Ids")]
    public Id id;
    public abstract int Value { get; set; }
    public virtual void Earn()
    {
        Currencies.Earn(id, Value);
    }

    public virtual bool Spend(out Action spend)
    {
        return Currencies.Spend(id, Value, out spend);
    }

    public static void Clear(Id id) => Currencies.Clear(id);

#if UNITY_EDITOR
    private Texture2D icon;
    [NonSerialized] public bool isControls;
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
    
    protected virtual IEnumerable<CurrencyIdGroup> Groups => AssetDatabaseUtils.LoadAllAssets<CurrencyIdGroup>();
    protected virtual IEnumerable<Id> Ids => Groups.SelectMany(group => group);

#endif
}