//-----------------------------------------------------------------------
// <copyright file="BoundsFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(BoundsFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Bounds"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{UnityEngine.Bounds}" />
    public class BoundsFormatter : MinimalBaseFormatter<Bounds>
    {
        private static readonly Serializer<Vector3> Vector3Serializer = Serializer.Get<Vector3>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Bounds value, IDataReader reader)
        {
            value.center = Vector3Serializer.ReadValue(reader);
            value.size = Vector3Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Bounds value, IDataWriter writer)
        {
            Vector3Serializer.WriteValue(value.center, writer);
            Vector3Serializer.WriteValue(value.size, writer);
        }
    }
}