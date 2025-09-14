using System;
using JetBrains.Annotations;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class Get<T> : IGetRaw
{
    public abstract T Data { get; }
    public Type Type => typeof(T);
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
    Type Type { get; }
}

public interface IKeyGet<T>
{
    public T Data { get; }
    public string Key { get; }
}

[Serializable]
[HideReferenceObjectPicker]
public class Cast<T> : Get<T>
{
    [SerializeReference] 
    [HideLabel] public IGetRaw data;
    public override T Data => data.Data.Cast<T>();
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
public abstract class BaseGetRaw<T> : Get<T>, IKeyGet<T>
{
    public string propertyPath;
    [SerializeReference] public IGetRaw data;
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
            accessor ??= PathAccessor.GetRef(d, propertyPath);
            return accessor.Get(d).Cast<T>();
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
            accessor ??= PathAccessor.Get<T>(d, propertyPath);
            return accessor.Get(d);
        }
    }
}

[Serializable]
public class FromBuffer<T> : Get<T>, IGetRaw
{
    object IGetRaw.Data => DataBuffer.value;
    public override T Data => DataBuffer.Get<T>();
}

[Serializable]
public class FromKeyBuffer<T> : Get<T>, IGetRaw, IKeyGet<T>
{
    public string key;
    
    object IGetRaw.Data => DataBuffer.GetRaw(key);
    public override T Data => DataBuffer.Get<T>(key);
    [JsonIgnore] public string Key => key;
}

[Serializable]
public class FromHierarchyPath<T> : Get<T>
{
    [SerializeReference] public Get<Transform> root;
    public string path;
    public override T Data => root.Data.FindComponent<T>(path);
}