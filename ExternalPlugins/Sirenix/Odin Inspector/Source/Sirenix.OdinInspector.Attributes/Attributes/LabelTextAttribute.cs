//-----------------------------------------------------------------------
// <copyright file="LabelTextAttribute.cs" company="Sirenix ApS">
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
    /// <para>LabelText is used to change the labels of properties.</para>
    /// <para>Use this if you want a different label than the name of the property.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how LabelText is applied to a few property fields.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
    /// {
    ///		[LabelText("1")]
    ///		public int MyInt1;
    ///
    ///		[LabelText("2")]
    ///		public int MyInt2;
    ///
    ///		[LabelText("3")]
    ///		public int MyInt3;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class LabelTextAttribute : Attribute
    {
        /// <summary>
        /// The new text of the label.
        /// </summary>
        public string Text;

        /// <summary>
        /// Whether the label text should be nicified before it is displayed, IE, "m_someField" becomes "Some Field".
        /// If the label text is resolved via a member reference, an expression, or the like, then the evaluated result
        /// of that member reference or expression will be nicified.
        /// </summary>
        public bool NicifyText;

        /// <summary>
        /// The icon to be displayed.
        /// </summary>
        public SdfIconType Icon;

        /// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
        public string IconColor;

        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        public LabelTextAttribute(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Give a property a custom icon.
        /// </summary>
        /// <param name="icon">The icon to be shown next to the property.</param>
        public LabelTextAttribute(SdfIconType icon)
        {
            this.Icon = icon;
        }

        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        /// <param name="nicifyText">Whether to nicify the label text.</param>
        public LabelTextAttribute(string text, bool nicifyText)
        {
            this.Text = text;
            this.NicifyText = nicifyText;
        }

        /// <summary>
        /// Give a property a custom label with a custom icon.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        /// <param name="icon">The icon to be displayed.</param>
        public LabelTextAttribute(string text, SdfIconType icon)
        {
            this.Text = text;
            this.Icon = icon;
        }

        /// <summary>
        /// Give a property a custom label with a custom icon.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        /// <param name="nicifyText">Whether to nicify the label text.</param>
        /// <param name="icon">The icon to be displayed.</param>
        public LabelTextAttribute(string text, bool nicifyText, SdfIconType icon)
        {
            this.Text = text;
            this.NicifyText = nicifyText;
            this.Icon = icon;
        }
    }
}