//-----------------------------------------------------------------------
// <copyright file="DictionaryExamples.cs" company="Sirenix ApS">
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
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(DictionaryDrawerSettings))]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
    internal class DictionaryExamples
    {
        [InfoBox("In order to serialize dictionaries, all we need to do is to inherit our class from SerializedMonoBehaviour.")]
        public Dictionary<int, Material> IntMaterialLookup;

        public Dictionary<string, string> StringStringDictionary;

        [DictionaryDrawerSettings(KeyLabel = "Custom Key Name", ValueLabel = "Custom Value Label")]
        public Dictionary<SomeEnum, MyCustomType> CustomLabels = new Dictionary<SomeEnum, MyCustomType>()
        {
            { SomeEnum.First, new MyCustomType() },
            { SomeEnum.Second, new MyCustomType() },
        };

        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, List<int>> StringListDictionary = new Dictionary<string, List<int>>()
        {
            { "Numbers", new List<int>(){ 1, 2, 3, 4, } },
        };

        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<SomeEnum, MyCustomType> EnumObjectLookup = new Dictionary<SomeEnum, MyCustomType>()
        {
            { SomeEnum.Third, new MyCustomType() },
            { SomeEnum.Fourth, new MyCustomType() },
        };

        [InlineProperty(LabelWidth = 90)]
        public struct MyCustomType
        {
            public int SomeMember;
            public GameObject SomePrefab;
        }

        public enum SomeEnum
        {
            First, Second, Third, Fourth, AndSoOn
        }


#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            IntMaterialLookup = new Dictionary<int, Material>()
            {
                { 1, ExampleHelper.GetMaterial() },
                { 7, ExampleHelper.GetMaterial() },
            };

            StringStringDictionary = new Dictionary<string, string>()
            {
                { "One", ExampleHelper.GetString() },
                { "Seven", ExampleHelper.GetString() },
            };
        }
#endif 
    }
}
#endif