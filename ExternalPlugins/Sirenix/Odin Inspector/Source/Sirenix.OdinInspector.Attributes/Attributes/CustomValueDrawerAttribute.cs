//-----------------------------------------------------------------------
// <copyright file="CustomValueDrawerAttribute.cs" company="Sirenix ApS">
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
    /// Instead of making a new attribute, and a new drawer, for a one-time thing, you can with this attribute, make a method that acts as a custom property drawer.
    /// These drawers will out of the box have support for undo/redo and multi-selection.
    /// </summary>
    /// <example>
    /// Usage:
    /// <code>
    /// public class CustomDrawerExamples : MonoBehaviour
    /// {
    ///     public float From = 2, To = 7;
    ///
    ///     [CustomValueDrawer("MyStaticCustomDrawerStatic")]
    ///     public float CustomDrawerStatic;
    ///
    ///     [CustomValueDrawer("MyStaticCustomDrawerInstance")]
    ///     public float CustomDrawerInstance;
    ///
    ///     [CustomValueDrawer("MyStaticCustomDrawerArray")]
    ///     public float[] CustomDrawerArray;
    ///
    /// #if UNITY_EDITOR
    ///
    ///     private static float MyStaticCustomDrawerStatic(float value, GUIContent label)
    ///     {
    ///         return EditorGUILayout.Slider(value, 0f, 10f);
    ///     }
    ///
    ///     private float MyStaticCustomDrawerInstance(float value, GUIContent label)
    ///     {
    ///         return EditorGUILayout.Slider(value, this.From, this.To);
    ///     }
    ///
    ///     private float MyStaticCustomDrawerArray(float value, GUIContent label)
    ///     {
    ///         return EditorGUILayout.Slider(value, this.From, this.To);
    ///     }
    ///
    /// #endif
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class CustomValueDrawerAttribute : Attribute
    {
        /// <summary>
        /// A resolved string that defines the custom drawer action to take, such as an expression or method invocation.
        /// </summary>
        public string Action;

        /// <summary>
        /// Instead of making a new attribute, and a new drawer, for a one-time thing, you can with this attribute, make a method that acts as a custom property drawer.
        /// These drawers will out of the box have support for undo/redo and multi-selection.
        /// </summary>
        /// <param name="action">A resolved string that defines the custom drawer action to take, such as an expression or method invocation.</param>
        public CustomValueDrawerAttribute(string action)
        {
            this.Action = action;
        }
    }
}