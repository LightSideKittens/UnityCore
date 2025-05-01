//-----------------------------------------------------------------------
// <copyright file="SirenixBuildNameAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class SirenixBuildNameAttribute : Attribute
    {
        public string BuildName { get; private set; }

        public SirenixBuildNameAttribute(string buildName)
        {
            this.BuildName = buildName;
        }
    }
}