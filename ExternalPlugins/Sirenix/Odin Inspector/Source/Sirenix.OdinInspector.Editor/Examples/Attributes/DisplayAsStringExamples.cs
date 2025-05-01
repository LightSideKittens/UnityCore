//-----------------------------------------------------------------------
// <copyright file="DisplayAsStringExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DisplayAsStringAttribute))]
    internal class DisplayAsStringExamples
    {
        [InfoBox(
            "Instead of disabling values in the inspector in order to show some information or debug a value. " +
            "You can use DisplayAsString to show the value as text, instead of showing it in a disabled drawer")]
        [DisplayAsString]
        public Color SomeColor;

        [BoxGroup("SomeBox")]
        [HideLabel]
        [DisplayAsString]
        public string SomeText = "Lorem Ipsum";

		[InfoBox("The DisplayAsString attribute can also be configured to enable or disable overflowing to multiple lines.")]
		[HideLabel]
		[DisplayAsString]
		public string Overflow = "A very very very very very very very very very long string that has been configured to overflow.";

		[HideLabel]
		[DisplayAsString(false)]
		public string DisplayAllOfIt = "A very very very very very very very very long string that has been configured to not overflow.";

		[InfoBox("Additionally, you can also configure the string's alignment, font size, and whether it should support rich text or not.")]
		[DisplayAsString(false, 20, TextAlignment.Center, true)]
		public string CustomFontSizeAlignmentAndRichText = "This string is <b><color=#FF5555><i>super</i> <size=24>big</size></color></b> and centered.";
    }
}
#endif