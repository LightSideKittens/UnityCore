//-----------------------------------------------------------------------
// <copyright file="DropEvents.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    /// <summary>
    /// This class is due to undergo refactoring.
    /// </summary>
    public enum DropEvents
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        None = 0,

        /// <summary>
        /// Not yet documented.
        /// </summary>
        Referenced = 1,

        /// <summary>
        /// Not yet documented.
        /// </summary>
        Moved = 2,

        /// <summary>
        /// Not yet documented.
        /// </summary>
        Copied = 3,

        /// <summary>
        /// Not yet documented.
        /// </summary>
        Canceled = 4,
    }
}
#endif