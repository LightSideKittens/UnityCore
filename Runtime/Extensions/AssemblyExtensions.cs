using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

public static class AssemblyExtensions
{
    public static List<Assembly> GetRelevantAssemblies(this Assembly baseAssembly)
    {
        Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Assembly> relevantAssemblies = new List<Assembly>();
        foreach (var assembly in allAssemblies)
        {
            if (assembly.GetReferencedAssemblies().Any(a => a.FullName == baseAssembly.FullName))
            {
                relevantAssemblies.Add(assembly);
            }
        }
        relevantAssemblies.Add(baseAssembly);
        return relevantAssemblies;
    }
    
    public static HashSet<Type> GetVisibleTypes(this Assembly assembly)
    {
        HashSet<Type> visibleTypes = new HashSet<Type>();
        visibleTypes.AddRange(assembly.GetTypes());
        AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
        foreach (var referencedAssemblyName in referencedAssemblies)
        {
            Assembly referencedAssembly = Assembly.Load(referencedAssemblyName);
            visibleTypes.AddRange(referencedAssembly.GetExportedTypes());
        }

        return visibleTypes;
    }
}
