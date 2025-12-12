using System;
using UnityEngine;

/// <summary>
/// Статический провайдер Unicode данных.
/// Данные загружаются один раз и используются всеми компонентами.
/// </summary>
public static class UnicodeData
{
    #region Unicode Codepoint Constants

    // ASCII Control Characters
    public const int Tab = 0x0009;
    public const int LineFeed = 0x000A;
    public const int VerticalTab = 0x000B;
    public const int FormFeed = 0x000C;
    public const int CarriageReturn = 0x000D;
    public const int Space = 0x0020;
    public const int Hyphen = 0x002D;
    public const int NextLine = 0x0085;

    // Special Spaces and Hyphens
    public const int NoBreakSpace = 0x00A0;
    public const int SoftHyphen = 0x00AD;
    public const int NonBreakingHyphen = 0x2011;

    // Zero Width Characters
    public const int ZeroWidthSpace = 0x200B;
    public const int ZeroWidthNonJoiner = 0x200C;
    public const int ZeroWidthJoiner = 0x200D;
    public const int WordJoiner = 0x2060;

    // Directional Formatting Characters
    public const int LeftToRightMark = 0x200E;
    public const int RightToLeftMark = 0x200F;
    public const int ArabicLetterMark = 0x061C;

    // Separators
    public const int LineSeparator = 0x2028;
    public const int ParagraphSeparator = 0x2029;

    // Brackets (for BiDi pairing)
    public const int LeftParenthesis = 0x0028;
    public const int RightParenthesis = 0x0029;
    public const int LeftPointingAngleBracket = 0x2329;
    public const int RightPointingAngleBracket = 0x232A;
    public const int LeftAngleBracket = 0x3008;
    public const int RightAngleBracket = 0x3009;

    // Arabic Script
    public const int ArabicLam = 0x0644;
    public const int ArabicAlefMaddaAbove = 0x0622;
    public const int ArabicAlefHamzaAbove = 0x0623;
    public const int ArabicAlefHamzaBelow = 0x0625;
    public const int ArabicAlef = 0x0627;

    // Arabic Presentation Forms (Lam-Alef Ligatures)
    public const int ArabicLigatureLamAlefMaddaIsolated = 0xFEF5;
    public const int ArabicLigatureLamAlefMaddaFinal = 0xFEF6;
    public const int ArabicLigatureLamAlefHamzaAboveIsolated = 0xFEF7;
    public const int ArabicLigatureLamAlefHamzaAboveFinal = 0xFEF8;
    public const int ArabicLigatureLamAlefHamzaBelowIsolated = 0xFEF9;
    public const int ArabicLigatureLamAlefHamzaBelowFinal = 0xFEFA;
    public const int ArabicLigatureLamAlefIsolated = 0xFEFB;
    public const int ArabicLigatureLamAlefFinal = 0xFEFC;

    // Special Characters
    public const int ReplacementCharacter = 0xFFFD;
    public const int DottedCircle = 0x25CC;

    // Script Ranges
    public const int ArabicBlockStart = 0x0600;
    public const int ArabicBlockEnd = 0x06FF;
    public const int ArabicSupplementStart = 0x0750;
    public const int ArabicSupplementEnd = 0x077F;
    public const int ArabicExtendedAStart = 0x08A0;
    public const int ArabicExtendedAEnd = 0x08FF;
    public const int ArabicPresentationFormsAStart = 0xFB50;
    public const int ArabicPresentationFormsAEnd = 0xFDFF;
    public const int ArabicPresentationFormsBStart = 0xFE70;
    public const int ArabicPresentationFormsBEnd = 0xFEFF;

    #endregion

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
    public static bool IsInitialized => initialized && !initializationFailed && provider != null;

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
