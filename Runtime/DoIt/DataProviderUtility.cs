#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DataProviderUtility
{
    private static IReadOnlyList<Type> cachedArgs;

    static DataProviderUtility()
    {
        EditorApplication.delayCall += CacheAllProviderGenericArgs;
    }

    private static void CacheAllProviderGenericArgs()
    {
        EditorApplication.delayCall -= CacheAllProviderGenericArgs;
        
        var args = TypeCache
            .GetFieldsWithAttribute<SerializeReference>()
            .Select(f => f.FieldType)
            .Where(ft =>
                ft.IsGenericType &&
                ft.GetGenericTypeDefinition() == typeof(Get<>) &&
                !ft.ContainsGenericParameters)
            .SelectMany(ft =>
            {
                var t = ft.GetGenericArguments()[0];
                
                return new[]
                {
                    typeof(SetBuffer<>).MakeGenericType(t),
                    typeof(SetKeyBuffer<>).MakeGenericType(t)
                };
            })
            .Distinct()
            .ToList();

        cachedArgs = args;
    }
    
    public static IReadOnlyList<Type> GetAllProviderGenericArgs()
    {
        if (cachedArgs == null)
            CacheAllProviderGenericArgs();
        return cachedArgs;
    }
}
#endif