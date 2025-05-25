using System;
using JetBrains.Annotations;
using LSCore.Extensions.Unity;
using UnityEngine;

[Serializable]
public abstract class Get<T>
{
    public abstract T Data { get; }

    public static implicit operator T(Get<T> provider)
    {
        return provider.Data;
    }
}

public interface IGetRaw<[UsedImplicitly]T>
{
    public object Data { get; }
}

[Serializable]
public class SerializeField<T> : Get<T>, IGetRaw<T>
{
    [SerializeField] public T data;
    object IGetRaw<T>.Data => data;
    public override T Data => data;
}

[Serializable]
public class SerializeReference<T> : Get<T>, IGetRaw<T>
{
    [SerializeReference] public T data;
    object IGetRaw<T>.Data => data;
    public override T Data => data;
}

[Serializable]
public class CastBuffer<T> : Get<T>
{
    [SerializeReference] public IGetRaw<T> provider;
    public override T Data => (T)provider.Data;
}

[Serializable]
public abstract class BaseGetRaw<T> : Get<T>
{
    public string propertyPath;
    [SerializeReference] public IGetRaw<T> data;
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
public class StructProperty<T> : BaseGetRaw<T>
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
    object IGetRaw<T>.Data => DataBuffer<object>.value;
    public override T Data => DataBuffer<T>.value;
}

[Serializable]
public class FromKeyBuffer<T> : Get<T>, IGetRaw<T>
{
    public string key;
    
    object IGetRaw<T>.Data => StringDict<object>.Get(key);
    public override T Data => StringDict<T>.Get(key);
}

[Serializable]
public class FromHierarchyPath<T> : Get<T>
{
    [SerializeReference] public Get<Transform> root;
    public string path;
    private T result;
    
    public override T Data
    {
        get
        {
            if (result == null || result.Equals(null))
            {
                result = root.Data.FindComponent<T>(path);
            }
            
            return result;
        }
    }
}