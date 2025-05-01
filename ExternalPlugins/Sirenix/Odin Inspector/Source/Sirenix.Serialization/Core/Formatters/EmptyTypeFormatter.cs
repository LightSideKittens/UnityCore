//-----------------------------------------------------------------------
// <copyright file="EmptyTypeFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// A formatter for empty types. It writes no data, and skips all data that is to be read, deserializing a "default" value.
    /// </summary>
    public class EmptyTypeFormatter<T> : EasyBaseFormatter<T>
    {
        /// <summary>
        /// Skips the entry to read.
        /// </summary>
        protected override void ReadDataEntry(ref T value, string entryName, EntryType entryType, IDataReader reader)
        {
            // Just skip
            reader.SkipEntry();
        }

        /// <summary>
        /// Does nothing at all.
        /// </summary>
        protected override void WriteDataEntries(ref T value, IDataWriter writer)
        {
            // Do nothing
        }
    }
}