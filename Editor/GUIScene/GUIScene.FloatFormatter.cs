using System;
using System.Globalization;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class GUIScene
    {
        public static class FloatFormatter
        {
            public static string Format(float nextPower, float value)
            {
                string str;

                if (nextPower > 1f)
                {
                    str = $"{(int)value}";
                }
                else
                {
                    int zeroCount = GetLeadingZeroCount(nextPower);
                    str = value.ToString($"F{zeroCount}", CultureInfo.InvariantCulture);
                }

                return str;
            }

            private static int GetLeadingZeroCount(float number)
            {
                string s = number.ToString("F7", CultureInfo.InvariantCulture);
                s = s.Replace(".", string.Empty);
                    
                int zeroCount = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = s[i];
                    if (ch == '0')
                    {
                        zeroCount++;
                        continue;
                    }
                        
                    break;
                }

                return zeroCount;
            }
            
            public static Vector2 NextPowerOfScale(Vector3 scale, int baseValue)
            {
                var min = Math.Pow(baseValue, -5);
                var max = Mathf.Pow(baseValue, 6);
                return new Vector2(NextPowerOf((int)(scale.x / min), baseValue) / max,
                    NextPowerOf((int)(scale.y / min), baseValue) / max);
            }

            private static int NextPowerOf(int value, int baseValue)
            {
                if (value < 1)
                    return 1;

                float logValue = Mathf.Log(value, baseValue);
                int ceilPower = Mathf.CeilToInt(logValue);
                int candidate = (int)Mathf.Pow(baseValue, ceilPower);
                if (candidate <= value)
                {
                    candidate *= baseValue;
                }

                return candidate;
            }
        }
    }
}