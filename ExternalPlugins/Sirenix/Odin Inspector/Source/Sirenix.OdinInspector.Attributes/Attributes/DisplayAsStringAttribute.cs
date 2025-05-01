//-----------------------------------------------------------------------
// <copyright file="DisplayAsStringAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#define ODIN_PREVIEW
using UnityEngine;

namespace Sirenix.OdinInspector
{
#pragma warning disable

#pragma warning disable

    using System;

    /// <summary>
    /// <para>DisplayAsString is used on any property, and displays a string in the inspector as text.</para>
    /// <para>Use this for when you want to show a string in the inspector, but not allow for any editing.</para>
    /// </summary>
    /// <remarks>
    /// <para>DisplayAsString uses the property's ToString method to display the property as a string.</para>
    /// </remarks>
    /// <example>
    /// <para>The following example shows how DisplayAsString is used to display a string property as text in the inspector.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///    {
    ///        [DisplayAsString]
    ///        public string MyInt = 5;
    ///
    ///        // You can combine with <see cref="HideLabelAttribute"/> to display a message in the inspector.
    ///        [DisplayAsString, HideLabel]
    ///        public string MyMessage = "This string will be displayed as text in the inspector";
    ///
    ///        [DisplayAsString(false)]
    ///        public string InlineMessage = "This string is very long, but has been configured to not overflow.";
    ///    }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="MultiLinePropertyAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class DisplayAsStringAttribute : Attribute
    {
        /// <summary>
        /// If <c>true</c>, the string will overflow past the drawn space and be clipped when there's not enough space for the text.
        /// If <c>false</c> the string will expand to multiple lines, if there's not enough space when drawn.
        /// </summary>
        public bool Overflow;

        /// <summary>
        /// How the string should be aligned.
        /// </summary>
        public TextAlignment Alignment;

        /// <summary>
        /// The size of the font.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// If <c>true</c> the string will support rich text.
        /// </summary>
        public bool EnableRichText;

        /// <summary>
        /// String for formatting the value. Type must implement the <c>IFormattable</c> interface.
        /// </summary>
        public string Format;

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        public DisplayAsStringAttribute()
        {
            this.Overflow = true;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        public DisplayAsStringAttribute(bool overflow)
        {
            this.Overflow = overflow;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="alignment">How the string should be aligned.</param>
        public DisplayAsStringAttribute(TextAlignment alignment)
        {
            this.Alignment = alignment;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        public DisplayAsStringAttribute(int fontSize)
        {
            this.FontSize = fontSize;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        /// <param name="alignment">How the string should be aligned.</param>
        public DisplayAsStringAttribute(bool overflow, TextAlignment alignment)
        {
            this.Overflow = overflow;
            this.Alignment = alignment;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        /// <param name="fontSize">The size of the font.</param>
        public DisplayAsStringAttribute(bool overflow, int fontSize)
        {
            this.Overflow = overflow;
            this.FontSize = fontSize;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="alignment">How the string should be aligned.</param>
        public DisplayAsStringAttribute(int fontSize, TextAlignment alignment)
        {
            this.FontSize = fontSize;
            this.Alignment = alignment;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="alignment">How the string should be aligned.</param>
        public DisplayAsStringAttribute(bool overflow, int fontSize, TextAlignment alignment)
        {
            this.Overflow = overflow;
            this.FontSize = fontSize;
            this.Alignment = alignment;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="alignment">How the string should be aligned.</param>
        /// <param name="enableRichText">If <c>true</c> the string will support rich text.</param>
        public DisplayAsStringAttribute(TextAlignment alignment, bool enableRichText)
        {
            this.Alignment = alignment;
            this.EnableRichText = enableRichText;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="enableRichText">If <c>true</c> the string will support rich text.</param>
        public DisplayAsStringAttribute(int fontSize, bool enableRichText)
        {
            this.FontSize = fontSize;
            this.EnableRichText = enableRichText;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        /// <param name="alignment">How the string should be aligned.</param>
        /// <param name="enableRichText">If <c>true</c> the string will support rich text.</param>
        public DisplayAsStringAttribute(bool overflow, TextAlignment alignment, bool enableRichText)
        {
            this.Overflow = overflow;
            this.Alignment = alignment;
            this.EnableRichText = enableRichText;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="enableRichText">If <c>true</c> the string will support rich text.</param>
        public DisplayAsStringAttribute(bool overflow, int fontSize, bool enableRichText)
        {
            this.Overflow = overflow;
            this.FontSize = fontSize;
            this.EnableRichText = enableRichText;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="alignment">How the string should be aligned.</param>
        /// <param name="enableRichText">If <c>true</c> the string will support rich text.</param>
        public DisplayAsStringAttribute(int fontSize, TextAlignment alignment, bool enableRichText)
        {
            this.FontSize = fontSize;
            this.Alignment = alignment;
            this.EnableRichText = enableRichText;
        }

        /// <summary>
        /// Displays the property as a string in the inspector.
        /// </summary>
        /// <param name="overflow">Value indicating if the string should overflow past the available space, or expand to multiple lines when there's not enough horizontal space.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="alignment">How the string should be aligned.</param>
        /// <param name="enableRichText">If <c>true</c> the string will support rich text.</param>
        public DisplayAsStringAttribute(bool overflow, int fontSize, TextAlignment alignment, bool enableRichText)
        {
            this.Overflow = overflow;
            this.FontSize = fontSize;
            this.Alignment = alignment;
            this.EnableRichText = enableRichText;
        }
    }
}