//-----------------------------------------------------------------------
// <copyright file="ButtonGroupAttribute.cs" company="Sirenix ApS">
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
    /// <para>ButtonGroup is used on any instance function, and adds buttons to the inspector organized into horizontal groups.</para>
    /// <para>Use this to organize multiple button in a tidy horizontal group.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how ButtonGroup is used to organize two buttons into one group.</para>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ButtonGroup("MyGroup")]
    ///		private void A()
    ///		{
    ///			// ..
    ///		}
    ///
    ///		[ButtonGroup("MyGroup")]
    ///		private void B()
    ///		{
    ///			// ..
    ///		}
    ///	}
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following example shows how ButtonGroup can be used to create multiple groups of buttons.</para>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ButtonGroup("First")]
    ///		private void A()
    ///		{ }
    ///
    ///		[ButtonGroup("First")]
    ///		private void B()
    ///		{ }
    ///
    ///		[ButtonGroup("")]
    ///		private void One()
    ///		{ }
    ///
    ///		[ButtonGroup("")]
    ///		private void Two()
    ///		{ }
    ///
    ///		[ButtonGroup("")]
    ///		private void Three()
    ///		{ }
    ///	}
    /// </code>
    /// </example>
    /// <seealso cref="ButtonAttribute"/>
	/// <seealso cref="InlineButtonAttribute"/>
    /// <seealso cref="BoxGroupAttribute"/>
    /// <seealso cref="FoldoutGroupAttribute"/>
    /// <seealso cref="HorizontalGroupAttribute"/>
    /// <seealso cref="TabGroupAttribute"/>
    /// <seealso cref="ToggleGroupAttribute"/>
    [IncludeMyAttributes, ShowInInspector]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ButtonGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// Gets the height of the button. If it's zero or below then use default.
        /// </summary>
        public int ButtonHeight;

        /// <summary>
        /// The alignment of the icon that is displayed inside the button.
        /// </summary>
        public IconAlignment IconAlignment
        {
            get => buttonIconAlignment;
            set
            {
                buttonIconAlignment = value;
                HasDefinedButtonIconAlignment = true;
            }
        }

        /// <summary>
        /// The alignment of the button represented by a range from 0 to 1 where 0 is the left edge of the available space and 1 is the right edge.
        /// </summary>
        public int ButtonAlignment
        {
            get => buttonAlignment;
            set
            {
                buttonAlignment = value;
                HasDefinedButtonAlignment = true;
            }
        }

        /// <summary>
        /// Whether the button should stretch to fill all of the available space. Default value is true.
        /// </summary>
        public bool Stretch
        {
            get => stretch;
            set
            {
                stretch = value;
                HasDefinedStretch = true;
            }
        }

        public bool HasDefinedButtonIconAlignment { get; private set; }
        public bool HasDefinedButtonAlignment { get; private set; }
        public bool HasDefinedStretch { get; private set; }

        private IconAlignment buttonIconAlignment;
        private int buttonAlignment;
        private bool stretch;

        /// <summary>
        /// Organizes the button into the specified button group.
        /// </summary>
        /// <param name="group">The group to organize the button into.</param>
        /// <param name="order">The order of the group in the inspector..</param>
        public ButtonGroupAttribute(string group = "_DefaultGroup", float order = 0)
            : base(group, order)
        { }
    }
}