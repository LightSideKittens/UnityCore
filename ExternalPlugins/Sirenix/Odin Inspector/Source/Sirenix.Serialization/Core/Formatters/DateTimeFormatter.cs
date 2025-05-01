//-----------------------------------------------------------------------
// <copyright file="DateTimeFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(DateTimeFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Custom formatter for the <see cref="DateTime"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{System.DateTime}" />
    public sealed class DateTimeFormatter : MinimalBaseFormatter<DateTime>
    {
        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref DateTime value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.Integer)
            {
                long binary;
                reader.ReadInt64(out binary);
                value = DateTime.FromBinary(binary);
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref DateTime value, IDataWriter writer)
        {
            writer.WriteInt64(null, value.ToBinary());
        }
    }
}