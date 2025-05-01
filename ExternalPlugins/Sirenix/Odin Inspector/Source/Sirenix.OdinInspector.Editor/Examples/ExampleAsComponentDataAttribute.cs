//-----------------------------------------------------------------------
// <copyright file="ExampleAsComponentDataAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples.Internal
{
#pragma warning disable

    using System;

    public class ExampleAsComponentDataAttribute : Attribute
    {
        public string[] AttributeDeclarations;
        public string[] Namespaces;
    }
}
#endif