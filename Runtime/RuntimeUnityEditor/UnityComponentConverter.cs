using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class UnityComponentConverter : JsonConverter
{
    private static readonly Dictionary<Type, (Action<JToken, object, JsonSerializer> WriteFunc,
        Func<JToken, Type, object, JsonSerializer, object> ReadFunc)> specialConverters = new()
    {
        {typeof(Transform), (WriteTransform, ReadTransform)},
        {typeof(SpriteRenderer), (WriteSpriteRenderer, ReadSpriteRenderer)},
    };
    
    public static Component currentComp;
    public static string rootPath;
    public static string pathForForceNull;
    private static UnityComponentConverter currentConverter;
    private readonly BiDictionary<string, object> hashToObject;
    private bool isEditor;

    public UnityComponentConverter(BiDictionary<string, object> hashToObject, bool isEditor)
    {
        this.isEditor = isEditor;
        this.hashToObject = hashToObject;
    }
    
    public override bool CanConvert(Type objectType)
    {
        return typeof(Component).IsAssignableFrom(objectType);
    }
    
    public override void WriteJson(JsonWriter writer, object valu, JsonSerializer serializer)
    {
        Object value = (Object)valu;
        currentConverter = this;
        if (value == null)
        {
            if (pathForForceNull == writer.Path) writer.WriteNull();
            else writer.WriteUndefined();
            
            return;
        }
        
        Type type = value.GetType();
        bool isRootObject = writer.Path == rootPath;
        
        var jObject = SerializeUnityReference(value);
        
        if (!isRootObject)
        {
            jObject.WriteTo(writer);
            return;
        }
        
        if (specialConverters.TryGetValue(type, out var data))
        { 
            data.WriteFunc(jObject, value, serializer);
            jObject.WriteTo(writer);
            return;
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            var isSerializeReference = field.IsDefined(typeof(SerializeReference), true);
            if (field.IsDefined(typeof(NonSerializedAttribute), true))
                continue;

            object fieldValue = field.GetValue(value);
            JToken token = SerializeFieldValue(field, fieldValue, serializer, isSerializeReference);
            jObject[field.Name] = token;
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var isSerializeReference = field.IsDefined(typeof(SerializeReference), true);
            if (!field.IsDefined(typeof(SerializeField), true) && !isSerializeReference)
                continue;
            if (field.IsDefined(typeof(NonSerializedAttribute), true))
                continue;

            object fieldValue = field.GetValue(value);
            JToken token = SerializeFieldValue(field, fieldValue, serializer, isSerializeReference);
            jObject[field.Name] = token;
        }

        jObject.WriteTo(writer);
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        currentConverter = this;
        
        if (reader.TokenType == JsonToken.Null) return null;
        if (reader.TokenType == JsonToken.Undefined) return existingValue;
        
        JObject jObject = JObject.Load(reader);
        
        if (!jObject.TryGetValue("type", out JToken typeToken))
        {
            throw new JsonSerializationException("Отсутствует информация о типе ('type') в JSON.");
        }

        string typeName = typeToken.ToString();
        Type targetType = Type.GetType(typeName);
        
        bool isRootObject = reader.Path == rootPath;

        if (!isRootObject)
        {
            return DeserializeUnityReference(jObject, existingValue);
        }

        if (targetType == null)
        {
            return null;
        }
        
        if (specialConverters.TryGetValue(targetType, out var data))
        {
            return data.ReadFunc(jObject, objectType, existingValue, serializer);
        }
        
        object instance;
        
        if (typeof(Component).IsAssignableFrom(targetType))
        {
            instance = currentComp;
        }
        else
        {
            instance = Activator.CreateInstance(targetType);
        }
        
        foreach (FieldInfo field in targetType.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            var isSerializeReference = field.IsDefined(typeof(SerializeReference), true);
            if (field.IsDefined(typeof(NonSerializedAttribute), true))
                continue;

            if (jObject.TryGetValue(field.Name, out JToken token))
            {
               DeserializeFieldValue(instance, field, token, serializer, isSerializeReference);
            }
        }
        
        foreach (FieldInfo field in targetType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var isSerializeReference = field.IsDefined(typeof(SerializeReference), true);
            if (!field.IsDefined(typeof(SerializeField), true) && !isSerializeReference)
                continue;
            if (field.IsDefined(typeof(NonSerializedAttribute), true))
                continue;

            if (jObject.TryGetValue(field.Name, out JToken token))
            {
                DeserializeFieldValue(instance, field, token, serializer, isSerializeReference);
            }
        }

        return instance;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
    
    private JToken SerializeFieldValue(FieldInfo field, object fieldValue, JsonSerializer serializer, bool isSerializeReference)
    {
        if (!typeof(Object).IsAssignableFrom(field.FieldType))
        {
            if (fieldValue == null)
            {
                return JValue.CreateNull();
            }
            
            var token = JToken.FromObject(fieldValue, serializer);
            if (isSerializeReference)
            {
                token["type"] = fieldValue.GetType().AssemblyQualifiedName;
            }
            return token;
        }
        
        return SerializeUnityReference(fieldValue);
    }
    
    private void DeserializeFieldValue(object instance, FieldInfo field, JToken token, JsonSerializer serializer, bool isSerializeReference)
    {
        object v;
        if (!typeof(Object).IsAssignableFrom(field.FieldType))
        {
            if (isSerializeReference)
            {
                var type = Type.GetType(token["type"].ToString());
                v = token.ToObject(type, serializer);
                goto setValue;
            }
            
            v = token.ToObject(field.FieldType, serializer);
            goto setValue;
        }
        
        v = DeserializeUnityReference(token, null);
        
        setValue:
        if (v != null)
        {
            field.SetValue(instance, v);
        }
    }
    
    public static T DeserializeUnityReference<T>(JToken token, object existingValue) where T : Object
    {
        return DeserializeUnityReference(token, existingValue) as T;
    }
    
    public static object DeserializeUnityReference(JToken token, object existingValue)
    {
        if (token == null || token.Type == JTokenType.Null) return null;
        if(token.Type == JTokenType.Undefined) return existingValue;
        JObject obj = (JObject)token;
        string hashKey = obj["hash"]!.ToString();
        currentConverter.hashToObject.TryGetValueFromKey(hashKey, out var foundObj);
        return foundObj;
    }
    
    public static JToken SerializeUnityReference(object obj)
    {
        if (obj == null) return JValue.CreateNull();
        
        if (currentConverter.isEditor)
        {
            if (currentConverter.hashToObject.TryGetKeyFromValue(obj, out var hashKey))
            {
                return new JObject
                {
                    ["hash"] = hashKey,
                    ["type"] = obj.GetType().AssemblyQualifiedName
                };
            }
            
            return JValue.CreateUndefined();
        }
        
        return new JObject
        {
            ["hash"] = obj.GetHashCode().ToString(),
            ["type"] = obj.GetType().AssemblyQualifiedName
        };
    }
    
    public static JToken Serialize(object comp, JsonSerializer serializer, string pathForForceNull = null)
    {
        UnityComponentConverter.pathForForceNull = pathForForceNull;
        using JTokenWriter jsonWriter = new JTokenWriter();
        serializer.Serialize(jsonWriter, comp);
        var compToken = jsonWriter.Token!;
        UnityComponentConverter.pathForForceNull = null;
        return compToken;
    }

    public static void Populate(object value, JToken comp, JsonSerializer serializer)
    {
        var type = Type.GetType(comp["type"]!.ToString());
        using var reader = comp.CreateReader();
        rootPath = reader.Path;
        currentComp = value as Component;
        serializer.Deserialize(reader, type);
        currentComp = null;
    }
}
