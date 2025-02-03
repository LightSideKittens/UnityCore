using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.Extensions
{
    public static partial class JTokenExtensions
    {
        public static Vector2 ToVector2(this JToken token)
        {
            float x = token["x"].ToObject<float>();
            float y = token["y"].ToObject<float>();
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
            float x = token["x"].ToObject<float>();
            float y = token["y"].ToObject<float>();
            float z = token["z"].ToObject<float>();
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
        
        
        
        public static Quaternion ToQuaternion(this JToken token)
        {
            float x = token["x"].ToObject<float>();
            float y = token["y"].ToObject<float>();
            float z = token["z"].ToObject<float>();
            float w = token["w"].ToObject<float>();
            return new Quaternion(x, y, z, w);
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
            float r = token["r"].ToObject<float>();
            float g = token["g"].ToObject<float>();
            float b = token["b"].ToObject<float>();
            float a = token["a"].ToObject<float>();
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
    }
}