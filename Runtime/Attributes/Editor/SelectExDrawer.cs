using System.Collections.Generic;
using LSCore.Extensions;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class SelectExDrawer : OdinAttributeDrawer<SelectExAttribute, string>
{
    private string expression;
    private bool show;
    private static GUIStyle style;
    private static List<string> incorrectPart = new();
    
    protected override void Initialize()
    {
        base.Initialize();
        expression = ValueEntry.SmartValue;
    }
    
    protected override void DrawPropertyLayout(GUIContent label)
    {
        Draw(label, ref expression);

        if (SelectEx.TryParseEveryPart(expression, incorrectPart))
        {
            ValueEntry.SmartValue = expression;
        }
        else
        {
            EditorGUILayout.HelpBox($"Invalid SelectEx: {string.Join(", ", incorrectPart)}", MessageType.Error);
        }
    }

    private void Draw(GUIContent label, ref string val)
    {
        val = EditorGUILayout.TextField(label, val);
    }
}
