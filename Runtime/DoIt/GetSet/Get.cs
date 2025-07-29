using System;
using JetBrains.Annotations;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class Get<T> : IGetRaw<T>
{
    public abstract T Data { get; }
    object IGetRaw.Data => Data;

    public static implicit operator T(Get<T> provider)
    {
        return provider.Data;
    }

    public static implicit operator Get<T>(T data)
    {
        if (typeof(Object).IsAssignableFrom(typeof(T)))
        {
            return new SerializeField<T>{data = data};
        }
        
        return new SerializeReference<T>{data = data};
    }
}

public interface IGetRaw
{
    object Data { get; }
}

public interface IGetRaw<[UsedImplicitly]T> : IGetRaw
{
}

public interface IKeyGet<T>
{
    public T Data { get; }
    public string Key { get; }
}

[Serializable]
[HideReferenceObjectPicker]
public class SerializeField<T> : Get<T>
{
    [SerializeField] 
    [HideLabel] public T data;
    public override T Data => data;
}

[Serializable]
[HideReferenceObjectPicker]
public class SerializeReference<T> : Get<T>
{
    [SerializeReference] 
    [HideLabel] public T data;
    public override T Data => data;
}

[Serializable]
[HideReferenceObjectPicker]
public class Cast<T> : Get<T>
{
    [SerializeReference] public IGetRaw provider;
    
    public override T Data
    {
        get
        {
            var obj = provider.Data;

            if (obj is not T val)
            {
                if (obj is Component comp)
                {
                    return comp.GetComponent<T>();
                }
                
                if(obj is GameObject go)
                {
                    return go.GetComponent<T>();
                }
            }
            else
            {
                return val;
            }
            
            return (T)obj; 
        }
    }
}

[Serializable]
public abstract class BaseGetRaw<T> : Get<T>, IKeyGet<T>
{
    public string propertyPath;
    [SerializeReference] public IGetRaw<T> data;
    [JsonIgnore] public string Key => propertyPath;
}

[Serializable]
public class Property<T> : BaseGetRaw<T>
{
    private ObjectPathAccessor accessor;
    
    public override T Data
    {
        get
        {
            var d = data.Data;
            accessor ??= PathAccessorCache.GetRef(d, propertyPath);
            return (T)accessor.Get(d);
        }
    }
}

[Serializable]
public class StructProperty<T> : BaseGetRaw<T> where T : struct
{
    private TypedPathAccessor<T> accessor;
    
    public override T Data
    {
        get
        {
            var d = data.Data;
            accessor ??= PathAccessorCache.Get<T>(d, propertyPath);
            return accessor.Get(d);
        }
    }
}

[Serializable]
public class FromBuffer<T> : Get<T>, IGetRaw<T>
{
    object IGetRaw.Data => DataBuffer<object>.value;
    public override T Data => DataBuffer<T>.value;
}

[Serializable]
public class FromKeyBuffer<T> : Get<T>, IGetRaw<T>, IKeyGet<T>
{
    public string key;
    
    object IGetRaw.Data => StringDict<object>.Get(string.Concat(key, typeof(T).GetSimpleFullName()));
    public override T Data => StringDict<T>.Get(key);
    [JsonIgnore] public string Key => key;
}

[Serializable]
public class FromHierarchyPath<T> : Get<T>
{
    [SerializeReference] public Get<Transform> root;
    public string path;
    public override T Data => root.Data.FindComponent<T>(path);
}