//-----------------------------------------------------------------------
// <copyright file="GenericCollectionFormatterLocator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(GenericCollectionFormatterLocator), -100)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using Utilities;
    using System;

    internal class GenericCollectionFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, bool allowWeakFallbackFormatters, out IFormatter formatter)
        {
            Type elementType;
            if (step != FormatterLocationStep.AfterRegisteredFormatters || !GenericCollectionFormatter.CanFormat(type, out elementType))
            {
                formatter = null;
                return false;
            }

            try
            {
                formatter = (IFormatter)Activator.CreateInstance(typeof(GenericCollectionFormatter<,>).MakeGenericType(type, elementType));
            }
            catch (Exception ex)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (allowWeakFallbackFormatters && (ex is ExecutionEngineException || ex.GetBaseException() is ExecutionEngineException))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    formatter = new WeakGenericCollectionFormatter(type, elementType);
                }
                else throw;
            }

            return true;
        }
    }
}