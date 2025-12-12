using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Настройки UniText для Unicode данных.
/// Должен находиться в Resources/UniTextSettings.asset
/// </summary>
public sealed class UniTextSettings : ScriptableObject
{
    private const string ResourcePath = "UniTextSettings";

    [SerializeField]
    private TextAsset unicodeDataAsset;

    [Header("Default Font")]
    [SerializeField]
    [Tooltip("Default font used when UniText has no font assigned")]
    private UniTextFontAsset defaultFontAsset;

    [Header("Font Fallback")]
    [SerializeField]
    [Tooltip("Global fallback font assets used when character is not found in primary font or its fallbacks")]
    private List<UniTextFontAsset> fallbackFontAssets;

    /// <summary>
    /// Бинарные данные Unicode.
    /// </summary>
    public TextAsset UnicodeDataAsset => unicodeDataAsset;

    /// <summary>
    /// Default font asset used when UniText has no font assigned.
    /// </summary>
    public static UniTextFontAsset DefaultFontAsset => Instance?.defaultFontAsset;

    /// <summary>
    /// Global fallback font assets.
    /// </summary>
    public static List<UniTextFontAsset> FallbackFontAssets
    {
        get => Instance?.fallbackFontAssets;
        set
        {
            if (Instance != null)
                Instance.fallbackFontAssets = value;
        }
    }

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
