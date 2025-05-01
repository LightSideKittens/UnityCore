//-----------------------------------------------------------------------
// <copyright file="TabGroupExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(TabGroupAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System" })]
    internal class TabGroupExamples
    {
        [TabGroup("General")]
        public string playerName1;
        [TabGroup("General")]
        public int playerLevel1;
        [TabGroup("General")]
        public string playerClass1;

        [TabGroup("Stats")]
        public int strength1;
        [TabGroup("Stats")]
        public int dexterity1;
        [TabGroup("Stats")]
        public int intelligence1;

        [TabGroup("Quests")]
        public bool hasMainQuest1;
        [TabGroup("Quests")]
        public int mainQuestProgress1;
        [TabGroup("Quests")]
        public bool hasSideQuest1;
        [TabGroup("Quests")]
        public int sideQuestProgress1;


        [TabGroup("tab2", "General", SdfIconType.ImageAlt, TextColor = "green")]
        public string playerName2;
        [TabGroup("tab2", "General")]
        public int playerLevel2;
        [TabGroup("tab2", "General")]
        public string playerClass2;

        [TabGroup("tab2", "Stats", SdfIconType.BarChartLineFill, TextColor = "blue")]
        public int strength2;
        [TabGroup("tab2", "Stats")]
        public int dexterity2;
        [TabGroup("tab2", "Stats")]
        public int intelligence2;

        [TabGroup("tab2", "Quests", SdfIconType.Question, TextColor = "@questColor", TabName = "")]
        public bool hasMainQuest2;

        [TabGroup("tab2", "Quests")]
        public Color questColor = new Color(1, 0.5f, 0);

        [TabGroup("shrink tabs", "World Map", SdfIconType.Map, TextColor = "orange", TabLayouting = TabLayouting.Shrink)]
        [TabGroup("shrink tabs", "Character", SdfIconType.PersonFill, TextColor = "orange")]
        [TabGroup("shrink tabs", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("shrink tabs", "Abilities", TextColor = "green")]
        [TabGroup("shrink tabs", "Missions", SdfIconType.ExclamationSquareFill, TextColor = "yellow")]
        [TabGroup("shrink tabs", "Collection 1", TextColor = "blue")]
        [TabGroup("shrink tabs", "Collection 2", TextColor = "blue")]
        [TabGroup("shrink tabs", "Collection 3", TextColor = "blue")]
        [TabGroup("shrink tabs", "Collection 4", TextColor = "blue")]
        [TabGroup("shrink tabs", "Settings", SdfIconType.GearFill, TextColor = "grey")]
        [TabGroup("shrink tabs", "Guide", SdfIconType.QuestionSquareFill, TextColor = "blue")]
        public float a, b, c, d;

        [TabGroup("multi row", "World Map", SdfIconType.Map, TextColor = "orange", TabLayouting = TabLayouting.MultiRow)]
        [TabGroup("multi row", "Character", SdfIconType.PersonFill, TextColor = "orange")]
        [TabGroup("multi row", "Wand", SdfIconType.Magic, TextColor = "red")]
        [TabGroup("multi row", "Abilities", TextColor = "green")]
        [TabGroup("multi row", "Missions", SdfIconType.ExclamationSquareFill, TextColor = "yellow")]
        [TabGroup("multi row", "Collection 1", TextColor = "blue")]
        [TabGroup("multi row", "Collection 2", TextColor = "blue")]
        [TabGroup("multi row", "Collection 3", TextColor = "blue")]
        [TabGroup("multi row", "Collection 4", TextColor = "blue")]
        [TabGroup("multi row", "Settings", SdfIconType.GearFill, TextColor = "grey")]
        [TabGroup("multi row", "Guide", SdfIconType.QuestionSquareFill, TextColor = "blue")]
        public float e, f, g, h;

        [Serializable]
        [HideLabel]
        public class MyTabObject
        {
            public int A;
            public int B;
            public int C;
        }
    }
}
#endif