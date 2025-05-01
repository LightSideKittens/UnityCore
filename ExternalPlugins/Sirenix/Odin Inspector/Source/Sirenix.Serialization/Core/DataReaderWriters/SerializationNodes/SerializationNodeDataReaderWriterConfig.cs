//-----------------------------------------------------------------------
// <copyright file="SerializationNodeDataReaderWriterConfig.cs" company="Sirenix ApS">
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
    /// Shared config class for <see cref="SerializationNodeDataReader"/> and <see cref="SerializationNodeDataWriter"/>.
    /// </summary>
    public static class SerializationNodeDataReaderWriterConfig
    {
        /// <summary>
        /// The string to use to separate node id's from their names.
        /// </summary>
        public const string NodeIdSeparator = "|";
    }
}