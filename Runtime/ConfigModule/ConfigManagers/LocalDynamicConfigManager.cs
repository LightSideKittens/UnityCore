using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public class LocalDynamicConfigManager<T> : BaseConfigManager<T> where T : LocalDynamicConfig, new()
    {
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

        public LocalDynamicConfigManager(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public void LoadOnNextAccess() => wasLoaded = false;

        protected string GetFullFileName(string path)
        {
            return $"{path}.json";
        }

        internal virtual void Load()
        {
            string json;
            
            (string current, string target) data;
            if (!Migrator.Path.TryGet(fullPath, out data))
            {
                var fullFileName = GetFullFileName(fullPath);
                json = File.ReadAllText(fullFileName);
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

        internal virtual void Save()
        {
            var json = Serialize();
            string fullFileName = GetFullFileName(fullPath);
            Directory.CreateDirectory(fullFileName);
            File.WriteAllText(fullFileName,json); 
        }

        internal void Delete()
        {
            cached.OnDeleting();
            OnDelete();
            cached.OnDeleted();
        }

        protected virtual void OnDelete()
        {
            string fullFileName = GetFullFileName(fullPath);
            File.Delete(fullFileName);
        }
    }
}