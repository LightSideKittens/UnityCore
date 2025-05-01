//-----------------------------------------------------------------------
// <copyright file="RegisterDefaultValueResolverCreatorAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    using System;

    /// <summary>
    /// This attribute can be placed on an assembly to register a value resolver creator that should be queried when a value resolver is being created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterDefaultValueResolverCreatorAttribute : Attribute
    {
        public Type ResolverCreatorType;
        public double Order;

        /// <summary>
        /// This attribute can be placed on an assembly to register a value resolver creator that should be queried when a value resolver is being created.
        /// </summary>
        /// <param name="resolverCreatorType">The resolver </param>
        /// <param name="order"></param>
        public RegisterDefaultValueResolverCreatorAttribute(Type resolverCreatorType, double order)
        {
            this.ResolverCreatorType = resolverCreatorType;
            this.Order = order;
        }
    }
}
#endif