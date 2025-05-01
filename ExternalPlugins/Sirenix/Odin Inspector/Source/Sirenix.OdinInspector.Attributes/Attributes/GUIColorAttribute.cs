//-----------------------------------------------------------------------
// <copyright file="GUIColorAttribute.cs" company="Sirenix ApS">
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
    using UnityEngine;

    /// <summary>
    /// <para>GUIColor is used on any property and changes the GUI color used to draw the property.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how GUIColor is used on a properties to create a rainbow effect.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[GUIColor(1f, 0f, 0f)]
    ///		public int A;
    ///	
    ///		[GUIColor(1f, 0.5f, 0f, 0.2f)]
    ///		public int B;
	///	
	///		[GUIColor("GetColor")]
	///		public int C;
	///		
	///		private Color GetColor() { return this.A == 0 ? Color.red : Color.white; }
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class GUIColorAttribute : Attribute
    {
        /// <summary>
        /// The GUI color of the property.
        /// </summary>
        public Color Color;

        /// <summary>Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow.</summary>
        public string GetColor;

        /// <summary>
        /// Sets the GUI color for the property.
        /// </summary>
        /// <param name="r">The red channel.</param>
        /// <param name="g">The green channel.</param>
        /// <param name="b">The blue channel.</param>
        /// <param name="a">The alpha channel.</param>
        public GUIColorAttribute(float r, float g, float b, float a = 1f)
        {
            this.Color = new Color(r, g, b, a);
        }

        /// <summary>
        /// Sets the GUI color for the property.
        /// </summary>
        /// <param name="getColor">Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor").</param>
        public GUIColorAttribute(string getColor)
        {
            this.GetColor = getColor;
        }
    }
}