using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LSCore.Attributes;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace LSCore
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Serializable]
    public class ShowWindow : LSAction
    {
        public class WindowTypes : LocalDynamicConfig
        {
            public List<Type> types;
            public static WindowTypes Config => Manager.Config;
            public static ResourcesConfigManager<WindowTypes> Manager =>
                ConfigMaster<ResourcesConfigManager<WindowTypes>>.Default;
        }
        

        static ShowWindow()
        {
#if UNITY_EDITOR
            EditorApplication.update += Initialize;
#else
            World.Created += Initialize;
#endif
        }

        private static void Initialize()
        {
#if UNITY_EDITOR
            EditorApplication.update -= Initialize;
#else
            World.Created -= Initialize;
#endif
            var type = typeof(BaseWindow<>);
#if UNITY_EDITOR
            var allAssembly = type.Assembly.GetRelevantAssemblies();
            WindowTypes.Manager.Delete();
            
            WindowTypes.Config.types = allAssembly
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => t.IsSubclassOfRawGeneric(type))
                .ToList();

            WindowTypes.Manager.Save();
#endif
            

            foreach (var target in WindowTypes.Config.types)
            {
                var method = target.GetMethod("Show", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, new[] { typeof(ShowWindowOption) }, null);
                Action<ShowWindowOption> action = (Action<ShowWindowOption>)Delegate.CreateDelegate(typeof(Action<ShowWindowOption>), method);
                actions.Add(target.FullName, action);
            }
        }
        
        private static Dictionary<string, Action<ShowWindowOption>> actions = new();
        [ValueDropdown("GetKeys")]
        public string window;
        public ShowWindowOption option;

        public Assembly rootObjectAssembly;

#if UNITY_EDITOR
        [GetInspectorProperty]
        private void GetRootObject(InspectorProperty property)
        {
            rootObjectAssembly = property.SerializationRoot.Info.TypeOfValue.Assembly;
        }
#endif
        
        private IEnumerable<string> GetKeys()
        {
            if (rootObjectAssembly == null) return null;
            var set = rootObjectAssembly.GetVisibleTypes().Select(x => x.FullName).ToHashSet();
            set.IntersectWith(actions.Keys);
            return set;
        }

        public override void Invoke()
        {
            actions[window](option);
        }
    }
}