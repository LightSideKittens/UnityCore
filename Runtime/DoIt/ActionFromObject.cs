using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class ActionFromObject<T> : DoIt
{
    public Object obj;
    [SerializeReference] public T value;
    [ValueDropdown("Methods")] 
    public string method = string.Empty;
    private MethodInfo targetMethod;
    private ValueDropdownList<string> Methods => SerializedMethodFromObjectUtils.GetMethodsList(obj, typeof(void), typeof(T));

    public override void Do()
    {
        targetMethod ??= SerializedMethodFromObjectUtils.DeserializeMethodInfo(obj, method, typeof(T));
        targetMethod.Invoke(obj, new object[]{value});
    }
}