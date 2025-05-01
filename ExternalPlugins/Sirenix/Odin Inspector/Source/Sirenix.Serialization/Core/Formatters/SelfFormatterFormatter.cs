//-----------------------------------------------------------------------
// <copyright file="SelfFormatterFormatter.cs" company="Sirenix ApS">
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
    /// Formatter for types that implement the <see cref="ISelfFormatter"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BaseFormatter{T}" />
    public sealed class SelfFormatterFormatter<T> : BaseFormatter<T> where T : ISelfFormatter
    {
        /// <summary>
        /// Calls <see cref="ISelfFormatter.Deserialize" />  on the value to deserialize.
        /// </summary>
        protected override void DeserializeImplementation(ref T value, IDataReader reader)
        {
            value.Deserialize(reader);
        }

        /// <summary>
        /// Calls <see cref="ISelfFormatter.Serialize" />  on the value to deserialize.
        /// </summary>
        protected override void SerializeImplementation(ref T value, IDataWriter writer)
        {
            value.Serialize(writer);
        }
    }

    public sealed class WeakSelfFormatterFormatter : WeakBaseFormatter
    {
        public WeakSelfFormatterFormatter(Type serializedType) : base(serializedType)
        {
        }

        /// <summary>
        /// Calls <see cref="ISelfFormatter.Deserialize" />  on the value to deserialize.
        /// </summary>
        protected override void DeserializeImplementation(ref object value, IDataReader reader)
        {
            ((ISelfFormatter)value).Deserialize(reader);
        }

        /// <summary>
        /// Calls <see cref="ISelfFormatter.Serialize" />  on the value to deserialize.
        /// </summary>
        protected override void SerializeImplementation(ref object value, IDataWriter writer)
        {
            ((ISelfFormatter)value).Serialize(writer);
        }
    }
}