using System;
using System.Collections.Generic;
using LSCore.Extensions;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;

public class SelectExDrawer : OdinAttributeDrawer<SelectExAttribute, string>
{
    private string expression;
    private int count = 10;
    private static List<string> incorrectPart = new();
    private ValueResolver<int> countResolver;
    
    protected override void Initialize()
    {
        base.Initialize();
        countResolver = ValueResolver.Get(Property, Attribute.valueGetter, count);
        expression = ValueEntry.SmartValue;
        count = EditorUtils.GetInt(Property.Tree.WeakTargets[0], count);
    }
    
    protected override void DrawPropertyLayout(GUIContent label)
    {
        if (countResolver.HasError)
        {
            countResolver.DrawError();
            return;
        }
        
        EditorGUILayout.BeginHorizontal();
        
        using (EditorUtils.SetLabelWidth(80))
        {
            expression = EditorGUILayout.TextField(label, expression);
            
            using (EditorUtils.SetFieldWidth(1))
            {
                if (Attribute.valueGetter == null)
                {
                    count = Math.Clamp(EditorGUILayout.IntField("Count", count), 0, int.MaxValue);
                }
                else
                {
                    count = countResolver.GetValue();
                    EditorGUILayout.LabelField($"Count: {count}");
                }
            }

            EditorUtils.SetInt(Property.Tree.WeakTargets[0], count);
        }
        
        EditorGUILayout.EndHorizontal();

        if (SelectEx.TryParseEveryPart(expression, count, incorrectPart))
        {
            ValueEntry.SmartValue = expression;
        }
        else
        {
            EditorGUILayout.HelpBox($"Invalid SelectEx: {string.Join(", ", incorrectPart)}", MessageType.Error);
        }
    }
}
