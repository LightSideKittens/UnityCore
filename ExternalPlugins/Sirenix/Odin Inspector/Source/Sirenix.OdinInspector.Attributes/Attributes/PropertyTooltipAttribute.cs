//-----------------------------------------------------------------------
// <copyright file="PropertyTooltipAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>PropertyTooltip is used on any property, and creates tooltips for when hovering the property in the inspector.</para>
    /// <para>Use this to explain the purpose, or how to use a property.</para>
    /// </summary>
    /// <remarks>
    /// <para>This is similar to Unity's <see cref="UnityEngine.TooltipAttribute"/> but can be applied to both fields and properties.</para>
    /// </remarks>
    /// <example>
    /// <para>The following example shows how PropertyTooltip is applied to various properties.</para>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[PropertyTooltip("This is an int property.")]
    ///		public int MyField;
    ///
    ///		[ShowInInspector, PropertyTooltip("This is another int property.")]
    ///		public int MyProperty { get; set; }
    ///	}
    /// </code>
    /// </example>
    /// <seealso cref="UnityEngine.TooltipAttribute"/>
    /// <seealso cref="ShowInInspectorAttribute"/>
    /// <seealso cref="PropertySpaceAttribute"/>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="PropertyOrderAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class PropertyTooltipAttribute : Attribute
    {
        /// <summary>
        /// The message shown in the tooltip.
        /// </summary>
        public string Tooltip;

        /// <summary>
        /// Adds a tooltip to the property in the inspector.
        /// </summary>
        /// <param name="tooltip">The message shown in the tooltip.</param>
        public PropertyTooltipAttribute(string tooltip)
        {
            this.Tooltip = tooltip;
        }
    }
}