//-----------------------------------------------------------------------
// <copyright file="SuffixLabelExamples.cs" company="Sirenix ApS">
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

	using UnityEngine;

    [AttributeExample(typeof(SuffixLabelAttribute),
        "The SuffixLabel attribute draws a label at the end of a property. " +
        "It's useful for conveying intend about a property.")]
    internal class SuffixLabelExamples
	{
		[SuffixLabel("Prefab")]
		public GameObject GameObject;

		[Space(15)]
		[InfoBox(
            "Using the Overlay property, the suffix label will be drawn on top of the property instead of behind it.\n" +
			"Use this for a neat inline look.")]
		[SuffixLabel("ms", Overlay = true)]
		public float Speed;

		[SuffixLabel("radians", Overlay = true)]
		public float Angle;

		[Space(15)]
		[InfoBox("The Suffix attribute also supports referencing a member string field, property, or method by using $.")]
		[SuffixLabel("$Suffix", Overlay = true)]
		public string Suffix = "Dynamic suffix label";

        [InfoBox("The Suffix attribute also supports expressions by using @.")]
        [SuffixLabel("@DateTime.Now.ToString(\"HH:mm:ss\")", true)]
        public string Expression;

        [SuffixLabel("Suffix with icon", SdfIconType.HeartFill)]
        public string IconAndText1;

        [SuffixLabel(SdfIconType.HeartFill)]
        public string OnlyIcon1;

        [SuffixLabel("Suffix with icon", SdfIconType.HeartFill, Overlay = true)]
        public string IconAndText2;

        [SuffixLabel(SdfIconType.HeartFill, Overlay = true)]
        public string OnlyIcon2;
    }
}
#endif