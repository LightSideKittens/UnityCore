//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ShowDrawerChainAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System" })]
    internal class ShowDrawerChainExamples
    {
#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [HorizontalGroup(Order = 1)]
        [ShowInInspector, ToggleLeft]
        public bool ToggleHideIf { get { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); return UnityEditor.EditorApplication.timeSinceStartup % 3 < 1.5f; } }

        [HorizontalGroup]
        [ShowInInspector, HideLabel, ProgressBar(0, 1.5f)]
        private double Animate { get { return Math.Abs(UnityEditor.EditorApplication.timeSinceStartup % 3 - 1.5f); } }
#endif

        [InfoBox(
            "Any drawer not used will be greyed out so that you can more easily debug the drawer chain. You can see this by toggling the above toggle field.\n\n" +
            "If you have any custom drawers they will show up with green names in the drawer chain.")]
        [ShowDrawerChain]
        [HideIf("ToggleHideIf")]
        [PropertyOrder(2)]
        public GameObject SomeObject;

        [Range(0, 10)]
        [ShowDrawerChain]
        public float SomeRange;
    } 
}
#endif