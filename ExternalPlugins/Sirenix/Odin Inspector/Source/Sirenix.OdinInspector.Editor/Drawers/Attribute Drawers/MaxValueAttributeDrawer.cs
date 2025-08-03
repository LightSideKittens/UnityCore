//-----------------------------------------------------------------------
// <copyright file="MaxValueAttributeDrawer.cs" company="Sirenix ApS">
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
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(0, 9000, 0)]
    public sealed class MaxValueAttributeDrawer<T> : OdinAttributeDrawer<MaxValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> maxValueGetter;

        public override bool CanDrawTypeFilter(Type type)
        {
            return IsNumber || IsVector;
        }

        protected override void Initialize()
        {
            this.maxValueGetter = ValueResolver.Get<double>(this.Property, this.Attribute.Expression, this.Attribute.MaxValue);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.maxValueGetter.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.maxValueGetter.ErrorMessage);
                this.CallNextDrawer(label);
            }
            else
            {
                this.CallNextDrawer(label);
                T value = this.ValueEntry.SmartValue;
                var max = this.maxValueGetter.GetValue();
                this.ValueEntry.SmartValue = GenericNumberUtility.Clamp(value, double.MinValue, max);
            }
        }
    }
}
#endif