using System;
using System.Reflection;
using DG.Tweening;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

[Serializable]
public class TweenGetter : FuncFromObject<Tween> { }

public class FuncFromObject<T> : LSAction
{
    public Object obj;
    public T value;
    [ValueDropdown("Methods")] 
    public string method = string.Empty;
    private MethodInfo targetMethod;
    private ValueDropdownList<string> Methods => SerializedMethodFromObjectUtils.GetMethodsList(obj, typeof(T));
    
    public void Init()
    { 
        targetMethod = SerializedMethodFromObjectUtils.DeserializeMethodInfo(obj, method);
    }

    public override void Invoke()
    {
        value = targetMethod != null ? (T)targetMethod.Invoke(obj, null) : default;
    }
}