//-----------------------------------------------------------------------
// <copyright file="BoxGroupExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(BoxGroupAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
    internal class BoxGroupExamples
    {
        // Box with a title.
        [BoxGroup("Some Title")]
        public string A;

        [BoxGroup("Some Title")]
        public string B;

        // Box with a centered title.
        [BoxGroup("Centered Title", centerLabel: true)]
        public string C;

        [BoxGroup("Centered Title")]
        public string D;

        // Box with a title received from a field.
        [BoxGroup("$G")]
        public string E = "Dynamic box title 2";

        [BoxGroup("$G")]
        public string F;
        
        // No title
        [BoxGroup]
        public string G;

        [BoxGroup]
        public string H;

        // A named box group without a title.
        [BoxGroup("NoTitle", false)]
        public string I;

        [BoxGroup("NoTitle")]
        public string J;

        [BoxGroup("A Struct In A Box"), HideLabel]
        public SomeStruct BoxedStruct;

        public SomeStruct DefaultStruct;

        [Serializable]
        public struct SomeStruct
        {
            public int One;
            public int Two;
            public int Three;
        }
    }

}
#endif