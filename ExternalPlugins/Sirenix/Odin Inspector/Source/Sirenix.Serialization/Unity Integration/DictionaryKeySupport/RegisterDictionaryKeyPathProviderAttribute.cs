//-----------------------------------------------------------------------
// <copyright file="RegisterDictionaryKeyPathProviderAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterDictionaryKeyPathProviderAttribute : Attribute
    {
        public readonly Type ProviderType;

        public RegisterDictionaryKeyPathProviderAttribute(Type providerType)
        {
            this.ProviderType = providerType;
        }
    }
}