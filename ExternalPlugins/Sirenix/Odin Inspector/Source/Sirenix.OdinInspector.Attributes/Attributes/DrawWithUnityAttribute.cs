//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityAttribute.cs" company="Sirenix ApS">
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
    /// <para>DrawWithUnity can be applied to a field or property to make Odin draw it using Unity's old drawing system. Use it if you want to selectively disable Odin drawing for a particular member.</para>
    /// </summary>
    /// <remarks>
    /// <para>Note that this attribute does not mean "disable Odin completely for this property"; it is visual only in nature, and in fact represents an Odin drawer which calls into Unity's old property drawing system. As Odin is still ultimately responsible for arranging the drawing of the property, and since other attributes exist with a higher priority than this attribute, and it is not guaranteed that Unity will draw the property if another attribute is present to override this one.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DrawWithUnityAttribute : Attribute
    {
        public bool PreferImGUI = false;
    }
}