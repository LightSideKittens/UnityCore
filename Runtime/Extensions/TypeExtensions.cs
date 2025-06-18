﻿using System;

namespace LSCore.Extensions
{
    public static class TypeExtensions
    {
        public static string GetSimpleFullName(this Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
        
        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type baseType)
        {
            while (toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (baseType == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}