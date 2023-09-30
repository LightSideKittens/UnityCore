using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class WindowsExtender
{
    public static Harmony Harmony { get; }
    public static MethodInfo OnPreGUIMethod { get; }
    public static MethodInfo OnPostGUIMethod { get; }
    public static Dictionary<Type, BaseWindowExtender> Extenders { get; } = new();
    
    static WindowsExtender()
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
        OnPreGUIMethod = typeof(WindowsExtender).GetMethod(nameof(OnPreGUI), flags);
        OnPostGUIMethod = typeof(WindowsExtender).GetMethod(nameof(OnPostGUI), flags);
        
        var assembly = Assembly.Load("LGCore.Editor.WindowExtensions");
        var types = assembly.GetTypes();
        Harmony = new Harmony("com.yourname.yourproject");
        var baseType = typeof(BaseWindowExtender);
        
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            
            if (type.IsSubclassOf(baseType))
            { 
                Activator.CreateInstance(type);
            }
        }
    }

    public static void OnPreGUI<T>() where T : BaseWindowExtender
    {
        var extender = Extenders[typeof(T)];
        extender.SetWindowIfNull();
        extender.OnPreGUI();
    }

    public static void OnPostGUI<T>() where T : BaseWindowExtender => Extenders[typeof(T)].OnPostGUI();
}