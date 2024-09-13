using System;
using UnityEngine;
#pragma warning disable CS0162
namespace Core.Server
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
#if DEBUG
            try { action?.Invoke(); }
            catch (Exception e) { Debug.LogException(e); }
            return;
#endif
            action?.Invoke();
        }
        
        public static void SafeInvoke<T>(this Action<T> action, T data)
        {
#if DEBUG
            try { action?.Invoke(data); }
            catch (Exception e) { Debug.LogException(e); }
            return;
#endif
            action?.Invoke(data);
        }
        
        public static void SafeInvoke<T, T1>(this Action<T, T1> action, T data, T1 data1)
        {
#if DEBUG
            try { action?.Invoke(data, data1); }
            catch (Exception e) { Debug.LogException(e); }
            return;
#endif
            action?.Invoke(data, data1);
        }
    }
}