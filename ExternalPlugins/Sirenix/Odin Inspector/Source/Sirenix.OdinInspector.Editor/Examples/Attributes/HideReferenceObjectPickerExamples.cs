//-----------------------------------------------------------------------
// <copyright file="HideReferenceObjectPickerExamples.cs" company="Sirenix ApS">
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

    [ShowOdinSerializedPropertiesInInspector]
    [AttributeExample(typeof(HideReferenceObjectPickerAttribute),
        "When the object picker is hidden, you can right click and set the instance to null, in order to set a new value.\n\n" +
        "If you don't want this behavior, you can use DisableContextMenu attribute to ensure people can't change the value.")]
    internal class HideReferenceObjectPickerExamples
    {
        [Title("Hidden Object Pickers")]
        [HideReferenceObjectPicker]
        public MyCustomReferenceType OdinSerializedProperty1 = new MyCustomReferenceType();

        [HideReferenceObjectPicker]
        public MyCustomReferenceType OdinSerializedProperty2 = new MyCustomReferenceType();

        [Title("Shown Object Pickers")]
        public MyCustomReferenceType OdinSerializedProperty3 = new MyCustomReferenceType();

        public MyCustomReferenceType OdinSerializedProperty4 = new MyCustomReferenceType();

        // Protip: You can also put the HideInInspector attribute on the class definition itself to hide it globally for all members.
        // [HideReferenceObjectPicker]
        public class MyCustomReferenceType
        {
            public int A;
            public int B;
            public int C;
        }
    }
}
#endif