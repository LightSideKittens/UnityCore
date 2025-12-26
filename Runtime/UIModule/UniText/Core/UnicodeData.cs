using System;
using System.Runtime.CompilerServices;
using UnityEngine;


public static class UnicodeData
{
    #region Unicode Codepoint Constants

    public const int Tab = 0x0009;
    public const int LineFeed = 0x000A;
    public const int VerticalTab = 0x000B;
    public const int FormFeed = 0x000C;
    public const int CarriageReturn = 0x000D;
    public const int Space = 0x0020;
    public const int Hyphen = 0x002D;
    public const int NextLine = 0x0085;

    public const int NoBreakSpace = 0x00A0;
    public const int SoftHyphen = 0x00AD;
    public const int NonBreakingHyphen = 0x2011;

    public const int ZeroWidthSpace = 0x200B;
    public const int ZeroWidthNonJoiner = 0x200C;
    public const int ZeroWidthJoiner = 0x200D;
    public const int WordJoiner = 0x2060;

    public const int LeftToRightMark = 0x200E;
    public const int RightToLeftMark = 0x200F;
    public const int ArabicLetterMark = 0x061C;

    public const int LineSeparator = 0x2028;
    public const int ParagraphSeparator = 0x2029;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLineBreak(int cp)
    {
        return cp == LineFeed || cp == LineSeparator || cp == ParagraphSeparator;
    }

    public const int LeftParenthesis = 0x0028;
    public const int RightParenthesis = 0x0029;
    public const int LeftPointingAngleBracket = 0x2329;
    public const int RightPointingAngleBracket = 0x232A;
    public const int LeftAngleBracket = 0x3008;
    public const int RightAngleBracket = 0x3009;

    public const int ArabicLam = 0x0644;
    public const int ArabicAlefMaddaAbove = 0x0622;
    public const int ArabicAlefHamzaAbove = 0x0623;
    public const int ArabicAlefHamzaBelow = 0x0625;
    public const int ArabicAlef = 0x0627;

    public const int ArabicLigatureLamAlefMaddaIsolated = 0xFEF5;
    public const int ArabicLigatureLamAlefMaddaFinal = 0xFEF6;
    public const int ArabicLigatureLamAlefHamzaAboveIsolated = 0xFEF7;
    public const int ArabicLigatureLamAlefHamzaAboveFinal = 0xFEF8;
    public const int ArabicLigatureLamAlefHamzaBelowIsolated = 0xFEF9;
    public const int ArabicLigatureLamAlefHamzaBelowFinal = 0xFEFA;
    public const int ArabicLigatureLamAlefIsolated = 0xFEFB;
    public const int ArabicLigatureLamAlefFinal = 0xFEFC;

    public const int ReplacementCharacter = 0xFFFD;
    public const int DottedCircle = 0x25CC;

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

    public static IUnicodeDataProvider Provider
    {
        get
        {
            EnsureInitialized();
            return provider;
        }
    }


    public static bool IsInitialized => provider != null;


    public static void EnsureInitialized()
    {
        if (IsInitialized)
            return;
        
        var settings = UniTextSettings.Instance;
        if (settings == null || settings.UnicodeDataAsset == null)
        {
            Debug.LogError("UnicodeData: Failed to initialize - UniTextSettings or UnicodeDataAsset is null.");
            return;
        }

        try
        {
            provider = new BinaryUnicodeDataProvider(settings.UnicodeDataAsset.bytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"UnicodeData: Failed to parse Unicode data: {ex.Message}");
        }
    }


    public static void Initialize(IUnicodeDataProvider dataProvider)
    {
        if (dataProvider == null)
            throw new ArgumentNullException(nameof(dataProvider));

        provider = dataProvider;
    }


    public static void Initialize(byte[] unicodeData)
    {
        if (unicodeData == null)
            throw new ArgumentNullException(nameof(unicodeData));

        provider = new BinaryUnicodeDataProvider(unicodeData);
    }
}