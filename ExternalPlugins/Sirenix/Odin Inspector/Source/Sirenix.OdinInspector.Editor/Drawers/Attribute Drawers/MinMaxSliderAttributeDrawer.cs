//-----------------------------------------------------------------------
// <copyright file="MinMaxSliderAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws Vector2 properties marked with <see cref="MinMaxSliderAttribute"/>.
    /// </summary>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    public sealed class MinMaxSliderAttributeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, Vector2>
    {
        private ValueResolver<double> minGetter;
        private ValueResolver<double> maxGetter;
        private ValueResolver<Vector2> rangeGetter;

        protected override void Initialize()
        {
            if (this.Attribute.MinMaxValueGetter != null)
            {
                this.rangeGetter = ValueResolver.Get<Vector2>(this.Property, this.Attribute.MinMaxValueGetter, new Vector2(this.Attribute.MinValue, this.Attribute.MaxValue));
            }
            else
            {
                this.minGetter = ValueResolver.Get<double>(this.Property, this.Attribute.MinValueGetter, this.Attribute.MinValue);
                this.maxGetter = ValueResolver.Get<double>(this.Property, this.Attribute.MaxValueGetter, this.Attribute.MaxValue);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Vector2 range;

            if (this.rangeGetter != null)
                range = this.rangeGetter.GetValue();
            else
                range = new Vector2((float)this.minGetter.GetValue(), (float)this.maxGetter.GetValue());

            EditorGUI.BeginChangeCheck();
            var value = SirenixEditorFields.MinMaxSlider(label, this.ValueEntry.SmartValue, range, this.Attribute.ShowFields);
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = value;
            }
        }
    }
}
#endif