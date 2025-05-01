//-----------------------------------------------------------------------
// <copyright file="ReflectionOrEmittedBaseFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#if (UNITY_EDITOR || UNITY_STANDALONE) && !ENABLE_IL2CPP
#define CAN_EMIT
#endif

namespace Sirenix.Serialization
{
#pragma warning disable

    public abstract class ReflectionOrEmittedBaseFormatter<T> : ReflectionFormatter<T>
    {
#if CAN_EMIT

        protected override void DeserializeImplementation(ref T value, IDataReader reader)
        {
            var formatter = FormatterEmitter.GetEmittedFormatter(typeof(T), reader.Context.Config.SerializationPolicy) as FormatterEmitter.RuntimeEmittedFormatter<T>;

            if (formatter == null)
                return;

            int count = 0;
            string name;
            EntryType entry;

            while ((entry = reader.PeekEntry(out name)) != EntryType.EndOfNode && entry != EntryType.EndOfArray && entry != EntryType.EndOfStream)
            {
                formatter.Read(ref value, name, entry, reader);

                count++;

                if (count > 1000)
                {
                    reader.Context.Config.DebugContext.LogError("Breaking out of infinite reading loop!");
                    break;
                }
            }
        }

        protected override void SerializeImplementation(ref T value, IDataWriter writer)
        {
            var formatter = FormatterEmitter.GetEmittedFormatter(typeof(T), writer.Context.Config.SerializationPolicy) as FormatterEmitter.RuntimeEmittedFormatter<T>;

            if (formatter == null)
                return;

            formatter.Write(ref value, writer);
        }
#endif
    }
}