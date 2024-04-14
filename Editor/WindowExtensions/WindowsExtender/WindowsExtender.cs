using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Sirenix.Utilities;
using UnityEditor;

[InitializeOnLoad]
internal static class WindowsExtender
{
    public static Harmony Harmony { get; }
    public static MethodInfo OnPreGUIMethod { get; }
    public static MethodInfo OnPostGUIMethod { get; }
    public static Dictionary<Type, BaseWindowExtender> Extenders { get; } = new();
    
    static WindowsExtender()
    {
        var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        PlayerSettings.SetApiCompatibilityLevel(currentGroup, ApiCompatibilityLevel.NET_Unity_4_8);
        AssetDatabase.Refresh();
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
        OnPreGUIMethod = typeof(WindowsExtender).GetMethod(nameof(OnPreGUI), flags);
        OnPostGUIMethod = typeof(WindowsExtender).GetMethod(nameof(OnPostGUI), flags);

        var baseType = typeof(BaseWindowExtender);
        var types = AssemblyUtilities.GetTypes(AssemblyTypeFlags.PluginTypes | AssemblyTypeFlags.PluginEditorTypes)
            .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();
        
        Harmony = new Harmony("com.yourname.yourproject");
        
        for (int i = 0; i < types.Count; i++)
        {
            Activator.CreateInstance(types[i]);
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