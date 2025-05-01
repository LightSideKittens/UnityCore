//-----------------------------------------------------------------------
// <copyright file="HideInEditorModeAttribute.cs" company="Sirenix ApS">
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
    /// <para>HideInEditorMode is used on any property, and hides the property when not in play mode.</para>
    /// <para>Use this when you only want a property to only be visible play mode.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how HideInEditorMode is used to hide a property when in the editor.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[HideInEditorMode]
    ///		public int MyInt;
    ///	}
    /// </code>
    /// </example>
	/// <seealso cref="HideInPlayModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    [DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class HideInEditorModeAttribute : Attribute
	{ }
}