#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class GenerateGuidDrawer : OdinAttributeDrawer<GenerateGuidAttribute, string>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var value = ValueEntry.SmartValue;
        
        SirenixEditorGUI.BeginHorizontalToolbar();
        GUILayout.Label(label, GUILayout.Width(20));
        
        GUILayout.FlexibleSpace();
        GUILayout.Label(value, GUILayout.Width(250));
        if (SirenixEditorGUI.IconButton(EditorIcons.Refresh)
            || !Guid.TryParse(value, out _))
        {
            value = Guid.NewGuid().ToString("N");
            ValueEntry.SmartValue = value;
        }
        
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}

#endif