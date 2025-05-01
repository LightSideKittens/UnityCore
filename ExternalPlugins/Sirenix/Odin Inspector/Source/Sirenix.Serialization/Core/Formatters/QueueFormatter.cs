//-----------------------------------------------------------------------
// <copyright file="QueueFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(QueueFormatter<,>), weakFallback: typeof(WeakQueueFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using Sirenix.Serialization.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Custom generic formatter for the generic type definition <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type of the formatted queue.</typeparam>
    /// <seealso cref="BaseFormatter{System.Collections.Generic.Queue{T}}" />
    public class QueueFormatter<TQueue, TValue> : BaseFormatter<TQueue>
        where TQueue : Queue<TValue>, new()
    {
        private static readonly Serializer<TValue> TSerializer = Serializer.Get<TValue>();
        private static readonly bool IsPlainQueue = typeof(TQueue) == typeof(Queue<TValue>);

        static QueueFormatter()
        {
            // This exists solely to prevent IL2CPP code stripping from removing the generic type's instance constructor
            // which it otherwise seems prone to do, regardless of what might be defined in any link.xml file.

            new QueueFormatter<Queue<int>, int>();
        }

        public QueueFormatter()
        {
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns>
        /// A null value.
        /// </returns>
        protected override TQueue GetUninitializedObject()
        {
            return null;
        }

        /// <summary>
        /// Provides the actual implementation for deserializing a value of type <see cref="T" />.
        /// </summary>
        /// <param name="value">The uninitialized value to serialize into. This value will have been created earlier using <see cref="BaseFormatter{T}.GetUninitializedObject" />.</param>
        /// <param name="reader">The reader to deserialize with.</param>
        protected override void DeserializeImplementation(ref TQueue value, IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.StartOfArray)
            {
                try
                {
                    long length;
                    reader.EnterArray(out length);

                    if (IsPlainQueue)
                    {
                        value = (TQueue)new Queue<TValue>((int)length);
                    }
                    else
                    {
                        value = new TQueue();
                    }

                    // We must remember to register the queue reference ourselves, since we return null in GetUninitializedObject
                    this.RegisterReferenceID(value, reader);

                    // There aren't any OnDeserializing callbacks on queues.
                    // Hence we don't invoke this.InvokeOnDeserializingCallbacks(value, reader, context);
                    for (int i = 0; i < length; i++)
                    {
                        if (reader.PeekEntry(out name) == EntryType.EndOfArray)
                        {
                            reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
                            break;
                        }

                        value.Enqueue(TSerializer.ReadValue(reader));

                        if (reader.IsInArrayNode == false)
                        {
                            // Something has gone wrong
                            reader.Context.Config.DebugContext.LogError("Reading array went wrong. Data dump: " + reader.GetDataDump());
                            break;
                        }
                    }
                }
                finally
                {
                    reader.ExitArray();
                }
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
        protected override void SerializeImplementation(ref TQueue value, IDataWriter writer)
        {
            try
            {
                writer.BeginArrayNode(value.Count);

                foreach (var element in value)
                {
                    try
                    {
                        TSerializer.WriteValue(element, writer);
                    }
                    catch (Exception ex)
                    {
                        writer.Context.Config.DebugContext.LogException(ex);
                    }
                }
            }
            finally
            {
                writer.EndArrayNode();
            }
        }
    }

    public class WeakQueueFormatter : WeakBaseFormatter
    {
        private readonly Serializer ElementSerializer;
        private readonly bool IsPlainQueue;
        private MethodInfo EnqueueMethod;

        public WeakQueueFormatter(Type serializedType) : base(serializedType)
        {
            var args = serializedType.GetArgumentsOfInheritedOpenGenericClass(typeof(Queue<>));
            this.ElementSerializer = Serializer.Get(args[0]);
            this.IsPlainQueue = serializedType.IsGenericType && serializedType.GetGenericTypeDefinition() == typeof(Queue<>);
            this.EnqueueMethod = serializedType.GetMethod("Enqueue", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { args[0] }, null);

            if (this.EnqueueMethod == null)
            {
                throw new SerializationAbortException("Can't serialize type '" + serializedType.GetNiceFullName() + "' because no proper Enqueue method was found.");
            }
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
                try
                {
                    long length;
                    reader.EnterArray(out length);

                    if (IsPlainQueue)
                    {
                        value = Activator.CreateInstance(this.SerializedType, (int)length);
                    }
                    else
                    {
                        value = Activator.CreateInstance(this.SerializedType);
                    }

                    var collection = (ICollection)value;

                    // We must remember to register the queue reference ourselves, since we return null in GetUninitializedObject
                    this.RegisterReferenceID(value, reader);

                    var enqueueParams = new object[1];

                    // There aren't any OnDeserializing callbacks on queues.
                    // Hence we don't invoke this.InvokeOnDeserializingCallbacks(value, reader, context);
                    for (int i = 0; i < length; i++)
                    {
                        if (reader.PeekEntry(out name) == EntryType.EndOfArray)
                        {
                            reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
                            break;
                        }

                        enqueueParams[0] = this.ElementSerializer.ReadValueWeak(reader);
                        EnqueueMethod.Invoke(value, enqueueParams);

                        if (reader.IsInArrayNode == false)
                        {
                            // Something has gone wrong
                            reader.Context.Config.DebugContext.LogError("Reading array went wrong. Data dump: " + reader.GetDataDump());
                            break;
                        }
                    }
                }
                finally
                {
                    reader.ExitArray();
                }
            }
            else
            {
                reader.SkipEntry();
            }
        }

        protected override void SerializeImplementation(ref object value, IDataWriter writer)
        {
            try
            {
                var collection = (ICollection)value;

                writer.BeginArrayNode(collection.Count);

                foreach (var element in collection)
                {
                    try
                    {
                        this.ElementSerializer.WriteValueWeak(element, writer);
                    }
                    catch (Exception ex)
                    {
                        writer.Context.Config.DebugContext.LogException(ex);
                    }
                }
            }
            finally
            {
                writer.EndArrayNode();
            }
        }
    }
}