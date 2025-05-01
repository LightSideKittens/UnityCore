//-----------------------------------------------------------------------
// <copyright file="DisableContextMenuAttribute.cs" company="Sirenix ApS">
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
	/// <para>DisableContextMenu is used on any property and disables the context menu for that property.</para>
	/// <para>Use this if you do not want the context menu to be available for a property.</para>
    /// </summary>
	/// <example>
	/// <para>The following example shows how DisableContextMenu is used on a property.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[DisableContextMenu]
	///		public Vector3 MyVector;
	///	}
	/// </code>
    /// </example>
	/// <seealso cref="CustomContextMenuAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class DisableContextMenuAttribute : Attribute
    {
        /// <summary>
        /// Whether to disable the context menu for the member itself.
        /// </summary>
        public bool DisableForMember;

        /// <summary>
        /// Whether to disable the context menu for collection elements.
        /// </summary>
        public bool DisableForCollectionElements;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableContextMenuAttribute" /> class.
        /// </summary>
        /// <param name="disableForMember">Whether to disable the context menu for the member itself.</param>
        /// <param name="disableCollectionElements">Whether to also disable the context menu of collection elements.</param>
        public DisableContextMenuAttribute(bool disableForMember = true, bool disableCollectionElements = false)
        {
            this.DisableForMember = disableForMember;
            this.DisableForCollectionElements = disableCollectionElements;
        }
    }
}