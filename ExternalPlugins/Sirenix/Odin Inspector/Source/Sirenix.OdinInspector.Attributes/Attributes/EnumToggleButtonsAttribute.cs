//-----------------------------------------------------------------------
// <copyright file="EnumToggleButtonsAttribute.cs" company="Sirenix ApS">
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
    /// <para>Draws an enum in a horizontal button group instead of a dropdown.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyComponent : MonoBehvaiour
    /// {
    ///     [EnumToggleButtons]
    ///     public MyBitmaskEnum MyBitmaskEnum;
    ///
    ///     [EnumToggleButtons]
    ///     public MyEnum MyEnum;
    /// }
    ///
    /// [Flags]
    /// public enum MyBitmaskEnum
    /// {
    ///     A = 1 &lt;&lt; 1, // 1
    ///     B = 1 &lt;&lt; 2, // 2
    ///     C = 1 &lt;&lt; 3, // 4
    ///     ALL = A | B | C
    /// }
    ///
    /// public enum MyEnum
    /// {
    ///     A,
    ///     B,
    ///     C
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class EnumToggleButtonsAttribute : Attribute
    {
    }
}