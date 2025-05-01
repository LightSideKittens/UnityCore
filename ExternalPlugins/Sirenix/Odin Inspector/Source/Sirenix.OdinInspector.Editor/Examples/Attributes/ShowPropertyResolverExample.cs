//-----------------------------------------------------------------------
// <copyright file="ShowPropertyResolverExample.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using System.Collections.Generic;
    using UnityEngine;

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(ShowPropertyResolverAttribute),
        Description = "The ShowPropertyResolver attribute allows you to debug how your properties are handled by Odin behind the scenes.")]
    [ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class ShowPropertyResolverExample
    {
        [ShowPropertyResolver]
        public string MyString;

        [ShowPropertyResolver]
        public List<int> MyList;

        [ShowPropertyResolver]
        public Dictionary<int, Vector3> MyDictionary;
    }
}
#endif