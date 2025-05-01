//-----------------------------------------------------------------------
// <copyright file="GlobalConfig.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public interface IGlobalConfigEvents
    {
        void OnConfigAutoCreated();
        void OnConfigInstanceFirstAccessed();
    }

    /// <summary>
    /// <para>
    /// A GlobalConfig singleton, automatically created and saved as a ScriptableObject in the project at the specified path.
    /// This only happens if the UnityEditor is present. If it's not, a non-persistent ScriptableObject is created at run-time.
    /// </para>
    /// <para>
    /// Remember to locate the path within a resources folder if you want the config file to be loaded at runtime without the Unity editor being present.
    /// </para>
    /// <para>
    /// The asset path is specified by defining a <see cref="GlobalConfigAttribute"/>. If no attribute is defined it will be saved in the root assets folder.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// [GlobalConfig("Assets/Resources/MyConfigFiles/")]
    /// public class MyGlobalConfig : GlobalConfig&lt;MyGlobalConfig&gt;
    /// {
    ///     public int MyGlobalVariable;
    /// }
    ///
    /// void SomeMethod()
    /// {
    ///     int value = MyGlobalConfig.Instance.MyGlobalVariable;
    /// }
    /// </code>
    /// </example>
    public abstract class GlobalConfig<T> : ScriptableObject, IGlobalConfigEvents where T : GlobalConfig<T>, new()
    {
        private static GlobalConfigAttribute configAttribute;

        // Referenced via reflection by EditorOnlyModeConfig
        public static GlobalConfigAttribute ConfigAttribute
        {
            get
            {
                if (configAttribute == null)
                {
                    configAttribute = typeof(T).GetCustomAttribute<GlobalConfigAttribute>();

                    if (configAttribute == null)
                    {
                        configAttribute = new GlobalConfigAttribute(typeof(T).GetNiceName());
                    }
                }

                return configAttribute;
            }
        }

        private static T instance;

        /// <summary>
        /// Gets a value indicating whether this instance has instance loaded.
        /// </summary>
        public static bool HasInstanceLoaded => GlobalConfigUtility<T>.HasInstanceLoaded;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static T Instance => GlobalConfigUtility<T>.GetInstance(ConfigAttribute.AssetPath);

        /// <summary>
        /// Tries to load the singleton instance.
        /// </summary>
        public static void LoadInstanceIfAssetExists() => GlobalConfigUtility<T>.LoadInstanceIfAssetExists(ConfigAttribute.AssetPath);

        /// <summary>
        /// Opens the config in a editor window. This is currently only used internally by the Sirenix.OdinInspector.Editor assembly.
        /// </summary>
        public void OpenInEditor()
        {
#if UNITY_EDITOR


            Type windowType = null;

            try
            {
                Assembly editorAssembly = null;

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "Sirenix.OdinInspector.Editor")
                    {
                        editorAssembly = assembly;
                        break;
                    }
                }

                if (editorAssembly != null)
                {
                    windowType = editorAssembly.GetType("Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
                }
            }
            catch
            {
            }


            if (windowType != null)
            {
                windowType.GetMethods().Where(x => x.Name == "OpenWindow" && x.GetParameters().Length == 1).First()
                    .Invoke(null, new object[] { this });
            }
            else
            {
                Debug.LogError("Failed to open window, could not find Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
            }
#else
            Debug.Log("Downloading, installing and launching the Unity Editor so we can open this config window in the editor, please stand by until pigs can fly and hell has frozen over...");
#endif
        }

        protected virtual void OnConfigInstanceFirstAccessed()
        {
        }

        protected virtual void OnConfigAutoCreated()
        {
        }

        void IGlobalConfigEvents.OnConfigAutoCreated() => this.OnConfigAutoCreated();
        void IGlobalConfigEvents.OnConfigInstanceFirstAccessed() => this.OnConfigInstanceFirstAccessed();
    }

    public static class GlobalConfigUtility<T> where T : ScriptableObject
    {
        private static T instance;

        /// <summary>
        /// Gets a value indicating whether this instance has instance loaded.
        /// </summary>
        public static bool HasInstanceLoaded
        {
            get
            {
                return instance != null;
            }
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static T GetInstance(string defaultAssetFolderPath, string defaultFileNameWithoutExtension = null)
        {
            if (instance == null)
            {
                //if (!ConfigAttribute.UseAsset)
                //{
                //    instance = ScriptableObject.CreateInstance<T>();
                //    instance.name = typeof(T).GetNiceName();
                //}
                //else
                {
                    LoadInstanceIfAssetExists(defaultAssetFolderPath, defaultFileNameWithoutExtension);

                    T inst = instance;

#if UNITY_EDITOR
                    var fileName = defaultFileNameWithoutExtension ?? typeof(T).GetNiceName();
                    string fullPath = Application.dataPath + "/" + defaultAssetFolderPath + fileName + ".asset";

                    if (inst == null && UnityEditor.EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
                    {
                        Debug.LogWarning(defaultAssetFolderPath + fileName + ".asset" + " was prevented from being generated because the PREVENT_SIRENIX_FILE_GENERATION key was defined in Unity's EditorPrefs.");
                        instance = ScriptableObject.CreateInstance<T>();

                        return instance;
                    }

                    if (inst == null)
                    {
                        if (File.Exists(fullPath) && UnityEditor.EditorSettings.serializationMode == UnityEditor.SerializationMode.ForceText)
                        {
                            if (Editor.AssetScriptGuidUtility.TryUpdateAssetScriptGuid(fullPath, typeof(T)))
                            {
                                Debug.Log("Could not load config asset at first, but successfully detected forced text asset serialization, and corrected the config asset m_Script guid.");
                                LoadInstanceIfAssetExists(defaultAssetFolderPath, defaultFileNameWithoutExtension);
                                inst = instance;
                            }
                            else
                            {
                                Debug.LogWarning("Could not load config asset, and failed to auto-correct config asset m_Script guid.");
                            }
                        }
                    }
#endif

                    if (inst == null)
                    {
                        inst = ScriptableObject.CreateInstance<T>();

#if UNITY_EDITOR
                        // TODO: What do we do if it gives us a path to a package?
                        // Can we figure out where the package is actually located, and can we assume we have write rights?
                        // Can we use purely the AssetDatabase and not do any IO manually?

                        var assetPathWithAssetsPrefix = defaultAssetFolderPath;
                        if (!assetPathWithAssetsPrefix.StartsWith("Assets/"))
                        {
                            assetPathWithAssetsPrefix = "Assets/" + assetPathWithAssetsPrefix.TrimStart('/');
                        }

                        if (!Directory.Exists(assetPathWithAssetsPrefix))
                        {
                            Directory.CreateDirectory(new DirectoryInfo(assetPathWithAssetsPrefix).FullName);
                            UnityEditor.AssetDatabase.Refresh();
                        }

                        string niceName = fileName;

                        string assetPath;
                        if (defaultAssetFolderPath.StartsWith("Assets/"))
                        {
                            assetPath = defaultAssetFolderPath + niceName + ".asset";
                        }
                        else
                        {
                            assetPath = "Assets/" + defaultAssetFolderPath + niceName + ".asset";
                        }

                        if (File.Exists(fullPath))
                        {
                            Debug.LogWarning(
                                "Could not load config asset of type " + niceName + " from project path '" + assetPath + "', " +
                                "but an asset file already exists at the path, so could not create a new asset either. The config " +
                                "asset for '" + niceName + "' has been lost, probably due to an invalid m_Script guid. Set forced " +
                                "text serialization in Edit -> Project Settings -> Editor -> Asset Serialization -> Mode and trigger " +
                                "a script reload to allow Odin to auto-correct this.");
                        }
                        else
                        {
                            instance = inst;

                            if (inst is IGlobalConfigEvents ee)
                            {
                                ee.OnConfigAutoCreated();
                            }

                            UnityEditor.EditorApplication.delayCall += () =>
                            {
                                if (inst != null)
                                {
                                    UnityEditor.AssetDatabase.CreateAsset(inst, assetPath);
                                    UnityEditor.AssetDatabase.SaveAssets();
                                    UnityEditor.AssetDatabase.Refresh();
                                }
                            };
                        }
#endif
                    }

                    instance = inst;
                }

                if (instance is IGlobalConfigEvents e)
                {
                    e.OnConfigInstanceFirstAccessed();
                }
            }

            return instance;
        }

        internal static void LoadInstanceIfAssetExists(string assetPath, string defaultFileNameWithoutExtension = null)
        {
            var fileName = defaultFileNameWithoutExtension ?? typeof(T).GetNiceName();

            if (assetPath.Contains("/resources/", StringComparison.OrdinalIgnoreCase))
            {
                var resourcesPath = assetPath;
                var i = resourcesPath.LastIndexOf("/resources/", StringComparison.OrdinalIgnoreCase);
                if (i >= 0)
                {
                    resourcesPath = resourcesPath.Substring(i + "/resources/".Length);
                }


                string niceName = fileName;
                instance = Resources.Load<T>(resourcesPath + niceName);
            }
#if UNITY_EDITOR
            else
            {
                string niceName = fileName;
                instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath + niceName + ".asset");

                // It could be a package and not located in the Assets folder:
                if (instance == null)
                {
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/" + assetPath + niceName + ".asset");
                }
            }

            // If it is relocated
            if (instance == null)
            {
                var relocatedScriptableObject = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name);
                if (relocatedScriptableObject.Length > 0)
                {
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(relocatedScriptableObject[0]));
                }
            }
#endif
        }
    }
}