//-----------------------------------------------------------------------
// <copyright file="AttributeExampleUtilities.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sirenix.Reflection.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public static class AttributeExampleUtilities
    {
        private static readonly CategoryComparer CategorySorter = new CategoryComparer();

        private static readonly Type[] AttributeTypes;
        private static readonly Dictionary<Type, OdinRegisterAttributeAttribute> AttributeRegisterMap;

        static AttributeExampleUtilities()
        {
            AttributeRegisterMap = AssemblyUtilities.GetAllAssemblies()
                .SelectMany(a => a.GetAttributes<OdinRegisterAttributeAttribute>(true))
                .Concat(InternalAttributeRegistry.Attributes)
                .Cast<OdinRegisterAttributeAttribute>()
                .Where(attr => OdinInspectorVersion.IsEnterprise || !attr.IsEnterprise)
                .ToDictionary(x => x.AttributeType);

            AttributeTypes = AttributeRegisterMap.Keys.ToArray();
        }

        public static IEnumerable<Type> GetAllOdinAttributes()
        {
            return AttributeTypes;
        }

        public static IEnumerable<string> GetAttributeCategories(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration) && registration.Categories != null)
            {
                // TODO: Cache this?
                return registration.Categories.Split(',').Select(x => x.Trim());
            }
            else
            {
                return new string[] { "Uncategorized" };
            }
        }

        public static string GetAttributeDescription(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration))
            {
                return registration.Description;
            }
            else
            {
                return null;
            }
        }

        public static string GetOnlineDocumentationUrl(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration))
            {
                return registration.DocumentationUrl;
            }

            return null;
        }

        public static bool GetIsEnterprise(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinRegisterAttributeAttribute registration;
            if (AttributeRegisterMap.TryGetValue(attributeType, out registration))
            {
                return registration.IsEnterprise;
            }

            return false;
        }

        public static void BuildMenuTree(OdinMenuTree tree)
        {
            foreach (var a in GetAllOdinAttributes())
            {
                // TODO: tags?
                string search = a.Name + " " + string.Join(" ", GetAttributeExampleInfos(a).Select(x => x.Name).ToArray());

                foreach (var c in GetAttributeCategories(a))
                {
                    var item = new OdinMenuItem(tree, a.GetNiceName().Replace("Attribute", "").SplitPascalCase(), a)
                    {
                        Value = a,
                        SearchString = search,
                    };
                    search = null; // Only allow the user to find the first item of an attribute by search.

                    tree.AddMenuItemAtPath(c, item);
                }
            }

            tree.MenuItems.Sort(CategorySorter);
            tree.MarkDirty();
        }

        // TODO: The names of GetAttributeExampleInfos and GetExample methods are kinda confusing
        // and doesn't clearly indicate the difference between the two methods.
        public static AttributeExampleInfo[] GetAttributeExampleInfos(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            AttributeExampleInfo[] examples;
            if (InternalAttributeExampleInfoMap.Map.TryGetValue(attributeType, out examples) == false)
            {
                examples = new AttributeExampleInfo[0];
            }

            return examples;
        }

        public static OdinAttributeExampleItem GetExample<T>() where T : Attribute
        {
            return GetExample(typeof(T));
        }

        public static OdinAttributeExampleItem GetExample(Type attributeType)
        {
            OdinRegisterAttributeAttribute registration;
            AttributeRegisterMap.TryGetValue(attributeType, out registration);
            return new OdinAttributeExampleItem(attributeType, registration);
        }

        private class CategoryComparer : IComparer<OdinMenuItem>
        {
            private static readonly Dictionary<string, int> Order = new Dictionary<string, int>()
            {
                { "Essentials", -10 },
                { "Misc", 8 },
                { "Meta", 9 },
                { "Unity", 10 },
                { "Debug", 50 },
            };

            public int Compare(OdinMenuItem x, OdinMenuItem y)
            {
                int xOrder;
                int yOrder;
                if (Order.TryGetValue(x.Name, out xOrder) == false) xOrder = 0;
                if (Order.TryGetValue(y.Name, out yOrder) == false) yOrder = 0;

                if (xOrder == yOrder)
                {
                    return x.Name.CompareTo(y.Name);
                }
                else
                {
                    return xOrder.CompareTo(yOrder);
                }
            }
        }
    }

    public class OdinAttributeExampleItem
    {
        static GUIStyle tabStyle;
        static float codeHeight = 200;

        private static GUIStyle headerGroupStyle;
        private static GUIStyle tabGroupStyle;
        private Type attributeType;
        private OdinRegisterAttributeAttribute registration;
        private AttributeExamplePreview[] examples;
        private Vector2 pos;
        private Rect dragRect;
        private AttributeExamplePreview selectedExample;

        public readonly string Name;

        public bool DrawCodeExample { get; set; }

        public OdinAttributeExampleItem(Type attributeType, OdinRegisterAttributeAttribute registration)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            this.attributeType = attributeType;
            this.registration = registration;
            this.Name = this.attributeType.GetNiceName().SplitPascalCase();
            this.DrawCodeExample = true;

            var exampleInfos = AttributeExampleUtilities.GetAttributeExampleInfos(attributeType);
            this.examples = new AttributeExamplePreview[exampleInfos.Length];
            for (int i = 0; i < exampleInfos.Length; i++)
            {
                this.examples[i] = new AttributeExamplePreview(exampleInfos[i]);
            }

            this.selectedExample = examples.FirstOrDefault();
        }

        public void Draw()
        {
            headerGroupStyle = headerGroupStyle ?? new GUIStyle()
            {
                padding = new RectOffset(10, 10, 10, 20),
            };
            tabGroupStyle = tabGroupStyle ?? new GUIStyle()
            {
                padding = new RectOffset(20, 20, 20, 20),
            };

            codeHeight -= SirenixEditorGUI.SlideRect(this.dragRect.Expand(5).AddY(2), MouseCursor.SplitResizeUpDown).y;

            var headerRect = EditorGUILayout.BeginVertical(headerGroupStyle);
            {
                EditorGUI.DrawRect(headerRect, SirenixGUIStyles.BoxBackgroundColor);

                GUILayout.Label(this.Name, SirenixGUIStyles.SectionHeader);

                if (string.IsNullOrEmpty(this.registration.DocumentationUrl) == false)
                {
                    var rect = GUILayoutUtility.GetLastRect()
                        .AlignCenterY(20)
                        .AlignRight(120);

                    if (GUI.Button(rect, "Documentation", SirenixGUIStyles.MiniButton))
                    {
                        Help.BrowseURL(this.registration.DocumentationUrl);
                    }
                }

                if (string.IsNullOrEmpty(this.registration.Description) == false)
                {
                    GUILayout.Space(10);
                    GUILayout.Label(this.registration.Description, SirenixGUIStyles.MultiLineLabel);
                }
            }
            EditorGUILayout.EndVertical();

            if (this.examples.Length == 0)
            {
                GUILayout.Label("No examples available.");
                return;
            }
            else
            {
                if (this.examples.Length > 1)
                {
                    var toolbarRect = EditorGUILayout.BeginHorizontal();

                    if (Event.current.type == EventType.Repaint)
                    {
                        EditorGUI.DrawRect(toolbarRect, SirenixGUIStyles.BoxBackgroundColor);
                        EditorGUI.DrawRect(toolbarRect.AlignTop(1), new Color(0, 0, 0, 0.3f));
                    }

                    tabStyle = tabStyle ?? new GUIStyle() { padding = new RectOffset(5, 5, 7, 7) };

                    foreach (var item in this.examples)
                    {
                        GUIContent label = GUIHelper.TempContent(" " + item.ExampleInfo.Name, GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), false), null);

                        var prev = EditorGUIUtility.GetIconSize();
                        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                        var rect = GUILayoutUtility.GetRect(label, tabStyle).Expand(0.5f);

                        if (item == this.selectedExample)
                        {
                            var bg = EditorGUIUtility.isProSkin ? SirenixGUIStyles.DarkEditorBackground : new Color(0.78f, 0.78f, 0.78f, 1f);

                            EditorGUI.DrawRect(rect, bg);
                            SirenixEditorGUI.DrawBorders(rect.Expand(1, 1, 0, 0), 1, 1, 1, 0);
                        }
                        else
                        {
                            SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1);
                            EditorGUI.DrawRect(rect.AlignRight(1), new Color(0, 0, 0, 0.3f));
                        }

                        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                        {
                            this.selectedExample = item;
                        }

                        if (this.selectedExample != item && rect.Contains(Event.current.mousePosition))
                        {
                            GUIHelper.PushColor(new Color(1, 1, 1, 0.4f));
                            EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
                            SirenixEditorGUI.DrawBorders(rect, 0, 0, 1, 0);
                            GUIHelper.PopColor();
                        }

                        var activeLabel = EditorGUIUtility.isProSkin ? SirenixGUIStyles.WhiteLabelCentered : SirenixGUIStyles.LabelCentered;
                        GUI.Label(rect, label, this.selectedExample == item ? activeLabel : SirenixGUIStyles.LabelCentered);
                        EditorGUIUtility.SetIconSize(prev);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUI.DrawRect(headerRect.AlignBottom(1), SirenixGUIStyles.BorderColor);
                }
            }

            this.pos = EditorGUILayout.BeginScrollView(this.pos, GUILayoutOptions.ExpandWidth(true));
            GUILayout.BeginVertical(tabGroupStyle);
            this.selectedExample.Draw();
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            if (Event.current.type == EventType.Repaint)
                this.dragRect = GUILayoutUtility.GetRect(0, 4);
            else
                GUILayoutUtility.GetRect(0, 4);

            this.selectedExample.DrawCode(codeHeight);
        }

        public void OnDeselected()
        {
            foreach (var example in this.examples)
            {
                example.OnDeselected();
            }
        }
    }

    internal class AttributeExamplePreview
    {
        private static GUIStyle codeTextStyle;

        public AttributeExampleInfo ExampleInfo;
        private PropertyTree tree;
        private string highlightedCode = null;
        private string highlightedCodeAsComponent = null;
        private Vector2 scrollPosition;
        //private bool showRaw;
        private bool showComponent;

        public AttributeExamplePreview(AttributeExampleInfo exampleInfo)
        {
            this.ExampleInfo = exampleInfo;

            try
            {
                this.highlightedCode = SyntaxHighlighter.Parse(this.ExampleInfo.Code);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                this.highlightedCode = this.ExampleInfo.Code;
                //this.showRaw = true;
            }

            try
            {
                this.highlightedCodeAsComponent = SyntaxHighlighter.Parse(this.ExampleInfo.CodeAsComponent);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                this.highlightedCodeAsComponent = this.ExampleInfo.CodeAsComponent;
                //this.showRaw = true;
            }
        }

        public void Draw()
        {
            var hasOdinSerializedMembers = this.ExampleInfo.ExampleType.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), false);
            var hasNote = this.ExampleInfo.Description != null || hasOdinSerializedMembers;

            if (hasNote)
            {
                GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);

                if (this.ExampleInfo.Description != null)
                {
                    GUILayout.Label(this.ExampleInfo.Description, SirenixGUIStyles.MultiLineLabel);
                }

                GUILayout.EndVertical();

                var seperatorRect = GUILayoutUtility.GetRect(0, 25);
                seperatorRect.xMin -= 20;
                seperatorRect.xMax += 20;
                seperatorRect.y += 10;
                seperatorRect.height = 5;
                SirenixEditorGUI.DrawThickHorizontalSeperator(seperatorRect);

                if (hasOdinSerializedMembers)
                {
                    SirenixEditorGUI.InfoMessageBox("Note that this example requires Odin's serialization to be enabled to work, since it uses types that Unity will not serialize. If you copy the example as a component using the 'Copy Component' or 'Create Component Script' buttons, the code will have been set up with Odin's serialization enabled already.");
                    GUILayout.Space(9);
                }

            }

            if (this.tree == null)
            {
                this.tree = PropertyTree.Create(this.ExampleInfo.PreviewObject);
            }

            GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
            this.tree.Draw(false);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
        }

        public void DrawCode(float height)
        {
            Rect rect = SirenixEditorGUI.BeginToolbarBox();
            SirenixEditorGUI.DrawSolidRect(rect, SyntaxHighlighter.BackgroundColor);
            EditorGUI.DrawRect(rect.AlignTop(1).AddY(-1), SirenixGUIStyles.BorderColor);

            SirenixEditorGUI.BeginToolbarBoxHeader();
            {
                GUILayout.Space(-4);
                if (SirenixEditorGUI.ToolbarButton(this.showComponent ? "View Shortened Code" : "View Component Code"))
                {
                    this.showComponent = !this.showComponent;
                }

                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton("Copy View"))
                {
                    if (this.showComponent)
                    {
                        Clipboard.Copy(this.ExampleInfo.CodeAsComponent);
                    }
                    else
                    {
                        Clipboard.Copy(this.ExampleInfo.Code);
                    }
                }
                if (this.ExampleInfo.CodeAsComponent != null)
                {
                    if (SirenixEditorGUI.ToolbarButton("Save Component Script"))
                    {
                        string filePath = EditorUtility.SaveFilePanelInProject("Create Component File", this.ExampleInfo.ExampleType.Name + "Component.cs", "cs", "Choose a location to save the example as a component script.");

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            File.WriteAllText(filePath, this.ExampleInfo.CodeAsComponent);
                            AssetDatabase.Refresh();
                        }

                        GUIHelper.ExitGUI(true);
                    }
                }
                GUILayout.Space(-4);

            }
            SirenixEditorGUI.EndToolbarBoxHeader();

            if (codeTextStyle == null)
            {
                codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
                codeTextStyle.normal.textColor = SyntaxHighlighter.TextColor;
                codeTextStyle.active.textColor = SyntaxHighlighter.TextColor;
                codeTextStyle.focused.textColor = SyntaxHighlighter.TextColor;
                codeTextStyle.wordWrap = false;
            }

            GUIContent codeContent = this.showComponent ?
                GUIHelper.TempContent(/*this.showRaw ? this.ExampleInfo.CodeAsComponent.TrimEnd('\n', '\r') : */this.highlightedCodeAsComponent)
                : GUIHelper.TempContent(/*this.showRaw ? this.ExampleInfo.Code.TrimEnd('\n', '\r') : */this.highlightedCode);
            Vector2 size = codeTextStyle.CalcSize(codeContent);

            GUILayout.BeginVertical();
            {
                this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, false, false, GUILayout.Height(height));
                var codeRect = GUILayoutUtility.GetRect(size.x + 50, size.y).AddXMin(4).AddY(2);
                EditorGUI.SelectableLabel(codeRect, codeContent.text, codeTextStyle);
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            SirenixEditorGUI.EndToolbarBox();
        }

        public void OnDeselected()
        {
            if (this.tree != null)
            {
                this.tree.Dispose();
                this.tree = null;
            }
        }
    }
}
#endif