//-----------------------------------------------------------------------
// <copyright file="DefaultMethodDrawer.cs" company="Sirenix ApS">
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

    using ActionResolvers;
    using ValueResolvers;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using Utilities.Editor;
    using UnityEngine.Rendering;

    /// <summary>
    /// The default method drawer that draws most buttons.
    /// </summary>
    [DrawerPriority(0, 0, 0.11)]
    public sealed class DefaultMethodDrawer : MethodDrawer
    {
        internal static bool DontDrawMethodParameters;

        private bool drawParameters;
        private bool hasReturnValue;
        private bool shouldDrawResult;
        private string name;
        private ButtonAttribute buttonAttribute;
        private GUIStyle style;
        private GUIStyle toggleBtnStyle;
        private ValueResolver<string> labelGetter;
        private GUIContent label;
        private ButtonStyle btnStyle;
        private bool expanded;
        private Color btnColor;
        private bool hasGUIColorAttribute;
        private bool hasInvokedOnce;
        private ActionResolver buttonActionResolver;
        private ValueResolver<object> buttonValueResolver;
        private bool hideLabel;
        private ValueResolver<string> tooltipValueResolver;
        private int buttonHeight;
        private float buttonAlignment;
        private IconAlignment buttonIconAlignment;
        private bool stretch;
        private bool drawnByGroup;

        [ShowOdinSerializedPropertiesInInspector]
        private class MethodResultInspector
        {
            [HideReferenceObjectPicker, HideLabel]
            public object Value;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.expanded = false;
            this.buttonAttribute = this.Property.GetAttribute<ButtonAttribute>();
            this.hasGUIColorAttribute = this.Property.GetAttribute<GUIColorAttribute>() != null;
            this.drawParameters = this.Property.Children.Count > 0 && !DontDrawMethodParameters && (this.buttonAttribute == null || this.buttonAttribute.DisplayParameters);
            this.hasReturnValue = this.Property.Children.Count > 0 && this.Property.Children[this.Property.Children.Count - 1].Name == ButtonParameterPropertyResolver.RETURN_VALUE_NAME;
            this.hideLabel = this.Property.Attributes.HasAttribute<HideLabelAttribute>() || this.buttonAttribute?.Name == string.Empty;

            var tooltip = this.Property.GetAttribute<PropertyTooltipAttribute>()?.Tooltip;

            if (tooltip != null)
            {
                this.tooltipValueResolver = ValueResolver.GetForString(this.Property, tooltip);
            }

            this.buttonHeight = this.Property.Context.GetGlobal("ButtonHeight", this.buttonAttribute != null && this.buttonAttribute.HasDefinedButtonHeight
                ? this.buttonAttribute.ButtonHeight
                : GeneralDrawerConfig.Instance.ButtonHeight).Value;
            this.buttonHeight = Mathf.Max(this.buttonHeight, (int)EditorGUIUtility.singleLineHeight);

            this.buttonIconAlignment = this.Property.Context.GetGlobal("IconAlignment", this.buttonAttribute != null && this.buttonAttribute.HasDefinedButtonIconAlignment
                ? this.buttonAttribute.IconAlignment
                : GeneralDrawerConfig.Instance.ButtonIconAlignment).Value;

            this.buttonAlignment = this.Property.Context.GetGlobal("ButtonAlignment", this.buttonAttribute != null && this.buttonAttribute.HasDefinedButtonAlignment
                ? this.buttonAttribute.ButtonAlignment
                : GeneralDrawerConfig.Instance.ButtonAlignment).Value;

            this.stretch = this.Property.Context.GetGlobal("StretchButton", this.buttonAttribute != null && this.buttonAttribute.HasDefinedStretch
                ? this.buttonAttribute.Stretch
                : GeneralDrawerConfig.Instance.StretchButtons).Value;

            this.style = this.Property.Context.GetGlobal("ButtonStyle", this.buttonHeight > 20 ? SirenixGUIStyles.Button : EditorStyles.miniButton).Value;

            this.drawnByGroup = this.Property.Context.GetGlobal("DrawnByGroup", false).Value;

            this.shouldDrawResult = GeneralDrawerConfig.Instance.ShowButtonResultsByDefault;

            if (this.buttonAttribute != null)
            {
                if (!this.buttonAttribute.DisplayParameters)
                {
                    if (this.hasReturnValue)
                    {
                        this.buttonValueResolver = ValueResolver.Get<object>(this.Property, null);
                    }
                    else
                    {
                        this.buttonActionResolver = ActionResolver.Get(this.Property, null);
                    }
                }

                this.btnStyle = this.buttonAttribute.Style;
                this.expanded = this.buttonAttribute.Expanded;

                if (!string.IsNullOrEmpty(this.buttonAttribute.Name))
                {
                    this.labelGetter = ValueResolver.GetForString(this.Property, this.buttonAttribute.Name);
                }

                if (this.buttonAttribute.DrawResultIsSet)
                {
                    // The attribute always overrides the global default setting if the value is set there
                    this.shouldDrawResult = this.buttonAttribute.DrawResult;
                }
            }

            if (!this.shouldDrawResult && this.hasReturnValue && this.Property.Children.Count == 1)
            {
                // If we only have the result property as a child, and we shouldn't draw the result,
                //   then of course we don't want to draw the button as a foldout with parameters, so
                //   we disable that to draw it as a "normal" parameterless, result-less button.
                this.drawParameters = false;
            }

            if (this.drawParameters && this.btnStyle == ButtonStyle.FoldoutButton && !this.expanded)
            {
                if (this.buttonHeight > 20)
                {
                    this.style = SirenixGUIStyles.ButtonLeft;
                    this.toggleBtnStyle = SirenixGUIStyles.ButtonRight;
                }
                else
                {
                    this.style = EditorStyles.miniButtonLeft;
                    this.toggleBtnStyle = EditorStyles.miniButtonRight;
                }
            }
        }

        /// <summary>
        /// Draws the property layout.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent lbl)
        {
            if (this.buttonActionResolver != null && this.buttonActionResolver.HasError)
            {
                this.buttonActionResolver.DrawError();
            }

            if (this.buttonValueResolver != null && this.buttonValueResolver.HasError)
            {
                this.buttonValueResolver.DrawError();
            }

            this.labelGetter?.DrawError();
            this.tooltipValueResolver?.DrawError();

            this.label = new GUIContent(this.hideLabel ? "" : this.labelGetter != null ? this.labelGetter.GetValue() : this.Property.NiceName);

            if (this.tooltipValueResolver != null)
                this.label.tooltip = this.tooltipValueResolver.GetValue() ?? "";
            else if (!string.IsNullOrEmpty(lbl?.tooltip))
                this.label.tooltip = lbl?.tooltip;
            else
                this.label.tooltip = this.label.text;

            this.Property.Label = this.label;

            this.btnColor = GUI.color;
            var contentColor = this.hasGUIColorAttribute ? GUIColorAttributeDrawer.CurrentOuterColor : this.btnColor;
            GUIHelper.PushColor(contentColor);

            var h = this.Property.Context.GetGlobal("ButtonHeight", 0).Value;
            var s = this.Property.Context.GetGlobal("ButtonStyle", (GUIStyle)null).Value;

            if (this.buttonHeight != h && h != 0 || s != null && this.style != s)
            {
                this.Initialize();
            }

            if (!this.drawParameters)
            {
                this.DrawNormalButton();
            }
            else if (this.btnStyle == ButtonStyle.FoldoutButton)
            {
                if (this.expanded)
                {
                    this.DrawNormalButton();
                    EditorGUI.indentLevel++;
                    this.DrawParameters(false);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    this.DrawFoldoutButton();
                }
            }
            else if (this.btnStyle == ButtonStyle.CompactBox)
            {
                this.DrawCompactBoxButton();
            }
            else if (this.btnStyle == ButtonStyle.Box)
            {
                this.DrawBoxButton();
            }

            GUIHelper.PopColor();
        }

        private void DrawBoxButton()
        {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginToolbarBoxHeader();

            if (this.expanded)
            {
                EditorGUILayout.LabelField(this.label);
            }
            else
            {
                this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, this.label);
            }

            SirenixEditorGUI.EndToolbarBoxHeader();
            this.DrawParameters(true);
            SirenixEditorGUI.EndToolbarBox();
        }

        private void DrawCompactBoxButton()
        {
            var hasIcon = this.buttonAttribute != null && this.buttonAttribute.HasDefinedIcon;

            SirenixEditorGUI.BeginBox();
            var rect = SirenixEditorGUI.BeginToolbarBoxHeader().AlignRight(hasIcon ? 90 : 70).Padding(1f);
            rect.height -= 1f;

            GUIHelper.PushColor(this.btnColor);
            var invokeLabel = new GUIContent(this.hideLabel ? "" : "Invoke", this.label.tooltip);

            if (hasIcon)
            {
                if (SirenixEditorGUI.SDFIconButton(rect, invokeLabel, this.buttonAttribute.Icon, this.buttonIconAlignment))
                {
                    this.InvokeButton();
                }
            }
            else if (GUI.Button(rect, invokeLabel))
            {
                this.InvokeButton();
            }

            GUIHelper.PopColor();

            if (this.expanded)
            {
                EditorGUILayout.LabelField(this.label);
            }
            else
            {
                this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, this.label);
            }

            SirenixEditorGUI.EndToolbarBoxHeader();
            this.DrawParameters(false);
            SirenixEditorGUI.EndToolbarBox();
        }

        private void DrawNormalButton()
        {
            SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(this.label?.text, this.style, this.buttonAttribute?.HasDefinedIcon ?? false, this.buttonHeight, out _, out _, out _, out var minimalButtonWidth);

            var btnRect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0, this.buttonHeight, GUILayout.MinWidth(minimalButtonWidth)));

            if (!this.stretch && !this.drawnByGroup)
            {
                var availableEmptySpace = btnRect.width - minimalButtonWidth;
                btnRect.x += this.buttonAlignment * availableEmptySpace;
                btnRect.width = minimalButtonWidth;
            }

            GUIHelper.PushColor(this.btnColor);
            if (SirenixEditorGUI.SDFIconButton(btnRect, this.label, this.buttonAttribute?.Icon ?? SdfIconType.None, this.buttonIconAlignment, this.style))
            {
                this.InvokeButton();
            }
            GUIHelper.PopColor();
        }

        private void DrawFoldoutButton()
        {
            var btnRect = GUILayoutUtility.GetRect(GUIContent.none, this.style, GUILayoutOptions.Height(this.buttonHeight));
            btnRect = EditorGUI.IndentedRect(btnRect);

            GUIHelper.PushColor(this.btnColor);

            var foldoutRect = btnRect.AlignRight(20);
            if (GUI.Button(foldoutRect, GUIContent.none, this.toggleBtnStyle))
            {
                this.Property.State.Expanded = !this.Property.State.Expanded;
            }

            btnRect.width -= foldoutRect.width;
            if (!this.Property.State.Expanded)
            {
                foldoutRect.x -= 1;
                foldoutRect.yMin -= 1;
            }

            if (this.Property.State.Expanded) EditorIcons.TriangleDown.Draw(foldoutRect, 16);
            else EditorIcons.TriangleLeft.Draw(foldoutRect, 16);

            if (this.buttonAttribute != null && this.buttonAttribute.HasDefinedIcon)
            {
                if (SirenixEditorGUI.SDFIconButton(btnRect, this.label, this.buttonAttribute.Icon, this.buttonIconAlignment, this.style))
                {
                    this.InvokeButton();
                }
            }
            else
            {
                if (GUI.Button(btnRect, this.label, this.style))
                {
                    this.InvokeButton();
                }
            }

            GUIHelper.PopColor();

            EditorGUI.indentLevel++;
            this.DrawParameters(false);
            EditorGUI.indentLevel--;
        }

        private void DrawParameters(bool appendButton)
        {
            if (SirenixEditorGUI.BeginFadeGroup(this, this.Property.State.Expanded || this.expanded))
            {
                GUILayout.Space(0);
                for (int i = 0; i < this.Property.Children.Count; i++)
                {
                    bool isResult = false;

                    if (this.hasReturnValue && i == this.Property.Children.Count - 1)
                    {
                        if (!this.shouldDrawResult)
                        {
                            // Don't draw the result if we shouldn't
                            break;
                        }

                        if (!this.hasInvokedOnce && i != 0)
                        {
                            // Don't draw the result if we have parameters and we haven't invoked the method yet
                            // If there's just the result type, it makes more sense to actually draw the result,
                            //   otherwise the UX feels weird.
                            break;
                        }

                        if (i != 0)
                        {
                            SirenixEditorGUI.DrawThickHorizontalSeparator();
                        }

                        isResult = true;
                    }

                    if (isResult && !this.hasInvokedOnce)
                    {
                        // Disable the result if we're drawing it, but haven't invoked the method yet
                        GUIHelper.PushGUIEnabled(false);
                    }

                    this.Property.Children[i].Draw();

                    if (isResult && !this.hasInvokedOnce)
                    {
                        GUIHelper.PopGUIEnabled();
                    }
                }

                if (appendButton)
                {
                    var rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.BottomBoxPadding).Expand(3);
                    SirenixEditorGUI.DrawHorizontalLineSeperator(rect.x, rect.y, rect.width);
                    this.DrawNormalButton();
                    EditorGUILayout.EndVertical();
                }
            }
            SirenixEditorGUI.EndFadeGroup();
        }

        private void InvokeButton()
        {
            try
            {
                bool inspectResultInDropdown = this.hasReturnValue && Event.current.button == 1;

                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();

                if (((this.Property.Info.GetMemberInfo() as MethodInfo) ?? this.Property.Info.GetMethodDelegate().Method).IsGenericMethodDefinition)
                {
                    Debug.LogError("Cannot invoke a generic method definition.");
                    return;
                }

                if (this.buttonAttribute == null || this.buttonAttribute.DirtyOnClick)
                {
                    if (this.Property.ParentValueProperty != null)
                    {
                        this.Property.ParentValueProperty.RecordForUndo("Clicked Button '" + this.Property.NiceName + "'", true);
                    }

                    foreach (var target in this.Property.SerializationRoot.ValueEntry.WeakValues.OfType<UnityEngine.Object>())
                    {
                        InspectorUtilities.RegisterUnityObjectDirty(target);
                    }
                }

                if (this.buttonActionResolver != null)
                {
                    this.buttonActionResolver.DoActionForAllSelectionIndices();
                }
                else if (this.buttonValueResolver != null)
                {
                    for (int i = 0; i < this.Property.Tree.WeakTargets.Count; i++)
                    {
                        object result = this.buttonValueResolver.GetValue(i);
                        this.Property.Children[this.Property.Children.Count - 1].ValueEntry.WeakValues[i] = result;
                    }
                }
                else
                {
                    var methodInfo = (MethodInfo)this.Property.Info.GetMemberInfo();
                    if (methodInfo != null)
                    {
                        this.InvokeMethodInfo(methodInfo);
                    }
                    else
                    {
                        this.InvokeDelegate();
                    }
                }

                if (inspectResultInDropdown)
                {
                    object resultValue = this.Property.Children[this.Property.Children.Count - 1].ValueEntry.WeakSmartValue;
                    OdinEditorWindow.InspectObjectInDropDown(new MethodResultInspector() { Value = resultValue });
                }
            }
            finally
            {
                GUIHelper.ExitGUI(true);
            }
        }

        private void InvokeDelegate()
        {
            try
            {
                var argCount = this.hasReturnValue ? this.Property.Children.Count - 1 : this.Property.Children.Count;
                var arguments = new object[argCount];
                for (int j = 0; j < arguments.Length; j++)
                {
                    arguments[j] = this.Property.Children[j].ValueEntry.WeakSmartValue;
                }
                object result = this.Property.Info.GetMethodDelegate().DynamicInvoke(arguments);

                for (int j = 0; j < arguments.Length; j++)
                {
                    this.Property.Children[j].ValueEntry.WeakSmartValue = arguments[j];
                }

                if (this.hasReturnValue)
                {
                    this.Property.Children[this.Property.Children.Count - 1].ValueEntry.WeakSmartValue = result;
                }

                if (!this.hasInvokedOnce)
                {
                    this.Property.Tree.DelayActionUntilRepaint(() => this.hasInvokedOnce = true);
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.IsExitGUIException())
                {
                    throw ex.AsExitGUIException();
                }

                Debug.LogException(ex);
            }
            catch (ExitGUIException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                if (ex.IsExitGUIException())
                {
                    throw ex.AsExitGUIException();
                }

                Debug.LogException(ex);
            }
        }

        private void InvokeMethodInfo(MethodInfo methodInfo)
        {
            var parentValueProperty = this.Property.ParentValueProperty;
            var targets = this.Property.ParentValues;
            var argCount = this.hasReturnValue ? this.Property.Children.Count - 1 : this.Property.Children.Count;

            for (int i = 0; i < targets.Count; i++)
            {
                object value = targets[i];

                if (ReferenceEquals(value, null) == false || methodInfo.IsStatic)
                {
                    try
                    {
                        var arguments = new object[argCount];
                        for (int j = 0; j < arguments.Length; j++)
                        {
                            arguments[j] = this.Property.Children[j].ValueEntry.WeakSmartValue;
                        }

                        object result;

                        if (methodInfo.IsStatic)
                        {
                            result = methodInfo.Invoke(null, arguments);
                        }
                        else
                        {
                            result = methodInfo.Invoke(value, arguments);
                        }

                        for (int j = 0; j < arguments.Length; j++)
                        {
                            this.Property.Children[j].ValueEntry.WeakSmartValue = arguments[j];
                        }

                        if (this.hasReturnValue)
                        {
                            this.Property.Children[this.Property.Children.Count - 1].ValueEntry.WeakSmartValue = result;
                        }

                        if (!this.hasInvokedOnce)
                        {
                            this.Property.Tree.DelayActionUntilRepaint(() => this.hasInvokedOnce = true);
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.IsExitGUIException())
                        {
                            throw ex.AsExitGUIException();
                        }

                        Debug.LogException(ex);
                    }
                    catch (ExitGUIException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsExitGUIException())
                        {
                            throw ex.AsExitGUIException();
                        }

                        Debug.LogException(ex);
                    }

                    if (parentValueProperty != null && value.GetType().IsValueType)
                    {
                        // If it's a struct, it will have been boxed and the invoke call might
                        // have changed the struct and this won't be reflected in the original,
                        // unboxed source struct.

                        // Therefore, set the source value to the boxed struct that we just invoked on.
                        parentValueProperty.ValueEntry.WeakValues[i] = value;
                    }
                }
            }
        }
    }
}
#endif