namespace LSCore.ConfigModule
{
    public abstract class ConfigManager
    {
        public abstract string Folder { get; }
        public abstract string Name { get; }

        public T Load<T>()
        {
            return default;
        }
        
        public void Save()
        {
            
        }
    }
}