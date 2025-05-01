//-----------------------------------------------------------------------
// <copyright file="GradientAlphaKeyFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(GradientAlphaKeyFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="GradientAlphaKey"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.GradientAlphaKey}" />
    public class GradientAlphaKeyFormatter : MinimalBaseFormatter<GradientAlphaKey>
    {
        private static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref GradientAlphaKey value, IDataReader reader)
        {
            value.alpha = GradientAlphaKeyFormatter.FloatSerializer.ReadValue(reader);
            value.time = GradientAlphaKeyFormatter.FloatSerializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref GradientAlphaKey value, IDataWriter writer)
        {
            GradientAlphaKeyFormatter.FloatSerializer.WriteValue(value.alpha, writer);
            GradientAlphaKeyFormatter.FloatSerializer.WriteValue(value.time, writer);
        }
    }
}