//-----------------------------------------------------------------------
// <copyright file="ButtonStyle.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Button style for methods with parameters.
    /// </summary>
    public enum ButtonStyle
    {
        /// <summary>
        /// Draws a foldout box around the parameters of the method with the button on the box header itself.
        /// This is the default style of a method with parameters.
        /// </summary>
        CompactBox,

        /// <summary>
        /// Draws a button with a foldout to expose the parameters of the method.
        /// </summary>
        FoldoutButton,

        /// <summary>
        /// Draws a foldout box around the parameters of the method with the button at the bottom of the box.
        /// </summary>
        Box,
    }
}