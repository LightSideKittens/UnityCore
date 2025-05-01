//-----------------------------------------------------------------------
// <copyright file="HideMonoScriptAttribute.cs" company="Sirenix ApS">
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
    /// Apply HideMonoScript to your class to prevent the Script property from being shown in the inspector.
    /// <remarks>
    /// <para>This attribute has the same effect on a single type that the global configuration option "Show Mono Script In Editor" in "Preferences -> Odin Inspector -> General -> Drawers" has globally when disabled.</para>
    /// </remarks>
    /// </summary>
    /// <example>
    /// <para>The following example shows how to use this attribute.</para>
    /// <code>
    /// [HideMonoScript]
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     // The Script property will not be shown for this component in the inspector
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class HideMonoScriptAttribute : Attribute
    {
    }
}