//-----------------------------------------------------------------------
// <copyright file="ShowInInspectorExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ShowInInspectorAttribute),
        "ShowInInspector is used to display properties that otherwise wouldn't be shown in the inspector, such as non-serialized fields or properties.")]
    internal class ShowInInspectorExamples
    {
#pragma warning disable // These fields are in fact being used; let's not have bothersome warnings.
        [ShowInInspector]
        private int myPrivateInt;

        [ShowInInspector]
        public int MyPropertyInt { get; set; }

        [ShowInInspector]
        public int ReadOnlyProperty
        {
            get { return this.myPrivateInt; }
        }

        [ShowInInspector]
        public static bool StaticProperty { get; set; }
    }
}
#endif