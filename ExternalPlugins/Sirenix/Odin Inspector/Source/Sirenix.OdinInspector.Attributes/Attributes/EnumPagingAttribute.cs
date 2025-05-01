//-----------------------------------------------------------------------
// <copyright file="EnumPagingAttribute.cs" company="Sirenix ApS">
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
    /// <para>Draws an enum selector in the inspector with next and previous buttons to let you cycle through the available values for the enum property.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// public enum MyEnum
    /// {
    ///     One,
    ///     Two,
    ///     Three,
    /// }
    /// 
    /// public class MyMonoBehaviour : MonoBehaviour
    /// {
    ///     [EnumPaging]
    ///     public MyEnum Value;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class EnumPagingAttribute : Attribute
    {
    }
}