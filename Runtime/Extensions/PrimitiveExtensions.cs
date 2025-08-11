using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.Extensions
{
    public static class PrimitiveExtensions
    {
        public static int ToPosNeg(this bool b) => b ? 1 : -1;
        public static int ToInt(this bool b) => b ? 1 : 0;
        public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
        
        public static bool TryCast<T>(this object obj, out T value)
        {
            if (obj is T casted)
            {
                value = casted;
                return true;
            }

            var cast = obj.GetType().GetCastMethodDelegate(typeof(T));
            if (cast != null)
            {
                value = (T)cast(obj);
                return true;
            }

            object raw;
            GameObject go = obj as GameObject;
            if (go is null)
            {
                if (obj is Component comp)
                {
                    go = comp.gameObject;
                }
            }

            if (typeof(GameObject) == typeof(T))
            {
                raw = go;
            }
            else
            {
                raw = go.GetComponent(typeof(T));

                if (raw is null)
                {
                    var comps = go.GetComponents(typeof(Component));
                    for (var i = 0; i < comps.Length; i++)
                    {
                        var comp = comps[i];
                        cast = comp.GetType().GetCastMethodDelegate(typeof(T));
                        if (cast != null)
                        {
                            value = (T)cast(obj);
                            return true;
                        }
                    }
                }
            }
            
            value = (T)raw;
            return raw != null;
        }

        private static List<Component> compsBuffer = new();
        
        public static T Cast<T>(this object obj)
        {
            if (obj is T casted)
            {
                return casted;
            }

            var cast = obj.GetType().GetCastMethodDelegate(typeof(T));
            if (cast != null)
            {
                return (T)cast(obj);
            }

            object raw;
            GameObject go = obj as GameObject;
            if (go is null)
            {
                if (obj is Component comp)
                {
                    go = comp.gameObject;
                }
            }

            if (typeof(GameObject) == typeof(T))
            {
                raw = go;
            }
            else
            {
                raw = go.GetComponent(typeof(T));

                if (raw is null)
                {
                    go.GetComponents(typeof(Component), compsBuffer);
                    for (var i = 0; i < compsBuffer.Count; i++)
                    {
                        var comp = compsBuffer[i];
                        if (comp is T castedComp)
                        {
                            return castedComp;
                        }

                        cast = comp.GetType().GetCastMethodDelegate(typeof(T));
                        if (cast != null)
                        {
                            return (T)cast(comp);
                        }
                    }
                }
            }
            
            return (T)raw;
        }
        
        public static object Cast(this object obj, Type type)
        {
            var objType = obj.GetType();
            
            if (type.IsAssignableFrom(objType))
            {
                return obj;
            }

            var cast = objType.GetCastMethodDelegate(type);
            if (cast != null)
            {
                return cast(obj);
            }

            object raw;
            GameObject go = obj as GameObject;
            if (go is null)
            {
                if (obj is Component comp)
                {
                    go = comp.gameObject;
                }
            }

            if (typeof(GameObject) == type)
            {
                raw = go;
            }
            else
            {
                raw = go.GetComponent(type);

                if (raw is null)
                {
                    go.GetComponents(typeof(Component), compsBuffer);
                    for (var i = 0; i < compsBuffer.Count; i++)
                    {
                        var comp = compsBuffer[i];
                        var compType = comp.GetType();
                        if (type.IsAssignableFrom(compType))
                        {
                            return comp;
                        }

                        cast = comp.GetType().GetCastMethodDelegate(type);
                        if (cast != null)
                        {
                            return cast(comp);
                        }
                    }
                }
            }
            
            return raw;
        }
    }
}