//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainAttribute.cs" company="Sirenix ApS">
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
    /// ShowDrawerChain lists all prepend, append and value drawers being used in the inspector.
    /// This is great in situations where you want to debug, and want to know which drawers might be involved in drawing the property.
    /// </para>
    /// <para>Your own custom drawers are highlighted with a green label.</para>
    /// <para>Drawers, that have not been called during the draw chain, will be greyed out in the inspector to make it clear which drawers have had an effect on the properties.</para>
    /// </summary>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ShowDrawerChain]
    ///		public int IndentedInt;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowDrawerChainAttribute : Attribute
    {
    }
}