//-----------------------------------------------------------------------
// <copyright file="FoldoutGroupAttributeExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(FoldoutGroupAttribute))]
    internal class FoldoutGroupAttributeExamples
    {
        [FoldoutGroup("Group 1")]
        public int A;

        [FoldoutGroup("Group 1")]
        public int B;

        [FoldoutGroup("Group 1")]
        public int C;

        [FoldoutGroup("Collapsed group", expanded: false)]
        public int D;

        [FoldoutGroup("Collapsed group")]
        public int E;

        [FoldoutGroup("$GroupTitle", expanded: true)]
        public int One;

        [FoldoutGroup("$GroupTitle")]
        public int Two;

        public string GroupTitle = "Dynamic group title";
    }
}
#endif