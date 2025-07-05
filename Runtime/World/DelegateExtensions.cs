using System;
using UnityEngine;
#pragma warning disable CS0162
namespace LSCore.Extensions
{
    public static class DelegateExtensions
    {
        public static void InverseInvoke(this Action action)
        {
            var invocationList = action.GetInvocationList();
            for (int i = invocationList.Length - 1; i >= 0; i--)
            {
                ((Action)invocationList[i])();
            }
        }
        
        public static void SafeInvoke(this Action action)
        {
            if (action != null)
            {
                foreach (var d in action.GetInvocationList())
                {
                    try
                    {
                        ((Action)d)();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        public static void SafeInvoke<T>(this Action<T> action, T data)
        {
            if (action != null)
            {
                foreach (var d in action.GetInvocationList())
                {
                    try
                    {
                        ((Action<T>)d)(data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
        
        public static void SafeInvoke<T, T1>(this Action<T, T1> action, T data, T1 data1)
        {
            if (action != null)
            {
                foreach (var d in action.GetInvocationList())
                {
                    try
                    {
                        ((Action<T, T1>)d)(data, data1);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
    }
}