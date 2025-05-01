//-----------------------------------------------------------------------
// <copyright file="PrimitiveArrayFormatter.cs" company="Sirenix ApS">
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

    using Sirenix.Serialization.Utilities;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Formatter for all primitive one-dimensional arrays.
    /// </summary>
    /// <typeparam name="T">The element type of the formatted array. This type must be an eligible primitive array type, as determined by <see cref="FormatterUtilities.IsPrimitiveArrayType(System.Type)"/>.</typeparam>
    /// <seealso cref="MinimalBaseFormatter{T[]}" />
    public sealed class PrimitiveArrayFormatter<T> : MinimalBaseFormatter<T[]> where T : struct
    {
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
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref T[] value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.PrimitiveArray)
            {
                reader.ReadPrimitiveArray(out value);
                this.RegisterReferenceID(value, reader);
            }
            else
            {
                reader.SkipEntry();
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref T[] value, IDataWriter writer)
        {
            writer.WritePrimitiveArray(value);
        }
    }

    public sealed class WeakPrimitiveArrayFormatter : WeakMinimalBaseFormatter
    {
        private static readonly Dictionary<Type, PrimitiveArrayType> PrimitiveTypes = new Dictionary<Type, PrimitiveArrayType>(FastTypeComparer.Instance)
        {
            { typeof(char   ), PrimitiveArrayType.PrimitiveArray_char     },
            { typeof(sbyte  ), PrimitiveArrayType.PrimitiveArray_sbyte    },
            { typeof(short  ), PrimitiveArrayType.PrimitiveArray_short    },
            { typeof(int    ), PrimitiveArrayType.PrimitiveArray_int      },
            { typeof(long   ), PrimitiveArrayType.PrimitiveArray_long     },
            { typeof(byte   ), PrimitiveArrayType.PrimitiveArray_byte     },
            { typeof(ushort ), PrimitiveArrayType.PrimitiveArray_ushort   },
            { typeof(uint   ), PrimitiveArrayType.PrimitiveArray_uint     },
            { typeof(ulong  ), PrimitiveArrayType.PrimitiveArray_ulong    },
            { typeof(decimal), PrimitiveArrayType.PrimitiveArray_decimal  },
            { typeof(bool   ), PrimitiveArrayType.PrimitiveArray_bool     },
            { typeof(float  ), PrimitiveArrayType.PrimitiveArray_float    },
            { typeof(double ), PrimitiveArrayType.PrimitiveArray_double   },
            { typeof(Guid   ), PrimitiveArrayType.PrimitiveArray_Guid     },
        };

        public enum PrimitiveArrayType
        {
            PrimitiveArray_char     ,
            PrimitiveArray_sbyte    ,
            PrimitiveArray_short    ,
            PrimitiveArray_int      ,
            PrimitiveArray_long     ,
            PrimitiveArray_byte     ,
            PrimitiveArray_ushort   ,
            PrimitiveArray_uint     ,
            PrimitiveArray_ulong    ,
            PrimitiveArray_decimal  ,
            PrimitiveArray_bool     ,
            PrimitiveArray_float    ,
            PrimitiveArray_double   ,
            PrimitiveArray_Guid     ,
        }

        private readonly Type ElementType;
        private readonly PrimitiveArrayType PrimitiveType;

        public WeakPrimitiveArrayFormatter(Type arrayType, Type elementType) : base(arrayType)
        {
            this.ElementType = elementType;
            
            if (!PrimitiveTypes.TryGetValue(elementType, out this.PrimitiveType))
            {
                throw new SerializationAbortException("The type '" + elementType.GetNiceFullName() + "' is not a type that can be written as a primitive array, yet the primitive array formatter is being used for it.");
            }
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns>
        /// A null value.
        /// </returns>
        protected override object GetUninitializedObject()
        {
            return null;
        }

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref object value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.PrimitiveArray)
            {
                switch (this.PrimitiveType)
                {
                    case PrimitiveArrayType.PrimitiveArray_char:
                        {
                            char[] readValue;
                            reader.ReadPrimitiveArray<char>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_sbyte:
                        {
                            sbyte[] readValue;
                            reader.ReadPrimitiveArray<sbyte>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_short:
                        {
                            short[] readValue;
                            reader.ReadPrimitiveArray<short>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_int:
                        {
                            int[] readValue;
                            reader.ReadPrimitiveArray<int>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_long:
                        {
                            long[] readValue;
                            reader.ReadPrimitiveArray<long>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_byte:
                        {
                            byte[] readValue;
                            reader.ReadPrimitiveArray<byte>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_ushort:
                        {
                            ushort[] readValue;
                            reader.ReadPrimitiveArray<ushort>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_uint:
                        {
                            uint[] readValue;
                            reader.ReadPrimitiveArray<uint>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_ulong:
                        {
                            ulong[] readValue;
                            reader.ReadPrimitiveArray<ulong>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_decimal:
                        {
                            decimal[] readValue;
                            reader.ReadPrimitiveArray<decimal>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_bool:
                        {
                            bool[] readValue;
                            reader.ReadPrimitiveArray<bool>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_float:
                        {
                            float[] readValue;
                            reader.ReadPrimitiveArray<float>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_double:
                        {
                            double[] readValue;
                            reader.ReadPrimitiveArray<double>(out readValue);
                            value = readValue;
                        }
                        break;
                    case PrimitiveArrayType.PrimitiveArray_Guid:
                        {
                            Guid[] readValue;
                            reader.ReadPrimitiveArray<Guid>(out readValue);
                            value = readValue;
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                this.RegisterReferenceID(value, reader);
            }
            else
            {
                reader.SkipEntry();
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref object value, IDataWriter writer)
        {
            switch (this.PrimitiveType)
            {
                case PrimitiveArrayType.PrimitiveArray_char   : writer.WritePrimitiveArray<char   >((char   [])value); break;
                case PrimitiveArrayType.PrimitiveArray_sbyte  : writer.WritePrimitiveArray<sbyte  >((sbyte  [])value); break;
                case PrimitiveArrayType.PrimitiveArray_short  : writer.WritePrimitiveArray<short  >((short  [])value); break;
                case PrimitiveArrayType.PrimitiveArray_int    : writer.WritePrimitiveArray<int    >((int    [])value); break;
                case PrimitiveArrayType.PrimitiveArray_long   : writer.WritePrimitiveArray<long   >((long   [])value); break;
                case PrimitiveArrayType.PrimitiveArray_byte   : writer.WritePrimitiveArray<byte   >((byte   [])value); break;
                case PrimitiveArrayType.PrimitiveArray_ushort : writer.WritePrimitiveArray<ushort >((ushort [])value); break;
                case PrimitiveArrayType.PrimitiveArray_uint   : writer.WritePrimitiveArray<uint   >((uint   [])value); break;
                case PrimitiveArrayType.PrimitiveArray_ulong  : writer.WritePrimitiveArray<ulong  >((ulong  [])value); break;
                case PrimitiveArrayType.PrimitiveArray_decimal: writer.WritePrimitiveArray<decimal>((decimal[])value); break;
                case PrimitiveArrayType.PrimitiveArray_bool   : writer.WritePrimitiveArray<bool   >((bool   [])value); break;
                case PrimitiveArrayType.PrimitiveArray_float  : writer.WritePrimitiveArray<float  >((float  [])value); break;
                case PrimitiveArrayType.PrimitiveArray_double : writer.WritePrimitiveArray<double >((double [])value); break;
                case PrimitiveArrayType.PrimitiveArray_Guid   : writer.WritePrimitiveArray<Guid   >((Guid   [])value); break;
            }
        }
    }
}