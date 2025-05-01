//-----------------------------------------------------------------------
// <copyright file="EnumToggleButtonsExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(EnumToggleButtonsAttribute))]
    internal class EnumToggleButtonsExamples
    {
        [Title("Default")]
        public SomeBitmaskEnum DefaultEnumBitmask;

        [Title("Standard Enum")]
        [EnumToggleButtons]
        public SomeEnum SomeEnumField;

        [EnumToggleButtons, HideLabel]
        public SomeEnum WideEnumField;

        [Title("Bitmask Enum")]
        [EnumToggleButtons]
        public SomeBitmaskEnum BitmaskEnumField;

        [EnumToggleButtons, HideLabel]
        public SomeBitmaskEnum EnumFieldWide;

        [Title("Icon Enum")]
        [EnumToggleButtons, HideLabel]
        public SomeEnumWithIcons EnumWithIcons;

        [EnumToggleButtons, HideLabel]
        public SomeEnumWithIconsAndNames EnumWithIconsAndNames;

        public enum SomeEnum
        {
            First, Second, Third, Fourth, AndSoOn
        }

        public enum SomeEnumWithIcons
        {
            [LabelText(SdfIconType.TextLeft)] TextLeft,
            [LabelText(SdfIconType.TextCenter)] TextCenter,
            [LabelText(SdfIconType.TextRight)] TextRight,
        }

        public enum SomeEnumWithIconsAndNames
        {
            [LabelText("Align Left", SdfIconType.TextLeft)] TextLeft,
            [LabelText("Align Center", SdfIconType.TextCenter)] TextCenter,
            [LabelText("Align Right", SdfIconType.TextRight)] TextRight,
        }

        [System.Flags]
        public enum SomeBitmaskEnum
        {
            A = 1 << 1,
            B = 1 << 2,
            C = 1 << 3,
            All = A | B | C
        }
    }
}
#endif