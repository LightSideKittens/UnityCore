#if !UNITY_EDITOR
#define RUNTIME
#endif

using System;
using System.Diagnostics;
using System.IO;
using LSCore.Extensions;
using LSCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static LSCore.ConfigModule.FolderNames;

namespace LSCore.ConfigModule
{
    [Serializable]
    public abstract class BaseConfig<T> where T : BaseConfig<T>, new()
    {
        public static T Config => getter();

        private static string DataPath
        {
            get
            {
                #if UNITY_EDITOR
                return Application.dataPath;
                #else
                return Application.persistentDataPath;
                #endif
            }
        }
        
        protected string FullFileName => System.IO.Path.Combine(FolderPath, $"{FileName}.{Ext}");
        protected string FolderPath => System.IO.Path.Combine(DataPath, Configs, GeneralFolderName, FolderName);

        protected virtual string Ext => FileExtensions.Json;
        protected virtual string FileName => $"{char.ToLower(typeof(T).Name[0])}{typeof(T).Name[1..]}";
        protected virtual string GeneralFolderName => SaveData;
        protected virtual string FolderName => string.Empty;

        [JsonIgnore] protected virtual JsonSerializerSettings Settings { get; } = new()
        {
            ContractResolver = UnityJsonContractResolver.Instance
        };
        
        protected static T instance;
        private static Func<T> getter;

        static BaseConfig()
        {
            LoadOnNextAccess();

            var needInit = !typeof(T).IsSubclassOfRawGeneric(typeof(BaseResourcesConfig<>));
            
            if (needInit)
            {
                Init();
            }
        }

        private static void Init()
        {
            Editor_Init();
            Runtime_Init();
            
            [Conditional("UNITY_EDITOR")]
            static void Editor_Init()
            {
                World.Destroyed += Save;
                World.Destroyed += LoadOnNextAccess;
            }

            [Conditional("RUNTIME")]
            static void Runtime_Init()
            {
                World.ApplicationPaused += Save;
            }
        }

        public static void LoadOnNextAccess()
        {
            getter = StaticConstructor;
        }

        private static T StaticConstructor()
        {
            instance = new T();
            Load();

            return instance;
        }
        
        private static T GetInstance() => instance;

        protected virtual void SetDefault(){}
        protected virtual void OnLoading(){}
        protected virtual void OnLoaded(){}
        protected virtual void OnSaving(){}
        protected virtual void OnSaved(){}

        private static string GetJsonText()
        {
            var fullFileName = instance.FullFileName;
            string json;

            if (File.Exists(fullFileName))
            {
                json = File.ReadAllText(fullFileName);
            }
            else
            {
                json = Resources.Load<TextAsset>(Path.Combine(instance.FolderName, instance.FileName))?.text;
            }

            return json;
        }
        
        protected static T Load()
        {
            instance.OnLoading();

            var json = GetJsonText();
            
            if (string.IsNullOrEmpty(json) == false)
            {
                Deserialize(json);
            }
            else
            {
                instance.SetDefault();
            }
            
            getter = GetInstance;
            instance.OnLoaded();
            return instance;
        }
        
        protected internal static void Save()
        {
            instance.OnSaving();
            var folderPath = instance.FolderPath;
            
            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            var json = Serialize(instance);
            
            File.WriteAllText(instance.FullFileName,json);
            instance.OnSaved();
        }

        internal static JToken GetJToken()
        {
            var json = GetJsonText();
            return JToken.Parse(json);
        }

        private static void Deserialize(string json)
        {
            Burger.Log($"[{typeof(T).Name}] Loaded (Deserialized)");
            JsonConvert.PopulateObject(json, instance, instance.Settings);
        }

        private static string Serialize(T config)
        {
            Burger.Log($"[{typeof(T).Name}] Saved (Serialized)");
            var json = JsonConvert.SerializeObject(config, config.Settings);

            return json;
        }
    }
}