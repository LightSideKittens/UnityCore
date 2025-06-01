using System;
using Sirenix.OdinInspector;
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

public class DestroyEvent : MonoBehaviour, DestroyEvent.I
{
    public interface I
    {
        event Action Destroyed;
    }
    
    public event Action Destroyed;

    private void OnDestroy()
    {
        Destroyed?.Invoke();
    }
}

[Serializable]
public class SetKeyBuffer<T> : DoIt
{
    [HideInInspector]
    [GetContext]
    public Object root;
    
    public string key;
    [SerializeReference] public Get<T> data;
    
    public override void Invoke()
    {
        var d = data.Data;
        
        switch (root)
        {
            case DestroyEvent.I e:
                e.Destroyed += Remove;
                break;
            case Component component:
            {
                var de = component.gameObject.AddComponent<DestroyEvent>();
                de.Destroyed += Remove;
                root = de;
                break;
            }
            default:
                throw new InvalidOperationException(
                    $"Cannot get {typeof(DestroyEvent.I)}: {(root == null ? "root is null. Open scene or prefab and find SetKeyBuffer in Inspector to auto-apply root" : root.GetType().Name)}"
                );
        }
        
        StringDict<object>.Set(string.Concat(key, typeof(T)), d);
        StringDict<T>.Set(key, d);
    }
    
    private void Remove()
    {
        StringDict<object>.Remove(string.Concat(key, typeof(T)));
        StringDict<T>.Remove(key);
    }
}

[Serializable]
public class UnmanagedSetKeyBuffer<T> : DoIt
{
    public string key;
    [SerializeReference] public Get<T> data;

    public override void Invoke()
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

    public override void Invoke()
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