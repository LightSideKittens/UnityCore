//-----------------------------------------------------------------------
// <copyright file="ButtonSizes.cs" company="Sirenix ApS">
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
    /// Various built-in button sizes.
    /// </summary>
    public enum ButtonSizes
    {
        /// <summary>
        /// Small button size, fits well with properties in the inspector.
        /// </summary>
        Small = 0,

        /// <summary>
        /// A larger button.
        /// </summary>
        Medium = 22,

        /// <summary>
        /// A very large button. 
        /// </summary>
        Large = 31,

        /// <summary>
        /// A gigantic button. Twice as big as Large 
        /// </summary>
        Gigantic = 62,
    }
}