//-----------------------------------------------------------------------
// <copyright file="ShowPropertyResolverAttribute.cs" company="Sirenix ApS">
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
    /// <para>
    /// ShowPropertyResolver shows the property resolver responsible for bringing the member into the property tree.
    /// This is useful in situations where you want to debug why a particular member that is normally not shown in the inspector suddenly is.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ShowPropertyResolver]
    ///		public int IndentedInt;
    ///	}
    /// </code>
    /// </example>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowPropertyResolverAttribute : Attribute
    {
    }
}