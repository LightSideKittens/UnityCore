//-----------------------------------------------------------------------
// <copyright file="IOverridesSerializationFormat.cs" company="Sirenix ApS">
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
    /// Indicates that an Odin-serialized Unity object controls its own serialization format. Every time it is serialized, it will be asked which format to use.
    /// </summary>
    public interface IOverridesSerializationFormat
    {
        /// <summary>
        /// Gets the format to use for serialization.
        /// </summary>
        DataFormat GetFormatToSerializeAs(bool isPlayer);
    }
}