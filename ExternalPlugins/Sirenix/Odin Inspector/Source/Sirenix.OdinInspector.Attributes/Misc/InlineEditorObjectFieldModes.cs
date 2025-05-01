//-----------------------------------------------------------------------
// <copyright file="InlineEditorObjectFieldModes.cs" company="Sirenix ApS">
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
    /// How the InlineEditor attribute drawer should draw the object field.
    /// </summary>
    public enum InlineEditorObjectFieldModes
    {
        /// <summary>
        /// Draws the object field in a box.
        /// </summary>
        Boxed,

        /// <summary>
        /// Draws the object field with a foldout.
        /// </summary>
        Foldout,

        /// <summary>
        /// Hides the object field unless it's null.
        /// </summary>
        Hidden,

        /// <summary>
        /// Hidden the object field also when the object is null.
        /// </summary>
        CompletelyHidden,
    }
}