using UnityEngine;

namespace LSCore.ConfigModule
{
    public abstract class BaseConfig
    {
        public static string ConfigTag => "[Config]".ToTag(new Color(1f, 0.49f, 0.25f));
        protected internal virtual void OnDeserializing(){}
        protected internal virtual void OnDeserialized(){}
        protected internal virtual void OnSerializing(){}
        protected internal virtual void OnSerialized(){}
    }
    
    public abstract class LocalDynamicConfig : BaseConfig
    {
        protected internal virtual void AddMigrations(){}
        protected internal virtual void SetDefault(){}
        protected internal virtual void OnDeleting() {}
        protected internal virtual void OnDeleted() {}
    }
}