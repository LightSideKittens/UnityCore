//-----------------------------------------------------------------------
// <copyright file="VerticalGroupExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(VerticalGroupAttribute),
        "VerticalGroup, similar to HorizontalGroup, groups properties together vertically in the inspector.\n" +
        "By itself it doesn't do much, but combined with other groups, like HorizontalGroup, it can be very useful. It can also be used in TableLists to create columns.")]
    internal class VerticalGroupExamples
	{
		[HorizontalGroup("Split")]
		[VerticalGroup("Split/Left")]
		public InfoMessageType First;

		[VerticalGroup("Split/Left")]
		public InfoMessageType Second;

		[HideLabel]
		[VerticalGroup("Split/Right")]
		public int A;

		[HideLabel]
		[VerticalGroup("Split/Right")]
		public int B;
	}
}
#endif