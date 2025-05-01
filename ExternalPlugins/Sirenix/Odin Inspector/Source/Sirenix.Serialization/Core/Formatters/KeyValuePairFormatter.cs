//-----------------------------------------------------------------------
// <copyright file="KeyValuePairFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(KeyValuePairFormatter<,>), weakFallback: typeof(WeakKeyValuePairFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Custom generic formatter for the generic type definition <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="BaseFormatter{System.Collections.Generic.KeyValuePair{TKey, TValue}}" />
    public sealed class KeyValuePairFormatter<TKey, TValue> : BaseFormatter<KeyValuePair<TKey, TValue>>
    {
        private static readonly Serializer<TKey> KeySerializer = Serializer.Get<TKey>();
        private static readonly Serializer<TValue> ValueSerializer = Serializer.Get<TValue>();

        /// <summary>
        /// Provides the actual implementation for serializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to serialize with.</param>
        protected override void SerializeImplementation(ref KeyValuePair<TKey, TValue> value, IDataWriter writer)
        {
            KeySerializer.WriteValue(value.Key, writer);
            ValueSerializer.WriteValue(value.Value, writer);
        }

        /// <summary>
        /// Provides the actual implementation for deserializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="BaseFormatter{T}.GetUninitializedObject" />.</param>
        /// <param name="reader">The reader to deserialize with.</param>

        protected override void DeserializeImplementation(ref KeyValuePair<TKey, TValue> value, IDataReader reader)
        {
            value = new KeyValuePair<TKey, TValue>(
                KeySerializer.ReadValue(reader),
                ValueSerializer.ReadValue(reader)
            );
        }
    }

    public sealed class WeakKeyValuePairFormatter : WeakBaseFormatter
    {
        private readonly Serializer KeySerializer;
        private readonly Serializer ValueSerializer;

        private readonly PropertyInfo KeyProperty;
        private readonly PropertyInfo ValueProperty;

        public WeakKeyValuePairFormatter(Type serializedType) : base(serializedType)
        {
            var args = serializedType.GetGenericArguments();

            this.KeySerializer = Serializer.Get(args[0]);
            this.ValueSerializer = Serializer.Get(args[1]);

            this.KeyProperty = serializedType.GetProperty("Key");
            this.ValueProperty = serializedType.GetProperty("Value");
        }

        protected override void SerializeImplementation(ref object value, IDataWriter writer)
        {
            KeySerializer.WriteValueWeak(KeyProperty.GetValue(value, null), writer);
            ValueSerializer.WriteValueWeak(ValueProperty.GetValue(value, null), writer);
        }

        protected override void DeserializeImplementation(ref object value, IDataReader reader)
        {
            value = Activator.CreateInstance(this.SerializedType, 
                KeySerializer.ReadValueWeak(reader),
                ValueSerializer.ReadValueWeak(reader)
            );
        }
    }
}