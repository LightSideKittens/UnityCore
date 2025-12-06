using System;
using UnityEngine;

/// <summary>
/// Статический провайдер Unicode данных.
/// Данные загружаются один раз и используются всеми компонентами.
/// </summary>
public static class UnicodeData
{
    private static IUnicodeDataProvider provider;
    private static bool initialized;
    private static bool initializationFailed;

    /// <summary>
    /// Провайдер Unicode данных.
    /// Автоматически инициализируется при первом доступе.
    /// </summary>
    public static IUnicodeDataProvider Provider
    {
        get
        {
            EnsureInitialized();
            return provider;
        }
    }

    /// <summary>
    /// Проверка инициализации без автоматической загрузки.
    /// </summary>
    public static bool IsInitialized => initialized && provider != null;

    /// <summary>
    /// Инициализировать с данными из UniTextSettings.
    /// Вызывается автоматически при первом доступе к Provider.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized = true;

        var settings = UniTextSettings.Instance;
        if (settings == null || settings.UnicodeDataAsset == null)
        {
            initializationFailed = true;
            Debug.LogError("UnicodeData: Failed to initialize - UniTextSettings or UnicodeDataAsset is null.");
            return;
        }

        try
        {
            provider = new BinaryUnicodeDataProvider(settings.UnicodeDataAsset.bytes);
        }
        catch (Exception ex)
        {
            initializationFailed = true;
            Debug.LogError($"UnicodeData: Failed to parse Unicode data: {ex.Message}");
        }
    }

    /// <summary>
    /// Явная инициализация с внешними данными.
    /// Используется для тестирования или альтернативных источников данных.
    /// </summary>
    public static void Initialize(IUnicodeDataProvider dataProvider)
    {
        if (dataProvider == null)
            throw new ArgumentNullException(nameof(dataProvider));

        provider = dataProvider;
        initialized = true;
        initializationFailed = false;
    }

    /// <summary>
    /// Явная инициализация с бинарными данными.
    /// </summary>
    public static void Initialize(byte[] unicodeData)
    {
        if (unicodeData == null)
            throw new ArgumentNullException(nameof(unicodeData));

        provider = new BinaryUnicodeDataProvider(unicodeData);
        initialized = true;
        initializationFailed = false;
    }

    /// <summary>
    /// Сбросить инициализацию (для тестирования).
    /// </summary>
    public static void Reset()
    {
        provider = null;
        initialized = false;
        initializationFailed = false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Сброс при выходе из Play Mode в редакторе.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnDomainReload()
    {
        Reset();
    }
#endif
}
