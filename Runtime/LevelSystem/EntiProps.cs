using System;
using System.Collections.Generic;
using Battle.Data.GameProperty;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace Battle.Data
{
    public class EntiProps : BaseConfig<EntiProps>
    {
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

        protected override JsonSerializerSettings Settings { get; } = new()
        {
            Converters = new List<JsonConverter> { new PropConverter() },
        };
        
        [JsonProperty("props")] private PropsByEntityId props = new();

        public static PropsByEntityId ByName => Config.props;
        public static Props GetProps(string entityId) => Config.props[entityId];
        public static void Clear() => Config.props.Clear();
    }
}