//-----------------------------------------------------------------------
// <copyright file="InlineEditorAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector.Editor.Internal;
    using Sirenix.Utilities.Editor;
    using Sirenix.Serialization;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
    using UnityEditor.UIElements;
    using System.Reflection;

    /// <summary>
    /// Static GUI information reguarding the InlineEditor attribute.
    /// </summary>
    public static class InlineEditorAttributeDrawer
    {
        /// <summary>
        /// Gets a value indicating how many InlineEditors we are currently in.
        /// </summary>
        public static int CurrentInlineEditorDrawDepth { get; internal set; }
    }

    /// <summary>
    /// Draws properties marked with <see cref="InlineEditorAttribute"/>.
    /// </summary>
    /// <seealso cref="InlineEditorAttribute"/>
    /// <seealso cref="DrawWithUnityAttribute"/>
    [DrawerPriority(0, 0, 3000)]
    public class InlineEditorAttributeDrawer<T> : OdinAttributeDrawer<InlineEditorAttribute, T>, IDisposable where T : UnityEngine.Object
    {
        public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

        private static Type animationClipEditorType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AnimationClipEditor");

        private static System.Reflection.PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible", Flags.AllMembers);
        private static Stack<LayoutSettings> layoutSettingsStack = new Stack<LayoutSettings>();
        private Editor editor;
        private Editor previewEditor;
        private UnityEngine.Object target;
        private Rect inlineEditorRect;
        private Vector2 scrollPos;
        private bool drawHeader;
        private bool drawGUI;
        private bool drawPreview;
        private bool alwaysVisible;
        private bool targetIsOpenForEdit;
        private OdinImGuiElement element;
        private bool hasCheckedCurrentEditorForElement;
        private bool allowSceneObjects;
        private bool isAnimationClip;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Attribute.ExpandedHasValue && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0)
            {
                this.Property.State.Expanded = this.Attribute.Expanded;
            }
            
            this.allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(this.Property);

            this.isAnimationClip = this.Property.ValueEntry.TypeOfValue == typeof(AnimationClip);
        }

        /// <summary>
        /// Draws the property layout.
        /// </summary>
        /// <param name="label">The label.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect valueRect;
            switch (this.Attribute.ObjectFieldMode)
            {
                case InlineEditorObjectFieldModes.Boxed:
                    this.alwaysVisible = false;
                    SirenixEditorGUI.BeginToolbarBox();
                    SirenixEditorGUI.BeginToolbarBoxHeader();
                    if (this.ValueEntry.SmartValue)
                    {
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, label, out valueRect);
                        this.DrawPolymorphicObjectField(valueRect);
                    }
                    else
                    {
                        this.DrawPolymorphicObjectField(label);
                    }
                    SirenixEditorGUI.EndToolbarBoxHeader();
                    GUIHelper.PushHierarchyMode(false);
                    this.DrawEditor();
                    GUIHelper.PopHierarchyMode();
                    SirenixEditorGUI.EndToolbarBox();
                    break;
                case InlineEditorObjectFieldModes.Foldout:
                    this.alwaysVisible = false;
                    if (this.ValueEntry.SmartValue)
                    {
                        this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, label, out valueRect);
                        this.DrawPolymorphicObjectField(valueRect);
                    }
                    else
                    {
                        this.DrawPolymorphicObjectField(label);
                    }
                    EditorGUI.indentLevel++;
                    this.DrawEditor();
                    EditorGUI.indentLevel--;
                    break;
                case InlineEditorObjectFieldModes.Hidden:
                    this.alwaysVisible = true;
                    if (!(UnityEngine.Object)this.ValueEntry.WeakSmartValue)
                    {
                        this.DrawPolymorphicObjectField(label);
                    }
                    this.DrawEditor();
                    break;
                case InlineEditorObjectFieldModes.CompletelyHidden:
                    this.alwaysVisible = true;
                    this.DrawEditor();
                    break;
            }
        }

        private void DrawPolymorphicObjectField(Rect position)
        {
            bool isPolymorphic = this.ValueEntry.BaseValueType == typeof(object) ||
                                 !typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.BaseValueType) ||
                                 this.ValueEntry.BaseValueType.IsInterface;

            if (isPolymorphic)
            {
                if (GeneralDrawerConfig.Instance.useOldPolymorphicField)
                {
                    EditorGUI.BeginChangeCheck();
                    object newValue = OdinInternalEditorFields.PolymorphicObjectField(position, GUIContent.none, this.Property, this.allowSceneObjects);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            this.ValueEntry.WeakValues[0] = newValue;
                            for (int j = 1; j < this.ValueEntry.ValueCount; j++)
                            {
                                // NOTE: "Sirenix.Serialization." is important for later Unity versions
                                this.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
                            }
                        });
                    }
                }
                else
                {
                    OdinInternalEditorFields.PolymorphicObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                                    position, GUIContent.none, this.Property, this.allowSceneObjects);
                }
            }
            else
            {
                if (GeneralDrawerConfig.Instance.useOldUnityObjectField)
                {
                    this.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityObjectField(position, null,
                                                                                               this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                                                                                               this.ValueEntry.BaseValueType,
                                                                                               this.allowSceneObjects);
                }
                else
                {
                    OdinInternalEditorFields.UnityObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                              position, null, this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                                                              this.ValueEntry.BaseValueType,
                                                              this.allowSceneObjects,
                                                              property: this.Property);
                }
            }
        }

        private void DrawPolymorphicObjectField(GUIContent label)
        {
            bool isPolymorphic = this.ValueEntry.BaseValueType == typeof(object) ||
                                 !typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.BaseValueType) ||
                                 this.ValueEntry.BaseValueType.IsInterface;

            if (isPolymorphic)
            {
                if (GeneralDrawerConfig.Instance.useOldPolymorphicField)
                {
                    EditorGUI.BeginChangeCheck();
                    object newValue = OdinInternalEditorFields.PolymorphicObjectField(label, this.Property, this.allowSceneObjects);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            this.ValueEntry.WeakValues[0] = newValue;
                            for (int j = 1; j < this.ValueEntry.ValueCount; j++)
                            {
                                // NOTE: "Sirenix.Serialization." is important for later Unity versions
                                this.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
                            }
                        });
                    }
                }
                else
                {
                    OdinInternalEditorFields.PolymorphicObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD, label, this.Property, this.allowSceneObjects);
                }
            }
            else
            {
                if (GeneralDrawerConfig.Instance.useOldUnityObjectField)
                {
                    this.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityObjectField(label, this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                                                                                               this.ValueEntry.BaseValueType,
                                                                                               this.allowSceneObjects);
                }
                else
                {
                    OdinInternalEditorFields.UnityObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                              EditorGUILayout.GetControlRect(),
                                                              label, this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                                                              this.ValueEntry.BaseValueType,
                                                              this.allowSceneObjects,
                                                              property: this.Property);
                }
            }
        }

        private OdinImGuiElement TryCreateInspectorElementAndSetClasses(Editor targetEditor)
        {
            if (targetEditor.GetType().GetMethod("CreateInspectorGUI", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public) != null)
            {
                var element = new InspectorElement(targetEditor);
                return new OdinImGuiElement(element);
            }
            else
            {
                return null;
            }
        }

        private void DrawEditor()
        {
            var obj = this.ValueEntry.SmartValue;

            if (this.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict)
            {
                SirenixEditorGUI.InfoMessageBox("reference-path-conflict");
            }
            else
            {
                if (this.alwaysVisible || SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded))
                {
                    this.UpdateEditors();

                    if (this.Attribute.MaxHeight != 0)
                    {
                        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, GUILayoutOptions.MaxHeight(200));
                    }

                    var prev = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = false;
                    EditorGUI.BeginChangeCheck();
                    this.DoTheDrawing();
                    if (EditorGUI.EndChangeCheck())
                    {
                        var e = this.Property.BaseValueEntry as PropertyValueEntry;
                        if (e != null)
                        {
                            for (int i = 0; i < e.ValueCount; i++)
                            {
                                e.TriggerOnChildValueChanged(i);
                            }
                        }
                    }
                    EditorGUI.showMixedValue = prev;
                    if (this.Attribute.MaxHeight != 0)
                    {
                        EditorGUILayout.EndScrollView();
                    }
                }
                else
                {
                    if (this.editor != null)
                    {
                        this.DestroyEditors();
                    }
                }

                if (!this.alwaysVisible)
                {
                    SirenixEditorGUI.EndFadeGroup();
                }
            }

        }

        private void DoTheDrawing()
        {
            if (IsGameObject && !this.Attribute.DrawPreview)
            {
                SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject's components cannot be directly inspected inline in the editor. Choose an InlineEditorMode that includes a preview to draw a GameObject preview.");
                OdinInternalEditorFields.UnityObjectField(this.ValueEntry.SmartValue, typeof(GameObject), this.allowSceneObjects);

                GUILayout.BeginHorizontal();
                {
                    GUIHelper.PushGUIEnabled(this.ValueEntry.SmartValue != null);

                    string text = this.ValueEntry.SmartValue != null ? ("Open Inspector window for " + this.ValueEntry.SmartValue.name) : "Open Inspector window (null)";

                    if (GUILayout.Button(GUIHelper.TempContent(text)))
                    {
                        GUIHelper.OpenInspectorWindow(this.ValueEntry.SmartValue);
                        GUIHelper.ExitGUI(true);
                    }

                    text = this.ValueEntry.SmartValue != null ? ("Select " + this.ValueEntry.SmartValue.name) : "Select GO (null)";

                    if (GUILayout.Button(GUIHelper.TempContent(text)))
                    {
                        Selection.activeObject = this.ValueEntry.SmartValue;
                        GUIHelper.ExitGUI(true);
                    }

                    GUIHelper.PopGUIEnabled();
                }
                GUILayout.EndHorizontal();
                return;
            }

            if (this.editor != null && this.editor.SafeIsUnityNull() == false)
            {
                SaveLayoutSettings();
                InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth++;
                try
                {
                    if (!this.targetIsOpenForEdit)
                    {
                        GUIHelper.PushGUIEnabled(false);
                    }


                    var alignment = this.Attribute.PreviewAlignment;
                    var drawPreviewHorizontally = this.drawPreview && (alignment == PreviewAlignment.Left || alignment == PreviewAlignment.Right);
                    var drawPreviewVertically = this.drawPreview && (alignment == PreviewAlignment.Top || alignment == PreviewAlignment.Bottom);

                    if (!drawGUI && drawPreviewHorizontally)
                    {
                        // There is nothing to draw left or right of.
                        drawPreviewHorizontally = false;
                        drawPreviewVertically = true;
                        alignment = alignment == PreviewAlignment.Left ? PreviewAlignment.Top : PreviewAlignment.Bottom;
                    }

                    if (drawPreviewHorizontally)
                    {
                        GUILayout.BeginHorizontal();

                        if (this.Attribute.PreviewAlignment == PreviewAlignment.Left)
                        {
                            GUILayout.BeginVertical();
                            this.DrawPreview(alignment);
                            GUILayout.EndVertical();
                        }

                        GUILayout.BeginVertical();
                    }
                    else if (drawPreviewVertically && this.Attribute.PreviewAlignment == PreviewAlignment.Top)
                    {
                        this.DrawPreview(alignment);
                    }

                    // Brace for impact
                    if (this.drawHeader)
                    {
                        var tmp = Event.current.rawType;
                        EditorGUILayout.BeginFadeGroup(0.9999f); // This one fixes some layout issues for reasons beyond me, but locks the input.
                        Event.current.type = tmp;                // Lets undo that shall we?
                        GUILayout.Space(0);                      // Yeah i know. But it removes some unwanted top padding.
                        this.editor.DrawHeader();
                        GUILayout.Space(1);                      // This adds the the 1 pixel border clipped from the fade group.
                        EditorGUILayout.EndFadeGroup();
                    }
                    else
                    {
                        // Many of unity editors will not work if the header is not drawn.
                        // So lets draw it anyway. -_-
                        GUIHelper.BeginDrawToNothing();
                        this.editor.DrawHeader();
                        GUIHelper.EndDrawToNothing();
                    }

                    if (this.drawGUI)
                    {
                        if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && !hasCheckedCurrentEditorForElement)
                        {
                            hasCheckedCurrentEditorForElement = true;
                            this.element = TryCreateInspectorElementAndSetClasses(this.editor);
                        }

                        if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && this.element != null)
                        {
                            ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.element);
                        }
                        else
                        {
                            var prev = GeneralDrawerConfig.Instance.ShowMonoScriptInEditor;
                            try
                            {
                                GeneralDrawerConfig.Instance.ShowMonoScriptInEditor = false;
                                EditorGUILayout.BeginVertical();

                                var prevIsSet = UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this.editor.target);

                                if (!this.drawHeader)
                                {
                                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this.editor.target, true);
                                }

                                this.editor.OnInspectorGUI();

                                if (!this.drawHeader)
                                {
                                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(this.editor.target, prevIsSet);
                                }

                                EditorGUILayout.EndVertical();
                            }
                            finally
                            {
                                GeneralDrawerConfig.Instance.ShowMonoScriptInEditor = prev;
                            }
                        }
                    }

                    if (drawPreviewHorizontally)
                    {
                        GUILayout.EndVertical();

                        if (this.Attribute.PreviewAlignment == PreviewAlignment.Right)
                        {
                            GUILayout.BeginVertical();
                            this.DrawPreview(alignment);
                            GUILayout.EndVertical();
                        }

                        GUILayout.EndHorizontal();
                    }
                    else if (drawPreviewVertically && alignment == PreviewAlignment.Bottom)
                    {
                        this.DrawPreview(alignment);
                    }

                    if (!this.targetIsOpenForEdit)
                    {
                        GUIHelper.PopGUIEnabled();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.IsExitGUIException())
                    {
                        throw ex.AsExitGUIException();
                    }
                    else
                    {
                        Debug.LogException(ex);
                    }
                }
                finally
                {
                    InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth--;
                    RestoreLayout();
                }
            }
        }

        private void DrawPreview(PreviewAlignment alignment)
        {
            var isHorizontal = alignment == PreviewAlignment.Left || alignment == PreviewAlignment.Right;

            // previewEditor.HasPreviewGUI() reports 'false' for GameObject from the scene. But the user has asked for a preview, so a preview they'll get!
            if (!this.drawPreview || (!this.previewEditor.HasPreviewGUI() && !(this.previewEditor.target is GameObject))) return;

            var size = isHorizontal ? this.Attribute.PreviewWidth : this.Attribute.PreviewHeight;

            if (this.isAnimationClip)
            {
                if (isHorizontal)
                {
                    if (size < 200)
                    {
                        size = 200;
                    }
                }
                else if (size < 90)
                {
                    size = 90;
                }
            }
            
            var layoutOptions = GUILayoutOptions.EmptyGUIOptions;

            switch (alignment)
            {
                case PreviewAlignment.Left:
                case PreviewAlignment.Right:
                layoutOptions = GUILayoutOptions.Width(size).ExpandHeight();
                break;
                case PreviewAlignment.Top:
                case PreviewAlignment.Bottom:
                layoutOptions = GUILayoutOptions.ExpandWidth().Height(size);
                break;
            }

            var rect = EditorGUILayout.GetControlRect(false, size, layoutOptions);
            
            var tmp = GUI.enabled;
            GUI.enabled = true;

            if (this.isAnimationClip && this.previewEditor.GetType() == animationClipEditorType)
            {
                // IMPORTANT: This method WILL set GUI.enabled = false in some cases, ensure to reverse that after exiting this scope.
                this.DrawAnimationClipEditorPreview(rect);
            }
            else
            {
                this.previewEditor.DrawPreview(rect);
            }

            GUI.enabled = tmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <remarks>Will set <see cref="GUI.enabled">GUI.enabled</see> to false during some cases, to avoid the Preview eating events when it really shouldn't.</remarks>
        private void DrawAnimationClipEditorPreview(Rect rect)
        {
            // IMPORTANT: When AnimationClips are drawn inline without calling AnimationClipEditor.OnInspectorGUI, the timings are off.
            //            This workaround ensures OnInspectorGUI is called while drawing them in a disabled area with a size of 0,
            //            making the result of the drawing call invisible and non-interactive.
            if (!this.drawGUI)
            {
                GUIHelper.BeginDrawToNothing();
                {
                    this.previewEditor.OnInspectorGUI();
                }
                GUIHelper.EndDrawToNothing();
            }

            switch (Event.current.type)
            {
                case EventType.ScrollWheel:
                case EventType.DragPerform:
                    if (!Event.current.IsMouseOver(rect))
                    {
                        GUI.enabled = false;
                    }

                    break;
            }

            this.previewEditor.DrawPreview(rect);
        }

        private void UpdateEditors()
        {
            this.targetIsOpenForEdit = true;

            var unityObj = (UnityEngine.Object)this.ValueEntry.WeakSmartValue;

            if (this.editor != null && !unityObj)
            {
                this.DestroyEditors();
            }

            bool createNewEditor = unityObj != null && (this.editor == null || this.target != unityObj || this.target == null);

            if (createNewEditor && this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                if (this.ValueEntry.WeakValues[0] == null)
                {
                    createNewEditor = false;
                }

                if (createNewEditor)
                {
                    var type = this.ValueEntry.WeakValues[0].GetType();

                    for (int i = 1; i < this.ValueEntry.ValueCount; i++)
                    {
                        if (!this.ValueEntry.Values[i] || this.ValueEntry.Values[i].GetType() != type)
                        {
                            createNewEditor = false;
                            break;
                        }
                    }
                }

                if (!createNewEditor)
                {
                    SirenixEditorGUI.InfoMessageBox("Cannot perform multi-editing on objects of different type.");
                }
            }

            if (createNewEditor)
            {
                this.target = unityObj;
                bool isGameObject = unityObj as GameObject;
                this.drawHeader = isGameObject ? this.Attribute.DrawHeader : this.Attribute.DrawHeader;
                this.drawGUI = isGameObject ? false : this.Attribute.DrawGUI;
                this.drawPreview = this.Attribute.DrawPreview || isGameObject && this.Attribute.DrawGUI;

                if (this.editor != null)
                {
                    this.DestroyEditors();
                }

                this.hasCheckedCurrentEditorForElement = false;
                this.editor = Editor.CreateEditor(this.ValueEntry.WeakValues.FilterCast<UnityEngine.Object>().ToArray());

                var component = this.target as Component;
                if (component != null)
                {
                    this.previewEditor = Editor.CreateEditor(component.gameObject);
                }
                else
                {
                    this.previewEditor = this.editor;
                }

                var materialEditor = this.editor as MaterialEditor;
                if (materialEditor != null && materialForceVisibleProperty != null)
                {
                    materialForceVisibleProperty.SetValue(materialEditor, true, null);
                }

                if (this.Attribute.DisableGUIForVCSLockedAssets && AssetDatabase.Contains(this.target))
                {
                    this.targetIsOpenForEdit = AssetDatabase.IsOpenForEdit(this.target);
                }
            }
        }

        private void DestroyEditors()
        {
            this.targetIsOpenForEdit = true;

            if (this.previewEditor != this.editor && this.previewEditor != null)
            {
                try
                {
                    UnityEngine.Object.DestroyImmediate(this.previewEditor);
                }
                catch { }
                this.previewEditor = null;
            }

            if (this.editor != null)
            {
                if (element != null)
                {
                    var capture = element;
                    element = null;

                    EditorApplication.delayCall += () =>
                    {
                        if (capture.parent != null)
                        {
                            capture?.RemoveFromHierarchy();
                        }
                    };
                }

                try
                {
                    UnityEngine.Object.DestroyImmediate(this.editor);
                }
                catch (Exception) { }
                this.editor = null;
            }
        }

        private static void SaveLayoutSettings()
        {
            layoutSettingsStack.Push(new LayoutSettings()
            {
                Skin = GUI.skin,
                Color = GUI.color,
                ContentColor = GUI.contentColor,
                BackgroundColor = GUI.backgroundColor,
                Enabled = GUI.enabled,
                IndentLevel = EditorGUI.indentLevel,
                FieldWidth = EditorGUIUtility.fieldWidth,
                LabelWidth = GUIHelper.ActualLabelWidth,
                HierarchyMode = EditorGUIUtility.hierarchyMode,
                WideMode = EditorGUIUtility.wideMode,
            });
        }

        private static void RestoreLayout()
        {
            var settings = layoutSettingsStack.Pop();

            GUI.skin = settings.Skin;
            GUI.color = settings.Color;
            GUI.contentColor = settings.ContentColor;
            GUI.backgroundColor = settings.BackgroundColor;
            GUI.enabled = settings.Enabled;
            EditorGUI.indentLevel = settings.IndentLevel;
            EditorGUIUtility.fieldWidth = settings.FieldWidth;
            GUIHelper.BetterLabelWidth = settings.LabelWidth;
            EditorGUIUtility.hierarchyMode = settings.HierarchyMode;
            EditorGUIUtility.wideMode = settings.WideMode;
        }

        void IDisposable.Dispose()
        {
            this.DestroyEditors();
        }

        private struct LayoutSettings
        {
            public GUISkin Skin;
            public Color Color;
            public Color ContentColor;
            public Color BackgroundColor;
            public bool Enabled;
            public int IndentLevel;
            public float FieldWidth;
            public float LabelWidth;
            public bool HierarchyMode;
            public bool WideMode;
        }
    }
}
#endif