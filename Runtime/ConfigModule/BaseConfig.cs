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
[assembly: InternalsVisibleTo("LSCore.DebugModule")]

namespace LSCore.ConfigModule
{
    [Serializable]
    public abstract class BaseConfig
    {
        protected static class ByName<T> where T : BaseConfig, new()
        {
            private static readonly Dictionary<string, T> map = new();
            
            public static T Get(string name, bool isAutoSave = true)
            {
                if (map.TryGetValue(name, out var config)) return config;
                
                config = new T();
                
                if (string.IsNullOrEmpty(name))
                {
                    name = config.FileName;
                }
                else
                {
                    config.FileName = name;
                }
                
                if (isAutoSave) SetupAutoSave(name);
                config.Load();
                map.Add(name, config);

                return config;
            }
            
            private static void SetupAutoSave(string name)
            {
                Editor_Init(name);
                Runtime_Init(name);
            
                [Conditional("UNITY_EDITOR")]
                static void Editor_Init(string name)
                {
                    World.Destroyed += OnDestoy;
                    return;

                    void OnDestoy()
                    {
                        World.Destroyed -= OnDestoy;
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
        
        protected string FullFileName => Path.Combine(FolderPath, $"{FileName}.json");
        protected string FolderPath => Path.Combine(DataPath, Configs, GeneralFolderName, FolderName);
        
        protected virtual string FileName { get; private set; }
        internal virtual string GeneralFolderName => SaveData;
        protected virtual string FolderName => string.Empty;
        
#if UNITY_EDITOR
        protected virtual bool LogEnabled => true;
#endif

        [JsonIgnore] protected virtual JsonSerializerSettings Settings { get; } = new()
        {
            ContractResolver = UnityJsonContractResolver.Instance
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
                json = Resources.Load<TextAsset>(Path.Combine(FolderName, FileName))?.text;
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
                Migrator.Migrate(FileName, token);
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
            var folderPath = FolderPath;
            
            if (Directory.Exists(folderPath) == false)
            {
                Directory.CreateDirectory(folderPath);
            }

            var json = Serialize();
            
            File.WriteAllText(FullFileName,json); 
            OnSaved();
        }

        internal JToken GetJToken()
        {
            var json = GetJsonText();
            return JToken.Parse(json);
        }

        private void Deserialize(string json)
        {
            Log($"[{FileName}] Loaded (Deserialized)");
            JsonConvert.PopulateObject(json, this, Settings);
        }

        private string Serialize()
        {
            Log($"[{FileName}] Saved (Serialized)");
            var json = JsonConvert.SerializeObject(this, Settings);

            return json;
        }

        [Conditional("DEBUG")]
        private void Log(string message)
        {
#if UNITY_EDITOR
            if (LogEnabled)
#endif
                Burger.Log(message);
        }
    }

    [Serializable]
    public abstract class BaseConfig<T> : BaseConfig where T : BaseConfig<T>, new()
    {
        private static T instance;
        private static Func<T> getter = Get;
        public static T Config => getter();

        protected override string FileName => $"{char.ToLower(typeof(T).Name[0])}{typeof(T).Name[1..]}";
        protected override void OnDestroy() => getter = Get;
        private static T Get()
        {
            getter = GetInstance;
            instance = ByName<T>.Get(string.Empty);
            return instance;
        }

        private static T GetInstance() => instance;

        public static void LoadOnNextAccess()
        {
            if(instance == null) return;
            ByName<T>.LoadOnNextAccess(instance.FileName);
            getter = Get;
        }
    }
}