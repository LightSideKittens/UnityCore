//-----------------------------------------------------------------------
// <copyright file="ConvertUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class ConvertUtility
    {
        private static readonly List<ICustomConverter> CustomConverters = new List<ICustomConverter>();

        private static readonly DoubleLookupDictionary<Type, Type, object> StrongCastLookup = new DoubleLookupDictionary<Type, Type, object>(FastTypeComparer.Instance, FastTypeComparer.Instance);
        private static readonly DoubleLookupDictionary<Type, Type, Func<object, object>> WeakCastLookup = new DoubleLookupDictionary<Type, Type, Func<object, object>>(FastTypeComparer.Instance, FastTypeComparer.Instance);

        public static bool CanConvert<TFrom, TTo>()
        {
            return CanConvert(typeof(TFrom), typeof(TTo));
        }

        public static bool CanConvert(Type from, Type to)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            if (from == to)
            {
                return true;
            }

            if (to == typeof(object))
            {
                return true;
            }

            if (to == typeof(string))
            {
                return true;
            }

            if (from.IsCastableTo(to))
            {
                return true;
            }

            if (GenericNumberUtility.IsNumber(from) && GenericNumberUtility.IsNumber(to))
            {
                return true;
            }

            if (from == typeof(Sprite) && typeof(Texture).IsAssignableFrom(to)) return true;
            if (to == typeof(Sprite) && typeof(Texture).IsAssignableFrom(from)) return true;
            if (from == typeof(GameObject) && (typeof(Component).IsAssignableFrom(to) || to.IsInterface)) return true;
            if (to == typeof(GameObject) && typeof(Component).IsAssignableFrom(from)) return true;

            for (int i = 0; i < CustomConverters.Count; i++)
            {
                if (CustomConverters[i].CanConvert(from, to))
                {
                    return true;
                }
            }

            return GetCastDelegate(from, to) != null;
        }

        public static bool TryConvert<TFrom, TTo>(TFrom value, out TTo result)
        {
            if (value is TTo)
            {
                result = (TTo)(object)value;
                return true;
            }

            if (typeof(TTo) == typeof(object))
            {
                result = (TTo)(object)value;
                return true;
            }

            if (typeof(TTo) == typeof(string))
            {
                result = value != null ? (TTo)(object)value.ToString() : default(TTo);
                return true;
            }

            if (GenericNumberUtility.IsNumber(typeof(TFrom)) && GenericNumberUtility.IsNumber(typeof(TTo)))
            {
                result = GenericNumberUtility.ConvertNumber<TTo>(value);
                return true;
            }

            if (TryUnityConvert(value, typeof(TTo), out var unityResult))
            {
                result = (TTo)(object)unityResult;
                return true;
            }

            for (int i = 0; i < CustomConverters.Count; i++)
            {
                if (CustomConverters[i].TryConvert(value, typeof(TTo), out var customResult))
                {
                    result = (TTo)customResult;
                    return true;
                }
            }

            var cast = GetCastDelegate<TFrom, TTo>();

            if (cast == null)
            {
                result = default(TTo);
                return false;
            }

            result = cast(value);
            return true;
        }

        public static TTo Convert<TFrom, TTo>(TFrom value)
        {
            if (value is TTo)
            {
                return (TTo)(object)value;
            }

            if (typeof(TTo) == typeof(string))
            {
                return value != null ? (TTo)(object)value.ToString() : default(TTo);
            }

            if (GenericNumberUtility.IsNumber(typeof(TFrom)) && GenericNumberUtility.IsNumber(typeof(TTo)))
            {
                return GenericNumberUtility.ConvertNumber<TTo>(value);
            }

            if (TryUnityConvert(value, typeof(TTo), out var unityResult))
            {
                return (TTo)(object)unityResult;
            }

            for (int i = 0; i < CustomConverters.Count; i++)
            {
                if (CustomConverters[i].TryConvert(value, typeof(TTo), out var customResult))
                {
                    return (TTo)customResult;
                }
            }

            var cast = GetCastDelegate<TFrom, TTo>();

            if (cast == null)
            {
                throw new InvalidCastException();
            }

            return cast(value);
        }

        public static bool TryWeakConvert(object value, Type to, out object result)
        {
            // Fix this lazy mess

            try
            {
                result = WeakConvert(value, to);
                return true;
            }
            catch (InvalidCastException)
            {
                result = null;
                return false;
            }
        }

        public static object WeakConvert(object value, Type to)
        {
            if (value == null)
            {
                if (to.IsValueType)
                {
                    return Activator.CreateInstance(to);
                }

                return null;
            }

            if (to == typeof(object)) return value;

            var typeOfValue = value.GetType();

            if (to.IsAssignableFrom(typeOfValue))
            {
                return value;
            }

            if (to == typeof(string))
            {
                return value.ToString();
            }

            if (GenericNumberUtility.IsNumber(typeOfValue) && GenericNumberUtility.IsNumber(to))
            {
                return GenericNumberUtility.ConvertNumberWeak(value, to);
            }

            if (TryUnityConvert(value, to, out var unityResult))
            {
                return unityResult;
            }

            for (int i = 0; i < CustomConverters.Count; i++)
            {
                if (CustomConverters[i].TryConvert(value, to, out var customResult))
                {
                    return customResult;
                }
            }

            var cast = GetCastDelegate(typeOfValue, to);

            if (cast == null)
            {
                throw new InvalidCastException("Can't convert from " + typeOfValue.Name + " to " + to.Name);
            }

            return cast(value);
        }

        public static T Convert<T>(object value)
        {
            if (value is T)
            {
                return (T)value;
            }

            if (value == null)
            {
                return default(T);
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }

            var typeOfValue = value.GetType();

            if (GenericNumberUtility.IsNumber(typeOfValue) && GenericNumberUtility.IsNumber(typeof(T)))
            {
                return GenericNumberUtility.ConvertNumber<T>(value);
            }

            if (TryUnityConvert(value, typeof(T), out var unityResult))
            {
                return (T)(object)unityResult;
            }

            for (int i = 0; i < CustomConverters.Count; i++)
            {
                if (CustomConverters[i].TryConvert(value, typeof(T), out var customResult))
                {
                    return (T)customResult;
                }
            }

            var cast = GetCastDelegate(typeOfValue, typeof(T));

            if (cast == null)
            {
                throw new InvalidCastException();
            }

            return (T)cast(value);
        }

        public static bool TryConvert<T>(object value, out T result)
        {
            if (value is T)
            {
                result = (T)value;
                return true;
            }

            if (value == null)
            {
                result = default(T);
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)value.ToString();
                return true;
            }

            var typeOfValue = value.GetType();

            if (GenericNumberUtility.IsNumber(typeOfValue) && GenericNumberUtility.IsNumber(typeof(T)))
            {
                result = GenericNumberUtility.ConvertNumber<T>(value);
                return true;
            }

            if (TryUnityConvert(value, typeof(T), out var unityResult))
            {
                result = (T)(object)unityResult;
                return true;
            }

            for (int i = 0; i < CustomConverters.Count; i++)
            {
                if (CustomConverters[i].TryConvert(value, typeof(T), out var customResult))
                {
                    result = (T)customResult;
                    return true;
                }
            }

            var cast = GetCastDelegate(typeOfValue, typeof(T));

            if (cast == null)
            {
                result = default(T);
                return false;
            }

            result = (T)cast(value);
            return true;
        }

        private static bool TryUnityConvert(object fromWeak, Type to, out UnityEngine.Object result)
        {
            if ((!typeof(UnityEngine.Object).IsAssignableFrom(to) && !to.IsInterface) || !(fromWeak is UnityEngine.Object from))
            {
                result = null;
                return false;
            }

            if (to == typeof(GameObject) && from is Component com && com != null)
            {
                result = com.gameObject;
                return true;
            }

            if ((typeof(Component).IsAssignableFrom(to) || to.IsInterface) && from is GameObject go && go != null)
            {
                com = go.GetComponent(to);

                if (com != null)
                {
                    result = com;
                    return true;
                }
            }

            if (to == typeof(Sprite))
            {
                if (from is Texture tex && tex != null && AssetDatabase.Contains(tex))
                {
                    var assetPath = AssetDatabase.GetAssetPath(tex);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                    if (sprite != null)
                    {
                        result = sprite;
                        return true;
                    }
                }
            }

            if (to == typeof(Texture))
            {
                if (from is Sprite sprite && sprite != null && sprite.texture != null)
                {
                    result = sprite.texture;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static Func<object, object> GetCastDelegate(Type from, Type to)
        {
            Func<object, object> castDelegate;
            if (!WeakCastLookup.TryGetInnerValue(from, to, out castDelegate))
            {
                castDelegate = TypeExtensions.GetCastMethodDelegate(from, to);
                WeakCastLookup.AddInner(from, to, castDelegate);
            }
            return castDelegate;
        }

        private static Func<TFrom, TTo> GetCastDelegate<TFrom, TTo>()
        {
            object del;
            Func<TFrom, TTo> castDelegate;
            if (!StrongCastLookup.TryGetInnerValue(typeof(TFrom), typeof(TTo), out del))
            {
                castDelegate = TypeExtensions.GetCastMethodDelegate<TFrom, TTo>();
                StrongCastLookup.AddInner(typeof(TFrom), typeof(TTo), castDelegate);
            }
            else
            {
                castDelegate = (Func<TFrom, TTo>)del;
            }

            return castDelegate;
        }

        public static void AddCustomConverter(ICustomConverter converter)
        {
            _ = converter ?? throw new ArgumentNullException(nameof(converter));
            CustomConverters.Add(converter);
        }

        public interface ICustomConverter
        {
            bool CanConvert(Type from, Type to);
            bool TryConvert(object obj, Type to, out object result);
        }
    }
}
#endif