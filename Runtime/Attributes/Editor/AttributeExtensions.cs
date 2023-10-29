using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Attributes
{
    public static class AttributeExtensions
    {
        public static FieldInfo GetFieldViaPath(this Type type, string path, BindingFlags flags)
        {
            Type parentType = type;
            FieldInfo fi = type.GetField(path, flags);
            var perDot = path.Split('.');
            
            foreach (var fieldName in perDot)
            {
                fi = parentType.GetField(fieldName, flags);
                if (fi != null)
                {
                    parentType = fi.FieldType;
                    
                    if (parentType.IsArray)
                    {
                        parentType = parentType.GetElementType();
                    }
                    else if(typeof(IList).IsAssignableFrom(parentType))
                    {
                        var isList = false;

                        while (isList == false)
                        {
                            while (parentType.IsGenericType == false)
                            {
                                parentType = parentType.BaseType;
                            }
                            
                            isList = parentType.GetGenericTypeDefinition() == typeof(List<>);
                        }

                        parentType = parentType.GenericTypeArguments[0];
                    }
                }
                else
                {
                    return null;
                }
            }
            
            return fi != null ? fi : null;
        }
    }
}