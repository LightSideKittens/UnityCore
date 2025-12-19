using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UniText))]
[CanEditMultipleObjects]
public class UniTextEditor : Editor
{
    private InspectorProperty modRegistersProp;
    private UniText uniText;
    private PropertyTree propertyTree;

    private void OnEnable()
    {
        propertyTree = PropertyTree.Create(serializedObject);
        modRegistersProp = propertyTree.RootProperty.Children["modRegisters"];
    }

    private void OnDisable()
    {
        propertyTree.Dispose();
    }

    public override void OnInspectorGUI()
    {
        uniText = (UniText)target;

        DrawField("Text", uniText.Text, v => uniText.Text = v,
            () => EditorGUILayout.TextArea(uniText.Text, GUILayout.MinHeight(60)));

        Header("Font");
        DrawField("Font Asset", uniText.Font, v => uniText.Font = v);
        DrawField("Font Size", uniText.FontSize, v => uniText.FontSize = v);

        Header("Layout");
        DrawField("Base Direction", uniText.BaseDirection, v => uniText.BaseDirection = v);
        DrawField("Word Wrap", uniText.EnableWordWrap, v => uniText.EnableWordWrap = v);

        Header("Alignment");
        DrawField("Horizontal", uniText.HorizontalAlignment, v => uniText.HorizontalAlignment = v);
        DrawField("Vertical", uniText.VerticalAlignment, v => uniText.VerticalAlignment = v);

        Header("Auto Size");
        DrawField("Enable", uniText.EnableAutoSize, v => uniText.EnableAutoSize = v);
        if (uniText.EnableAutoSize)
        {
            EditorGUI.indentLevel++;
            DrawField("Min Size", uniText.MinFontSize, v => uniText.MinFontSize = v);
            DrawField("Max Size", uniText.MaxFontSize, v => uniText.MaxFontSize = v);
            EditorGUI.indentLevel--;
        }

        Header("Modifiers");
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        propertyTree.BeginDraw(true);
        modRegistersProp.Draw();
        propertyTree.EndDraw();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var t in targets)
            {
                var ut = (UniText)t;
                ut.ForceFullReinitialization();
                ut.SetDirtyAll();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void Header(string label)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    private void DrawField<T>(string label, T value, Action<T> setter, Func<T> customDraw = null)
    {
        EditorGUI.BeginChangeCheck();
        T newValue = customDraw != null ? customDraw() : DrawValue(label, value);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(uniText, $"Change {label}");
            setter(newValue);
        }
    }

    private T DrawValue<T>(string label, T value)
    {
        return value switch
        {
            string s => (T)(object)EditorGUILayout.TextField(label, s),
            float f => (T)(object)EditorGUILayout.FloatField(label, f),
            bool b => (T)(object)EditorGUILayout.Toggle(label, b),
            Enum e => (T)(object)EditorGUILayout.EnumPopup(label, e),
            UnityEngine.Object o => (T)(object)EditorGUILayout.ObjectField(label, o, typeof(T), false),
            _ => value
        };
    }
}
