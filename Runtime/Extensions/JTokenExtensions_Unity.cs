using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.Extensions
{
    public static partial class JTokenExtensions
    {
        private static CultureInfo invariantCulture = CultureInfo.InvariantCulture;
        
        public static Vector2 ToVector2(this JToken token)
        {
            float x = Convert.ToSingle(((JValue)token["x"]).Value, invariantCulture);
            float y = Convert.ToSingle(((JValue)token["y"]).Value, invariantCulture);
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
            float x = Convert.ToSingle(((JValue)token["x"]).Value, invariantCulture);
            float y = Convert.ToSingle(((JValue)token["y"]).Value, invariantCulture);
            float z = Convert.ToSingle(((JValue)token["z"]).Value, invariantCulture);
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
            float x = Convert.ToSingle(((JValue)token["x"]).Value, invariantCulture);
            float y = Convert.ToSingle(((JValue)token["y"]).Value, invariantCulture);
            float z = Convert.ToSingle(((JValue)token["z"]).Value, invariantCulture);
            float w = Convert.ToSingle(((JValue)token["w"]).Value, invariantCulture);
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
            float r = Convert.ToSingle(((JValue)token["r"]).Value, invariantCulture);
            float g = Convert.ToSingle(((JValue)token["g"]).Value, invariantCulture);
            float b = Convert.ToSingle(((JValue)token["b"]).Value, invariantCulture);
            float a = Convert.ToSingle(((JValue)token["a"]).Value, invariantCulture);
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