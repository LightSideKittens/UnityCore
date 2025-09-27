//-----------------------------------------------------------------------
// <copyright file="CustomValueDrawerAttributeDrawer.cs" company="Sirenix ApS">
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
    using Utilities.Editor;
    using UnityEngine;
    using System.Collections;
    using Sirenix.OdinInspector.Editor.ActionResolvers;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using ANamedValue = ActionResolvers.NamedValue;
    using VNamedValue = ValueResolvers.NamedValue;

    /// <summary>
    /// Draws properties marked with <see cref="ValidateInputAttribute"/>.
    /// </summary>
    /// <seealso cref="ValidateInputAttribute"/>

    [DrawerPriority(0, 0, double.MaxValue)]
    public class CustomValueDrawerAttributeDrawer<T> : OdinAttributeDrawer<CustomValueDrawerAttribute, T>
    {
        private ActionResolver customAction;
        private ValueResolver customValue;

        private static readonly VNamedValue[] customDrawerArgsValue = new VNamedValue[2]
        {
            new VNamedValue("label", typeof(GUIContent)),
            new VNamedValue("callNextDrawer", typeof(Func<GUIContent, bool>))
        };

        private static readonly ANamedValue[] customDrawerArgsAction = new ANamedValue[2]
        {
            new ANamedValue("label", typeof(GUIContent)),
            new ANamedValue("callNextDrawer", typeof(Func<GUIContent, bool>))
        };

        public override bool CanDrawTypeFilter(Type type)
        {
            return !typeof(IList).IsAssignableFrom(type);
        }

        protected override void Initialize()
        {
            this.customValue = ValueResolver.Get(this.ValueEntry.BaseValueType, this.Property, this.Attribute.Action, customDrawerArgsValue);

            if (!this.customValue.HasError)
            {
                this.customValue.Context.NamedValues.Set("callNextDrawer", (Func<GUIContent, bool>)this.CallNextDrawer);
            }
            
            else if (this.customValue.ErrorMessage.Contains("void"))
            {
                this.customAction = ActionResolver.Get(this.Property, this.Attribute.Action, customDrawerArgsAction);

                if (!this.customAction.HasError)
                {
                    this.customAction.Context.NamedValues.Set("callNextDrawer", (Func<GUIContent, bool>)this.CallNextDrawer);
                }
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.customAction != null)
            {
                if (this.customAction.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.customAction.ErrorMessage);
                    this.CallNextDrawer(label);
                }
                else
                {
                    this.customAction.Context.NamedValues.Set("label", label);
                    this.customAction.DoAction();
                }
            }
            else
            {
                if (this.customValue.ErrorMessage != null)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.customValue.ErrorMessage);
                    this.CallNextDrawer(label);
                }
                else
                {
                    this.customValue.Context.NamedValues.Set("label", label);
                    this.ValueEntry.SmartValue = (T)this.customValue.GetWeakValue();
                }
            }
        }
    }
}
#endif