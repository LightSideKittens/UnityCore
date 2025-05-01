//-----------------------------------------------------------------------
// <copyright file="RegisterDefaultActionResolverAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterDefaultActionResolverAttribute : Attribute
    {
        public Type ResolverType;
        public double Order;

        public RegisterDefaultActionResolverAttribute(Type resolverType, double order)
        {
            this.ResolverType = resolverType;
            this.Order = order;
        }
    }
}
#endif