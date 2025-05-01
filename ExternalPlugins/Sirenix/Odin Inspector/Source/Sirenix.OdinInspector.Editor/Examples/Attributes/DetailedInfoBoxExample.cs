//-----------------------------------------------------------------------
// <copyright file="DetailedInfoBoxExample.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DetailedInfoBoxAttribute))]
    internal class DetailedInfoBoxExample
    {
        [DetailedInfoBox("Click the DetailedInfoBox...",
            "... to reveal more information!\n" +
            "This allows you to reduce unnecessary clutter in your editors, and still have all the relavant information available when required.")]
        public int Field;
    }
}
#endif