//-----------------------------------------------------------------------
// <copyright file="DataFormat.cs" company="Sirenix ApS">
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
    /// Specifies a data format to read and write in.
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// A custom packed binary format. This format is most efficient and almost allocation-free,
        /// but its serialized data is not human-readable.
        /// </summary>
        Binary = 0,

        /// <summary>
        /// A JSON format compliant with the json specification found at "http://www.json.org/".
        /// <para />
        /// This format has rather sluggish performance and allocates frightening amounts of string garbage.
        /// </summary>
        JSON = 1,

        /// <summary>
        /// A format that does not serialize to a byte stream, but to a list of data nodes in memory
        /// which can then be serialized by Unity.
        /// <para />
        /// This format is highly inefficient, and is primarily used for ensuring that Unity assets
        /// are mergeable by individual values when saved in Unity's text format. This makes
        /// serialized values more robust and data recovery easier in case of issues.
        /// <para />
        /// This format is *not* recommended for use in builds.
        /// </summary>
        Nodes = 2
    }
}