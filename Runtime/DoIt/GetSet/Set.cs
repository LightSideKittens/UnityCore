using System;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class SetBuffer<T> : DoIt
{
    [SerializeReference] public Get<T> data;

    public override void Do()
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
    
    public override void Do()
    {
        var d = data.Data;
        DestroyEvent.AddOnDestroy(d, Remove);
        StringDict<object>.Set(string.Concat(key, typeof(T).GetSimpleFullName()), d);
        StringDict<T>.Set(key, d);
    }
    
    private void Remove()
    {
        StringDict<object>.Remove(string.Concat(key, typeof(T).GetSimpleFullName()));
        StringDict<T>.Remove(key);
    }
}

[Serializable]
public class UnmanagedSetKeyBuffer<T> : DoIt
{
    public string key;
    [SerializeReference] public Get<T> data;

    public override void Do()
    {
        var d = data.Data;
        StringDict<object>.Set(string.Concat(key, typeof(T)), d);
        StringDict<T>.Set(key, d);
    }
}

[Serializable]
public class RemoveKeyBuffer<T> : DoIt
{
    public string key;

    public override void Do()
    {
        StringDict<object>.Remove(string.Concat(key, typeof(T)));
        StringDict<T>.Remove(key);
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

    public override void Do()
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

    public override void Do()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessorCache.GetRef(t, propertyPath);
        accessor.Set(t, v);
    }
}

[Serializable]
public class SetStruct<TValue> : DoIt where TValue : struct
{
    public string propertyPath;
    [SerializeReference] public IGetRaw<object> target;
    [SerializeReference] public Get<TValue> value;
    private TypedPathAccessor<TValue> accessor;
    
    public override void Do()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessorCache.Get<TValue>(t, propertyPath);
        accessor.Set(t, v);
    }
}