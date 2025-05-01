//-----------------------------------------------------------------------
// <copyright file="VisibleStateExample.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

	[AttributeExample(typeof(OnStateUpdateAttribute), "The following example shows how OnStateUpdate can be used to control the visible state of a property.")]
	internal class VisibleStateExample
	{
		[OnStateUpdate("@$property.State.Visible = ToggleMyInt")]
		public int MyInt;
		
		public bool ToggleMyInt;
	}
}
#endif