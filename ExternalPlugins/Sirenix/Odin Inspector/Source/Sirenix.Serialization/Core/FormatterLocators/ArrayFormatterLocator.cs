//-----------------------------------------------------------------------
// <copyright file="ArrayFormatterLocator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatterLocator(typeof(ArrayFormatterLocator), -80)]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    internal class ArrayFormatterLocator : IFormatterLocator
    {
        public bool TryGetFormatter(Type type, FormatterLocationStep step, ISerializationPolicy policy, bool allowWeakFallbackFormatters, out IFormatter formatter)
        {
            if (!type.IsArray)
            {
                formatter = null;
                return false;
            }

            var elementType = type.GetElementType();

            if (type.GetArrayRank() == 1)
            {
                if (FormatterUtilities.IsPrimitiveArrayType(elementType))
                {
                    try
                    {
                        formatter = (IFormatter)Activator.CreateInstance(typeof(PrimitiveArrayFormatter<>).MakeGenericType(elementType));
                    }
                    catch (Exception ex)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        if (allowWeakFallbackFormatters && (ex is ExecutionEngineException || ex.GetBaseException() is ExecutionEngineException))
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            formatter = new WeakPrimitiveArrayFormatter(type, elementType);
                        }
                        else throw;
                    }
                }
                else
                {
                    try
                    {
                        formatter = (IFormatter)Activator.CreateInstance(typeof(ArrayFormatter<>).MakeGenericType(elementType));
                    }
                    catch (Exception ex)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        if (allowWeakFallbackFormatters && (ex is ExecutionEngineException || ex.GetBaseException() is ExecutionEngineException))
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            formatter = new WeakArrayFormatter(type, elementType);
                        }
                        else throw;
                    }
                }
            }
            else
            {
                try
                {
                    formatter = (IFormatter)Activator.CreateInstance(typeof(MultiDimensionalArrayFormatter<,>).MakeGenericType(type, type.GetElementType()));
                }
                catch (Exception ex)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (allowWeakFallbackFormatters && (ex is ExecutionEngineException || ex.GetBaseException() is ExecutionEngineException))
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        formatter = new WeakMultiDimensionalArrayFormatter(type, elementType);
                    }
                    else throw;
                }
            }

            return true;
        }
    }
}