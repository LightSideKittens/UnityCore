//-----------------------------------------------------------------------
// <copyright file="AssemblyUtilities.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities
{
#pragma warning disable

	using Sirenix.Utilities.Editor;
	using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    [Flags]
    public enum AssemblyCategory
    {
        None = 0,
        Scripts = 1 << 0,                   // Compiled from source files in the project or a package by Unity; it's in the script assemblies folder.
        ImportedAssemblies = 1 << 1,        // Pre-compiled assembly imported into the project or a package by the user; it has import settings and a .meta file.
        UnityEngine = 1 << 2,               // Assembly that is part of the Unity engine; this does not include Unity packages, but only core engine assemblies.
        DotNetRuntime = 1 << 3,             // Assembly that is part of the .NET runtime itself.
        DynamicAssemblies = 1 << 4,         // Assembly that was emitted dynamically at runtime.
        Unknown = 1 << 5,                   // We just don't know what this is

        ProjectSpecific = Scripts | ImportedAssemblies,
        All = Scripts | ImportedAssemblies | UnityEngine | DotNetRuntime | DynamicAssemblies | Unknown,
    }

    /// <summary>
    /// A utility class for finding types in various asssembly.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class AssemblyUtilities
    {
		internal const string AssemblyTypeFlagsObsoleteMessage = "AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use " + nameof(AssemblyUtilities) + "." + nameof(GetAssemblyCategory) + "(assembly) instead.";

		//private static string[] userAssemblyPrefixes = new string[]
		//{
		//    "Assembly-CSharp",
		//    "Assembly-UnityScript",
		//    "Assembly-Boo",
		//    "Assembly-CSharp-Editor",
		//    "Assembly-UnityScript-Editor",
		//    "Assembly-Boo-Editor",
		//};

		private static string[] pluginAssemblyPrefixes = new string[]
        {
            "assembly-csharp-firstpass",
            "assembly-csharp-editor-firstpass",
            "assembly-unityscript-firstpass",
            "assembly-unityscript-editor-firstpass",
            "assembly-boo-firstpass",
            "assembly-boo-editor-firstpass",
        };

        private static readonly Dictionary<Assembly, bool> IsDynamicCache = new Dictionary<Assembly, bool>(ReferenceEqualityComparer<Assembly>.Default);
        private static readonly object IS_DYNAMIC_CACHE_LOCK = new object();
        private static readonly object ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK = new object();
        private static readonly object ASSEMBLY_CATEGORY_LOOKUP_LOCK = new object();

		private static Assembly unityEngineAssembly = typeof(UnityEngine.Object).Assembly;
        private static Assembly unityEditorAssembly = typeof(UnityEditor.Editor).Assembly;
        private static DirectoryInfo projectAssetsFolderDirectory;
		private static DirectoryInfo scriptAssembliesDirectory;
        private static DirectoryInfo mscorlibDirectory;
        private static DirectoryInfo unityEngineDirectory;

	    [Obsolete("", Consts.IsSirenixInternal)]
		private static Dictionary<Assembly, AssemblyTypeFlags> assemblyTypeFlagLookup = new Dictionary<Assembly, AssemblyTypeFlags>(100);
		private static Dictionary<Assembly, AssemblyCategory> assemblyCategoryLookup = new Dictionary<Assembly, AssemblyCategory>(100);

		static AssemblyUtilities()
        {
            var currentDir = Environment.CurrentDirectory.Replace("\\", "//").Replace("//", "/").TrimEnd('/');

			var dataPath = currentDir + "/Assets";
            var scriptAssembliesPath = currentDir + "/Library/ScriptAssemblies";

			projectAssetsFolderDirectory = new DirectoryInfo(dataPath);
            scriptAssembliesDirectory = new DirectoryInfo(scriptAssembliesPath);
            mscorlibDirectory = new DirectoryInfo(typeof(string).Assembly.GetAssemblyDirectory());
            unityEngineDirectory = new DirectoryInfo(typeof(UnityEngine.Object).Assembly.GetAssemblyDirectory());

            if (unityEngineDirectory.Parent.Name == "Managed")
            {
                unityEngineDirectory = unityEngineDirectory.Parent;
            }
		}

		public static AssemblyCategory GetAssemblyCategory(Assembly assembly)
		{
			if (assembly == null) throw new NullReferenceException("assembly");

			lock (ASSEMBLY_CATEGORY_LOOKUP_LOCK)
			{
				AssemblyCategory result;

				if (assemblyCategoryLookup.TryGetValue(assembly, out result) == false)
				{
					result = GetAssemblyCategoryPrivate(assembly);
					assemblyCategoryLookup[assembly] = result;
				}

				return result;
			}
		}

		[Obsolete(AssemblyUtilities.AssemblyTypeFlagsObsoleteMessage, Consts.IsSirenixInternal)]
		internal static AssemblyCategory LossyBadConvertAssemblyTypeFlagsToCategories(AssemblyTypeFlags flags)
        {
            AssemblyCategory result = default;

            if (flags.HasFlag(AssemblyTypeFlags.UserTypes))
            {
                result |= AssemblyCategory.Scripts;
            }

            if (flags.HasFlag(AssemblyTypeFlags.PluginTypes))
            {
                result |= AssemblyCategory.ProjectSpecific;
			}

			if (flags.HasFlag(AssemblyTypeFlags.UnityTypes))
            {
                result |= AssemblyCategory.UnityEngine;
            }

            if (flags.HasFlag(AssemblyTypeFlags.UserEditorTypes))
            {
                result |= AssemblyCategory.Scripts;
            }

            if (flags.HasFlag(AssemblyTypeFlags.PluginEditorTypes))
            {
                result |= AssemblyCategory.ProjectSpecific;
			}

            if (flags.HasFlag(AssemblyTypeFlags.UnityEditorTypes))
            {
                result |= AssemblyCategory.UnityEngine;
			}

			if (flags.HasFlag(AssemblyTypeFlags.OtherTypes))
            {
                result |= AssemblyCategory.DynamicAssemblies | AssemblyCategory.Unknown | AssemblyCategory.DotNetRuntime;
            }

            return result;
        }

		private static AssemblyCategory GetAssemblyCategoryPrivate(Assembly assembly)
        {
            if (assembly.IsDynamic()) return AssemblyCategory.DynamicAssemblies;

			string path = assembly.GetAssemblyDirectory();

			if (path != null && Directory.Exists(path))
			{
				var pathDir = new DirectoryInfo(path);

                if (pathDir.FullName == scriptAssembliesDirectory.FullName)
                {
                    return AssemblyCategory.Scripts;
                }

                if (File.Exists(assembly.Location + ".meta"))
                {
					return AssemblyCategory.ImportedAssemblies;
				}

                if (unityEngineDirectory.HasSubDirectory(pathDir))
                {
                    return AssemblyCategory.UnityEngine;
                }

                if (mscorlibDirectory.HasSubDirectory(pathDir))
                {
					return AssemblyCategory.DotNetRuntime;
				}
			}

            var name = assembly.GetName().Name;

			if (name.StartsWith("UnityEngine.") || name.StartsWith("UnityEditor."))
            {
                return AssemblyCategory.UnityEngine;
            }

            return AssemblyCategory.Unknown;
		}

		[Obsolete("Reload is no longer supported.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void Reload() { }

        /// <summary>
        /// Gets an <see cref="ImmutableList"/> of all assemblies in the current <see cref="System.AppDomain"/>.
        /// </summary>
        /// <returns>An <see cref="ImmutableList"/> of all assemblies in the current <see cref="AppDomain"/>.</returns>
        public static ImmutableList<Assembly> GetAllAssemblies()
        {
            return new ImmutableList<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
        }

        /// <summary>
        /// Gets the <see cref="AssemblyTypeFlags"/> for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The <see cref="AssemblyTypeFlags"/> for a given assembly.</returns>
        /// <exception cref="System.NullReferenceException"><paramref name="assembly"/> is null.</exception>
        [Obsolete(AssemblyTypeFlagsObsoleteMessage, Consts.IsSirenixInternal)]
        public static AssemblyTypeFlags GetAssemblyTypeFlag(this Assembly assembly)
        {
            if (assembly == null) throw new NullReferenceException("assembly");

            lock (ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK)
            {
                AssemblyTypeFlags result;

                if (assemblyTypeFlagLookup.TryGetValue(assembly, out result) == false)
                {
                    result = GetAssemblyTypeFlagNoLookup(assembly);

                    assemblyTypeFlagLookup[assembly] = result;
                }

                return result;
            }
        }
        
        [Obsolete("", Consts.IsSirenixInternal)]
        private static AssemblyTypeFlags GetAssemblyTypeFlagNoLookup(Assembly assembly)
		{
            string name = assembly.FullName.ToLower(CultureInfo.InvariantCulture);
            var category = GetAssemblyCategory(assembly);

			switch (category)
            {
                case AssemblyCategory.Unknown:
                    return AssemblyTypeFlags.OtherTypes;
                case AssemblyCategory.Scripts:
                    var isEditor = name.Contains("-editor");

					if (name.StartsWithAnyOf(pluginAssemblyPrefixes))
					{
						return isEditor ? AssemblyTypeFlags.PluginEditorTypes : AssemblyTypeFlags.PluginTypes;
					}
                    else
                    {
						return isEditor ? AssemblyTypeFlags.UserEditorTypes : AssemblyTypeFlags.UserTypes;
					}
                case AssemblyCategory.ImportedAssemblies:
                    return AssemblyTypeFlags.PluginTypes;
                case AssemblyCategory.UnityEngine:
					if (assembly.IsDependentOn(unityEditorAssembly))
                    {
						return AssemblyTypeFlags.UnityEditorTypes;
					}
					return AssemblyTypeFlags.UnityTypes;
                case AssemblyCategory.DotNetRuntime:
                case AssemblyCategory.DynamicAssemblies:
                default:
                    return AssemblyTypeFlags.OtherTypes;
            }

			//bool isEditor = isUser ? name.Contains("-editor") : assembly.IsDependentOn(unityEditorAssembly);


//			AssemblyTypeFlags result;
//            string path = assembly.GetAssemblyDirectory();
//            string name = assembly.FullName.ToLower(CultureInfo.InvariantCulture);

//            bool isInScriptAssemblies = false;
//            bool isInProject = false;

//            if (path != null && Directory.Exists(path))
//            {
//                var pathDir = new DirectoryInfo(path);

//                isInScriptAssemblies = pathDir.FullName == scriptAssembliesDirectory.FullName;
//                isInProject = projectAssetsFolderDirectory.HasSubDirectory(pathDir);
//            }

//            bool isUserScriptAssembly = name.StartsWithAnyOf(userAssemblyPrefixes, StringComparison.InvariantCultureIgnoreCase);
//            bool isPluginScriptAssembly = name.StartsWithAnyOf(pluginAssemblyPrefixes, StringComparison.InvariantCultureIgnoreCase);
//            bool isGame = assembly.IsDependentOn(unityEngineAssembly);
//            bool isPlugin = isPluginScriptAssembly || isInProject || (!isUserScriptAssembly && isInScriptAssemblies);
//            //bool isUnity = name.StartsWith("Unity.") || name.StartsWith("UnityEngine") || name.StartsWith("UnityEditor");


            


//            // HACK: Odin and other assemblies, but easpecially Odin, needs to be registered as a plugin if it's installed as a package from the Unity PackageManager.
//            // However there doesn't seemt to be any good way of figuring that out.

//            // TODO: Find a good way of figuring if it's a plugin when located installed as a package.
//            // Maybe it would be easier to figure out whether something was a Unity type, and then have plugin as fallback, instead of ther other way around, which
//            // is how it works now.
//            if (!isPlugin && name.StartsWith("sirenix."))
//            {
//                isPlugin = true;
//            }

//            bool isUser = !isPlugin && isUserScriptAssembly;

//#if UNITY_EDITOR
//            bool isEditor = isUser ? name.Contains("-editor") : assembly.IsDependentOn(unityEditorAssembly);

//            if (isUser)
//            {
//                isEditor = name.Contains("-editor");
//            }
//            else
//            {
//                isEditor = assembly.IsDependentOn(unityEditorAssembly);
//            }
//#else
//                bool isEditor = false;
//#endif
//            if (!isGame && !isEditor && !isPlugin && !isUser)
//            {
//                result = AssemblyTypeFlags.OtherTypes;
//            }
//            else if (isEditor && !isPlugin && !isUser)
//            {
//                result = AssemblyTypeFlags.UnityEditorTypes;
//            }
//            else if (isGame && !isEditor && !isPlugin && !isUser)
//            {
//                result = AssemblyTypeFlags.UnityTypes;
//            }
//            else if (isEditor && isPlugin && !isUser)
//            {
//                result = AssemblyTypeFlags.PluginEditorTypes;
//            }
//            else if (!isEditor && isPlugin && !isUser)
//            {
//                result = AssemblyTypeFlags.PluginTypes;
//            }
//            else if (isEditor && isUser)
//            {
//                result = AssemblyTypeFlags.UserEditorTypes;
//            }
//            else if (!isEditor && isUser)
//            {
//                result = AssemblyTypeFlags.UserTypes;
//            }
//            else
//            {
//                result = AssemblyTypeFlags.OtherTypes;
//            }

//            return result;
        }

        /// <summary>
        /// Determines whether an assembly is depended on another assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="otherAssembly">The other assembly.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="assembly"/> has a reference in <paramref name="otherAssembly"/> or <paramref name="assembly"/> is the same as <paramref name="otherAssembly"/>.
        /// </returns>
        /// <exception cref="System.NullReferenceException"><paramref name="assembly"/> is null.</exception>
        /// <exception cref="System.NullReferenceException"><paramref name="otherAssembly"/> is null.</exception>
        public static bool IsDependentOn(this Assembly assembly, Assembly otherAssembly)
        {
            if (assembly == null) throw new NullReferenceException("assembly");
            if (otherAssembly == null) throw new NullReferenceException("otherAssembly");

            if (assembly == otherAssembly)
            {
                return true;
            }

            var otherName = otherAssembly.GetName().ToString();
            var referencedAsssemblies = assembly.GetReferencedAssemblies();

            for (int i = 0; i < referencedAsssemblies.Length; i++)
            {
                if (otherName == referencedAsssemblies[i].ToString())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the assembly module is a of type <see cref="System.Reflection.Emit.ModuleBuilder"/>.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///   <c>true</c> if the specified assembly of type <see cref="System.Reflection.Emit.ModuleBuilder"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">assembly</exception>
        public static bool IsDynamic(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            bool result;

            lock (IS_DYNAMIC_CACHE_LOCK)
            {
                if (!IsDynamicCache.TryGetValue(assembly, out result))
                {
                    try
                    {
                        // Will cover both System.Reflection.Emit.AssemblyBuilder and System.Reflection.Emit.InternalAssemblyBuilder
                        result = assembly.GetType().FullName.EndsWith("AssemblyBuilder") || assembly.Location == null || assembly.Location == "";
                    }
                    catch
                    {
                        result = true;
                    }

                    IsDynamicCache.Add(assembly, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the full file path to a given assembly's containing directory.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The full file path to a given assembly's containing directory, or <c>Null</c> if no file path was found.</returns>
        /// <exception cref="System.NullReferenceException"><paramref name="assembly"/> is Null.</exception>
        public static string GetAssemblyDirectory(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            var path = assembly.GetAssemblyFilePath();
            if (path == null)
            {
                return null;
            }

            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the full directory path to a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The full directory path in which a given assembly is located, or <c>Null</c> if no file path was found.</returns>
        public static string GetAssemblyFilePath(this Assembly assembly)
        {
            if (assembly == null) return null;
            if (assembly.IsDynamic()) return null;
            if (assembly.CodeBase == null) return null;

            var filePrefix = @"file:///";
            var path = assembly.CodeBase;

            if (path.StartsWith(filePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                path = path.Substring(filePrefix.Length);
                path = path.Replace('\\', '/');

                if (File.Exists(path))
                {
                    return path;
                }

                if (!Path.IsPathRooted(path))
                {
                    if (File.Exists("/" + path))
                    {
                        path = "/" + path;
                    }
                    else
                    {
                        path = Path.GetFullPath(path);
                    }
                }

                if (!File.Exists(path))
                {
                    try
                    {
                        path = assembly.Location;
                    }
                    catch
                    {
                        return null;
                    }
                }
                else
                {
                    return path;
                }

                if (File.Exists(path))
                {
                    return path;
                }
            }

            if (File.Exists(assembly.Location))
            {
                return assembly.Location;
            }

            return null;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="fullName">The full name of the type, with or without any assembly information.</param>
        public static Type GetTypeByCachedFullName(string name)
        {
            return Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType(name);
        }

		/// <summary>
		/// Get types from the current AppDomain with a specified <see cref="AssemblyCategory"/> filter.
		/// </summary>
		/// <param name="assemblyFlags">The <see cref="AssemblyCategory"/> filters.</param>
		/// <returns>Types from the current AppDomain with the specified <see cref="AssemblyCategory"/> filters.</returns>
		public static IEnumerable<Type> GetTypes(AssemblyCategory assemblyFlags)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var flag = GetAssemblyCategory(assembly);

				if ((flag & assemblyFlags) != 0)
				{
					foreach (var type in assembly.SafeGetTypes())
					{
						yield return type;
					}
				}
			}
		}

		/// <summary>
		/// Get types from the current AppDomain with a specified <see cref="AssemblyTypeFlags"/> filter.
		/// </summary>
		/// <param name="assemblyTypeFlags">The <see cref="AssemblyTypeFlags"/> filters.</param>
		/// <returns>Types from the current AppDomain with the specified <see cref="AssemblyTypeFlags"/> filters.</returns>
		[Obsolete("AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use " + nameof(AssemblyUtilities) + "." + nameof(GetTypes) + "(" + nameof(AssemblyCategory) + " assemblyFlags) instead.", Consts.IsSirenixInternal)]
        public static IEnumerable<Type> GetTypes(AssemblyTypeFlags assemblyTypeFlags)
		{
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var flag = GetAssemblyTypeFlag(assembly);

                if ((flag & assemblyTypeFlags) != 0)
                {
                    foreach (var type in assembly.SafeGetTypes())
                    {
                        yield return type;
                    }
                }
            }
        }
        
        private static bool StartsWithAnyOf(this string str, string[] values)
        {
			for (int i = 0; i < values.Length; i++)
            {
				if (str.StartsWith(values[i], StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif