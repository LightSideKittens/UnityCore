//-----------------------------------------------------------------------
// <copyright file="ITemporaryContext.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Custom types used by the <see cref="TemporaryPropertyContext{T}"/> can choose to implement the ITemporaryContext
    /// interface in order to be notified when the context gets reset.
    /// </summary>
    public interface ITemporaryContext
    {
        /// <summary>
        /// Called by <see cref="TemporaryPropertyContext{T}"/> when the context gets reset.
        /// </summary>
        void Reset();
    }
}
#endif