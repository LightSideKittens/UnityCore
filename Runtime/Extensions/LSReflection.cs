using System;
using System.Reflection;

namespace LSCore.Extensions
{
    public static class LSReflection
    {
        private static object GetPrivatePropertyOrField(object obj, string name, Type type)
        {
            if (obj != null)
            {
                type = obj.GetType();
            }
            
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                return prop.GetValue(obj);
            }
            
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(obj);
            }

            throw new Exception($"Property or field '{name}' not found on type '{type.FullName}'");
        }

        public static object Eval(object obj, string expression)
        {
            var names = expression.Split('.');
            
            foreach (var name in names)
            {
                obj = GetPrivatePropertyOrField(obj, name, null);
            }
            return obj;
        }
        
        public static object Eval(Type type, string expression)
        {
            var names = expression.Split('.');
            object obj = null;
            
            foreach (var name in names)
            {
                obj = GetPrivatePropertyOrField(obj, name, type);
            }
            return obj;
        }
    }
}