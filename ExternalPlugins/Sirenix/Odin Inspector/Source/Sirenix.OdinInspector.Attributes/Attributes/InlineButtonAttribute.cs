//-----------------------------------------------------------------------
// <copyright file="InlineButtonAttribute.cs" company="Sirenix ApS">
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
	/// <para>The inline button adds a button to the end of a property.</para>
	/// </summary>
	/// <remarks>
	/// <note type="note">Due to a bug, multiple inline buttons are currently not supported.</note>
	/// </remarks>
	/// <example>
	/// <para>The following examples demonstrates how InlineButton can be used.</para>
	/// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		// Adds a button to the end of the A property.
	///		[InlineButton("MyFunction")]
	///		public int A;
	///
	///		// This is example demonstrates how you can change the label of the button.
	///		// InlineButton also supports refering to string members with $.
	///		[InlineButton("MyFunction", "Button")]
	///		public int B;
	///
	/// 	private void MyFunction()
	///		{
	///			// ...
	///		}
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="ButtonAttribute"/>
	/// <seealso cref="ButtonGroupAttribute"/>
    [DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public sealed class InlineButtonAttribute : Attribute
	{

		/// <summary>
		/// Name of member method to call when the button is clicked. Obsolete; use the Action member instead.
		/// </summary>
		[Obsolete("Use the Action member instead.", 
#if SIRENIX_INTERNAL
			true
#else
            false
#endif
		)]
		public string MemberMethod { get { return this.Action; } set { this.Action = value; } }

		/// <summary>
		/// A resolved string that defines the action to perform when the button is clicked, such as an expression or method invocation.
		/// </summary>
		public string Action;

		/// <summary>
		/// Optional label of the button.
		/// </summary>
		public string Label;

		/// <summary>
		/// Optional resolved string that specifies a condition for whether to show the inline button or not.
		/// </summary>
		public string ShowIf;

        /// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
        public string ButtonColor;

        /// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
        public string TextColor;

		public SdfIconType Icon;

		public IconAlignment IconAlignment;

		/// <summary>
		/// Draws a button to the right of the property.
		/// </summary>
		/// <param name="action">A resolved string that defines the action to perform when the button is clicked, such as an expression or method invocation.</param>
		/// <param name="label">Optional label of the button.</param>
		public InlineButtonAttribute(string action, string label = null)
		{
			this.Action = action;
			this.Label = label;
		}

		/// <summary>
		/// Draws a button to the right of the property.
		/// </summary>
		/// <param name="action">A resolved string that defines the action to perform when the button is clicked, such as an expression or method invocation.</param>
		/// <param name="icon">The icon to be shown inside the button.</param>
		/// <param name="label">Optional label of the button.</param>
		public InlineButtonAttribute(string action, SdfIconType icon, string label = null)
		{
			this.Action = action;
			this.Icon = icon;
			this.Label = label;
		}
	}
}