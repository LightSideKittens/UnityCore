using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LSCore.ConfigModule
{
    public interface ILocalConfigManager
    {
        string DefaultPath { get; }
        void SetFullPath(string fullPath);
    }

    public static class ConfigMaster<TManager> where TManager : ILocalConfigManager, new()
    {
        private static TManager meowfault;
        private static Dictionary<string, TManager> configs = new();

        public static TManager Default
        {
            get
            {
                if (meowfault == null)
                {
                    meowfault = new TManager();
                    meowfault.SetFullPath(meowfault.DefaultPath);
                }
                
                return meowfault;
            }
        }
        
        public static TManager Get(string path)
        {
            if (meowfault?.DefaultPath == path)
            {
                return meowfault;
            }
            
            if (!configs.TryGetValue(path, out var manager))
            {
                manager = new TManager();
                manager.SetFullPath(path);
                configs.Add(path, manager);
            }

            return manager;
        }
    }

    public abstract class LocalDynamicConfigManager<T> : BaseConfigManager<T>, ILocalConfigManager where T : LocalDynamicConfig, new()
    {
        protected override string Tag => $"{base.Tag} {FileNameTag}";
        private string FileNameTag => $"({Path.GetFileNameWithoutExtension(FullFileName)})".ToTag(new Color(1f, 0.8f, 0.05f));

        protected virtual string GetFullPath(string relativePath)
        {
            return ConfigPaths.Game.Dynamic(relativePath);
        }

        protected virtual string DefaultPath => typeof(T).Name;
        string ILocalConfigManager.DefaultPath => DefaultPath;

        void ILocalConfigManager.SetFullPath(string relativePath)
        {
            fullPath = GetFullPath(relativePath);
        }
        
        protected string fullPath;
        
        private bool wasLoaded;
        private bool wasSaved;
        private JObject token;

        public T Config
        {
            get
            {
                wasSaved = false;
                if (!wasLoaded) Load();
                return cached;
            }
        }

        public virtual void Unload()
        {
            wasLoaded = false;
            token = null;
            cached = null;
        }

        public bool IsFileExists => File.Exists(FullFileName);
        
        protected string FullFileName => string.Concat(fullPath, ".json");

        private string FullFileNameMeta => $"Full file name: {FullFileName}";
        public virtual void Load()
        {
            var fullFileName = FullFileName;
            string json = string.Empty;
            
            if (File.Exists(fullFileName))
            {
                json = File.ReadAllText(fullFileName);
            }
            else
            {
                cached = null;
                token = null;
            }
            
            SetMeta(FullFileNameMeta);
            Log("Loading");
            
            cached ??= new T();
            
            if (string.IsNullOrEmpty(json))
            {
                Log("Config created");
                cached.AddMigrations();
                cached.SetDefault();
                wasLoaded = true;
                return;
            }

            //TODO: Implement cool and convenient logic of path migration
            /*(string current, string target) data;
            if (!Migrator.Path.TryGet(fullPath, out data))
            {
                var fullFileName = GetFullFileName(fullPath);
                if (File.Exists(fullFileName))
                {
                    json = File.ReadAllText(fullFileName);
                }
                else
                {
                    cached.SetDefault();
                    wasLoaded = true;
                    return;
                }
            }
            else
            {
                var currentPath = GetFullFileName(data.current);
                var targetPath = GetFullFileName(data.target);

                if (File.Exists(targetPath))
                {
                    throw new Exception("File is already exists");
                }
                
                if (File.Exists(currentPath))
                {
                    json = File.ReadAllText(currentPath);
                    File.Delete(currentPath);
                }
                else
                {
                    cached.SetDefault();
                    wasLoaded = true;
                    return;
                }
                
                Save(targetPath, json);
            }*/
            
            SetMeta($"{FullFileNameMeta}\nJson:\n{json}");
            Log("Read json");
            token = JObject.Parse(json);
            var wasMigrated = Migrator.Type<T>.Migrate(token);
            wasMigrated |= Migrator.Migrate(fullPath, token);
            if (wasMigrated)
            {
                SetMeta($"{FullFileNameMeta}\nJson:\n{token}");
                Log("Migrated");
            }
            Deserialize(token);
            wasLoaded = true;
        }

        public virtual void Save()
        {
            if (!wasLoaded) return;
            if (wasSaved) return;
            
            wasSaved = true;
            string fullFileName = FullFileName;
            string json;
            
            if (token != null)
            {
                var target = SerializeAsToken(); 
                Migrator.Type<T>.PopulateMeta(token, target);
                Migrator.PopulateMeta(token, target);
                json = target.ToString();
            }
            else
            {
                json = Serialize();
            }
            
            Save(fullFileName, json);
        }

        protected void Save(string fullFileName, string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullFileName)!);
            File.WriteAllText(fullFileName,json);
        }
        
        public void Delete()
        {
            Log("Deleting");
            cached?.OnDeleting();
            OnDelete();
            cached?.OnDeleted();
            cached = null;
            token = null;
            wasLoaded = false;
            wasSaved = false;
        }

        
        protected virtual void OnDelete()
        {
            if (Exists)
            {
                string fullFileName = FullFileName;
                File.Delete(fullFileName);
            }
        }
        
        public bool Exists
        {
            get
            {
                string fullFileName = FullFileName;
                return File.Exists(fullFileName);
            }
        }
        
        /*public void AddPathMigration(string newPath)
        {
            Migrator.Path.Add(fullPath, GetPath(newPath));
        }*/
        
        public void AddMigration(Action<JToken> migrator)
        {
            Migrator.Add(fullPath, migrator);
        }
    }
}