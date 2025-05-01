//-----------------------------------------------------------------------
// <copyright file="HideReferenceObjectPickerAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Hides the polymorphic object-picker shown above the properties of non-Unity serialized reference types.
    /// </summary>
    /// <remarks>
    /// When the object picker is hidden, you can right click and set the instance to null, in order to set a new value.
    /// If you don't want this behavior, you can use <see cref="DisableContextMenu"/> attribute to ensure people can't change the value.
    /// </remarks>
    /// <seealso cref="Sirenix.Serialization.OdinSerializeAttribute"/>
    /// <example>
    /// <code>
    /// public class MyComponent : SerializedMonoBehaviour
    /// {
    ///     [Header("Hidden Object Pickers")]
    ///     [Indent]
    ///     [HideReferenceObjectPicker]
    ///     public MyCustomReferenceType OdinSerializedProperty1;
    ///
    ///     [Indent]
    ///     [HideReferenceObjectPicker]
    ///     public MyCustomReferenceType OdinSerializedProperty2;
    ///
    ///     [Indent]
    ///     [Header("Shown Object Pickers")]
    ///     public MyCustomReferenceType OdinSerializedProperty3;
    ///
    ///     [Indent]
    ///     public MyCustomReferenceType OdinSerializedProperty4;
    ///
    ///     public class MyCustomReferenceType
    ///     {
    ///         public int A;
    ///         public int B;
    ///         public int C;
    ///     }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class HideReferenceObjectPickerAttribute : Attribute
    {
    }
}