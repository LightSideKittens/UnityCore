using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public interface ILocalConfigManager
    {
        string DefaultPath { get; }
        void SetPath(string fullPath);
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
                    meowfault.SetPath(meowfault.DefaultPath);
                }
                
                return meowfault;
            }
        }
        
        public static TManager Get(string fullPath)
        {
            if (meowfault?.DefaultPath == fullPath)
            {
                return meowfault;
            }
            
            if (!configs.TryGetValue(fullPath, out var manager))
            {
                manager = new TManager();
                manager.SetPath(fullPath);
                configs.Add(fullPath, manager);
            }

            return manager;
        }
    }

    public class LocalDynamicConfigManager<T> : BaseConfigManager<T>, ILocalConfigManager where T : LocalDynamicConfig, new()
    {
        protected override string Tag => $"{base.Tag}.{Path.GetFileNameWithoutExtension(GetFullFileName(fullPath))}";

        protected virtual string GetPath(string path)
        {
            return path;
        }

        protected virtual string DefaultPath => typeof(T).Name;
        string ILocalConfigManager.DefaultPath => DefaultPath;

        void ILocalConfigManager.SetPath(string path)
        {
            fullPath = GetPath(path);
        }
        
        protected string fullPath;
        
        private bool wasLoaded;
        private JObject token;

        public T Config
        {
            get
            {
                if (!wasLoaded) Load();
                return cached;
            }
        }

        public void LoadOnNextAccess() => wasLoaded = false;

        protected string GetFullFileName(string path)
        {
            return $"{path}.json";
        }

        private string FullFileNameMeta => $"Full file name: {GetFullFileName(fullPath)}";
        public virtual void Load()
        {
            var fullFileName = GetFullFileName(fullPath);
            string json = string.Empty;
            
            if (File.Exists(fullFileName))
            {
                json = File.ReadAllText(fullFileName);
            }
            else
            {
                cached = null;
            }
            
            SetMeta(FullFileNameMeta);
            Log("Loading");

            if (cached == null)
            {
                Log("Config created");
                cached = new T();
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
            var json = token != null ? Serialize(token) : Serialize();
            string fullFileName = GetFullFileName(fullPath);
            Save(fullFileName, json);
        }

        protected void Save(string fullFileName, string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullFileName)!);
            File.WriteAllText(fullFileName,json); 
        }
        
        public void Delete()
        {
            cached?.OnDeleting();
            OnDelete();
            cached?.OnDeleted();
        }

        
        protected virtual void OnDelete()
        {
            string fullFileName = GetFullFileName(fullPath);
            File.Delete(fullFileName);
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