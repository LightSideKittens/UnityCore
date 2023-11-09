using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore.LevelSystem
{
    public partial class EntiProps : Config<EntiProps>
    {
        static EntiProps() => RegisterMigrations();
        static partial void RegisterMigrations();
        
        
        [Serializable]
        public class PropsByEntityId : Dictionary<string, Props> { }

        [Serializable]
        public class Props : Dictionary<string, Prop>
        {
            public float GetValue<T>() where T : BaseGameProperty
            {
                return this[typeof(T).Name].Value[FloatAndPercent.ValueKey];
            }
        }
        
        [JsonProperty("props")] private PropsByEntityId props = new();

        public PropsByEntityId ByName => props;
        public Props GetProps(string entityId) => props[entityId];
        public void Clear() => props.Clear();
    }
}