using UnityEngine;

/// <summary>
/// Настройки UniText для Unicode данных.
/// Должен находиться в Resources/UniTextUnicodeSettings.asset
/// </summary>
public sealed class UniTextSettings : ScriptableObject
{
    private const string ResourcePath = "UniTextSettings";

    [SerializeField]
    private TextAsset unicodeDataAsset;

    /// <summary>
    /// Бинарные данные Unicode.
    /// </summary>
    public TextAsset UnicodeDataAsset => unicodeDataAsset;

    private static UniTextSettings instance;

    /// <summary>
    /// Глобальный экземпляр настроек.
    /// Загружается из Resources/UniTextUnicodeSettings.asset
    /// </summary>
    public static UniTextSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<UniTextSettings>(ResourcePath);

                if (instance == null)
                {
                    Debug.LogError(
                        $"UniTextUnicodeSettings not found at Resources/{ResourcePath}.asset. " +
                        "Create it via Assets > Create > UniText > Unicode Settings and place in Resources folder.");
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Явно установить экземпляр настроек (для тестирования или альтернативной загрузки).
    /// </summary>
    public static void SetInstance(UniTextSettings settings)
    {
        instance = settings;
    }
}
