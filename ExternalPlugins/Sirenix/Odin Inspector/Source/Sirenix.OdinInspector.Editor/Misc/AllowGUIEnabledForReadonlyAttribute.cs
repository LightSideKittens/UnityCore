//-----------------------------------------------------------------------
// <copyright file="AllowGUIEnabledForReadonlyAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Some drawers don't want to have its GUI disabled, even if the property is read-only or a ReadOnly attribute is defined on the property.
    /// Use this attribute on any drawer to force GUI being enabled in these cases.
    /// </summary>
    /// <example>
    /// <code>
    ///
    /// [AllowGUIEnabledForReadonly]
    /// public sealed class SomeDrawerDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : class
    /// {
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowGUIEnabledForReadonlyAttribute : Attribute
    {
    }
}
#endif