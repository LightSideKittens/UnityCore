//-----------------------------------------------------------------------
// <copyright file="RegisterFormatterLocatorAttribute.cs" company="Sirenix ApS">
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
    public class RegisterFormatterLocatorAttribute : Attribute
    {
        public Type FormatterLocatorType { get; private set; }
        public int Priority { get; private set; }

        public RegisterFormatterLocatorAttribute(Type formatterLocatorType, int priority = 0)
        {
            this.FormatterLocatorType = formatterLocatorType;
            this.Priority = priority;
        }
    }
}