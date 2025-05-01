//-----------------------------------------------------------------------
// <copyright file="ArrayFormatter.cs" company="Sirenix ApS">
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
    /// Formatter for all non-primitive one-dimensional arrays.
    /// </summary>
    /// <typeparam name="T">The element type of the formatted array.</typeparam>
    /// <seealso cref="BaseFormatter{T[]}" />
    public sealed class ArrayFormatter<T> : BaseFormatter<T[]>
    {
        private static Serializer<T> valueReaderWriter = Serializer.Get<T>();

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns>
        /// A null value.
        /// </returns>
        protected override T[] GetUninitializedObject()
        {
            return null;
        }

        /// <summary>
        /// Provides the actual implementation for deserializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="BaseFormatter{T}.GetUninitializedObject" />.</param>
        /// <param name="reader">The reader to deserialize with.</param>
        protected override void DeserializeImplementation(ref T[] value, IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.StartOfArray)
            {
                long length;
                reader.EnterArray(out length);

                value = new T[length];

                // We must remember to register the array reference ourselves, since we return null in GetUninitializedObject
                this.RegisterReferenceID(value, reader);

                // There aren't any OnDeserializing callbacks on arrays.
                // Hence we don't invoke this.InvokeOnDeserializingCallbacks(value, reader, context);
                for (int i = 0; i < length; i++)
                {
                    if (reader.PeekEntry(out name) == EntryType.EndOfArray)
                    {
                        reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
                        break;
                    }

                    value[i] = valueReaderWriter.ReadValue(reader);

                    if (reader.PeekEntry(out name) == EntryType.EndOfStream)
                    {
                        break;
                    }
                }

                reader.ExitArray();
            }
            else
            {
                reader.SkipEntry();
            }
        }

        /// <summary>
        /// Provides the actual implementation for serializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to serialize with.</param>
        protected override void SerializeImplementation(ref T[] value, IDataWriter writer)
        {
            try
            {
                writer.BeginArrayNode(value.Length);

                for (int i = 0; i < value.Length; i++)
                {
                    valueReaderWriter.WriteValue(value[i], writer);
                }
            }
            finally
            {
                writer.EndArrayNode();
            }
        }
    }

    public sealed class WeakArrayFormatter : WeakBaseFormatter
    {
        private readonly Serializer ValueReaderWriter;
        private readonly Type ElementType;

        public WeakArrayFormatter(Type arrayType, Type elementType) : base(arrayType)
        {
            this.ValueReaderWriter = Serializer.Get(elementType);
            this.ElementType = elementType;
        }

        protected override object GetUninitializedObject()
        {
            return null;
        }

        protected override void DeserializeImplementation(ref object value, IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.StartOfArray)
            {
                long length;
                reader.EnterArray(out length);

                Array array = Array.CreateInstance(this.ElementType, length);
                value = array;

                // We must remember to register the array reference ourselves, since we return null in GetUninitializedObject
                this.RegisterReferenceID(value, reader);

                // There aren't any OnDeserializing callbacks on arrays.
                // Hence we don't invoke this.InvokeOnDeserializingCallbacks(value, reader, context);
                for (int i = 0; i < length; i++)
                {
                    if (reader.PeekEntry(out name) == EntryType.EndOfArray)
                    {
                        reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
                        break;
                    }

                    array.SetValue(ValueReaderWriter.ReadValueWeak(reader), i);

                    if (reader.PeekEntry(out name) == EntryType.EndOfStream)
                    {
                        break;
                    }
                }

                reader.ExitArray();
            }
            else
            {
                reader.SkipEntry();
            }
        }

        protected override void SerializeImplementation(ref object value, IDataWriter writer)
        {
            Array array = (Array)value;

            try
            {
                int length = array.Length;
                writer.BeginArrayNode(length);

                for (int i = 0; i < length; i++)
                {
                    ValueReaderWriter.WriteValueWeak(array.GetValue(i), writer);
                }
            }
            finally
            {
                writer.EndArrayNode();
            }
        }
    }
}