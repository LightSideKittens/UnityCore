//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DrawWithUnityAttribute))]
    internal class DrawWithUnityExamples
    {
        [InfoBox("If you ever experience trouble with one of Odin's attributes, there is a good chance that DrawWithUnity will come in handy; it will make Odin draw the value as Unity normally would.")]
        public GameObject ObjectDrawnWithOdin;

        [DrawWithUnity]
        public GameObject ObjectDrawnWithUnity;
    }
}
#endif