using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Object = UnityEngine.Object;

public static class SerializedMethodFromObjectUtils
{
    public const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                      BindingFlags.Static | BindingFlags.FlattenHierarchy;
    public static ValueDropdownList<string> GetMethodsList(Object obj, Type returnType, params Type[] args)
    {
        if (obj == null) return null;
                
        var list = new ValueDropdownList<string>();
        list.Add("Null", string.Empty);
            
        foreach (var method in GetMethods(obj, returnType, args))
        {
            list.Add($"{method.GetNiceName()}", SerializeMethodInfo(method));
        }

        return list;
    }
    
    public static string SerializeMethodInfo(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}";
    }

    public static MethodInfo DeserializeMethodInfo(Object obj, string serializedMethodInfo, params Type[] args)
    {
        if (string.IsNullOrEmpty(serializedMethodInfo)) return null;
        return obj.GetType().GetMethod(serializedMethodInfo, Flags, null, args, null);
    }
    
    public static IEnumerable<MethodInfo> GetMethods(Object obj, Type returnType, params Type[] args)
    {
        var type = obj.GetType();
        var methods = type.GetMethods(Flags);
        return methods
            .Where(m =>
            {
                if (m.ReturnType != returnType)
                {
                    return false;
                }
                
                var parameters = m.GetParameters();

                if (parameters.Length == args.Length)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] != parameters[i].ParameterType)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                return true;
            });
    }
    
    public static IEnumerable<(Object obj, IEnumerable<MethodInfo> methods)> GetMethods(IEnumerable<Object> objs, Type returnType, params Type[] args)
    {
        foreach (var obj in objs)
        {
            yield return (obj, GetMethods(obj, returnType, args));
        }
    }
}