//-----------------------------------------------------------------------
// <copyright file="InlinePropertyExamples.cs" company="Sirenix ApS">
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
    using System;
    using UnityEngine;

    [AttributeExample(typeof(InlinePropertyAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System" })]
    internal class InlinePropertyExamples
    {
        public Vector3 Vector3;

        public Vector3Int MyVector3Int;

        [InlineProperty(LabelWidth = 13)]
        public Vector2Int MyVector2Int;
   
        [Serializable]
        [InlineProperty(LabelWidth = 13)]
        public struct Vector3Int
        {
            [HorizontalGroup]
            public int X;

            [HorizontalGroup]
            public int Y;

            [HorizontalGroup]
            public int Z;
        }

        [Serializable]
        public struct Vector2Int
        {
            [HorizontalGroup]
            public int X;

            [HorizontalGroup]
            public int Y;
        }
    }
}
#endif