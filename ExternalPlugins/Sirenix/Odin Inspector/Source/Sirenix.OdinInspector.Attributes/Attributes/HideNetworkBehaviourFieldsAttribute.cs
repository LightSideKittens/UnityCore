//-----------------------------------------------------------------------
// <copyright file="HideNetworkBehaviourFieldsAttribute.cs" company="Sirenix ApS">
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
    /// Apply HideNetworkBehaviourFields to your class to prevent the special "Network Channel" and "Network Send Interval" properties from being shown in the inspector for a NetworkBehaviour.
    /// This attribute has no effect on classes that are not derived from NetworkBehaviour.
    /// </summary>
    /// <example>
    /// <para>The following example shows how to use this attribute.</para>
    /// <code>
    /// [HideNetworkBehaviourFields]
    /// public class MyComponent : NetworkBehaviour
    /// {
    ///     // The "Network Channel" and "Network Send Interval" properties will not be shown for this component in the inspector
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class HideNetworkBehaviourFieldsAttribute : Attribute
    {
    }
}