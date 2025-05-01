//-----------------------------------------------------------------------
// <copyright file="DrawWithVisualElementsAttribute.cs" company="Sirenix ApS">
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
    /// Force Odin to draw this value as an IMGUI-embedded UI Toolkit Visual Element.
    /// </summary>
    public class DrawWithVisualElementsAttribute : Attribute
    {
        public bool DrawCollectionWithImGUI;
    }
}