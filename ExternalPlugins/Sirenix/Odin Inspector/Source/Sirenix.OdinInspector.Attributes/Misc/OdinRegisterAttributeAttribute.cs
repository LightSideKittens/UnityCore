//-----------------------------------------------------------------------
// <copyright file="OdinRegisterAttributeAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class OdinRegisterAttributeAttribute : Attribute
    {
        public Type AttributeType;
        public string Categories;
        public string Description;
        public string DocumentationUrl;
        public bool IsEnterprise;

        public OdinRegisterAttributeAttribute(Type attributeType, string category, string description, bool isEnterprise)
        {
            this.AttributeType = attributeType;
            this.Categories = category;
            this.Description = description;
            this.IsEnterprise = isEnterprise;
        }

        public OdinRegisterAttributeAttribute(Type attributeType, string category, string description, bool isEnterprise, string url)
        {
            this.AttributeType = attributeType;
            this.Categories = category;
            this.Description = description;
            this.IsEnterprise = isEnterprise;
            this.DocumentationUrl = url;
        }
    }
}