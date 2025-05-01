//-----------------------------------------------------------------------
// <copyright file="TimeSpanFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(TimeSpanFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Custom formatter for the <see cref="TimeSpan"/> type.
    /// </summary>
    /// <seealso cref="MinimalBaseFormatter{System.TimeSpan}" />
    public sealed class TimeSpanFormatter : MinimalBaseFormatter<TimeSpan>
    {
        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref TimeSpan value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.Integer)
            {
                long ticks;
                reader.ReadInt64(out ticks);
                value = new TimeSpan(ticks);
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref TimeSpan value, IDataWriter writer)
        {
            writer.WriteInt64(null, value.Ticks);
        }
    }
}