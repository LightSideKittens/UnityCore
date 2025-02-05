using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.Extensions
{
    public static partial class JTokenExtensions
    {
        private static CultureInfo invariantCulture = CultureInfo.InvariantCulture;
        
        public static bool ToBool(this JToken token)
        {
            return Convert.ToBoolean(((JValue)token).Value, invariantCulture);
        }
        
        public static JValue ToJValue(this bool value)
        {
            return new JValue(value);
        }
        
        
        
        public static int ToInt(this JToken token)
        {
            return Convert.ToInt32(((JValue)token).Value, invariantCulture);
        }
        
        public static JValue ToJValue(this int value)
        {
            return new JValue(value);
        }
        
        
        
        public static float ToFloat(this JToken token)
        {
            return Convert.ToSingle(((JValue)token).Value, invariantCulture);
        }
        
        public static JValue ToJValue(this float value)
        {
            return new JValue(value);
        }
        
        
        
        public static Vector2 ToVector2(this JToken token)
        {
            float x = token["x"].ToFloat();
            float y = token["y"].ToFloat();
            return new Vector2(x, y);
        }
        
        public static JObject ToJObject(this Vector2 vector)
        {
            return new JObject
            {
                { "x", vector.x },
                { "y", vector.y },
            };
        }
        
        
        
        public static Vector3 ToVector3(this JToken token)
        {
            float x = token["x"].ToFloat();
            float y = token["y"].ToFloat();
            float z = token["z"].ToFloat();
            return new Vector3(x, y, z);
        }
        
        public static JObject ToJObject(this Vector3 vector)
        {
            return new JObject
            {
                { "x", vector.x },
                { "y", vector.y },
                { "z", vector.z },
            };
        }
        
        
        public static Vector4 ToVector4(this JToken token)
        {
            float x = token["x"].ToFloat();
            float y = token["y"].ToFloat();
            float z = token["z"].ToFloat();
            float w = token["w"].ToFloat();
            return new Vector4(x, y, z, w);
        }
        
        public static JObject ToJObject(this Vector4 vector)
        {
            return new JObject
            {
                { "x", vector.x },
                { "y", vector.y },
                { "z", vector.z },
                { "w", vector.w },
            };
        }
        
        
        
        public static Quaternion ToQuaternion(this JToken token)
        {
            var v = token.ToVector4();
            return new Quaternion(v.x, v.y, v.z, v.w);
        }
        
        public static JObject ToJObject(this Quaternion quaternion)
        {
            return new JObject
            {
                { "x", quaternion.x },
                { "y", quaternion.y },
                { "z", quaternion.z },
                { "w", quaternion.w },
            };
        }
        
        
        public static Color ToColor(this JToken token)
        {
            float r = token["r"].ToFloat();
            float g = token["g"].ToFloat();
            float b = token["b"].ToFloat();
            float a = token["a"].ToFloat();
            return new Color(r, g, b, a);
        }
        
        public static JObject ToJObject(this Color color)
        {
            return new JObject
            {
                { "r", color.r },
                { "g", color.g },
                { "b", color.b },
                { "a", color.a },
            };
        }
        
        
        public static Bounds ToBounds(this JToken token)
        {
            var center = token["center"].ToVector3();
            var size = token["size"].ToVector3();
            return new Bounds(center, size);
        }
        
        public static JObject ToJObject(this Bounds bounds)
        {
            return new JObject
            {
                { "center", bounds.center.ToJObject() },
                { "size", bounds.size.ToJObject() },
            };
        }
    }
}