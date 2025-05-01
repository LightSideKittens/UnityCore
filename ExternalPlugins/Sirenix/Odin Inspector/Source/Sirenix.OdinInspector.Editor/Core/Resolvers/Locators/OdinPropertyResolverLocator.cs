//-----------------------------------------------------------------------
// <copyright file="OdinPropertyResolverLocator.cs" company="Sirenix ApS">
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
    /// Base class for locator of <see cref="OdinPropertyResolver"/>. Use <see cref="DefaultOdinPropertyResolverLocator"/> for default implementation.
    /// </summary>
    public abstract class OdinPropertyResolverLocator
    {
        /// <summary>
        /// Gets an <see cref="OdinPropertyResolver"/> instance for the specified property.
        /// </summary>
        /// <param name="property">The property to get an <see cref="OdinPropertyResolver"/> instance for.</param>
        /// <returns>An instance of <see cref="OdinPropertyResolver"/> to resolver the specified property.</returns>
        public abstract OdinPropertyResolver GetResolver(InspectorProperty property);
    }
}
#endif