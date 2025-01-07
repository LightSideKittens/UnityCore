#if UNITY_EDITOR

using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class GenerateGuidDrawer : OdinAttributeDrawer<GenerateGuidAttribute, string>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var value = ValueEntry.SmartValue;

        if (Attribute.Hide)
        {
            if (!Guid.TryParse(value, out _))
            {
                Generate();
            }
            return;
        }
        
        SirenixEditorGUI.BeginIndentedVertical();
        SirenixEditorGUI.BeginHorizontalToolbar();
        if (label != null)
        {
            GUILayout.Label(label, GUILayout.Width(20));
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label(value, GUILayout.Width(250));
        if (SirenixEditorGUI.IconButton(EditorIcons.Refresh)
            || !Guid.TryParse(value, out _))
        {
            Generate();
        }

        SirenixEditorGUI.EndHorizontalToolbar();
        SirenixEditorGUI.EndIndentedVertical();

        void Generate()
        {
            value = Guid.NewGuid().ToString("N");
            ValueEntry.SmartValue = value;
        }
    }
}

#endif