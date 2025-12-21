using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    private static readonly Regex richTextTagRegex = new(@"</?[a-zA-Z][^>]*>", RegexOptions.Compiled);

    private readonly PooledList<ParsedRange> tempRanges = new(32);
    private readonly List<(int start, int end, int colorIndex)> highlightRanges = new(32);
    private readonly List<(int start, int end)> noparseOnlyRanges = new(32);
    private readonly StringBuilder highlightBuilder = new(256);

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
        EditorGUILayout.LabelField("Highlight", GUILayout.Width(60));
        enableHighlight = EditorGUILayout.Toggle(enableHighlight, GUILayout.Width(25));
        EditorGUILayout.LabelField("Size", GUILayout.Width(30));
        textAreaFontSize = EditorGUILayout.IntSlider(textAreaFontSize, 8, 24);
        EditorGUILayout.EndHorizontal();

        if (textAreaStyle == null || textAreaStyle.fontSize != textAreaFontSize || textAreaStyle.richText != enableHighlight)
            textAreaStyle = new GUIStyle(EditorStyles.textArea) { fontSize = textAreaFontSize, richText = enableHighlight };

        DrawHighlightedTextArea();
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

    private const string TextAreaControlName = "UniTextEditorTextArea";
    private bool needsRefocus;
    private bool needSetup;
    private int savedCursorIndex;
    private int savedSelectIndex;
    private TextEditor editor;
    
    private void DrawHighlightedTextArea()
    {
        var text = uniText.Text ?? "";
        var displayText = enableHighlight ? BuildHighlightedText(text) : text;
        
        GUI.SetNextControlName(TextAreaControlName);
        EditorGUI.BeginChangeCheck();

        string newText;
        if (textAreaExpand)
            newText = EditorGUILayout.TextArea(displayText, textAreaStyle, GUILayout.ExpandHeight(true));
        else
            newText = EditorGUILayout.TextArea(displayText, textAreaStyle, GUILayout.MinHeight(60));
        
        if (editor != null && needSetup && editor.hasSelection)
        {
            editor.DetectFocusChange();
            editor.cursorIndex = savedCursorIndex;
            editor.selectIndex = savedSelectIndex; 
            needSetup = false;
        }
        
        if (editor != null && needsRefocus)
        {
            if (Event.current.type == EventType.Repaint)
            { 
                EditorGUI.FocusTextInControl(TextAreaControlName);
                needsRefocus = false;
                needSetup = true;
            }
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            var activeField = typeof(EditorGUI).GetField("activeEditor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            editor = activeField?.GetValue(null) as TextEditor;
            
            if (editor != null)
            {
                savedCursorIndex = editor.cursorIndex;
                savedSelectIndex = editor.selectIndex;
            }

            Undo.RecordObject(uniText, "Change Text");
            uniText.Text = enableHighlight ? StripColorTags(newText) : newText;
            
            GUIUtility.keyboardControl = 0;
            needsRefocus = true;
        }
    }

    private string BuildHighlightedText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        CollectHighlightRanges(text);
        if (highlightRanges.Count == 0 && noparseOnlyRanges.Count == 0) return text;

        var allRanges = new List<(int start, int end, int colorIndex, bool noparseOnly)>();
        for (var i = 0; i < highlightRanges.Count; i++)
        {
            var (start, end, colorIndex) = highlightRanges[i];
            allRanges.Add((start, end, colorIndex, false));
        }
        for (var i = 0; i < noparseOnlyRanges.Count; i++)
        {
            var (start, end) = noparseOnlyRanges[i];
            allRanges.Add((start, end, -1, true));
        }
        allRanges.Sort((a, b) => a.start.CompareTo(b.start));

        highlightBuilder.Clear();
        var lastEnd = 0;

        for (var i = 0; i < allRanges.Count; i++)
        {
            var (start, end, colorIndex, noparseOnly) = allRanges[i];
            if (start < lastEnd) continue;

            if (start > lastEnd)
                highlightBuilder.Append(text, lastEnd, start - lastEnd);

            if (noparseOnly)
            {
                highlightBuilder.Append("<noparse>");
                highlightBuilder.Append(text, start, end - start);
                highlightBuilder.Append("</noparse>");
            }
            else
            {
                var colorHex = ColorUtility.ToHtmlStringRGB(tagColors[colorIndex % tagColors.Length]);
                highlightBuilder.Append("<color=#").Append(colorHex).Append("><noparse>");
                highlightBuilder.Append(text, start, end - start);
                highlightBuilder.Append("</noparse></color>");
            }
            lastEnd = end;
        }

        if (lastEnd < text.Length)
            highlightBuilder.Append(text, lastEnd, text.Length - lastEnd);

        return highlightBuilder.ToString();
    }

    private void CollectHighlightRanges(string text)
    {
        highlightRanges.Clear();
        noparseOnlyRanges.Clear();

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
                reg.rule.Finalize(text.Length, tempRanges);

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

        var matches = richTextTagRegex.Matches(text);
        foreach (Match match in matches)
        {
            var start = match.Index;
            var end = start + match.Length;
            if (!IsRangeCovered(start, end))
                noparseOnlyRanges.Add((start, end));
        }
    }

    private bool IsRangeCovered(int start, int end)
    {
        for (var i = 0; i < highlightRanges.Count; i++)
        {
            var (hStart, hEnd, _) = highlightRanges[i];
            if (hStart <= start && end <= hEnd)
                return true;
        }
        return false;
    }

    private static readonly Regex stripRegex = new(@"<color=#[A-Fa-f0-9]+><noparse>|</noparse></color>|<noparse>|</noparse>", RegexOptions.Compiled);

    private static string StripColorTags(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return stripRegex.Replace(text, "");
    }
}
