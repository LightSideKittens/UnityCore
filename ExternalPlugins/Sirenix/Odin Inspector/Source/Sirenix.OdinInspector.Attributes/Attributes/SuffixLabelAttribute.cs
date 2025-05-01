//-----------------------------------------------------------------------
// <copyright file="SuffixLabelAttribute.cs" company="Sirenix ApS">
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
    /// <para>The SuffixLabel attribute draws a label at the end of a property.</para>
    /// <para>Use this for conveying intend about a property. Is the distance measured in meters, kilometers, or in light years?.
    /// Is the angle measured in degrees or radians?
    /// Using SuffixLabel, you can place a neat label at the end of a property, to clearly show how the the property is used.</para>
    /// </summary>
    /// <example>
    /// <para>The following example demonstrates how SuffixLabel is used.</para>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		// The SuffixLabel attribute draws a label at the end of a property.
    ///		// It's useful for conveying intend about a property.
    ///		// Fx, this field is supposed to have a prefab assigned.
    ///		[SuffixLabel("Prefab")]
    ///		public GameObject GameObject;
    ///
    ///		// Using the Overlay property, the suffix label will be drawn on top of the property instead of behind it.
    ///		// Use this for a neat inline look.
    ///		[SuffixLabel("ms", Overlay = true)]
    ///		public float Speed;
    ///
    ///		[SuffixLabel("radians", Overlay = true)]
    ///		public float Angle;
    ///
    ///		// The SuffixLabel attribute also supports string member references by using $.
    ///		[SuffixLabel("$Suffix", Overlay = true)]
    ///		public string Suffix = "Dynamic suffix label";
    ///	}
    /// </code>
    /// </example>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="InlineButtonAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class SuffixLabelAttribute : Attribute
    {
        /// <summary>
        /// The label displayed at the end of the property.
        /// </summary>
        public string Label;

        /// <summary>
        /// If <c>true</c> the suffix label will be drawn on top of the property, instead of after.
        /// </summary>
        public bool Overlay;

        /// <summary>
        /// The icon to be displayed.
        /// </summary>
        public SdfIconType Icon
        {
            get => this.icon;
            set
            {
                this.icon = value;
                this.HasDefinedIcon = true;
            }
        }

        /// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
        public string IconColor;

        public bool HasDefinedIcon { get; private set; }

        private SdfIconType icon;

        /// <summary>
        /// Draws a label at the end of the property.
        /// </summary>
        /// <param name="label">The text of the label.</param>
        /// <param name="overlay">If <c>true</c> the suffix label will be drawn on top of the property, instead of after.</param>
        public SuffixLabelAttribute(string label, bool overlay = false)
        {
            this.Label = label;
            this.Overlay = overlay;
        }

        /// <summary>
        /// Draws a label at the end of the property.
        /// </summary>
        /// <param name="label">The text of the label.</param>
        /// <param name="icon">The icon to be displayed.</param>
        /// <param name="overlay">If <c>true</c> the suffix label will be drawn on top of the property, instead of after.</param>
        public SuffixLabelAttribute(string label, SdfIconType icon, bool overlay = false)
        {
            this.Label = label;
            this.Icon = icon;
            this.Overlay = overlay;
        }


        /// <summary>
        /// Draws a label at the end of the property.
        /// </summary>
        /// <param name="icon">The icon to be displayed.</param>
        public SuffixLabelAttribute(SdfIconType icon)
        {
            this.Icon = icon;
        }
    }
}