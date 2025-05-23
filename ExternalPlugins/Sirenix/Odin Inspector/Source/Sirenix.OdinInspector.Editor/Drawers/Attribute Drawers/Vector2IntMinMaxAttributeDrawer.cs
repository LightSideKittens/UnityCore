//-----------------------------------------------------------------------
// <copyright file="Vector2IntMinMaxAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#if UNITY_EDITOR

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws Vector2Int properties marked with <see cref="MinMaxSliderAttribute"/>.
    /// </summary>
    public class Vector2IntMinMaxAttributeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, Vector2Int>
    {
        private ValueResolver<float> minGetter;
        private ValueResolver<float> maxGetter;
        private ValueResolver<Vector2Int> vector2IntMinMaxGetter;

        /// <summary>
        /// Initializes the drawer by resolving any optional references to members for min/max value.
        /// </summary>
        protected override void Initialize()
        {
            // Min member reference.
            this.minGetter = ValueResolver.Get<float>(this.Property, this.Attribute.MinValueGetter, this.Attribute.MinValue);
            this.maxGetter = ValueResolver.Get<float>(this.Property, this.Attribute.MaxValueGetter, this.Attribute.MaxValue);

            // Min max member reference.
            if (this.Attribute.MinMaxValueGetter != null)
            {
                this.vector2IntMinMaxGetter = ValueResolver.Get<Vector2Int>(this.Property, this.Attribute.MinMaxValueGetter);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueResolver.DrawErrors(this.minGetter, this.maxGetter, this.vector2IntMinMaxGetter);

            // Get the range of the slider from the attribute or from member references.
            Vector2 range;
            if (this.vector2IntMinMaxGetter != null && !this.vector2IntMinMaxGetter.HasError)
            {
                range = (Vector2)this.vector2IntMinMaxGetter.GetValue();
            }
            else
            {
                range.x = this.minGetter.GetValue();
                range.y = this.maxGetter.GetValue();
            }

            EditorGUI.BeginChangeCheck();
            Vector2 value = SirenixEditorFields.MinMaxSlider(label, (Vector2)this.ValueEntry.SmartValue, range, this.Attribute.ShowFields);
            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.SmartValue = new Vector2Int((int)value.x, (int)value.y);
            }
        }
    }
}
#endif // UNITY_EDITOR 
#endif