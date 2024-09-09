using UnityEngine;

namespace LSCore.ConfigModule
{
    public class LocalStaticConfigManager<T> : BaseConfigManager<T> where T : LocalStaticConfig, new()
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
        
        public LocalStaticConfigManager(string fullPath)
        {
            this.fullPath = fullPath;
        }

        internal virtual void Load()
        {
            var json = Resources.Load<TextAsset>(fullPath)?.text ?? string.Empty;
            Deserialize(json);
            wasLoaded = true;
        }
    }
}