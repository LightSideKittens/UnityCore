//-----------------------------------------------------------------------
// <copyright file="TitleAlignments.cs" company="Sirenix ApS">
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
    /// Title alignment enum used by various attributes.
    /// </summary>
    /// <seealso cref="TitleGroupAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    public enum TitleAlignments
    {
        /// <summary>
        /// Title and subtitle left aligned.
        /// </summary>
        Left,

        /// <summary>
        /// Title and subtitle centered aligned.
        /// </summary>
        Centered,

        /// <summary>
        /// Title and subtitle right aligned.
        /// </summary>
        Right,

        /// <summary>
        /// Title on the left, subtitle on the right.
        /// </summary>
        Split,
    }
}