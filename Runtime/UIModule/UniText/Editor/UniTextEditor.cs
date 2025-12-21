using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
        propertyTree.OnPropertyValueChanged += OnPropChanged;
    }

    private void OnPropChanged(InspectorProperty property, int selectionIndex)
    {
        if (property.SerializationRoot.Children["modRegisters"] == modRegistersProp)
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var t in targets)
            {
                var ut = (UniText)t;
                ut.ReInitModifiers();
            }
        }
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
        EditorGUILayout.LabelField("Expand", GUILayout.Width(50));
        textAreaExpand = EditorGUILayout.Toggle(textAreaExpand, GUILayout.Width(25));
        EditorGUILayout.LabelField("Size", GUILayout.Width(50));
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
        DrawField("Fonts", uniText.Fonts, v => uniText.Fonts = v);
        DrawField("Appearance", uniText.Appearance, v => uniText.Appearance = v);
        DrawField("Font Size", uniText.FontSize, v => uniText.FontSize = v);
        DrawField("Enable Auto Size", uniText.EnableAutoSize, v => uniText.EnableAutoSize = v);
        if (uniText.EnableAutoSize)
        {
            DrawField("Min Size", uniText.MinFontSize, v => uniText.MinFontSize = v);
            DrawField("Max Size", uniText.MaxFontSize, v => uniText.MaxFontSize = v);
        }
        DrawField("Color", uniText.color, v => uniText.color = v);
        EndSection();

        BeginSection("Layout");
        DrawField("Base Direction", uniText.BaseDirection, v => uniText.BaseDirection = v);
        DrawField("Word Wrap", uniText.EnableWordWrap, v => uniText.EnableWordWrap = v);
        EditorGUILayout.Space(4);
        DrawAlignmentButtons();
        EndSection();

        BeginSection("Modifiers");
        serializedObject.Update();
        propertyTree.BeginDraw(true);
        modRegistersProp.Draw();
        propertyTree.EndDraw();
        serializedObject.ApplyModifiedProperties();
        EndSection();

        var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("Made with ❤️ by Light Side", style);
        EditorGUILayout.Space(-4);
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

    private static GUIContent[] alignIcons;
    private static GUIStyle alignButtonStyle;
    private static GUIStyle alignButtonSelectedStyle;

    private void LoadAlignIcons()
    {
        if (alignIcons != null) return;
        var script = MonoScript.FromScriptableObject(this);
        var scriptPath = AssetDatabase.GetAssetPath(script);
        var folder = scriptPath.Substring(0, scriptPath.LastIndexOf('/') + 1);

        alignIcons = new GUIContent[6];
        var names = new[] { "left-align", "h-center-align", "right-align", "top-align", "middle-align", "bottom-align" };
        for (var i = 0; i < 6; i++)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + names[i] + ".png");
            alignIcons[i] = tex != null ? new GUIContent(tex) : new GUIContent(names[i][0].ToString().ToUpper());
        }
    }

    private static Texture2D MakeTex(Color col)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }
    
    private void DrawAlignmentButtons()
    {
        LoadAlignIcons();
        if (alignButtonStyle == null)
        {
            alignButtonStyle = new GUIStyle(EditorStyles.miniButton) { fixedHeight = 26 };
            alignButtonSelectedStyle = new GUIStyle(alignButtonStyle);
            var selTex = MakeTex(new Color(0.29f, 0.59f, 0.32f));
            var deselTex = MakeTex(new Color(0.3f, 0.3f, 0.38f));
            alignButtonSelectedStyle.normal.background = selTex;
            alignButtonSelectedStyle.hover.background = selTex;
            alignButtonStyle.normal.background = deselTex;
            alignButtonStyle.hover.background = deselTex;
        }

        EditorGUILayout.LabelField("Alignment", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        var h = uniText.HorizontalAlignment;
        if (DrawAlignButton(0, h == HorizontalAlignment.Left)) h = HorizontalAlignment.Left;
        if (DrawAlignButton(1, h == HorizontalAlignment.Center)) h = HorizontalAlignment.Center;
        if (DrawAlignButton(2, h == HorizontalAlignment.Right)) h = HorizontalAlignment.Right;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(uniText, "Change Horizontal Alignment");
            uniText.HorizontalAlignment = h;
        }

        GUILayout.Space(8);

        EditorGUI.BeginChangeCheck();
        var v = uniText.VerticalAlignment;
        if (DrawAlignButton(3, v == VerticalAlignment.Top)) v = VerticalAlignment.Top;
        if (DrawAlignButton(4, v == VerticalAlignment.Middle)) v = VerticalAlignment.Middle;
        if (DrawAlignButton(5, v == VerticalAlignment.Bottom)) v = VerticalAlignment.Bottom;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(uniText, "Change Vertical Alignment");
            uniText.VerticalAlignment = v;
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private bool DrawAlignButton(int index, bool isSelected)
    {
        var style = isSelected ? alignButtonSelectedStyle : alignButtonStyle;
        return GUILayout.Button(alignIcons[index], style, GUILayout.Width(30));
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
        var newValue = value switch
        {
            string s => (T)(object)EditorGUILayout.TextField(label, s),
            float f => (T)(object)EditorGUILayout.FloatField(label, f),
            bool b => (T)(object)EditorGUILayout.Toggle(label, b),
            Enum e => (T)(object)EditorGUILayout.EnumPopup(label, e),
            Color c => (T)(object)EditorGUILayout.ColorField(label, c),
            _ => default,
        };

        if (typeof(Object).IsAssignableFrom(typeof(T)))
        {
            var obj = value as Object;
            return (T)(object)EditorGUILayout.ObjectField(label, obj, typeof(T), false);
        }
        
        return newValue;
    }
}
