using System;
using System.Reflection;

namespace UnityToolbarExtender
{
    public class LSGUIView
    {
        private static Type type;
        private static MethodInfo current;
        internal static object Current => current.Invoke(null, null);
        
        static LSGUIView()
        {
            type = Type.GetType("UnityEditor.GUIView,UnityEditor");
            current = type.GetProperty("current", BindingFlags.Static | BindingFlags.Public).GetGetMethod();
        }
    }
}