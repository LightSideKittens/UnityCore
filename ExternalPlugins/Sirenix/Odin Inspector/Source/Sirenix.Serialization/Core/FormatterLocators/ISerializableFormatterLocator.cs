//-----------------------------------------------------------------------
// <copyright file="ISerializableFormatterLocator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(ISerializableFormatterLocator), -110)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using Sirenix.Serialization.Utilities;
    using System;
    using System.Runtime.Serialization;

    internal class ISerializableFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, bool allowWeakFallbackFormatters, out IFormatter formatter)
        {
            if (step != FormatterLocationStep.AfterRegisteredFormatters || !typeof(ISerializable).IsAssignableFrom(type))
            {
                formatter = null;
                return false;
            }

            try
            {
                formatter = (IFormatter)Activator.CreateInstance(typeof(SerializableFormatter<>).MakeGenericType(type));
            }
            catch (Exception ex)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (allowWeakFallbackFormatters && (ex is ExecutionEngineException || ex.GetBaseException() is ExecutionEngineException))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    formatter = new WeakSerializableFormatter(type);
                }
                else throw;
            }

            return true;
        }
    }
}