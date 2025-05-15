using System;
using JetBrains.Annotations;
using LSCore.Extensions.Unity;
using UnityEngine;

[Serializable]
public abstract class DataProvider<T>
{
    public abstract T Data { get; }

    public static implicit operator T(DataProvider<T> provider)
    {
        return provider.Data;
    }
}

public interface IRawBufferDataProvider<[UsedImplicitly]T>
{
    public object Data { get; }
}

[Serializable]
public class RefDataProvider<T> : DataProvider<T>
{
    [SerializeField] public T data;
    public override T Data => data;
}

[Serializable]
public abstract class BaseRawBufferDataProvider<T> : DataProvider<T>
{
    public string propertyPath;
    [SerializeReference] public IRawBufferDataProvider<T> provider;
}

[Serializable]
public class CastBufferDataProvider<T> : BaseRawBufferDataProvider<T>
{
    public override T Data => (T)provider.Data;
}

[Serializable]
public class RawClassBufferDataProvider<T> : BaseRawBufferDataProvider<T>
{
    private ObjectPathAccessor accessor;
    
    public override T Data
    {
        get
        {
            if (accessor.Get == null)
            {
                accessor = PathAccessorCache.GetRef(provider.Data.GetType(), propertyPath);
            }
            
            return (T)accessor.Get(provider.Data);
        }
    }
}

[Serializable]
public class RawStructBufferDataProvider<T> : BaseRawBufferDataProvider<T>
{
    private TypedPathAccessor<T> accessor;
    
    public override T Data
    {
        get
        {
            if (accessor.Get == null)
            {
                accessor = PathAccessorCache.Get<T>(provider.Data.GetType(), propertyPath);
            }
            
            return accessor.Get(provider.Data);
        }
    }
}

[Serializable]
public class RawEnumBufferDataProvider<T> : BaseRawBufferDataProvider<T>
{
    private EnumPathAccessor accessor;
    
    public override T Data
    {
        get
        {
            if (accessor.GetRaw == null)
            {
                accessor = PathAccessorCache.GetEnum(provider.Data.GetType(), propertyPath);
            }
            
            return (T)accessor.GetRaw(provider.Data);
        }
    }
}

[Serializable]
public class BufferDataProvider<T> : DataProvider<T>, IRawBufferDataProvider<T>
{
    object IRawBufferDataProvider<T>.Data => DataBuffer<object>.value;
    public override T Data => DataBuffer<T>.value;
}

[Serializable]
public class KeyBufferDataProvider<T> : DataProvider<T>, IRawBufferDataProvider<T>
{
    public string key;
    
    object IRawBufferDataProvider<T>.Data => StringDict<object>.Get(key);
    public override T Data => StringDict<T>.Get(key);
}

[Serializable]
public class PathDataProvider<T> : DataProvider<T>
{
    [SerializeReference] public DataProvider<Transform> root;
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

[Serializable]
public class BufferDataSetter<T> : LSAction
{
    [SerializeReference] public DataProvider<T> provider;

    public override void Invoke()
    {
        DataBuffer<T>.value = provider.Data;
    }
}

[Serializable]
public class KeyBufferDataSetter<T> : LSAction
{
    public string key;
    [SerializeReference] public DataProvider<T> provider;

    public override void Invoke()
    {
        var data = provider.Data;
        StringDict<object>.Set(key, data);
        StringDict<T>.Set(key, data);
    }
}