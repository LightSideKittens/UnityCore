using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Telegram.Bot
{
    /// <summary>
    /// Статический класс, предоставляющий JsonSerializerSettings,
    /// настроенные для сериализации Bot API с использованием Newtonsoft.Json.
    /// </summary>
    public static class JsonBotAPI
    {
        /// <summary>
        /// JsonSerializerSettings, настроенные для сериализации Bot API.
        /// </summary>
        public static JsonSerializerSettings Settings { get; } = CreateSettings();

        private static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings
            {
                // Настройка именования свойств в snake_case
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                // Отключение записи значений по умолчанию
                DefaultValueHandling = DefaultValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
                // При необходимости можно настроить обработку null-значений:
                // NullValueHandling = NullValueHandling.Ignore,
            };

            // Здесь можно добавить дополнительные настройки или конвертеры, если потребуется

            return settings;
        }
    }
}