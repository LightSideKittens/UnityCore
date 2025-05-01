//-----------------------------------------------------------------------
// <copyright file="ResolverPriorityAttribute.cs" company="Sirenix ApS">
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

    using System;

    /// <summary>
    /// Priority for <see cref="OdinPropertyResolver"/> and <see cref="OdinAttributeProcessor"/> types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ResolverPriorityAttribute : Attribute
    {
        /// <summary>
        /// Priority of the resolver.
        /// </summary>
        public readonly double Priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolverPriorityAttribute"/> class.
        /// </summary>
        /// <param name="priority">The higher the priority, the earlier it will be processed.</param>
        public ResolverPriorityAttribute(double priority)
        {
            this.Priority = priority;
        }
    }
}
#endif