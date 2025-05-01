//-----------------------------------------------------------------------
// <copyright file="ISupportsPrefabSerialization.cs" company="Sirenix ApS">
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
    /// Indicates that an Odin-serialized Unity object supports prefab serialization.
    /// </summary>
    public interface ISupportsPrefabSerialization
    {
        /// <summary>
        /// Gets or sets the serialization data of the object.
        /// </summary>
        SerializationData SerializationData { get; set; }
    }
}