using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public static class MethodSerializer
{
    public const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                      BindingFlags.Static | BindingFlags.FlattenHierarchy;
    public static ValueDropdownList<string> GetMethodsList(Type targetType, params Type[] args)
    {
        var list = new ValueDropdownList<string>();
        list.Add("Null", string.Empty);
            
        foreach (var method in GetMethods(targetType, args))
        {
            list.Add($"{method.GetNiceName()}", SerializeMethodInfo(method));
        }

        return list;
    }
    
    public static string SerializeMethodInfo(MethodInfo methodInfo)
    {
        return $"{methodInfo.Name}";
    }

    public static MethodInfo DeserializeMethodInfo(Type targetType, string serializedMethodInfo, params Type[] args)
    {
        return targetType.GetMethod(serializedMethodInfo, Flags, null, args, null);
    }
    
    public static IEnumerable<MethodInfo> GetMethods(Type targetType, params Type[] args)
    {
        var methods = targetType.GetMethods(Flags);
        return methods
            .Where(m =>
            {
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
}