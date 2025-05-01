//-----------------------------------------------------------------------
// <copyright file="GenericNumberUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class GenericNumberUtility
    {
        private static HashSet<Type> Numbers = new HashSet<Type>(FastTypeComparer.Instance)
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(IntPtr),
            typeof(UIntPtr),
        };

        private static HashSet<Type> Vectors = new HashSet<Type>(FastTypeComparer.Instance)
        {
            typeof(Vector2),
            typeof(Vector2Int),
            typeof(Vector3),
            typeof(Vector3Int),
            typeof(Vector4),
        };

        public static bool IsNumber(Type type)
        {
            return Numbers.Contains(type);
        }

        public static bool IsVector(Type type)
        {
            return Vectors.Contains(type);
        }

        public static bool NumberIsInRange(object number, double min, double max)
        {
            if (number is sbyte)
            {
                var n = (sbyte)number;
                return n >= min && n <= max;
            }
            else if (number is byte)
            {
                var n = (byte)number;
                return n >= min && n <= max;
            }
            else if (number is short)
            {
                var n = (short)number;
                return n >= min && n <= max;
            }
            else if (number is ushort)
            {
                var n = (ushort)number;
                return n >= min && n <= max;
            }
            else if (number is int)
            {
                var n = (int)number;
                return n >= min && n <= max;
            }
            else if (number is uint)
            {
                var n = (uint)number;
                return n >= min && n <= max;
            }
            else if (number is long)
            {
                var n = (long)number;
                return n >= min && n <= max;
            }
            else if (number is ulong)
            {
                var n = (ulong)number;
                return n >= min && n <= max;
            }
            else if (number is float)
            {
                var n = (float)number;
                return IsFloatInRange(n, min, max);
            }
            else if (number is double)
            {
                var n = (double)number;
                return IsDoubleInRange(n, min, max);
            }
            else if (number is decimal)
            {
                var n = (decimal)number;
                return n >= (decimal)min && n <= (decimal)max;
            }
            else if (number is Vector2)
            {
                var n = (Vector2)number;
                return IsFloatInRange(n.x, min, max) &&
                       IsFloatInRange(n.y, min, max);
            }
            else if (number is Vector2Int)
            {
                var n = (Vector2Int)number;
                return n.x >= min && n.x <= max
                    && n.y >= min && n.y <= max;
            }
            else if (number is Vector3)
            {
                var n = (Vector3)number;
                return IsFloatInRange(n.x, min, max) &&
                       IsFloatInRange(n.y, min, max) &&
                       IsFloatInRange(n.z, min, max);
            }
            else if (number is Vector3Int)
            {
                var n = (Vector3Int)number;
                return n.x >= min && n.x <= max
                    && n.y >= min && n.y <= max
                    && n.z >= min && n.z <= max;
            }
            else if (number is Vector4)
            {
                var n = (Vector4)number;
                return IsFloatInRange(n.x, min, max) &&
                       IsFloatInRange(n.y, min, max) &&
                       IsFloatInRange(n.z, min, max) &&
                       IsFloatInRange(n.w, min, max);
            }
            else if (number is IntPtr)
            {
                var n = (long)(IntPtr)number;
                return n >= min && n <= max;
            }
            else if (number is UIntPtr)
            {
                var n = (ulong)(UIntPtr)number;
                return n >= min && n <= max;
            }

            return false;
        }

        internal static bool NumberIsInRange(object number, double min, double max, out string errorMessage)
        {
            errorMessage = null;

            string FormatFloatIssue(string label, float v)
            {
                if (float.IsNaN(v))
                    return $"{label} must be a number between {min} and {max}, but is NaN.";

                if (float.IsPositiveInfinity(v))
                {
                    if (double.IsPositiveInfinity(max))
                        return null;
                    return $"{label} must be ≤ {max}, but is Infinity.";
                }

                if (float.IsNegativeInfinity(v))
                {
                    if (double.IsNegativeInfinity(min))
                        return null;
                    return $"{label} must be ≥ {min}, but is -Infinity.";
                }

                if (v < min)
                    return $"{label} must be ≥ {min}, but is {v}.";
                if (v > max)
                    return $"{label} must be ≤ {max}, but is {v}.";

                return null;
            }

            string FormatDoubleIssue(string label, double v)
            {
                if (double.IsNaN(v))
                    return $"{label} must be a number between {min} and {max}, but is NaN.";

                if (double.IsPositiveInfinity(v))
                {
                    if (double.IsPositiveInfinity(max))
                        return null;
                    return $"{label} must be ≤ {max}, but is Infinity.";
                }

                if (double.IsNegativeInfinity(v))
                {
                    if (double.IsNegativeInfinity(min))
                        return null;
                    return $"{label} must be ≥ {min}, but is -Infinity.";
                }

                if (v < min)
                    return $"{label} must be ≥ {min}, but is {v}.";
                if (v > max)
                    return $"{label} must be ≤ {max}, but is {v}.";

                return null;
            }

            if (number is float f)
            {
                errorMessage = FormatFloatIssue("Value", f);
                return errorMessage == null;
            }

            if (number is double d)
            {
                errorMessage = FormatDoubleIssue("Value", d);
                return errorMessage == null;
            }

            if (number is decimal dec)
            {
                if (dec < (decimal)min)
                    errorMessage = $"Value must be ≥ {min}, but is {dec}.";
                else if (dec > (decimal)max)
                    errorMessage = $"Value must be ≤ {max}, but is {dec}.";
                return errorMessage == null;
            }

            if (number is sbyte sb)
            {
                if (sb < min) errorMessage = $"Value must be ≥ {min}, but is {sb}.";
                else if (sb > max) errorMessage = $"Value must be ≤ {max}, but is {sb}.";
                return errorMessage == null;
            }

            if (number is byte b)
            {
                if (b < min) errorMessage = $"Value must be ≥ {min}, but is {b}.";
                else if (b > max) errorMessage = $"Value must be ≤ {max}, but is {b}.";
                return errorMessage == null;
            }

            if (number is short s)
            {
                if (s < min) errorMessage = $"Value must be ≥ {min}, but is {s}.";
                else if (s > max) errorMessage = $"Value must be ≤ {max}, but is {s}.";
                return errorMessage == null;
            }

            if (number is ushort us)
            {
                if (us < min) errorMessage = $"Value must be ≥ {min}, but is {us}.";
                else if (us > max) errorMessage = $"Value must be ≤ {max}, but is {us}.";
                return errorMessage == null;
            }

            if (number is int i)
            {
                if (i < min) errorMessage = $"Value must be ≥ {min}, but is {i}.";
                else if (i > max) errorMessage = $"Value must be ≤ {max}, but is {i}.";
                return errorMessage == null;
            }

            if (number is uint ui)
            {
                if (ui < min) errorMessage = $"Value must be ≥ {min}, but is {ui}.";
                else if (ui > max) errorMessage = $"Value must be ≤ {max}, but is {ui}.";
                return errorMessage == null;
            }

            if (number is long l)
            {
                if (l < min) errorMessage = $"Value must be ≥ {min}, but is {l}.";
                else if (l > max) errorMessage = $"Value must be ≤ {max}, but is {l}.";
                return errorMessage == null;
            }

            if (number is ulong ul)
            {
                if (ul < min) errorMessage = $"Value must be ≥ {min}, but is {ul}.";
                else if (ul > max) errorMessage = $"Value must be ≤ {max}, but is {ul}.";
                return errorMessage == null;
            }

            if (number is IntPtr ip)
            {
                long val = (long)ip;
                if (val < min) errorMessage = $"Value must be ≥ {min}, but is {val}.";
                else if (val > max) errorMessage = $"Value must be ≤ {max}, but is {val}.";
                return errorMessage == null;
            }

            if (number is UIntPtr uip)
            {
                ulong val = (ulong)uip;
                if (val < min) errorMessage = $"Value must be ≥ {min}, but is {val}.";
                else if (val > max) errorMessage = $"Value must be ≤ {max}, but is {val}.";
                return errorMessage == null;
            }

            if (number is Vector2 v2)
            {
                List<string> issues = new List<string>();
                string x = FormatFloatIssue("x", v2.x);
                string y = FormatFloatIssue("y", v2.y);
                if (x != null) issues.Add(x);
                if (y != null) issues.Add(y);
                errorMessage = issues.Count > 0 ? string.Join("\n", issues.ToArray()) : null;
                return errorMessage == null;
            }

            if (number is Vector3 v3)
            {
                List<string> issues = new List<string>();
                string x = FormatFloatIssue("x", v3.x);
                string y = FormatFloatIssue("y", v3.y);
                string z = FormatFloatIssue("z", v3.z);
                if (x != null) issues.Add(x);
                if (y != null) issues.Add(y);
                if (z != null) issues.Add(z);
                errorMessage = issues.Count > 0 ? string.Join("\n", issues.ToArray()) : null;
                return errorMessage == null;
            }

            if (number is Vector4 v4)
            {
                List<string> issues = new List<string>();
                string x = FormatFloatIssue("x", v4.x);
                string y = FormatFloatIssue("y", v4.y);
                string z = FormatFloatIssue("z", v4.z);
                string w = FormatFloatIssue("w", v4.w);
                if (x != null) issues.Add(x);
                if (y != null) issues.Add(y);
                if (z != null) issues.Add(z);
                if (w != null) issues.Add(w);
                errorMessage = issues.Count > 0 ? string.Join("\n", issues.ToArray()) : null;
                return errorMessage == null;
            }

            if (number is Vector2Int v2i)
            {
                List<string> issues = new List<string>();
                if (v2i.x < min) issues.Add($"x must be ≥ {min}, but is {v2i.x}.");
                if (v2i.x > max) issues.Add($"x must be ≤ {max}, but is {v2i.x}.");
                if (v2i.y < min) issues.Add($"y must be ≥ {min}, but is {v2i.y}.");
                if (v2i.y > max) issues.Add($"y must be ≤ {max}, but is {v2i.y}.");
                errorMessage = issues.Count > 0 ? string.Join("\n", issues.ToArray()) : null;
                return errorMessage == null;
            }

            if (number is Vector3Int v3i)
            {
                List<string> issues = new List<string>();
                if (v3i.x < min) issues.Add($"x must be ≥ {min}, but is {v3i.x}.");
                if (v3i.x > max) issues.Add($"x must be ≤ {max}, but is {v3i.x}.");
                if (v3i.y < min) issues.Add($"y must be ≥ {min}, but is {v3i.y}.");
                if (v3i.y > max) issues.Add($"y must be ≤ {max}, but is {v3i.y}.");
                if (v3i.z < min) issues.Add($"z must be ≥ {min}, but is {v3i.z}.");
                if (v3i.z > max) issues.Add($"z must be ≤ {max}, but is {v3i.z}.");
                errorMessage = issues.Count > 0 ? string.Join("\n", issues.ToArray()) : null;
                return errorMessage == null;
            }

            errorMessage = "Unsupported type or not a numeric or vector value.";
            return false;
        }

        internal static bool IsFloatInRange(float value, double min, double max)
        {
            switch (value)
            {
                case float.NaN:
                    return false;

                case float.NegativeInfinity:
                    return double.IsNegativeInfinity(min);

                case float.PositiveInfinity:
                    return double.IsPositiveInfinity(max);

                default:
                    return value >= min && value <= max;
            }
        }

        internal static bool IsDoubleInRange(double value, double min, double max)
        {
            switch (value)
            {
                case double.NaN:
                    return false;

                case double.NegativeInfinity:
                    return double.IsNegativeInfinity(min);

                case double.PositiveInfinity:
                    return double.IsPositiveInfinity(max);

                default:
                    return value >= min && value <= max;
            }
        }

        public static T Clamp<T>(T number, double min, double max)
        {
            if (number is sbyte)
            {
                var n = (sbyte)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is byte)
            {
                var n = (byte)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is short)
            {
                var n = (short)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is ushort)
            {
                var n = (ushort)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is int)
            {
                var n = (int)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is uint)
            {
                var n = (uint)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is long)
            {
                var n = (long)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is ulong)
            {
                var n = (ulong)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is float)
            {
                var n = (float)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is double)
            {
                var n = (double)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is decimal)
            {
                var n = (decimal)(object)number;
                if (n < (decimal)min) return ConvertNumber<T>(min);
                if (n > (decimal)max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is Vector2)
            {
                var n = (Vector2)(object)number;

                if (n.x < min) n.x = (float)min;
                else if (n.x > max) n.x = (float)max;

                if (n.y < min) n.y = (float)min;
                else if (n.y > max) n.y = (float)max;

                return (T)(object)n;
            }
            else if (number is Vector2Int)
            {
                var n = (Vector2Int)(object)number;

                if (n.x < min) n.x = (int)min;
                else if (n.x > max) n.x = (int)max;

                if (n.y < min) n.y = (int)min;
                else if (n.y > max) n.y = (int)max;

                return (T)(object)n;
            }
            else if (number is Vector3)
            {
                var n = (Vector3)(object)number;

                if (n.x < min) n.x = (float)min;
                else if (n.x > max) n.x = (float)max;

                if (n.y < min) n.y = (float)min;
                else if (n.y > max) n.y = (float)max;

                if (n.z < min) n.z = (float)min;
                else if (n.z > max) n.z = (float)max;

                return (T)(object)n;
            }
            else if (number is Vector3Int)
            {
                var n = (Vector3Int)(object)number;

                if (n.x < min) n.x = (int)min;
                else if (n.x > max) n.x = (int)max;

                if (n.y < min) n.y = (int)min;
                else if (n.y > max) n.y = (int)max;

                if (n.z < min) n.z = (int)min;
                else if (n.z > max) n.z = (int)max;

                return (T)(object)n;
            }
            else if (number is Vector4)
            {
                var n = (Vector4)(object)number;

                if (n.x < min) n.x = (float)min;
                else if (n.x > max) n.x = (float)max;

                if (n.y < min) n.y = (float)min;
                else if (n.y > max) n.y = (float)max;

                if (n.z < min) n.z = (float)min;
                else if (n.z > max) n.z = (float)max;

                if (n.w < min) n.w = (float)min;
                else if (n.w > max) n.w = (float)max;

                return (T)(object)n;
            }
            else if (number is IntPtr)
            {
                var n = (long)(IntPtr)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }
            else if (number is UIntPtr)
            {
                var n = (ulong)(IntPtr)(object)number;
                if (n < min) return ConvertNumber<T>(min);
                if (n > max) return ConvertNumber<T>(max);
                return number;
            }

            return number;
        }

        public static T ConvertNumber<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static object ConvertNumberWeak(object value, Type to)
        {
            return Convert.ChangeType(value, to);
        }
    }
}
#endif