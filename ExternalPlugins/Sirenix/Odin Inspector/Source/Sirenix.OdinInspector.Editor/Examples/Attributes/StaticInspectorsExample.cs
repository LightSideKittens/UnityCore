//-----------------------------------------------------------------------
// <copyright file="StaticInspectorsExample.cs" company="Sirenix ApS">
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

    using UnityEngine;
    using System.Collections.Generic;
    using System;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(ShowInInspectorAttribute), "You can use the ShowInInspector attribute on static members to make them appear in the inspector as well.")]
    [ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class StaticInspectorsExample
    {
        [ShowInInspector]
        public static List<MySomeStruct> SomeStaticField;

        [ShowInInspector, PropertyRange(0, 0.1f)]
        public static float FixedDeltaTime
        {
            get { return Time.fixedDeltaTime; }
            set { Time.fixedDeltaTime = value; }
        }

        [Serializable]
        public struct MySomeStruct
        {
            [HideLabel, PreviewField(45)]
            [HorizontalGroup("Split", width: 45)]
            public Texture2D Icon;

            [FoldoutGroup("Split/$Icon")]
            [HorizontalGroup("Split/$Icon/Properties", LabelWidth = 40)]
            public int Foo;

            [HorizontalGroup("Split/$Icon/Properties")]
            public int Bar;
        }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [Button(ButtonSizes.Large), PropertyOrder(-1)]
        public static void AddToList()
        {
            int count = SomeStaticField.Count + 1000;
            SomeStaticField.Capacity = count;
            while (SomeStaticField.Count < count)
            {
                SomeStaticField.Add(new MySomeStruct() { Icon = ExampleHelper.GetTexture() });
            }
        }

        [OnInspectorInit]
        private static void CreateData()
        {
            SomeStaticField = new List<MySomeStruct>()
            {
                new MySomeStruct(){ Icon = ExampleHelper.GetTexture() },
                new MySomeStruct(){ Icon = ExampleHelper.GetTexture() },
                new MySomeStruct(){ Icon = ExampleHelper.GetTexture() },
            };
        }
#endif
    }
}
#endif