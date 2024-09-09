using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public interface ILocalConfigManager
    {
        void SetPath(string fullPath);
    }

    public static class ConfigMaster<TManager> where TManager : ILocalConfigManager, new()
    {
        private static Dictionary<string, TManager> configs = new();

        public static TManager Get(string fullPath)
        {
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
        protected virtual string GetPath(string path)
        {
            return path;
        }

        void ILocalConfigManager.SetPath(string fullPath)
        {
            this.fullPath = GetPath(fullPath);
        }
        
        protected string fullPath;
        
        private bool wasLoaded;

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

        public virtual void Load()
        {
            string json;
            
            (string current, string target) data;
            if (!Migrator.Path.TryGet(fullPath, out data))
            {
                var fullFileName = GetFullFileName(fullPath);
                if (File.Exists(fullFileName))
                {
                    json = File.ReadAllText(fullFileName);
                }
                else
                {
                    cached ??= new T();
                    cached.SetDefault();
                    wasLoaded = true;
                    return;
                }
            }
            else
            {
                var currentPath = GetFullFileName(data.current);
                json = File.ReadAllText(currentPath);
                File.Delete(currentPath);
                var targetPath = GetFullFileName(data.target);
                File.WriteAllText(targetPath,json);
            }

            var token = JToken.Parse(json);
            Migrator.Type<T>.Migrate(token);
            Migrator.Migrate(fullPath, token);
            json = token.ToString(Formatting.None);
            
            Deserialize(json);
            wasLoaded = true;
        }

        public virtual void Save()
        {
            var json = Serialize();
            string fullFileName = GetFullFileName(fullPath);
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
    }
}