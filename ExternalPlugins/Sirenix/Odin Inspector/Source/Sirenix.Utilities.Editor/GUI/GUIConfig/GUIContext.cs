//-----------------------------------------------------------------------
// <copyright file="GUIContext.cs" company="Sirenix ApS">
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

    using System;

    /// <summary>
    /// This class is due to undergo refactoring.
    /// </summary>
    public class GUIContext<T> : IControlContext
    {
        internal bool HasValue = false;

        /// <summary>
        /// The value.
        /// </summary>
        public T Value;

        /// <summary>
        /// Performs an implicit conversion from <see cref="GUIContext{T}"/> to <see cref="T"/>.
        /// </summary>
        public static implicit operator T(GUIContext<T> context)
        {
            if (context == null)
            {
                return default(T);
            }
            else
            {
                return context.Value;
            }
        }

        int IControlContext.LastRenderedFrameId { get; set; }
    }
}
#endif