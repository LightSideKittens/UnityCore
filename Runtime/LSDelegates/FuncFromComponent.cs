using System;
using System.Reflection;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Object = UnityEngine.Object;

[Serializable]
public class TweenGetter : FuncFromComponent<Tween> { }

public class FuncFromComponent<T> : LSFunc<T>
{
    public Object obj;
    [ValueDropdown("Methods")] public string method = string.Empty;
    private MethodInfo targetMethod;
        
    private ValueDropdownList<string> Methods
    {
        get
        {
            if (obj == null) return null;
                
            var list = new ValueDropdownList<string>();
            list.Add("Null", string.Empty);
            
            foreach (var method in obj.GetMethods(typeof(T)))
            {
                list.Add($"{method.GetNiceName()}", SerializeMethodInfo(method));
            }


            return list;
        }
    }
        
    public void Init()
    { 
        targetMethod = DeserializeMethodInfo(method);
    }
        
    public override T Invoke()
    {
        return targetMethod != null ? (T)targetMethod.Invoke(obj, null) : default;
    }

    private string SerializeMethodInfo(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}";
    }

    private MethodInfo DeserializeMethodInfo(string serializedMethodInfo)
    {
        if (serializedMethodInfo is null) return null;
        return obj.GetType().GetMethod(serializedMethodInfo,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null,
            Type.EmptyTypes, null);
    }
}