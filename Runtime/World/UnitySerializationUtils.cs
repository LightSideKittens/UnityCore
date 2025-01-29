using Sirenix.Serialization;

namespace LSCore
{
    public static class UnitySerializationUtils
    {
        public static void Serialize(
            UnityEngine.Object obj,
            ref SerializationData data,
            bool serializeUnityFields = false,
            SerializationContext context = null)
        {
            if(obj == null) return;
            UnitySerializationUtility.SerializeUnityObject(obj, ref data, serializeUnityFields, context);
        }
        
        public static void Deserialize(
            UnityEngine.Object obj,
            ref SerializationData data,
            DeserializationContext context = null)
        {
            if(obj == null) return;
            UnitySerializationUtility.DeserializeUnityObject(obj, ref data, context);
        }
    }
}