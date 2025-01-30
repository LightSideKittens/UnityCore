using System.IO;
using UnityEngine;

namespace LSCore.ConfigModule
{
    public class ResourcesConfigManager<T> : BaseConfigManager<T>, ILocalConfigManager where T : LocalDynamicConfig, new()
    {
        public string DefaultPath => typeof(T).Name;

        void ILocalConfigManager.SetPath(string path)
        {
            this.path = path;
        }
        
        protected string path;

        private bool wasLoaded;

        public T Config
        {
            get
            {
                if (!wasLoaded) Load();
                return cached;
            }
        }

        internal virtual void Load()
        {
            var json = Resources.Load<TextAsset>(path)?.text ?? string.Empty;
            Deserialize(json);
            wasLoaded = true;
        }
        
#if UNITY_EDITOR
        public virtual void Save()
        {
            var json = Serialize();
            string fullFileName = $"{ConfigPaths.Game.Resources(path)}.json";
            Directory.CreateDirectory(Path.GetDirectoryName(fullFileName)!);
            File.WriteAllText(fullFileName,json); 
        }
        
        public virtual void Delete()
        {
            string fullFileName = $"{ConfigPaths.Game.Resources(path)}.json";
            if (File.Exists(fullFileName))
            {
                File.Delete(fullFileName); 
            }
        }
#endif
    }
}