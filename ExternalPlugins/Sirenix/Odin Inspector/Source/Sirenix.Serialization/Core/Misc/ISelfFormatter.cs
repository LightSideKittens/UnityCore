//-----------------------------------------------------------------------
// <copyright file="ISelfFormatter.cs" company="Sirenix ApS">
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
    /// Specifies that a type is capable of serializing itself using an <see cref="IDataWriter"/> and an
    /// <see cref="IDataReader"/>.
    /// <para />
    /// The deserialized type instance will be created without a constructor call using the
    /// <see cref="System.Runtime.Serialization.FormatterServices.GetUninitializedObject(System.Type)"/>
    /// method if it is a reference type, otherwise it will be created using default(type).
    /// <para />
    /// Use <see cref="AlwaysFormatsSelfAttribute"/> to specify that a class which implements this
    /// interface should *always* format itself regardless of other formatters being specified.
    /// </summary>
    public interface ISelfFormatter
    {
        /// <summary>
        /// Serializes the instance's data using the given writer.
        /// </summary>
        void Serialize(IDataWriter writer);

        /// <summary>
        /// Deserializes data into the instance using the given reader.
        /// </summary>
        void Deserialize(IDataReader reader);
    }
}