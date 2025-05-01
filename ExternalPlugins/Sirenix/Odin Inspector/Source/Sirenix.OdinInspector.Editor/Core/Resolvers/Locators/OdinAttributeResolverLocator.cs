//-----------------------------------------------------------------------
// <copyright file="OdinAttributeResolverLocator.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Base class definition for OdinAttributeProcessorLocator. Responsible for finding and creating <see cref="OdinAttributeProcessor"/> instances to process attributes for properties.
    /// Default OdinAttributeProcessorLocator have been implemented as <see cref="DefaultOdinAttributeProcessorLocator"/>.
    /// </summary>
    public abstract class OdinAttributeProcessorLocator
    {
        /// <summary>
        /// Gets a list of <see cref="OdinAttributeProcessor"/> to process attributes for the specified child member of the parent property.
        /// </summary>
        /// <param name="parentProperty">The parent of the member.</param>
        /// <param name="member">Child member of the parent property.</param>
        /// <returns>List of <see cref="OdinAttributeProcessor"/> to process attributes for the specified member.</returns>
        public abstract List<OdinAttributeProcessor> GetChildProcessors(InspectorProperty parentProperty, MemberInfo member);

        /// <summary>
        /// Gets a list of <see cref="OdinAttributeProcessor"/> to process attributes for the specified property.
        /// </summary>
        /// <param name="property">The property to find attribute porcessors for.</param>
        /// <returns>List of <see cref="OdinAttributeProcessor"/> to process attributes for the speicied member.</returns>
        public abstract List<OdinAttributeProcessor> GetSelfProcessors(InspectorProperty property);
    }
}
#endif