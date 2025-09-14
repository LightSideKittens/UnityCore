using System;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CallMethod : DoIt
{
    [SerializeReference] public IGetRaw target;
    [SerializeReference] public IGetRaw[] arguments;
    [ValueDropdown("Methods")] public string method = string.Empty;
    [SerializeReference] public DoIt handleResult;
    
    private MethodInfo targetMethod;
    private ValueDropdownList<string> Methods => MethodSerializer.GetMethodsList(target.Type, arguments.Select(arg => arg.Type).ToArray());

    public override void Do()
    {
        targetMethod ??= MethodSerializer.DeserializeMethodInfo(target.Type, method, arguments.Select(arg => arg.Type).ToArray());
        var result = targetMethod.Invoke(target.Data, arguments.Select(arg => arg.Data).ToArray());
        DataBuffer.value = result;
        handleResult.Do();
    }
}