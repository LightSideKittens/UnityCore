using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class SetBuffer<T> : DoIt
{
    [SerializeReference] public Get<T> data;

    public override void Invoke()
    {
        var d = data.Data;
        DataBuffer<object>.value = d;
        DataBuffer<T>.value = d;
    }
}

[Serializable]
public class SetKeyBuffer<T> : DoIt
{
    public string key;
    [SerializeReference] public Get<T> data;

    public override void Invoke()
    {
        var d = data.Data;
        StringDict<object>.Set(key, d);
        StringDict<T>.Set(key, d);
    }
}

[Serializable]
public abstract class Set<TTarget, TValue> : DoIt
{
    public string propertyPath;
    [SerializeReference] public IGetRaw<TTarget> target;
    [SerializeReference] public Get<TValue> value;
}

[Serializable]
public class SetUnityObject : Set<Object, Object>
{
    private ObjectPathAccessor accessor;

    public override void Invoke()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessorCache.GetRef(t, propertyPath);
        accessor.Set(t, v);
    }
}

[Serializable]
public abstract class Set : Set<object, object>
{
}

[Serializable]
public class SetClass : Set
{
    private ObjectPathAccessor accessor;

    public override void Invoke()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessorCache.GetRef(t, propertyPath);
        accessor.Set(t, v);
    }
}

[Serializable]
public class SetStruct<TValue> : DoIt
{
    public string propertyPath;
    [SerializeReference] public IGetRaw<object> target;
    [SerializeReference] public Get<TValue> value;
    private TypedPathAccessor<TValue> accessor;
    
    public override void Invoke()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessorCache.Get<TValue>(t, propertyPath);
        accessor.Set(t, v);
    }
}