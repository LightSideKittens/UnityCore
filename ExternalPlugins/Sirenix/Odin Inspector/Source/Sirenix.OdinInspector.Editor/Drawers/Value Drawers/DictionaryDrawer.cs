//-----------------------------------------------------------------------
// <copyright file="DictionaryDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Reflection.Editor;

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using SerializationUtility = Sirenix.Serialization.SerializationUtility;

    /// <summary>
    /// Property drawer for <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    public class DictionaryDrawer<TDictionary, TKey, TValue> : OdinValueDrawer<TDictionary>, IDisposable where TDictionary : IDictionary<TKey, TValue>
    {
        private const string CHANGE_ID = "DICTIONARY_DRAWER";
        private static readonly bool KeyIsValueType = typeof(TKey).IsValueType;
        private static GUIStyle listItemStyle;
        private GUIPagingHelper paging = new GUIPagingHelper();
        private GeneralDrawerConfig config;
        private LocalPersistentContext<float> keyWidthOffset;
        private bool showAddKeyGUI = false;
        private bool? newKeyIsValid;
        private string newKeyErrorMessage;
        private TKey newKey;
        private TValue newValue;
        private StrongDictionaryPropertyResolver<TDictionary, TKey, TValue> dictionaryResolver;
        //private GUIContent label;
        private DictionaryDrawerSettings attrSettings;
        private bool disableAddKey;
        private GUIContent keyLabel;
        private GUIContent emptySpaceLabel;
        private GUIContent valueLabel;
        private float keyLabelWidth;
#pragma warning disable CS0414
        // TODO: Is this field necessary? Last time I changed something in the DictionaryDrawer I broke stuff, so this time I'm asking first.
        private float valueLabelWidth;
#pragma warning restore CS0414
        private TempKeyValuePair<TKey, TValue> tempKeyValue;
        private PropertyTree keyEntryPropertyTree;
        private IPropertyValueEntry<TKey> tempKeyEntry;
        private IPropertyValueEntry<TValue> tempValueEntry;
        private MultiCollectionFilter<StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>> filter;

        private static GUIStyle foldoutHeaderStyle;

        private static GUIStyle FoldoutHeaderStyle
        {
            get
            {
                if (foldoutHeaderStyle == null)
                {
                    foldoutHeaderStyle = new GUIStyle
                    {
                        padding = new RectOffset(4, 4, 2, 4)
                    };
                }

                return foldoutHeaderStyle;
            }
        }

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.ChildResolver is StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
        }

        protected override void Initialize()
        {
            var resolver = this.Property.ChildResolver as StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
            this.filter = new MultiCollectionFilter<StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>>(this.Property, resolver);
            
            listItemStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(7, 20, 3, 3)
            };

            var entry = this.ValueEntry;

            //this.label = this.Property.Label ?? new GUIContent(typeof(TDictionary).GetNiceName());
            this.attrSettings = entry.Property.GetAttribute<DictionaryDrawerSettings>() ?? new DictionaryDrawerSettings();
            this.keyWidthOffset = this.GetPersistentValue<float>("KeyColumnWidth", this.attrSettings.KeyColumnWidth);
            this.disableAddKey = entry.Property.Tree.PrefabModificationHandler.HasPrefabs && entry.SerializationBackend == SerializationBackend.Odin && !entry.Property.SupportsPrefabModifications;
            this.keyLabel = new GUIContent(this.attrSettings.KeyLabel);
            this.valueLabel = new GUIContent(this.attrSettings.ValueLabel);
            this.emptySpaceLabel = new GUIContent(" ");
            this.keyLabelWidth = EditorStyles.label.CalcSize(this.keyLabel).x + 20;
            this.valueLabelWidth = EditorStyles.label.CalcSize(this.valueLabel).x + 20;

            if (!this.disableAddKey)
            {
                this.tempKeyValue = new TempKeyValuePair<TKey, TValue>();
                this.keyEntryPropertyTree = PropertyTree.Create(this.tempKeyValue);
                keyEntryPropertyTree.UpdateTree();
                this.tempKeyEntry = (IPropertyValueEntry<TKey>)keyEntryPropertyTree.GetPropertyAtPath("Key").ValueEntry;
                this.tempValueEntry = (IPropertyValueEntry<TValue>)keyEntryPropertyTree.GetPropertyAtPath("Value").ValueEntry;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            this.dictionaryResolver = entry.Property.ChildResolver as StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
            this.config = GeneralDrawerConfig.Instance;
            this.paging.NumberOfItemsPerPage = this.config.NumberOfItemsPrPage;
            listItemStyle.padding.right = !entry.IsEditable || this.attrSettings.IsReadOnly ? 4 : 20;

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyMargin);
            {
                this.paging.Update(elementCount: this.filter.GetCount());
                this.DrawToolbar(entry, label);
                this.paging.Update(elementCount: this.filter.GetCount());

                if (!this.disableAddKey && this.attrSettings.IsReadOnly == false)
                {
                    this.DrawAddKey(entry);
                }

                float t;
                GUIHelper.BeginLayoutMeasuring();
                if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry.Property, this), this.Property.State.Expanded, out t))
                {
                    var rect = SirenixEditorGUI.BeginVerticalList(false);
                    if (this.attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
                    {
                        var maxWidth = rect.width - 90;
                        rect.xMin = rect.xMin + this.keyWidthOffset.Value + 8;
                        rect.xMax = rect.xMin + 10;

                        GUIHelper.PushGUIEnabled(true);
                        this.keyWidthOffset.Value = this.keyWidthOffset.Value + SirenixEditorGUI.SlideRect(rect).x;
                        GUIHelper.PopGUIEnabled();

                        if (Event.current.type == EventType.Repaint)
                        {
                            this.keyWidthOffset.Value = Mathf.Clamp(this.keyWidthOffset.Value, 30, maxWidth);
                        }

                        if (this.paging.ElementCount != 0)
                        {
                            var headerRect = SirenixEditorGUI.BeginListItem(false);
                            {
                                GUILayout.Space(14);
                                if (Event.current.type == EventType.Repaint)
                                {
                                    GUI.Label(headerRect.SetWidth(this.keyWidthOffset.Value + 13), this.keyLabel, SirenixGUIStyles.LabelCentered);
                                    GUI.Label(headerRect.AddXMin(this.keyWidthOffset.Value + 13), this.valueLabel, SirenixGUIStyles.LabelCentered);
                                    SirenixEditorGUI.DrawSolidRect(headerRect.AlignBottom(1), SirenixGUIStyles.BorderColor);
                                }
                            }
                            SirenixEditorGUI.EndListItem();
                        }
                    }

                    GUIHelper.PushHierarchyMode(false);
                    this.DrawElements(entry, label);
                    GUIHelper.PopHierarchyMode();
                    SirenixEditorGUI.EndVerticalList();
                }
                SirenixEditorGUI.EndFadeGroup();

                // Draw borders
                var outerRect = GUIHelper.EndLayoutMeasuring();
                if (t > 0.01f && Event.current.type == EventType.Repaint)
                {
                    Color col = SirenixGUIStyles.BorderColor;
                    outerRect.yMin -= 1;
                    SirenixEditorGUI.DrawBorders(outerRect, 1, col);
                    col.a *= t;
                    if (this.attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
                    {
                        // Draw Slide Rect Border
                        outerRect.width = 1;
                        outerRect.x += this.keyWidthOffset.Value + 13;
                        SirenixEditorGUI.DrawSolidRect(outerRect, col);
                    }
                }
            }
            SirenixEditorGUI.EndIndentedVertical();
        }

        private void DrawAddKey(IPropertyValueEntry<TDictionary> entry)
        {
            if (entry.IsEditable == false || this.attrSettings.IsReadOnly)
            {
                return;
            }

            if (SirenixEditorGUI.BeginFadeGroup(this, this.showAddKeyGUI))
            {
                var rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
                EditorGUI.DrawRect(rect, SirenixGUIStyles.BoxBackgroundColor);
                SirenixEditorGUI.DrawBorders(rect, 1, 1, 0, 0);
                {
                    if (typeof(TKey) == typeof(string) && this.newKey == null)
                    {
                        this.newKey = (TKey)(object)"";
                        this.newKeyIsValid = null;
                    }

                    if (this.newKeyIsValid == null)
                    {
                        this.newKeyIsValid = CheckKeyIsValid(entry, this.newKey, out this.newKeyErrorMessage);
                    }

                    this.tempKeyEntry.Property.Tree.BeginDraw(false);

                    // Key
                    {
                        //this.TempKeyValue.key = this.NewKey;
                        this.tempKeyEntry.Property.Update();

                        EditorGUI.BeginChangeCheck();

                        this.tempKeyEntry.Property.Draw(this.keyLabel);

                        bool changed1 = EditorGUI.EndChangeCheck();
                        bool changed2 = this.tempKeyEntry.ApplyChanges();

                        if (changed1 || changed2)
                        {
                            this.newKey = this.tempKeyValue.Key;
                            UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                            GUIHelper.RequestRepaint();
                        }
                    }

                    // Value
                    {
                        //this.TempKeyValue.value = this.NewValue;
                        this.tempValueEntry.Property.Update();
                        this.tempValueEntry.Property.Draw(this.valueLabel);
                        this.tempValueEntry.ApplyChanges();
                        this.newValue = this.tempKeyValue.Value;
                    }

                    this.tempKeyEntry.Property.Tree.InvokeDelayedActions();
                    var changed = this.tempKeyEntry.Property.Tree.ApplyChanges();

                    if (changed)
                    {
                        this.newKey = this.tempKeyValue.Key;
                        UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                        GUIHelper.RequestRepaint();
                    }

                    this.tempKeyEntry.Property.Tree.EndDraw();

                    GUIHelper.PushGUIEnabled(GUI.enabled && this.newKeyIsValid.Value);
                    if (GUILayout.Button(this.newKeyIsValid.Value ? "Add" : this.newKeyErrorMessage))
                    {
                        var keys = new object[entry.ValueCount];
                        var values = new object[entry.ValueCount];

                        for (int i = 0; i < keys.Length; i++)
                        {
                            keys[i] = SerializationUtility.CreateCopy(this.newKey);
                        }

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = SerializationUtility.CreateCopy(this.newValue);
                        }

                        this.dictionaryResolver.QueueSet(keys, values);
                        UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;
                        GUIHelper.RequestRepaint();

                        entry.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            this.newValue = default(TValue);
                            this.tempKeyValue.Value = default(TValue);
                            this.tempValueEntry.Update();
                        });
                    }
                    GUIHelper.PopGUIEnabled();
                }
                EditorGUILayout.EndVertical();
            }
            SirenixEditorGUI.EndFadeGroup();
        }

        private void DrawToolbar(IPropertyValueEntry<TDictionary> entry, GUIContent label)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (entry.ListLengthChangedFromPrefab) GUIHelper.PushIsBoldLabel(true);

                if (this.paging.ElementCount == 0)
                {
                    if (label != null)
                    {
                        GUILayout.Label(label, GUILayoutOptions.ExpandWidth(false));
                    }
                }
                else
                {
                    var newState = label != null ? SirenixEditorGUI.Foldout(this.Property.State.Expanded, label)
                        : SirenixEditorGUI.Foldout(this.Property.State.Expanded, "");
                    if (!newState && this.Property.State.Expanded)
                    {
                        this.showAddKeyGUI = false;
                    }
                    this.Property.State.Expanded = newState;
                }

                if (entry.ListLengthChangedFromPrefab) GUIHelper.PopIsBoldLabel();

                GUILayout.FlexibleSpace();
                
                this.filter.Draw();

                // Item Count
                if (this.config.ShowItemCount)
                {
                    if (entry.ValueState == PropertyValueState.CollectionLengthConflict)
                    {
                        int min = entry.Values.Min(x => x.Count);
                        int max = entry.Values.Max(x => x.Count);

                        var lbl = min + " / " + max + " items";
                        var lblWidth = EditorStyles.centeredGreyMiniLabel.CalcWidth(lbl) + 8;
                        var r = GUILayoutUtility.GetRect(lblWidth, 18f, GUILayoutOptions.ExpandWidth().ExpandHeight());
                        GUI.Label(r, lbl, EditorStyles.centeredGreyMiniLabel);
                    }
                    else
                    {
                        var lbl = this.paging.ElementCount == 0 ? "Empty" : this.paging.ElementCount + " items";
                        var lblWidth = EditorStyles.centeredGreyMiniLabel.CalcWidth(lbl) + 8;
                        var r = GUILayoutUtility.GetRect(lblWidth, 18f, GUILayoutOptions.ExpandWidth().ExpandHeight());
                        GUI.Label(r.SubY(1), lbl, EditorStyles.centeredGreyMiniLabel);
                    }
                }

                bool hidePaging =
                        this.config.HidePagingWhileCollapsed && this.Property.State.Expanded == false ||
                        this.config.HidePagingWhileOnlyOnePage && this.paging.PageCount == 1;

                if (!hidePaging)
                {
                    var wasEnabled = GUI.enabled;
                    bool pagingIsRelevant = this.paging.IsEnabled && this.paging.PageCount != 1;

                    GUI.enabled = wasEnabled && pagingIsRelevant && !this.paging.IsOnFirstPage;
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowLeft, true))
                    {
                        if (Event.current.button == 0)
                        {
                            this.paging.CurrentPage--;
                        }
                        else
                        {
                            this.paging.CurrentPage = 0;
                        }
                    }

                    GUI.enabled = wasEnabled && pagingIsRelevant;
                    var width = GUILayoutOptions.Width(10 + this.paging.PageCount.ToString().Length * 10);
                    this.paging.CurrentPage = EditorGUILayout.IntField(this.paging.CurrentPage + 1, width) - 1;
                    GUILayout.Label(GUIHelper.TempContent("/ " + this.paging.PageCount));

                    GUI.enabled = wasEnabled && pagingIsRelevant && !this.paging.IsOnLastPage;
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowRight, true))
                    {
                        if (Event.current.button == 0)
                        {
                            this.paging.CurrentPage++;
                        }
                        else
                        {
                            this.paging.CurrentPage = this.paging.PageCount - 1;
                        }
                    }

                    GUI.enabled = wasEnabled && this.paging.PageCount != 1;
                    if (this.config.ShowExpandButton)
                    {
                        if (SirenixEditorGUI.ToolbarButton(this.paging.IsEnabled ? EditorIcons.ArrowDown : EditorIcons.ArrowUp, true))
                        {
                            this.paging.IsEnabled = !this.paging.IsEnabled;
                        }
                    }
                    GUI.enabled = wasEnabled;
                }
                if (!this.disableAddKey && this.attrSettings.IsReadOnly != true)
                {
                    if (SirenixEditorGUI.ToolbarButton(SdfIconType.Plus))
                    {
                        this.showAddKeyGUI = !this.showAddKeyGUI;

                        if (this.showAddKeyGUI)
                        {
                            this.Property.State.Expanded = true;
                        }
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private static GUIStyle oneLineMargin;

        private static GUIStyle OneLineMargin
        {
            get
            {
                if (oneLineMargin == null)
                {
                    oneLineMargin = new GUIStyle() { margin = new RectOffset(8, 0, 0, 0) };
                }
                return oneLineMargin;
            }
        }

        private static GUIStyle headerMargin;

        private static GUIStyle HeaderMargin
        {
            get
            {
                if (headerMargin == null)
                {
                    headerMargin = new GUIStyle() { margin = new RectOffset(40, 0, 0, 0) };
                }
                return headerMargin;
            }
        }

        private void DrawElements(IPropertyValueEntry<TDictionary> entry, GUIContent label)
        {
            for (int i = this.paging.StartIndex; i < this.paging.EndIndex; i++)
            {
                var keyValuePairProperty = this.filter[i];
                var keyValuePairValue = (keyValuePairProperty.ValueEntry as IPropertyValueEntry<EditableKeyValuePair<TKey, TValue>>).SmartValue;

                Rect rect = SirenixEditorGUI.BeginListItem(false, listItemStyle);
                {
                    if (this.attrSettings.DisplayMode != DictionaryDisplayOptions.OneLine)
                    {
                        bool defaultExpanded;
                        switch (this.attrSettings.DisplayMode)
                        {
                            case DictionaryDisplayOptions.CollapsedFoldout:
                                defaultExpanded = false;
                                break;

                            case DictionaryDisplayOptions.ExpandedFoldout:
                                defaultExpanded = true;
                                break;

                            default:
                                defaultExpanded = SirenixEditorGUI.ExpandFoldoutByDefault;
                                break;
                        }
                        var isExpanded = keyValuePairProperty.Context.GetPersistent(this, "Expanded", defaultExpanded);

                        var headerRect = EditorGUILayout.BeginVertical(FoldoutHeaderStyle, GUILayout.MinHeight(22));

                        // This is needed so that the MinHeight takes over if the DrawKeyProperty isn't drawing anything.
                        GUILayout.Space(1);
                        GUILayout.Space(-1);

                        GUI.DrawTexture(
                            headerRect,
                            Texture2D.whiteTexture,
                            ScaleMode.StretchToFill,
                            false, 1f, new Color(0.24f, 0.24f, 0.24f),
                            Vector4.zero, new Vector4(3f, 3f, isExpanded.Value ? 0f : 3f, isExpanded.Value ? 0f : 3f));

                        GUI.DrawTexture(
                            headerRect,
                            Texture2D.whiteTexture,
                            ScaleMode.StretchToFill,
                            false, 1f, SirenixGUIStyles.BorderColor,
                            Vector4.one, new Vector4(3f, 3f, isExpanded.Value ? 0f : 3f, isExpanded.Value ? 0f : 3f));

                        if (keyValuePairValue.IsInvalidKey)
                        {
                            GUIHelper.PushColor(Color.red);
                        }
                        GUIHelper.PushIsDrawingDictionaryKey(true);
                        GUIHelper.PushLabelWidth(this.keyLabelWidth);
                        var keyProperty = keyValuePairProperty.Children[0];
                        this.DrawKeyProperty(keyProperty, this.emptySpaceLabel);
                        GUIHelper.PopLabelWidth();
                        GUIHelper.PopIsDrawingDictionaryKey();
                        if (keyValuePairValue.IsInvalidKey)
                        {
                            GUIHelper.PopColor();
                        }

                        isExpanded.Value = SirenixEditorGUI.Foldout(headerRect.AddX(4).AlignLeft(45), isExpanded.Value, this.keyLabel);
                        EditorGUILayout.EndVertical();
                        if (SirenixEditorGUI.BeginFadeGroup(isExpanded, isExpanded.Value))
                        {
                            var contentRect = EditorGUILayout.BeginVertical(FoldoutHeaderStyle);
                            GUILayout.Space(1);
                            GUI.DrawTexture(
                                contentRect,
                                Texture2D.whiteTexture,
                                ScaleMode.StretchToFill,
                                false, 1f, new Color(1f, 1f, 1f, 0.042f),
                                Vector4.zero, new Vector4(0f, 0f, 3f, 3f));
                            GUI.DrawTexture(
                                contentRect,
                                Texture2D.whiteTexture,
                                ScaleMode.StretchToFill,
                                false, 1f, SirenixGUIStyles.BorderColor,
                                new Vector4(1f, 0f, 1f, 1f), new Vector4(0f, 0f, 3f, 3f));
                            keyValuePairProperty.Children[1].Draw(null);
                            EditorGUILayout.EndVertical();
                        }
                        SirenixEditorGUI.EndFadeGroup();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.BeginVertical(GUILayoutOptions.Width(this.keyWidthOffset.Value));
                        {
                            var keyProperty = keyValuePairProperty.Children[0];

                            if (keyValuePairValue.IsInvalidKey)
                            {
                                GUIHelper.PushColor(Color.red);
                            }

                            if (this.attrSettings.IsReadOnly) GUIHelper.PushGUIEnabled(false);

                            GUIHelper.PushIsDrawingDictionaryKey(true);
                            GUIHelper.PushLabelWidth(10);

                            DrawKeyProperty(keyProperty, null);

                            GUIHelper.PopLabelWidth();
                            GUIHelper.PopIsDrawingDictionaryKey();

                            if (this.attrSettings.IsReadOnly) GUIHelper.PopGUIEnabled();

                            if (keyValuePairValue.IsInvalidKey)
                            {
                                GUIHelper.PopColor();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical(OneLineMargin);
                        {
                            GUIHelper.PushHierarchyMode(false);
                            var valueEntry = keyValuePairProperty.Children[1];
                            var tmp = GUIHelper.ActualLabelWidth;
                            GUIHelper.BetterLabelWidth = 150;
                            valueEntry.Draw(null);
                            GUIHelper.BetterLabelWidth = tmp;
                            GUIHelper.PopHierarchyMode();
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }

                    if (entry.IsEditable && !this.attrSettings.IsReadOnly && SirenixEditorGUI.SDFIconButton(new Rect(rect.xMax - 24 + 5, rect.y + 4 + ((int)rect.height - 23) / 2, 14, 14), SdfIconType.X, IconAlignment.LeftOfText, SirenixGUIStyles.IconButton))
                    {
                        this.dictionaryResolver.QueueRemoveKey(Enumerable.Range(0, entry.ValueCount)
                                                                         .Select(n => this.dictionaryResolver.GetKey(n, this.filter.GetCollectionIndex(i)))
                                                                         .ToArray());
                        
                        UnityEditorEventUtility.EditorApplication_delayCall += () => this.newKeyIsValid = null;

                        this.filter.Update();
                        
                        GUIHelper.RequestRepaint();
                    }
                }
                SirenixEditorGUI.EndListItem();
            }

            if (this.paging.IsOnLastPage && entry.ValueState == PropertyValueState.CollectionLengthConflict)
            {
                SirenixEditorGUI.BeginListItem(false);
                GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
                SirenixEditorGUI.EndListItem();
            }
        }

        private void DrawKeyProperty(InspectorProperty keyProperty, GUIContent keyLabel)
        {
            EditorGUI.BeginChangeCheck();

#if SIRENIX_INTERNAL
            var keyValuePairValue = (keyProperty.Parent.ValueEntry as IPropertyValueEntry<EditableKeyValuePair<TKey, TValue>>).SmartValue;

            if (keyValuePairValue.IsTempKey && !keyValuePairValue.IsInvalidKey)
            {
                GUIHelper.PushColor(Color.green);
            }
#endif

            keyProperty.Draw(keyLabel);

#if SIRENIX_INTERNAL
            if (keyValuePairValue.IsTempKey && !keyValuePairValue.IsInvalidKey)
            {
                GUIHelper.PopColor();
            }
#endif

            var guiChanged = EditorGUI.EndChangeCheck();

            bool valuesAreDirty = ValuesAreDirty(keyProperty);

            if (!guiChanged && valuesAreDirty)
            {
                this.dictionaryResolver.ValueApplyIsTemporary = true;
                ApplyChangesToProperty(keyProperty);
                this.dictionaryResolver.ValueApplyIsTemporary = false;
            }
            else if (guiChanged && !valuesAreDirty)
            {
                MarkPropertyDirty(keyProperty);
            }
        }

        private static void MarkPropertyDirty(InspectorProperty keyProperty)
        {
            keyProperty.ValueEntry.WeakValues.ForceMarkDirty();

            if (KeyIsValueType)
            {
                for (int i = 0; i < keyProperty.Children.Count; i++)
                {
                    MarkPropertyDirty(keyProperty.Children[i]);
                }
            }
        }

        private static void ApplyChangesToProperty(InspectorProperty keyProperty)
        {
            if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty) keyProperty.ValueEntry.ApplyChanges();

            if (KeyIsValueType)
            {
                for (int i = 0; i < keyProperty.Children.Count; i++)
                {
                    ApplyChangesToProperty(keyProperty.Children[i]);
                }
            }
        }

        private static bool ValuesAreDirty(InspectorProperty keyProperty)
        {
            if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty)
            {
                return true;
            }

            if (KeyIsValueType)
            {
                for (int i = 0; i < keyProperty.Children.Count; i++)
                {
                    if (ValuesAreDirty(keyProperty.Children[i])) return true;
                }
            }

            return false;
        }

        private static bool CheckKeyIsValid(IPropertyValueEntry<TDictionary> entry, TKey key, out string errorMessage)
        {
            if (!KeyIsValueType && object.ReferenceEquals(key, null))
            {
                errorMessage = "Key cannot be null.";
                return false;
            }

            var keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);

            if (entry.Property.Children[keyStr] == null)
            {
                errorMessage = "";
                return true;
            }
            else
            {
                errorMessage = "An item with the same key already exists.";
                return false;
            }
        }

        public void Dispose()
        {
            if (keyEntryPropertyTree != null)
            {
                keyEntryPropertyTree.Dispose();
                keyEntryPropertyTree = null;
            }

            this.filter?.Dispose();
        }
    }
}
#endif