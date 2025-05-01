//-----------------------------------------------------------------------
// <copyright file="OnInspectorInitAttribute.cs" company="Sirenix ApS">
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
    /// <para>The OnInspectorInit attribute takes in an action string as an argument (typically the name of a method to be invoked, or an expression to be executed), and executes that action when the property's drawers are initialized in the inspector.</para>
    /// <para>Initialization will happen at least once during the first drawn frame of any given property, but may also happen several times later, most often when the type of a polymorphic property changes and it refreshes its drawer setup and recreates all its children.</para>
    /// </summary>
    /// <example>
    /// <para>The following example demonstrates how OnInspectorInit works.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     // Display current time for reference.
    ///     [ShowInInspector, DisplayAsString, PropertyOrder(-1)]
    ///     public string CurrentTime { get { GUIHelper.RequestRepaint(); return DateTime.Now.ToString(); } }
    ///     
    ///     // OnInspectorInit executes the first time this string is about to be drawn in the inspector.
    ///     // It will execute again when the example is reselected.
    ///     [OnInspectorInit("@TimeWhenExampleWasOpened = DateTime.Now.ToString()")]
    ///     public string TimeWhenExampleWasOpened;
    ///     
    ///     // OnInspectorInit will not execute before the property is actually "resolved" in the inspector.
    ///     // Remember, Odin's property system is lazily evaluated, and so a property does not actually exist
    ///     // and is not initialized before something is actually asking for it.
    ///     // 
    ///     // Therefore, this OnInspectorInit attribute won't execute until the foldout is expanded.
    ///     [FoldoutGroup("Delayed Initialization", Expanded = false, HideWhenChildrenAreInvisible = false)]
    ///     [OnInspectorInit("@TimeFoldoutWasOpened = DateTime.Now.ToString()")]
    ///     public string TimeFoldoutWasOpened;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [DontApplyToListElements]
    [IncludeMyAttributes, HideInTables]
    public class OnInspectorInitAttribute : ShowInInspectorAttribute
    {
        public string Action;

        /// <summary>
        /// This constructor should be used when the attribute is placed directly on a method.
        /// </summary>
        public OnInspectorInitAttribute()
        {
        }

        /// <summary>
        /// This constructor should be used when the attribute is placed on a non-method member.
        /// </summary>
        public OnInspectorInitAttribute(string action)
        {
            this.Action = action;
        }
    }
}