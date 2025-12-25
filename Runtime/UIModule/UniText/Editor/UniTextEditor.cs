using System;
using System.Collections.Generic;
using System.Reflection;
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
    private static GUIStyle textAreaStyle = null;
    private static bool enableHighlight = true;
    
    private static readonly Color32[] tagColors =
    {
        new(102, 187, 255, 255),
        new(91, 255, 186, 255),
        new(255, 251, 93, 255),
        new(255, 179, 99, 255),
        new(255, 146, 248, 255),
        new(255, 114, 107, 255),
        new(150, 88, 255, 255),
        new(72, 139, 255, 255),
        new(113, 255, 87, 255),
    };
    
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
                cachedText = string.Empty;
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
        EditorGUILayout.LabelField("Highlight", GUILayout.Width(60));
        enableHighlight = EditorGUILayout.Toggle(enableHighlight, GUILayout.Width(25));
        EditorGUILayout.LabelField("Size", GUILayout.Width(50));
        textAreaFontSize = EditorGUILayout.IntSlider(textAreaFontSize, 8, 24);
        EditorGUILayout.EndHorizontal();

        if (textAreaStyle == null || textAreaStyle.fontSize != textAreaFontSize)
        {
            textAreaStyle = new GUIStyle(EditorStyles.textArea) { fontSize = textAreaFontSize };
        }

        DrawField(null, uniText.Text, v => uniText.Text = v, () =>
        {
            var option = textAreaExpand ? GUILayout.ExpandHeight(true) : GUILayout.Height(72 * (textAreaFontSize / 14f));
            
            textScrollPos = EditorGUILayout.BeginScrollView(textScrollPos, option);
            
            var result = EditorGUILayout.TextArea(uniText.Text, textAreaStyle, GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint && enableHighlight)
            {
                lastTextAreaRect = GUILayoutUtility.GetLastRect();
                HighlightTags(uniText.Text);
            }

            EditorGUILayout.EndScrollView();

            return result;
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
            GUI.enabled = false;
            EditorGUILayout.FloatField("Font Size", uniText.CurrentFontSize);
            GUI.enabled = true;
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
    
    private static FieldInfo modRegistersField;

    private readonly PooledList<ParsedRange> tempRanges = new(32);
    private readonly List<(int start, int end, int colorIndex)> highlightRanges = new();
    private static readonly Color tagColor = new(0.4f, 0.7f, 1f, 1f);
    private static readonly Color defaultColor = new(0.82f, 0.82f, 0.82f, 1f);
    private Rect lastTextAreaRect;

    private string cachedText;
    private int cachedTextHash;
    private Rect cachedRect;
    private Vector2 textScrollPos;

    private static GUIStyle charStyle;

    private void HighlightTags(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var textHash = text.GetHashCode();
        var needRebuild = cachedText != text || cachedTextHash != textHash;

        if (needRebuild)
        {
            CollectHighlightRanges(text);
            cachedText = text;
            cachedTextHash = textHash;
        }

        DrawCharLabels(text);
    }

    private void CollectHighlightRanges(string text)
    {
        highlightRanges.Clear();

        var modRegs = modRegistersProp?.ValueEntry?.WeakSmartValue as List<ModRegister>;

        var colorIndex = 0;
        if (modRegs != null)
        {
            for (var m = 0; m < modRegs.Count; m++)
            {
                var reg = modRegs[m];
                if (reg?.rule == null) continue;

                tempRanges.Clear();
                reg.rule.Reset();
                var idx = 0;
                while (idx < text.Length)
                {
                    var newIdx = reg.rule.TryMatch(text, idx, tempRanges);
                    idx = newIdx > idx ? newIdx : idx + 1;
                }
                reg.rule.Finalize(text, tempRanges);

                for (var i = 0; i < tempRanges.Count; i++)
                {
                    var range = tempRanges[i];
                    if (range.HasTags)
                    {
                        if (range.tagStart < range.tagEnd)
                            highlightRanges.Add((range.tagStart, range.tagEnd, colorIndex));
                        if (range.closeTagStart < range.closeTagEnd)
                            highlightRanges.Add((range.closeTagStart, range.closeTagEnd, colorIndex));
                    }
                    else if (range.start < range.end)
                    {
                        highlightRanges.Add((range.start, range.end, colorIndex));
                    }
                    colorIndex++;
                }
            }
        }
    }

    private void DrawCharLabels(string text)
    {
        if (charStyle == null || charStyle.fontSize != textAreaFontSize)
        {
            charStyle = new GUIStyle
            {
                fontSize = textAreaFontSize,
                font = textAreaStyle.font,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.UpperLeft,
                normal = { background = null }
            };
        }

        if (highlightRanges.Count == 0) return;

        var content = new GUIContent(text);
        var lineHeight = textAreaStyle.lineHeight;

        foreach (var (start, end, colorIndex) in highlightRanges)
        {
            var color = tagColors[colorIndex % tagColors.Length];
            charStyle.normal.textColor = color;

            for (var i = start; i < end && i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\n' || c == '\r') continue;

                var pos = textAreaStyle.GetCursorPixelPosition(lastTextAreaRect, content, i);
                var nextPos = textAreaStyle.GetCursorPixelPosition(lastTextAreaRect, content, i + 1);
                var charWidth = nextPos.x - pos.x;

                var charRect = new Rect(pos.x, pos.y, charWidth, lineHeight);
                GUI.Label(charRect, c.ToString(), charStyle);
            }
        }
    }
}
