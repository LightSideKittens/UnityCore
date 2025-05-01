//-----------------------------------------------------------------------
// <copyright file="ToggleLeftAttribute.cs" company="Sirenix ApS">
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
    /// <para>Draws the checkbox before the label instead of after.</para>
    /// </summary>
    /// <remarks>ToggleLeftAttribute can be used an all fields and properties of type boolean</remarks>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ToggleLeft]
    ///		public bool MyBoolean;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class ToggleLeftAttribute : Attribute
    {
    }
}