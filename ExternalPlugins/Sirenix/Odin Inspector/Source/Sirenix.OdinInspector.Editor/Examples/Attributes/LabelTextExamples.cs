//-----------------------------------------------------------------------
// <copyright file="LabelTextExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(LabelTextAttribute), "Specify a different label text for your properties.")]
    internal class LabelTextExamples
    {
        [LabelText("1")]
        public int MyInt1 = 1;

        [LabelText("2")]
        public int MyInt2 = 12;

        [LabelText("3")]
        public int MyInt3 = 123;

		[InfoBox("Use $ to refer to a member string.")]
		[LabelText("$MyInt3")]
		public string LabelText = "The label is taken from the number 3 above";

		[InfoBox("Use @ to execute an expression.")]
        [LabelText("@DateTime.Now.ToString(\"HH:mm:ss\")")]
        public string DateTimeLabel;

        [LabelText("Test", SdfIconType.HeartFill)]
        public int LabelIcon1 = 123;

        [LabelText("", SdfIconType.HeartFill)]
        public int LabelIcon2 = 123;
    }
}
#endif