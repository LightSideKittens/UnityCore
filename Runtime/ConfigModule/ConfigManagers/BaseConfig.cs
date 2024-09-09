namespace LSCore.ConfigModule
{
    public abstract class BaseConfig
    {
        public virtual void OnDeserializing(){}
        public virtual void OnDeserialized(){}
        public virtual void OnSerializing(){}
        public virtual void OnSerialized(){}
    }
    
    public abstract class LocalDynamicConfig : BaseConfig
    {
        public virtual void OnDeleting() {}
        public virtual void OnDeleted() {}
    }
    
    public abstract class LocalStaticConfig : BaseConfig
    {
        
    }
}