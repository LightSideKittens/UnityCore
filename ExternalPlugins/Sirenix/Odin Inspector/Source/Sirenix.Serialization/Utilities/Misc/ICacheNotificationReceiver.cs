//-----------------------------------------------------------------------
// <copyright file="ICacheNotificationReceiver.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    /// <summary>
    /// Provides notification callbacks for values that are cached using the <see cref="Cache{T}"/> class.
    /// </summary>
    public interface ICacheNotificationReceiver
    {
        /// <summary>
        /// Called when the cached value is freed.
        /// </summary>
        void OnFreed();

        /// <summary>
        /// Called when the cached value is claimed.
        /// </summary>
        void OnClaimed();
    }
}