//-----------------------------------------------------------------------
// <copyright file="SceneObjectsOnlyAttribute.cs" company="Sirenix ApS">
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
    /// <para>SceneObjectsOnly is used on object properties, and restricts the property to scene objects, and not project assets.</para>
    /// <para>Use this when you want to ensure an object is a scene object, and not from a project asset.</para>
    /// </summary>
    /// <example>
	/// <para>The following example shows a component with a game object property, that must be from a scene, and not a prefab asset.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
	/// {
	///		[SceneObjectsOnly]
	///		public GameObject MyPrefab;
	/// }
    /// </code>
    /// </example>
	/// <seealso cref="AssetsOnlyAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class SceneObjectsOnlyAttribute : Attribute
    {
    }
}