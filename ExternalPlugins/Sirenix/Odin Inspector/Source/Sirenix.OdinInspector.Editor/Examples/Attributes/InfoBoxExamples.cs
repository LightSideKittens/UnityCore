//-----------------------------------------------------------------------
// <copyright file="InfoBoxExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(InfoBoxAttribute),
        "This example demonstrates the use of the InfoBox attribute.\n" +
        "Any info box with a warning or error drawn in the inspector will also be found by the Scene Validation tool.")]
    internal class InfoBoxExamples
    {
        [Title("InfoBox message types")]
        [InfoBox("Default info box.")]
        public int A;

        [InfoBox("Warning info box.", InfoMessageType.Warning)]
        public int B;

        [InfoBox("Error info box.", InfoMessageType.Error)]
        public int C;

        [InfoBox("Info box without an icon.", InfoMessageType.None)]
        public int D;

        [Title("Conditional info boxes")]
        public bool ToggleInfoBoxes;

        [InfoBox("This info box is only shown while in editor mode.", InfoMessageType.Error, "IsInEditMode")]
        public float G;

        [InfoBox("This info box is hideable by a static field.", "ToggleInfoBoxes")]
        public float E;

        [InfoBox("This info box is hideable by a static field.", "ToggleInfoBoxes")]
        public float F;

		[Title("Info box member reference and attribute expressions")]
		[InfoBox("$InfoBoxMessage")]
        [InfoBox("@\"Time: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
		public string InfoBoxMessage = "My dynamic info box message";

        private static bool IsInEditMode()
        {
            return !Application.isPlaying;
        }
    }
}
#endif