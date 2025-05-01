//-----------------------------------------------------------------------
// <copyright file="OnInspectorDisposeExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(OnInspectorDisposeAttribute), "The following example demonstrates how OnInspectorDispose works.")]
    internal class OnInspectorDisposeExamples
    {
        [OnInspectorDispose("@UnityEngine.Debug.Log(\"Dispose event invoked!\")")]
        [ShowInInspector, InfoBox("When you change the type of this field, or set it to null, the former property setup is disposed. The property setup will also be disposed when you deselect this example."), DisplayAsString]
        public BaseClass PolymorphicField;

        public abstract class BaseClass { public override string ToString() { return this.GetType().Name; } }
        public class A : BaseClass { }
        public class B : BaseClass { }
        public class C : BaseClass { }
    }
}
#endif