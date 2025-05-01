//-----------------------------------------------------------------------
// <copyright file="RectFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(RectFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Rect"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Rect}" />
    public class RectFormatter : MinimalBaseFormatter<Rect>
    {
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Rect value, IDataReader reader)
        {
            value.x = RectFormatter.FloatSerializer.ReadValue(reader);
            value.y = RectFormatter.FloatSerializer.ReadValue(reader);
            value.width = RectFormatter.FloatSerializer.ReadValue(reader);
            value.height = RectFormatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Rect value, IDataWriter writer)
        {
            RectFormatter.FloatSerializer.WriteValue(value.x, writer);
            RectFormatter.FloatSerializer.WriteValue(value.y, writer);
            RectFormatter.FloatSerializer.WriteValue(value.width, writer);
            RectFormatter.FloatSerializer.WriteValue(value.height, writer);
        }
    }
}