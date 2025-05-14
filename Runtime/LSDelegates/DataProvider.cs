using System;
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

[Serializable]
public class RefDataProvider<T> : DataProvider<T>
{
    [SerializeField] public T data;
    public override T Data => data;
}

[Serializable]
public class BufferDataProvider<T> : DataProvider<T>
{
    public override T Data => DataBuffer<T>.value;
}

[Serializable]
public class KeyBufferDataProvider<T> : DataProvider<T>
{
    public string key;
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
            if (result.Equals(null))
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
        StringDict<T>.Set(key, provider.Data);
    }
}