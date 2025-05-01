//-----------------------------------------------------------------------
// <copyright file="ValidationDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.Utilities;
    using UnityEditor;

    [DrawerPriority(0, 10000, 0)]
    public class ValidationDrawer<T> : OdinValueDrawer<T>, IDisposable
    {
        private static Color HighlightedBgColor => EditorGUIUtility.isProSkin ? Color.Lerp(EditorWindowBgColor, Color.white, 0.15f) : new Color(239 / 255f, 239 / 255f, 239 / 255f, 1);
        private static Color EditorWindowBgColor => EditorGUIUtility.isProSkin ? DarkSkinEditorWindowBgColor : new Color(0.76f, 0.76f, 0.76f, 1f);
        private static Color DarkSkinEditorWindowBgColor => new Color(0.22f, 0.22f, 0.22f, 1f);

        private List<ValidationResult> oldValidationResults;
        private List<ValidationResult> validationResults;
        private bool rerunFullValidation;
        private bool revalidateEveryFrame;

        private bool subscribedToEvents;
        private bool odinValidatorIsInstalled;
		private int uniqueIdHashForThisValidationDrawer;
		private ValidationComponent validationComponent;
        private Dictionary<int, PropertyTree> issueFixerTrees = new Dictionary<int, PropertyTree>();
        private Dictionary<int, PropertyTree> metaDataTrees = new Dictionary<int, PropertyTree>();

        private GUIStyle messageBoxText;
        private GUIStyle MessageBoxText
        {
            get
            {
                if (this.messageBoxText == null)
                {
                    this.messageBoxText = new GUIStyle("label")
                    {
                        margin = new RectOffset(4, 4, 2, 2),
                        fontSize = 10,
                        richText = true,
                        wordWrap = true,
                    };
                }

                return this.messageBoxText;
            }
        }

        private GUIStyle fixArgsPadding;
        private GUIStyle FixArgsPadding
        {
            get
            {
                if (this.fixArgsPadding == null)
                {
                    this.fixArgsPadding = new GUIStyle()
                    {
                        padding = new RectOffset(5, 5, 5, 5),
                    };
                }

                return this.fixArgsPadding;
            }
        }

        private Texture2D _defaultIconTexture;
        private Texture2D defaultIconTexture
        {
            get
            {
                if (this._defaultIconTexture == null)
                {
                    this._defaultIconTexture = new Texture2D(20, 20) { hideFlags = HideFlags.HideAndDontSave };
                    CleanupUtility.DestroyObjectOnAssemblyReload(this._defaultIconTexture);
                }

                return this._defaultIconTexture;
            }
        }

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            var validation = property.GetComponent<ValidationComponent>();
            if (validation == null) return false;
            if (property.GetAttribute<DontValidateAttribute>() != null) return false;
            return validation.ValidatorLocator.PotentiallyHasValidatorsFor(property);
        }

        protected override void Initialize()
        {
            unchecked
            {
                this.uniqueIdHashForThisValidationDrawer = this.Property.GetHashCode() * "ValidationDrawer".GetHashCode();
            }

            this.validationComponent = this.Property.GetComponent<ValidationComponent>();
            this.validationComponent.ValidateProperty(ref this.validationResults, true);

            if (this.validationResults.Count > 0)
            {
                this.Property.Tree.OnUndoRedoPerformed += this.OnUndoRedoPerformed;
                this.ValueEntry.OnValueChanged += this.OnValueChanged;
                this.ValueEntry.OnChildValueChanged += this.OnChildValueChanged;
                this.subscribedToEvents = true;

                foreach (var result in this.validationResults)
                {
                    if (result.Setup.Validator is IValidator val && val.RevalidationCriteria == RevalidationCriteria.Always)
                    {
                        this.revalidateEveryFrame = true;
                    }

                    ValidationEvents.InvokeOnValidationStateChanged(new ValidationStateChangeInfo()
                    {
                        ValidationResult = result,
                    });
                }

                this.CreateIssueFixerTrees(this.validationResults);
            }
            else
            { 
                // NOTE: This is for maintaining consistent ControlIds for the next drawers.
                // this.SkipWhenDrawing = true;
            }

            this.odinValidatorIsInstalled = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinValidator.Editor.OdinValidatorWindow") != null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.validationResults.Count == 0)
            {
                GUIUtility.GetControlID(this.uniqueIdHashForThisValidationDrawer, FocusType.Passive);
                this.CallNextDrawer(label);
                return;
            } 
            
            GUILayout.BeginVertical();
            SirenixEditorGUI.BeginShakeableGroup();
            var resultsChanged = false;

            if (Event.current.type == EventType.Layout && (this.revalidateEveryFrame || this.rerunFullValidation))
            {
                var results = this.oldValidationResults;
                this.oldValidationResults = this.validationResults;

                if (results != null)
                    results.Clear();

                this.validationComponent.ValidateProperty(ref results, true);
                this.validationResults = results;

                //int oldErrorCount = 0,
                //    oldWarningCount = 0,
                //    newErrorCount = 0,
                //    newWarningCount = 0;

                //for (int i = 0; i < results.Count; i++)
                //{
                //    var result = results[i];

                //    if (result.ResultType == ValidationResultType.Error)
                //        newErrorCount++;
                //    else if (result.ResultType == ValidationResultType.Warning)
                //        newWarningCount++;
                //}

                //for (int i = 0; i < this.oldValidationResults.Count; i++)
                //{
                //    var result = this.oldValidationResults[i];

                //    if (result.ResultType == ValidationResultType.Error)
                //        oldErrorCount++;
                //    else if (result.ResultType == ValidationResultType.Warning)
                //        oldWarningCount++;
                //}


                //if (newErrorCount > oldErrorCount || newWarningCount > oldWarningCount)
                //{
                //    shakeGroup = true;
                //}
                //

                if (results.Count != this.oldValidationResults.Count)
                {
                     resultsChanged = true;
                    this.CreateIssueFixerTrees(results);

                    for (var i = 0; i < results.Count; i++)
                    {
                        ValidationEvents.InvokeOnValidationStateChanged(new ValidationStateChangeInfo()
                        {
                            ValidationResult = results[i],
                        });
                    }
                }
                else
                {
                    for (var i = 0; i < results.Count; i++)
                    {
                        var result = results[i];

                        if (!result.IsMatch(this.oldValidationResults[i]))
                        {
                            if (result.ResultType == ValidationResultType.Error || result.ResultType == ValidationResultType.Warning)
                            {
                                resultsChanged = true;
                                this.CreateIssueFixerTrees(results);
                            }

                            ValidationEvents.InvokeOnValidationStateChanged(new ValidationStateChangeInfo()
                            {
                                ValidationResult = result,
                            });
                        }
                    }
                }

                if (resultsChanged)
                {
                    SirenixEditorGUI.StartShakingGroup();
                }
            }

            for (var i = 0; i < this.validationResults.Count; i++)
            {
                var result = this.validationResults[i];
                var messageType = UnityEditor.MessageType.None;

                if (result.ResultType == ValidationResultType.Error)
                {
                    messageType = UnityEditor.MessageType.Error;
                }
                else if (result.ResultType == ValidationResultType.Warning)
                {
                    messageType = UnityEditor.MessageType.Warning;
                }
                else if (result.ResultType == ValidationResultType.Valid && !string.IsNullOrEmpty(result.Message))
                {
                    messageType = UnityEditor.MessageType.Info;
                }

                if (messageType != UnityEditor.MessageType.None)
                {
                    this.DrawMessageBoxWithButton(ref result[0], messageType, result[0].OnContextClick, i, resultsChanged);
                }
            }

            if (Event.current.type == EventType.Layout)
            {
                this.rerunFullValidation = false;
            }
            
            GUIUtility.GetControlID(this.uniqueIdHashForThisValidationDrawer, FocusType.Passive);
            this.CallNextDrawer(label);
            SirenixEditorGUI.EndShakeableGroup();
            GUILayout.EndVertical();
        }

        public void Dispose()
        {
            if (this.subscribedToEvents)
            {
                this.Property.Tree.OnUndoRedoPerformed -= this.OnUndoRedoPerformed;
                this.ValueEntry.OnValueChanged -= this.OnValueChanged;
                this.ValueEntry.OnChildValueChanged -= this.OnChildValueChanged;
            }

            this.issueFixerTrees.Values.ForEach(tree => tree?.Dispose());
            this.metaDataTrees.Values.ForEach(tree => tree?.Dispose());
            this.issueFixerTrees = null;
            this.metaDataTrees = null;
            this.validationResults = null;
            this.oldValidationResults = null;
        }

        private void OnUndoRedoPerformed()
        {
            this.rerunFullValidation = true;
        }

        private void OnValueChanged(int index)
        {
            this.rerunFullValidation = true;
        }

        private void OnChildValueChanged(int index)
        {
            this.rerunFullValidation = true;
        }

        private void DrawMessageBoxWithButton(ref ResultItem entry, MessageType messageType, Action<GenericMenu> onContextClick, int index, bool resultsChanged)
        {
            Texture icon = this.defaultIconTexture;

            // NOTE(Antonio): Apparently calling Graphics.Blit isn't allowed anymore during layout events?
            if (Event.current.type != EventType.Layout)
            {
                switch (messageType)
                {
                    case MessageType.Info:
                        icon = EditorIcons.UnityInfoIcon;
                        break;
                    case MessageType.Warning:
                        icon = EditorIcons.UnityWarningIcon;
                        break;
                    case MessageType.Error:
                        icon = EditorIcons.UnityErrorIcon;
                        break;
                    default:
                        icon = null;
                        break;
                }
            }

            int btnCount = 0;
            PropertyTree metaDataTree = null;
            int firstButtonIndex = -1;
            
            if (this.odinValidatorIsInstalled && entry.MetaData != null && entry.MetaData.Length > 0)
            {
                this.metaDataTrees.TryGetValue(index, out metaDataTree);

                for (int i = 0; i < entry.MetaData.Length; i++)
                {
                    if (entry.MetaData[i].Value is Action)
                    {
                        if (btnCount == 0)
                            firstButtonIndex = i;

                        btnCount++;
                    }
                }

                if (metaDataTree == null && btnCount < entry.MetaData.Length)
                {
                    metaDataTree = PropertyTree.Create(new ResultItemMetaDataDrawer(entry.MetaData, btnCount == 1));
                    this.metaDataTrees.Add(index, metaDataTree);
                }

                if (metaDataTree != null && resultsChanged)
                {
                    metaDataTree.Dispose();
                    metaDataTree = PropertyTree.Create(new ResultItemMetaDataDrawer(entry.MetaData, btnCount == 1));
                    this.metaDataTrees[index] = metaDataTree;
                }
            }
            
            var entireBox = SirenixEditorGUI.BeginVerticalWithoutUsingControlID(SirenixGUIStyles.MessageBox);
            if (btnCount > 0)
            {
                SirenixEditorGUI.BeginHorizontalWithoutUsingControlID(SirenixGUIStyles.None);
            }
            var messageRect = SirenixEditorGUI.BeginVerticalWithoutUsingControlID(SirenixGUIStyles.None);
            GUILayout.Label(GUIHelper.TempContent(entry.Message, icon), this.MessageBoxText);
            EditorGUILayout.EndVertical();
            if (btnCount > 0)
            {
                ref var firstButton = ref entry.MetaData[firstButtonIndex];
                var btnWidth = GUI.skin.button.CalcSize(GUIHelper.TempContent(firstButton.Name)).x + 10;

                SirenixEditorGUI.BeginVerticalWithoutUsingControlID(SirenixGUIStyles.None, GUILayoutOptions.Width(btnWidth));
                GUILayout.FlexibleSpace();

                if (this.DrawButtonWithoutUsingControlID(firstButton.Name, btnWidth))
                {
                    (firstButton.Value as Action)();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (!this.odinValidatorIsInstalled)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            var showFix = entry.Fix != null && entry.Fix.OfferInInspector;
            PropertyTree fixTree = null;
            
            if (showFix)
            {
                var fixTitle = new GUIContent(entry.Fix.Title ?? "Fix");
                var fixRect = EditorGUILayout.GetControlRect(false, 20f).Expand(4f, 4f, 0f, 3f);
                var message = entry.Message;

                SirenixEditorGUI.DrawBorders(fixRect, 0, 0, 1, 1);

                const float buttonSize = 85f;
                var fixTitleRect = fixRect.AlignLeft(fixRect.width - buttonSize).HorizontalPadding(6f);
                GUI.Label(fixTitleRect, fixTitle, SirenixGUIStyles.Label);

                var buttonRect = fixRect.AlignRight(buttonSize);
                var paddedButtonRect = buttonRect.HorizontalPadding(6f);
                var evt = Event.current;

                EditorGUI.DrawRect(buttonRect.Padding(1f), buttonRect.Contains(evt.mousePosition)
                    ? HighlightedBgColor
                    : Color.clear);

                GUI.Label(paddedButtonRect.AlignRight(50f), "Fix now", SirenixGUIStyles.LabelCentered);
                SdfIcons.DrawIcon(paddedButtonRect.AlignLeft(20f).Padding(3f), SdfIconType.Tools);
                EditorGUI.DrawRect(buttonRect.AlignLeft(1f), SirenixGUIStyles.BorderColor);

                var fixHasArguments = entry.Fix.ArgType != null;
                this.issueFixerTrees.TryGetValue(index, out fixTree);

                if (fixHasArguments && fixTree == null)
                {
                    var editorObject = entry.Fix.CreateEditorObject();
                    fixTree = PropertyTree.Create(editorObject);
                    this.issueFixerTrees.Add(index, fixTree);
                }

                if (evt.OnMouseUp(buttonRect, 0))
                {
                    this.Property.RecordForUndo(fixTitle.text, true);

                    if (fixHasArguments)
                    {
                        var args = fixTree.WeakTargets[0];
                        entry.Fix.Action.DynamicInvoke(args);
                    }
                    else
                    {
                        entry.Fix.Action.DynamicInvoke();
                    }

                    GUIHelper.ExitGUI(true);
                }
            }
            
            if (Event.current.OnMouseUp(messageRect, 1))
            {
                var menu = new GenericMenu();
                var message = entry.Message;
                menu.AddItem(new GUIContent("Copy message"), false, () => { Clipboard.Copy(message); });
                onContextClick?.Invoke(menu);
                menu.ShowAsContext();
                Event.current.Use();
            }
            
            if (fixTree != null)
            {
                EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
                fixTree.Draw(false);
                EditorGUILayout.EndVertical();
            }

            if (metaDataTree != null)
            {
                if (fixTree != null)
                {
                    GUILayout.Space(2);
                    var header = SirenixEditorGUI.BeginToolbarBoxHeader();
                    EditorGUI.DrawRect(header.AlignTop(1f).SetWidth(entireBox.width).SetX(entireBox.x), SirenixGUIStyles.BorderColor);
                    GUILayout.Label("Metadata");
                    SirenixEditorGUI.EndToolbarBoxHeader();
                }

                EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
                metaDataTree.Draw(false);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateIssueFixerTrees(List<ValidationResult> results)
        {
            this.issueFixerTrees.Values.ForEach(tree => tree?.Dispose());
            this.issueFixerTrees.Clear();

            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var fix = result[0].Fix;
                var fixHasArguments = fix?.ArgType != null;

                if (!fixHasArguments) continue;

                var editorObject = fix.CreateEditorObject();
                var fixTree = PropertyTree.Create(editorObject);
                this.issueFixerTrees.Add(i, fixTree);
            }
        }

        private bool DrawButtonWithoutUsingControlID(string name, float width)
        {
            var btnRect = GUILayoutUtility.GetRect(width, 19f, GUILayoutOptions.ExpandWidth(false).ExpandHeight(false));

            if (Event.current.OnMouseUp(btnRect, 0))
            {
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                SirenixGUIStyles.Button.Draw(
                    btnRect,
                    name,
                    Event.current.IsHovering(btnRect),
                    false,
                    false,
                    false);
            }

            return false;
        }
    }
}
#endif