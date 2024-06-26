using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

public static class ObjectExtensions
{
    public static IEnumerable<MethodInfo> GetMethods(this Object obj, Type returnType)
    {
        var type = obj.GetType();
        return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.ReturnType == returnType && m.GetParameters().Length == 0);
    }
    
    public static IEnumerable<(Object obj, IEnumerable<MethodInfo> methods)> GetMethods(this IEnumerable<Object> objs, Type returnType)
    {
        foreach (var obj in objs)
        {
            yield return (obj, obj.GetMethods(returnType));
        }
    }
}