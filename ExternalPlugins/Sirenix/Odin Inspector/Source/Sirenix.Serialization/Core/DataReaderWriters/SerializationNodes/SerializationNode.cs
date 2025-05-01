//-----------------------------------------------------------------------
// <copyright file="SerializationNode.cs" company="Sirenix ApS">
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

    using System;

    /// <summary>
    /// A serialization node as used by the <see cref="DataFormat.Nodes"/> format.
    /// </summary>
    [Serializable]
    public struct SerializationNode
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// The entry type of the node.
        /// </summary>
        public EntryType Entry;

        /// <summary>
        /// The data contained in the node. Depending on the entry type and name, as well as nodes encountered prior to this one, the format can vary wildly.
        /// </summary>
        public string Data;
    }
}