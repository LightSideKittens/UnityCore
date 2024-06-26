#if !UNITY_EDITOR
#define RUNTIME
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using static LSCore.ConfigModule.FolderNames;
using Debug = UnityEngine.Debug;

[assembly: InternalsVisibleTo("LSCore.DebugModule")]

namespace LSCore.ConfigModule
{
    [Serializable]
    public abstract class BaseConfig
    {
        protected static Stack<Action> onStaticConstructor = new();
        
        protected static class ByName<T> where T : BaseConfig, new()
        {
            private static readonly Dictionary<string, T> map = new();
            
            public static T Get(string name, bool isAutoSave)
            {
                if (map.TryGetValue(name, out var config)) return config;
                
                config = new T
                {
                    fileName = name
                };

                if (isAutoSave) SetupAutoSave(name);
                config.Load();
                map.Add(name, config);

                return config;
            }
            
            private static void SetupAutoSave(string name)
            {
                Editor_Init(name);
                Runtime_Init(name);
                return;

                [Conditional("UNITY_EDITOR")]
                static void Editor_Init(string name)
                {
                    World.Destroyed += OnDestroy;
                    return;

                    void OnDestroy()
                    {
                        World.Destroyed -= OnDestroy;
                        map[name].Save();
                        LoadOnNextAccess(name);
                    }
                }

                [Conditional("RUNTIME")]
                static void Runtime_Init(string name)
                {
                    World.ApplicationPaused += OnPause;

                    void OnPause()
                    {
                        World.ApplicationPaused -= OnPause;
                        map[name].Save();
                    }
                }
            }
            
            public static void LoadOnNextAccess(string name)
            {
                map[name].OnDestroy();
                map.Remove(name);
            }
        }

        public static string DataPath
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

        public static readonly string ConfigsPath = Path.Combine(DataPath, Configs);
        
        protected string FullFileName => Path.Combine(AbsoluteFolderPath, $"{fileName}.json");
        protected string AbsoluteFolderPath => Path.Combine(RootPath, RelativeFolderPath);
        
        protected virtual string RootPath => Path.Combine(ConfigsPath, DynamicData);
        protected virtual string RelativeFolderPath => string.Empty;
        
        private string fileName;
        
#if UNITY_EDITOR
        protected virtual bool LogEnabled => true;
#endif

        [JsonIgnore] protected virtual JsonSerializerSettings Settings { get; } = new()
        {
            ContractResolver = UnityJsonContractResolver.Instance,
            Error = (_, args) =>
            {
                args.ErrorContext.Handled = true;
            }
        };

        protected virtual void SetDefault(){}
        protected virtual void OnLoading(){}
        protected virtual void OnLoaded(){}
        protected virtual void OnSaving(){}
        protected virtual void OnSaved(){}
        protected virtual void OnDestroy(){}

        protected string GetJsonText()
        {
            var fullFileName = FullFileName;
            string json;

            if (File.Exists(fullFileName))
            {
                json = File.ReadAllText(fullFileName);
            }
            else
            {
                json = Resources.Load<TextAsset>(Path.Combine(RelativeFolderPath, fileName))?.text;
            }

            return json;
        }
        
        protected internal BaseConfig Load()
        {
            OnLoading();
            
            var json = GetJsonText();
            
            if (string.IsNullOrEmpty(json) == false)
            {
                var token = JToken.Parse(json);
                Migrator.Migrate(fileName, token);
                Deserialize(token.ToString());
            }
            else
            {
                SetDefault();
            }
            
            OnLoaded();
            return this;
        }
        
        protected internal void Save()
        {
            OnSaving();
            var folderPath = AbsoluteFolderPath;
            
            Directory.CreateDirectory(folderPath);

            var json = Serialize();
            
            File.WriteAllText(FullFileName,json); 
            OnSaved();
        }
        
        protected internal void Delete()
        {
            File.Delete(FullFileName); 
        }

        internal JToken GetJToken()
        {
            var json = GetJsonText();
            return JToken.Parse(json);
        }

        private string logTag => $"[{fileName}]".ToTag(new Color(0.19f, 0.97f, 1f));
        
        private void Deserialize(string json)
        {
            Log("Loaded (Deserialized)");

            JsonConvert.PopulateObject(json, this, Settings);
        }

        private string Serialize()
        {
            Log("Saved (Serialized)");
            var json = JsonConvert.SerializeObject(this, Settings);

            return json;
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
#if UNITY_EDITOR
            if (LogEnabled)
#endif
                Burger.Log($"{logTag} {message}");
        }
    }

    [Serializable]
    public abstract class BaseSingleConfig<T> : BaseConfig where T : BaseSingleConfig<T>, new()
    {
        private static T instance;
        private static Func<T> getter = Get;
        protected static string fileName = typeof(T).Name;
        protected static bool isAutoSave = true;
        public static T Config => getter();
        public static bool IsNull => instance == null;
        protected override void OnDestroy() => getter = Get;
        
        static BaseSingleConfig()
        {
            foreach (var action in onStaticConstructor)
            {
                action();
            }
        }
        
        private static T Get()
        {
            getter = GetInstance;
            instance = ByName<T>.Get(fileName, isAutoSave);
            return instance;
        }

        private static T GetInstance() => instance;

        public static void LoadOnNextAccess()
        {
            if(instance == null) return;
            ByName<T>.LoadOnNextAccess(fileName);
            getter = Get;
        }
    }
}