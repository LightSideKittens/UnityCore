using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using LSCore.Extensions;
using LSCore.JsonSerialization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class BaseUnityObjectConverter
{
    private UnityObjectReferenceConverter referenceConverter;

    protected BaseUnityObjectConverter(UnityObjectReferenceConverter referenceConverter)
    {
        this.referenceConverter = referenceConverter;
    }

    public JToken Serialize(object obj)
    {
        var token = SerializeReference(obj);
        OnSerialize(token, obj);
        return token;
    }
    
    protected abstract void OnSerialize(JToken token, object obj);
    public abstract void Populate(JToken jObj, object obj);
    
    protected JToken SerializeReference(object obj) => referenceConverter.Serialize(obj);
    protected object DeserializeReference(JToken jObj, object existingValue) => referenceConverter.Deserialize(jObj, existingValue);
    protected T DeserializeReference<T>(JToken jObj, object existingValue) where T : UnityEngine.Object
    {
        return referenceConverter.Deserialize(jObj, existingValue) as T;
    }
}

public class UnityObjectReferenceConverter
{
    private readonly BiDictionary<string, object> hashToObject;
    private bool isEditor;

    public UnityObjectReferenceConverter(BiDictionary<string, object> hashToObject, bool isEditor)
    {
        this.isEditor = isEditor;
        this.hashToObject = hashToObject;
    }
    
    public object Deserialize(JToken token, object existingValue)
    {
        if (token == null || token.Type == JTokenType.Null) return null;
        if(token.Type == JTokenType.Undefined) return existingValue;
        JObject obj = (JObject)token;
        string hashKey = obj["hash"]!.ToString();
        hashToObject.TryGetValueFromKey(hashKey, out var foundObj);
        return foundObj;
    }
    
    public JToken Serialize(object obj)
    {
        if (obj == null) return JValue.CreateNull();
        
        if (isEditor)
        {
            if (hashToObject.TryGetKeyFromValue(obj, out var hashKey))
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
}

public class UnityComponentSerializer
{
    public static Dictionary<Type, (Func<object, JObject> serialize, Func<JToken, object> deserialize)> TypeMap { get; } = new()
    {
        { typeof(Vector4), (j => ((Vector4)j).ToJObject(), obj => obj.ToVector4())},
        { typeof(Vector3), (j => ((Vector3)j).ToJObject(), obj => obj.ToVector3())},
        { typeof(Vector2), (j => ((Vector2)j).ToJObject(), obj => obj.ToVector2())},
        { typeof(Color), (j => ((Color)j).ToJObject(), obj => obj.ToColor())},
        { typeof(Bounds), (j => ((Bounds)j).ToJObject(), obj => obj.ToBounds())},
    };
    
    private Dictionary<Type, BaseUnityObjectConverter> converters = new();
    private UnityObjectReferenceConverter referenceConverter;
    private readonly BiDictionary<string, object> hashToObject;
    private bool isEditor;

    public UnityComponentSerializer(BiDictionary<string, object> hashToObject, bool isEditor)
    {
        this.isEditor = isEditor;
        this.hashToObject = hashToObject;
        referenceConverter = new UnityObjectReferenceConverter(hashToObject, isEditor);
        converters.Add(typeof(SpriteRenderer), new SpriteRendererConverter(referenceConverter));
        converters.Add(typeof(Transform), new TransformConverter(referenceConverter));
        converters.Add(typeof(Camera), new CameraConverter(referenceConverter));
    }
    
    private bool ShouldSerializeField(FieldInfo field, out bool isSerializeReference)
    {
        isSerializeReference = false;
        if (field.IsStatic) return false;
        if (field.IsDefined(typeof(NonSerializedAttribute), inherit: true)) return false; 
        var fieldType = field.FieldType;
        if (typeof(Object).IsAssignableFrom(fieldType)) return true;
        if (!fieldType.IsSerializable && !TypeMap.ContainsKey(fieldType)) return false;
        if (field.IsPublic) return true;
        if (field.IsDefined(typeof(SerializeField), inherit: true)) return true;
        if (field.IsDefined(typeof(SerializeReference), inherit: true))
        {
            isSerializeReference = true;
            return true;
        }
        
        return false;
    }
    
    private static bool IsPrimitive(Type type) => type.IsPrimitive || type == typeof(string) || type == typeof(decimal);

    public JToken Serialize(object value)
    {
        var type = value.GetType();

        if (converters.TryGetValue(type, out var converter))
        {
            return converter.Serialize(value);
        }
        
        var obj = referenceConverter.Serialize(value);
        SerializeFields(obj, value, type);
        return obj;
    }

    private void SerializeFields(JToken jObj, object value, Type type)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (ShouldSerializeField(field, out var isSerializeReference))
            {
                var fieldValue = field.GetValue(value);
                var fieldType = field.FieldType;
                JToken token;
                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    var list = (IList)fieldValue;
                    var arr = new JArray();
                    
                    for (int j = 0; j < list.Count; j++)
                    {
                        var element = list[j];
                        if (element == null)
                        {
                            arr.Add(JValue.CreateNull());
                            continue;
                        }
                        
                        arr.Add(SerializeValue(element, element.GetType(), isSerializeReference));
                    }
                    
                    token = arr;
                }
                else
                {
                    token = SerializeValue(fieldValue, fieldType, isSerializeReference);
                }

                jObj[field.Name] = token;
            }
        }
    }

    private JToken SerializeValue(object value, Type type, bool isSerializeReference)
    {
        if (value == null)
        {
            return JValue.CreateNull();
        }
                
        if (IsPrimitive(type))
        {
            return new JValue(value);
        }
        
        if(type.IsEnum)
        {
            return new JValue((int)value);
        }

        if (TypeMap.TryGetValue(type, out var data))
        {
            return data.serialize(value);
        }

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            return referenceConverter.Serialize(value);
        }

        var fieldValueType = value.GetType();
        var obj = new JObject();
        if (isSerializeReference)
        {
            obj["type"] = fieldValueType.AssemblyQualifiedName;
        }
        SerializeFields(obj, value, fieldValueType);
        return obj;
    }


    public void Populate(object value, JToken comp)
    {
        var type = value.GetType();

        if (converters.TryGetValue(type, out var converter))
        { 
            converter.Populate(comp, value);
        }
        
        DeserializeFields(value, comp, type);
    }

    private void DeserializeFields(object value, JToken token, Type type)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (ShouldSerializeField(field, out var isSerializeReference))
            {
                var tokenFieldValue = token[field.Name];
                var fieldValue = field.GetValue(value);
                var fieldType = field.FieldType;
                Type elementType = fieldType;
                
                if (tokenFieldValue is JArray array)
                {
                    IList list;
                    
                    if (fieldType.IsArray)
                    {
                        elementType = fieldType.GetElementType();
                        var dst = Array.CreateInstance(elementType, array.Count);
                        if (fieldValue == null)
                        {
                            list = dst;
                            goto fillList;
                        }
                        var src = (Array)fieldValue;
                        Array.Copy(src, dst, Math.Min(src.Length, dst.Length));
                        list = dst;
                    }
                    else
                    {
                        elementType = fieldType.GetGenericArguments()[0];
                        var dst = (IList)Activator.CreateInstance(fieldType);
                        if (fieldValue == null)
                        {
                            list = dst;
                            for (int j = 0; j < array.Count; j++)
                            {
                                list.Add(null);
                            }
                            goto fillList;
                        }
                        list = (IList)fieldValue;
                        var count = Math.Min(list.Count, array.Count);

                        for (int j = 0; j < count; j++)
                        {
                            dst.Add(list[j]);
                        }
                            
                        for (int j = count; j < array.Count; j++)
                        {
                            dst.Add(null);
                        }
                            
                        list = dst;
                    }

                    fillList:
                    for (int j = 0; j < array.Count; j++)
                    {
                        tokenFieldValue = array[j];
                        fieldValue = list[j];
                        if (tokenFieldValue.Type == JTokenType.Null)
                        {
                            list[j] = null;
                            continue;
                        }
                        
                        list[j] = DeserializeValue(tokenFieldValue, fieldValue, elementType, isSerializeReference);
                    }
                    
                    field.SetValue(value, list);
                }
                else
                {
                    field.SetValue(value, DeserializeValue(tokenFieldValue, fieldValue, elementType, isSerializeReference));
                }
            }
        }
    }

    private static CultureInfo invariantCulture = CultureInfo.InvariantCulture;
    
    private object DeserializeValue(JToken token, object value, Type type, bool isSerializeReference)
    {
        if (token.Type == JTokenType.Null)
        {
            return null;
        }
        
        if (IsPrimitive(type))
        {
            return Convert.ChangeType(((JValue)token).Value, type, invariantCulture);
        }
        
        if(type.IsEnum)
        {
            return Enum.ToObject(type, ((JValue)token).Value);
        }

        if (TypeMap.TryGetValue(type, out var data))
        {
            return data.deserialize(token);
        }

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            return referenceConverter.Deserialize(token, value);
        }
        
        if (isSerializeReference)
        {
            type = Type.GetType(token["type"]!.ToString());
        }

        if (value == null || value.GetType() != type)
        {
            value = Activator.CreateInstance(type);
        }
        
        DeserializeFields(value, token, type);
        return value;
    }
}