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

    private static bool textAreaExpand;
    private static int textAreaFontSize = 14;
    private static GUIStyle textAreaStyle;

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

        BeginSection("Text");
        EditorGUILayout.BeginHorizontal();
        textAreaExpand = EditorGUILayout.ToggleLeft("Expand", textAreaExpand, GUILayout.Width(60));
        EditorGUILayout.LabelField("Size", GUILayout.Width(30));
        textAreaFontSize = EditorGUILayout.IntSlider(textAreaFontSize, 8, 24);
        EditorGUILayout.EndHorizontal();

        if (textAreaStyle == null || textAreaStyle.fontSize != textAreaFontSize)
            textAreaStyle = new GUIStyle(EditorStyles.textArea) { fontSize = textAreaFontSize };

        DrawField(null, uniText.Text, v => uniText.Text = v, () =>
        {
            if (textAreaExpand)
                return EditorGUILayout.TextArea(uniText.Text, textAreaStyle, GUILayout.ExpandHeight(true));
            return EditorGUILayout.TextArea(uniText.Text, textAreaStyle, GUILayout.MinHeight(60));
        });
        EndSection();

        BeginSection("Font");
        DrawField("Font Asset", uniText.Font, v => uniText.Font = v);
        DrawField("Font Size", uniText.FontSize, v => uniText.FontSize = v);
        EndSection();

        BeginSection("Layout");
        DrawField("Base Direction", uniText.BaseDirection, v => uniText.BaseDirection = v);
        DrawField("Word Wrap", uniText.EnableWordWrap, v => uniText.EnableWordWrap = v);
        EndSection();

        BeginSection("Alignment");
        DrawField("Horizontal", uniText.HorizontalAlignment, v => uniText.HorizontalAlignment = v);
        DrawField("Vertical", uniText.VerticalAlignment, v => uniText.VerticalAlignment = v);
        EndSection();

        BeginSection("Auto Size");
        DrawField("Enable", uniText.EnableAutoSize, v => uniText.EnableAutoSize = v);
        if (uniText.EnableAutoSize)
        {
            DrawField("Min Size", uniText.MinFontSize, v => uniText.MinFontSize = v);
            DrawField("Max Size", uniText.MaxFontSize, v => uniText.MaxFontSize = v);
        }
        EndSection();

        BeginSection("Modifiers");
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
        EndSection();
    }

    private void BeginSection(string label)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    private void EndSection()
    {
        EditorGUILayout.EndVertical();
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
