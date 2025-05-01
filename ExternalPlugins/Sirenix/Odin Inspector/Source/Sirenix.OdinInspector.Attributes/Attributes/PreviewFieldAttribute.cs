//-----------------------------------------------------------------------
// <copyright file="PreviewFieldAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using UnityEngine;

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
    /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
    /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
    /// </para>
    /// <para>
    /// These object fields can also be selectively enabled and customized globally from the Odin preferences window.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how PreviewField is applied to a few property fields.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
    /// {
    ///		[PreviewField]
    ///		public UnityEngine.Object SomeObject;
    ///
    ///		[PreviewField]
    ///		public Texture SomeTexture;
    ///
    ///		[HorizontalGroup, HideLabel, PreviewField(30)]
    ///		public Material A, B, C, D, F;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class PreviewFieldAttribute : Attribute
    {
        private ObjectFieldAlignment alignment;
        private bool alignmentHasValue;
        private string previewGetter;

        /// <summary>
        /// The height of the object field
        /// </summary>
        public float Height;

        /// <summary>
        /// The FilterMode to be used for the preview.
        /// </summary>
        public FilterMode FilterMode = FilterMode.Bilinear;

        /// <summary>
        /// Left aligned.
        /// </summary>
        public ObjectFieldAlignment Alignment
        {
            get => this.alignment;
            set
            {
                this.alignment = value;
                this.alignmentHasValue = true;
            }
        }

        /// <summary>
        /// Whether an alignment value is specified.
        /// </summary>
        public bool AlignmentHasValue => this.alignmentHasValue;

        /// <summary>
        /// A resolved value that should resolve to the desired preview texture.
        /// </summary>
        public string PreviewGetter
        {
            get => this.previewGetter;
            set
            {
                this.previewGetter = value;
                this.PreviewGetterHasValue = true;
            }
        }

        public bool PreviewGetterHasValue { get; private set; }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        public PreviewFieldAttribute()
        {
            this.Height = 0;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="height">The height of the preview field.</param>
        public PreviewFieldAttribute(float height)
        {
            this.Height = height;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="previewGetter">A resolved value that should resolve to the desired preview texture.</param>
        /// <param name="filterMode">The filter mode to be used for the preview texture.</param>
        public PreviewFieldAttribute(string previewGetter, FilterMode filterMode = FilterMode.Bilinear)
        {
            this.PreviewGetter = previewGetter;
            this.FilterMode = filterMode;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="previewGetter">A resolved value that should resolve to the desired preview texture.</param>
        /// <param name="height">The height of the preview field.</param>
        /// <param name="filterMode">The filter mode to be used for the preview texture.</param>
        public PreviewFieldAttribute(string previewGetter, float height, FilterMode filterMode = FilterMode.Bilinear)
        {
            this.PreviewGetter = previewGetter;
            this.Height = height;
            this.FilterMode = filterMode;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="height">The height of the preview field.</param>
        /// <param name="alignment">The alignment of the preview field.</param>
        public PreviewFieldAttribute(float height, ObjectFieldAlignment alignment)
        {
            this.Height = height;
            this.Alignment = alignment;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="previewGetter">A resolved value that should resolve to the desired preview texture.</param>
        /// <param name="alignment">The alignment of the preview field.</param>
        /// <param name="filterMode">The filter mode to be used for the preview texture.</param>
        public PreviewFieldAttribute(string previewGetter, ObjectFieldAlignment alignment, FilterMode filterMode = FilterMode.Bilinear)
        {
            this.PreviewGetter = previewGetter;
            this.Alignment = alignment;
            this.FilterMode = filterMode;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="previewGetter">A resolved value that should resolve to the desired preview texture.</param>
        /// <param name="height">The height of the preview field.</param>
        /// <param name="alignment">The alignment of the preview field.</param>
        /// <param name="filterMode">The filter mode to be used for the preview texture.</param>
        public PreviewFieldAttribute(string previewGetter, float height, ObjectFieldAlignment alignment, FilterMode filterMode = FilterMode.Bilinear)
        {
            this.PreviewGetter = previewGetter;
            this.Height = height;
            this.Alignment = alignment;
            this.FilterMode = filterMode;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="alignment">The alignment of the preview field.</param>
        public PreviewFieldAttribute(ObjectFieldAlignment alignment)
        {
            this.Alignment = alignment;
        }
    }
}