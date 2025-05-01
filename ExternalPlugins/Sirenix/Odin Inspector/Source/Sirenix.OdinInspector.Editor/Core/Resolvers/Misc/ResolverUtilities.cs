//-----------------------------------------------------------------------
// <copyright file="ResolverUtilities.cs" company="Sirenix ApS">
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
    using System.Reflection;

    public static class ResolverUtilities
    {
        public static List<Assembly> GetResolverAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Assembly> results = new List<Assembly>(assemblies.Length);

            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];

                if (assembly.SafeIsDefined(typeof(ContainsOdinResolversAttribute), true) || (AssemblyUtilities.GetAssemblyCategory(assembly) & AssemblyCategory.ProjectSpecific) != 0)
                {
                    results.Add(assembly);
                }
            }

            return results;
        }

        public static double GetResolverPriority(Type resolverType)
        {
            var attr = resolverType.GetAttribute<ResolverPriorityAttribute>(inherit: true);
            if (attr != null) return attr.Priority;

            if (resolverType.Assembly == typeof(OdinEditor).Assembly)
            {
                return -0.1;
            }

            return 0;
        }
    }
}
#endif