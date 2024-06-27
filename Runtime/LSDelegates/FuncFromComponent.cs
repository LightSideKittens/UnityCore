using System;
using System.Reflection;
using DG.Tweening;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

[Serializable]
public class TweenGetter : FuncFromObject<Tween> { }

public class FuncFromObject<T> : LSFunc<T>
{
    public Object obj;
    [ValueDropdown("Methods")] 
    public string method = string.Empty;
    private MethodInfo targetMethod;
    private ValueDropdownList<string> Methods => SerializedMethodFromObjectUtils.GetMethodsList(obj, typeof(T));
        
    public void Init()
    { 
        targetMethod = SerializedMethodFromObjectUtils.DeserializeMethodInfo(obj, method);
    }

    public override T Invoke()
    {
        return targetMethod != null ? (T)targetMethod.Invoke(obj, null) : default;
    }
}