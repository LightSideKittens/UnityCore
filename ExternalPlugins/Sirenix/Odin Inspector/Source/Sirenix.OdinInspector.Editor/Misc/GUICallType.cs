//-----------------------------------------------------------------------
// <copyright file="GUICallType.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;

    /// <summary>
    /// Specifies hows any given drawer should drawer the property.
    /// Changing this behavior, also changes which methods should be overridden in the drawer.
    /// </summary>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>

    [Obsolete("Removed support GUICallType.Rect and DrawPropertyRect as it didn't really do much. You can get the same behaviour by overriding DrawPropertyLayout and calling GUILayoutUtility.GetRect or EditorGUILayout.GetControlRect.", true)]
    public enum GUICallType
    {
        /// <summary>
        /// GUILayout enabled the use of GUILayout, EditorGUILayout and <see cref="Utilities.Editor.SirenixEditorGUI"/>
        /// </summary>
        GUILayout,

        /// <summary>
        /// Draws the property using Unity's GUI, and EditorGUI.
        /// </summary>
        Rect
    }
}
#endif