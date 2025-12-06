#region Offline структуры

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

struct UnicodeProps
{
    public BidiClass bidiClass;
    public JoiningType joiningType;
    public JoiningGroup joiningGroup;
    public UnicodeScript script;
    public LineBreakClass lineBreakClass;
    public bool extendedPictographic;
    public GeneralCategory generalCategory;
    public EastAsianWidth eastAsianWidth;
    public GraphemeClusterBreak graphemeClusterBreak;
    public IndicConjunctBreak indicConjunctBreak;
}

/// <summary>
/// Entry for Script_Extensions property
/// </summary>
public struct ScriptExtensionEntry
{
    public int startCodePoint;
    public int endCodePoint;
    public UnicodeScript[] scripts;

    public ScriptExtensionEntry(int start, int end, UnicodeScript[] scripts)
    {
        this.startCodePoint = start;
        this.endCodePoint = end;
        this.scripts = scripts;
    }
}

public class UnicodeDataBuilder
{
    const int MaxCodePoint = 0x10FFFF;
    const int ScalarCount = MaxCodePoint + 1;

    readonly UnicodeProps[] props;
    readonly List<ScriptExtensionEntry> scriptExtensions = new();

    public UnicodeDataBuilder()
    {
        props = new UnicodeProps[ScalarCount];
        InitializeDefaults();
    }

    void InitializeDefaults()
    {
        for (int cp = 0; cp < ScalarCount; cp++)
        {
            props[cp].bidiClass = BidiClass.LeftToRight;
            props[cp].joiningType = JoiningType.NonJoining;
            props[cp].joiningGroup = JoiningGroup.NoJoiningGroup;
            props[cp].script = UnicodeScript.Unknown;
            props[cp].lineBreakClass = LineBreakClass.XX;
            props[cp].generalCategory = GeneralCategory.Cn; // Not assigned
            props[cp].eastAsianWidth = EastAsianWidth.N;    // Neutral
            props[cp].indicConjunctBreak = IndicConjunctBreak.None;
        }
    }

    public void LoadDerivedBidiClass(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string classPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || classPart.Length == 0)
                continue;

            BidiClass bidiClass = ParseBidiClass(classPart);

            ParseRangeAndApply(codeRangePart, cp => props[cp].bidiClass = bidiClass);
        }
    }

    public void LoadArabicShaping(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 4)
                continue;

            string codePart = semi[0].Trim();
            string joiningTypePart = semi[2].Trim();
            string joiningGroupPart = semi[3].Trim();

            if (codePart.Length == 0 || joiningTypePart.Length == 0 || joiningGroupPart.Length == 0)
                continue;

            int codePoint = ParseHexCodePoint(codePart);
            if (codePoint < 0 || codePoint > MaxCodePoint)
                continue;

            JoiningType joiningType = ParseJoiningType(joiningTypePart);
            JoiningGroup joiningGroup = ParseJoiningGroup(joiningGroupPart);

            props[codePoint].joiningType = joiningType;
            props[codePoint].joiningGroup = joiningGroup;
        }
    }


    public void LoadDerivedJoiningType(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string typePart = semi[1].Trim();

            if (codeRangePart.Length == 0 || typePart.Length == 0)
                continue;

            JoiningType joiningType = ParseJoiningType(typePart);

            ParseRangeAndApply(codeRangePart, cp => props[cp].joiningType = joiningType);
        }
    }

    public void LoadScripts(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string scriptPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || scriptPart.Length == 0)
                continue;

            UnicodeScript script = ParseScript(scriptPart);

            ParseRangeAndApply(codeRangePart, cp => props[cp].script = script);
        }
    }

    public void LoadLineBreak(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string lbcPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || lbcPart.Length == 0)
                continue;

            LineBreakClass lbc = ParseLineBreakClass(lbcPart);

            ParseRangeAndApply(codeRangePart, cp => props[cp].lineBreakClass = lbc);
        }
    }

    /// <summary>
    /// Load emoji-data.txt to extract Extended_Pictographic property
    /// </summary>
    public void LoadEmojiData(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string propertyPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || propertyPart.Length == 0)
                continue;

            // Only interested in Extended_Pictographic property
            if (propertyPart != "Extended_Pictographic")
                continue;

            ParseRangeAndApply(codeRangePart, cp => props[cp].extendedPictographic = true);
        }
    }

    /// <summary>
    /// Load DerivedGeneralCategory.txt to extract General_Category property
    /// Format: 0000..001F    ; Cc # [32] <control-0000>..<control-001F>
    /// </summary>
    public void LoadGeneralCategory(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string gcPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || gcPart.Length == 0)
                continue;

            GeneralCategory gc = ParseGeneralCategory(gcPart);
            ParseRangeAndApply(codeRangePart, cp => props[cp].generalCategory = gc);
        }
    }

    /// <summary>
    /// Load EastAsianWidth.txt to extract East_Asian_Width property
    /// Format: 0000..001F;N  # Cc    [32] <control-0000>..<control-001F>
    /// </summary>
    public void LoadEastAsianWidth(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string eawPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || eawPart.Length == 0)
                continue;

            EastAsianWidth eaw = ParseEastAsianWidth(eawPart);
            ParseRangeAndApply(codeRangePart, cp => props[cp].eastAsianWidth = eaw);
        }
    }

    /// <summary>
    /// Load GraphemeBreakProperty.txt to extract Grapheme_Cluster_Break property
    /// Format: 0600..0605    ; Prepend # Cf   [6] ARABIC NUMBER SIGN..ARABIC NUMBER MARK ABOVE
    /// </summary>
    public void LoadGraphemeBreakProperty(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string gcbPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || gcbPart.Length == 0)
                continue;

            GraphemeClusterBreak gcb = ParseGraphemeClusterBreak(gcbPart);
            ParseRangeAndApply(codeRangePart, cp => props[cp].graphemeClusterBreak = gcb);
        }
    }

    /// <summary>
    /// Load Indic_Conjunct_Break (InCB) from DerivedCoreProperties.txt.
    /// Format: 094D          ; InCB; Linker # Mn       DEVANAGARI SIGN VIRAMA
    /// Format: 0915..0939    ; InCB; Consonant # Lo  [37] ...
    /// </summary>
    public void LoadIndicConjunctBreak(string path)
    {
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Looking for lines with InCB property
            if (!line.Contains("InCB"))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 3)
                continue;

            string codeRangePart = semi[0].Trim();
            string propertyName = semi[1].Trim();
            string valuePart = semi[2].Trim();

            if (propertyName != "InCB")
                continue;

            if (codeRangePart.Length == 0 || valuePart.Length == 0)
                continue;

            IndicConjunctBreak incb = ParseIndicConjunctBreak(valuePart);
            if (incb != IndicConjunctBreak.None)
            {
                ParseRangeAndApply(codeRangePart, cp => props[cp].indicConjunctBreak = incb);
            }
        }
    }

    /// <summary>
    /// Load Script_Extensions from ScriptExtensions.txt.
    /// Format: 0640          ; Adlm Arab Mand Mani Ougr Phlp Rohg Sogd Syrc # Lm ARABIC TATWEEL
    /// Format: 064B..0655    ; Arab Syrc # Mn [11] ARABIC FATHATAN..ARABIC HAMZA BELOW
    /// </summary>
    public void LoadScriptExtensions(string path)
    {
        scriptExtensions.Clear();
        
        using var reader = new StreamReader(path);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // Strip comment
            int hashIdx = line.IndexOf('#');
            if (hashIdx >= 0)
                line = line.Substring(0, hashIdx);
            
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] semi = line.Split(';');
            if (semi.Length < 2)
                continue;

            string codeRangePart = semi[0].Trim();
            string scriptsPart = semi[1].Trim();

            if (codeRangePart.Length == 0 || scriptsPart.Length == 0)
                continue;

            // Parse code range
            int start, end;
            if (codeRangePart.Contains(".."))
            {
                string[] rangeParts = codeRangePart.Split(new[] { ".." }, StringSplitOptions.None);
                start = int.Parse(rangeParts[0], NumberStyles.HexNumber);
                end = int.Parse(rangeParts[1], NumberStyles.HexNumber);
            }
            else
            {
                start = end = int.Parse(codeRangePart, NumberStyles.HexNumber);
            }

            // Parse scripts list
            string[] scriptNames = scriptsPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var scripts = new List<UnicodeScript>();
            
            foreach (var name in scriptNames)
            {
                var script = ParseScriptShortName(name);
                if (script != UnicodeScript.Unknown)
                    scripts.Add(script);
            }

            if (scripts.Count > 0)
            {
                scriptExtensions.Add(new ScriptExtensionEntry(start, end, scripts.ToArray()));
            }
        }
        
        // Sort by start code point for binary search
        scriptExtensions.Sort((a, b) => a.startCodePoint.CompareTo(b.startCodePoint));
    }

    /// <summary>
    /// Get the loaded script extensions entries.
    /// </summary>
    public IReadOnlyList<ScriptExtensionEntry> GetScriptExtensionEntries() => scriptExtensions;

    private static IndicConjunctBreak ParseIndicConjunctBreak(string value)
    {
        return value switch
        {
            "Linker" => IndicConjunctBreak.Linker,
            "Consonant" => IndicConjunctBreak.Consonant,
            "Extend" => IndicConjunctBreak.Extend,
            "None" => IndicConjunctBreak.None,
            _ => IndicConjunctBreak.None
        };
    }

    private static GeneralCategory ParseGeneralCategory(string value)
    {
        return value switch
        {
            "Lu" => GeneralCategory.Lu,
            "Ll" => GeneralCategory.Ll,
            "Lt" => GeneralCategory.Lt,
            "Lm" => GeneralCategory.Lm,
            "Lo" => GeneralCategory.Lo,
            "Mn" => GeneralCategory.Mn,
            "Mc" => GeneralCategory.Mc,
            "Me" => GeneralCategory.Me,
            "Nd" => GeneralCategory.Nd,
            "Nl" => GeneralCategory.Nl,
            "No" => GeneralCategory.No,
            "Pc" => GeneralCategory.Pc,
            "Pd" => GeneralCategory.Pd,
            "Ps" => GeneralCategory.Ps,
            "Pe" => GeneralCategory.Pe,
            "Pi" => GeneralCategory.Pi,
            "Pf" => GeneralCategory.Pf,
            "Po" => GeneralCategory.Po,
            "Sm" => GeneralCategory.Sm,
            "Sc" => GeneralCategory.Sc,
            "Sk" => GeneralCategory.Sk,
            "So" => GeneralCategory.So,
            "Zs" => GeneralCategory.Zs,
            "Zl" => GeneralCategory.Zl,
            "Zp" => GeneralCategory.Zp,
            "Cc" => GeneralCategory.Cc,
            "Cf" => GeneralCategory.Cf,
            "Cs" => GeneralCategory.Cs,
            "Co" => GeneralCategory.Co,
            "Cn" => GeneralCategory.Cn,
            _ => GeneralCategory.Cn
        };
    }

    private static EastAsianWidth ParseEastAsianWidth(string value)
    {
        return value switch
        {
            "N" => EastAsianWidth.N,
            "A" => EastAsianWidth.A,
            "H" => EastAsianWidth.H,
            "W" => EastAsianWidth.W,
            "F" => EastAsianWidth.F,
            "Na" => EastAsianWidth.Na,
            _ => EastAsianWidth.N
        };
    }

    private static GraphemeClusterBreak ParseGraphemeClusterBreak(string value)
    {
        return value switch
        {
            "CR" => GraphemeClusterBreak.CR,
            "LF" => GraphemeClusterBreak.LF,
            "Control" => GraphemeClusterBreak.Control,
            "Extend" => GraphemeClusterBreak.Extend,
            "ZWJ" => GraphemeClusterBreak.ZWJ,
            "Regional_Indicator" => GraphemeClusterBreak.Regional_Indicator,
            "Prepend" => GraphemeClusterBreak.Prepend,
            "SpacingMark" => GraphemeClusterBreak.SpacingMark,
            "L" => GraphemeClusterBreak.L,
            "V" => GraphemeClusterBreak.V,
            "T" => GraphemeClusterBreak.T,
            "LV" => GraphemeClusterBreak.LV,
            "LVT" => GraphemeClusterBreak.LVT,
            _ => GraphemeClusterBreak.Other
        };
    }

    private void ParseRangeAndApply(string codeRangePart, Action<int> apply)
    {
        int rangeStart, rangeEnd;

        int dotsIndex = codeRangePart.IndexOf("..", StringComparison.Ordinal);
        if (dotsIndex >= 0)
        {
            string startHex = codeRangePart.Substring(0, dotsIndex);
            string endHex = codeRangePart.Substring(dotsIndex + 2);

            rangeStart = ParseHexCodePoint(startHex);
            rangeEnd = ParseHexCodePoint(endHex);
        }
        else
        {
            rangeStart = ParseHexCodePoint(codeRangePart);
            rangeEnd = rangeStart;
        }

        if (rangeStart < 0 || rangeEnd < 0 || rangeStart > rangeEnd)
            return;

        if (rangeEnd > MaxCodePoint)
            rangeEnd = MaxCodePoint;

        for (int cp = rangeStart; cp <= rangeEnd; cp++)
        {
            apply(cp);
        }
    }

    static string StripComment(string line)
    {
        int hashIndex = line.IndexOf('#');
        if (hashIndex >= 0)
            line = line.Substring(0, hashIndex);
        return line.Trim();
    }

    static int ParseHexCodePoint(string hex)
    {
        if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
            return value;
        return -1;
    }

    static BidiClass ParseBidiClass(string value)
    {
        return value switch
        {
            "L" => BidiClass.LeftToRight,
            "R" => BidiClass.RightToLeft,
            "AL" => BidiClass.ArabicLetter,
            "EN" => BidiClass.EuropeanNumber,
            "ES" => BidiClass.EuropeanSeparator,
            "ET" => BidiClass.EuropeanTerminator,
            "AN" => BidiClass.ArabicNumber,
            "CS" => BidiClass.CommonSeparator,
            "NSM" => BidiClass.NonspacingMark,
            "BN" => BidiClass.BoundaryNeutral,
            "B" => BidiClass.ParagraphSeparator,
            "S" => BidiClass.SegmentSeparator,
            "WS" => BidiClass.WhiteSpace,
            "ON" => BidiClass.OtherNeutral,
            "LRE" => BidiClass.LeftToRightEmbedding,
            "LRO" => BidiClass.LeftToRightOverride,
            "RLE" => BidiClass.RightToLeftEmbedding,
            "RLO" => BidiClass.RightToLeftOverride,
            "PDF" => BidiClass.PopDirectionalFormat,
            "LRI" => BidiClass.LeftToRightIsolate,
            "RLI" => BidiClass.RightToLeftIsolate,
            "FSI" => BidiClass.FirstStrongIsolate,
            "PDI" => BidiClass.PopDirectionalIsolate,
            _ => throw new InvalidDataException($"Unknown Bidi_Class value '{value}'.")
        };
    }

    static JoiningType ParseJoiningType(string value)
    {
        return value switch
        {
            "U" => JoiningType.NonJoining,
            "T" => JoiningType.Transparent,
            "C" => JoiningType.JoinCausing,
            "L" => JoiningType.LeftJoining,
            "R" => JoiningType.RightJoining,
            "D" => JoiningType.DualJoining,
            _ => JoiningType.NonJoining
        };
    }

    static JoiningGroup ParseJoiningGroup(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return JoiningGroup.NoJoiningGroup;

        // Convert to PascalCase
        string[] parts = value.Trim().Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new System.Text.StringBuilder();

        foreach (string part in parts)
        {
            if (part.Length == 0) continue;
            sb.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
                sb.Append(part.Substring(1).ToLowerInvariant());
        }

        string enumName = sb.ToString();

        if (Enum.TryParse<JoiningGroup>(enumName, out var result))
            return result;

        return JoiningGroup.NoJoiningGroup;
    }

    static UnicodeScript ParseScript(string value)
    {
        // Remove underscores: "Old_Italic" -> "OldItalic"
        string enumName = value.Trim().Replace("_", "");

        if (Enum.TryParse<UnicodeScript>(enumName, ignoreCase: true, out var result))
            return result;

        return UnicodeScript.Unknown;
    }

    /// <summary>
    /// Parse 4-letter script short name (from ScriptExtensions.txt) to UnicodeScript enum.
    /// </summary>
    static UnicodeScript ParseScriptShortName(string shortName)
    {
        return shortName.Trim() switch
        {
            "Adlm" => UnicodeScript.Adlam,
            "Aghb" => UnicodeScript.CaucasianAlbanian,
            "Ahom" => UnicodeScript.Ahom,
            "Arab" => UnicodeScript.Arabic,
            "Armi" => UnicodeScript.ImperialAramaic,
            "Armn" => UnicodeScript.Armenian,
            "Avst" => UnicodeScript.Avestan,
            "Bali" => UnicodeScript.Balinese,
            "Bamu" => UnicodeScript.Bamum,
            "Bass" => UnicodeScript.BassaVah,
            "Batk" => UnicodeScript.Batak,
            "Beng" => UnicodeScript.Bengali,
            "Bhks" => UnicodeScript.Bhaiksuki,
            "Bopo" => UnicodeScript.Bopomofo,
            "Brah" => UnicodeScript.Brahmi,
            "Brai" => UnicodeScript.Braille,
            "Bugi" => UnicodeScript.Buginese,
            "Buhd" => UnicodeScript.Buhid,
            "Cakm" => UnicodeScript.Chakma,
            "Cans" => UnicodeScript.CanadianAboriginal,
            "Cari" => UnicodeScript.Carian,
            "Cham" => UnicodeScript.Cham,
            "Cher" => UnicodeScript.Cherokee,
            "Chrs" => UnicodeScript.Chorasmian,
            "Copt" => UnicodeScript.Coptic,
            "Cpmn" => UnicodeScript.CyproMinoan,
            "Cprt" => UnicodeScript.Cypriot,
            "Cyrl" => UnicodeScript.Cyrillic,
            "Deva" => UnicodeScript.Devanagari,
            "Diak" => UnicodeScript.DivesAkuru,
            "Dogr" => UnicodeScript.Dogra,
            "Dsrt" => UnicodeScript.Deseret,
            "Dupl" => UnicodeScript.Duployan,
            "Egyp" => UnicodeScript.EgyptianHieroglyphs,
            "Elba" => UnicodeScript.Elbasan,
            "Elym" => UnicodeScript.Elymaic,
            "Ethi" => UnicodeScript.Ethiopic,
            "Gara" => UnicodeScript.Garay,
            "Geor" => UnicodeScript.Georgian,
            "Glag" => UnicodeScript.Glagolitic,
            "Gong" => UnicodeScript.GunjalaGondi,
            "Gonm" => UnicodeScript.MasaramGondi,
            "Goth" => UnicodeScript.Gothic,
            "Gran" => UnicodeScript.Grantha,
            "Grek" => UnicodeScript.Greek,
            "Gujr" => UnicodeScript.Gujarati,
            "Gukh" => UnicodeScript.GurungKhema,
            "Guru" => UnicodeScript.Gurmukhi,
            "Hang" => UnicodeScript.Hangul,
            "Hani" => UnicodeScript.Han,
            "Hano" => UnicodeScript.Hanunoo,
            "Hatr" => UnicodeScript.Hatran,
            "Hebr" => UnicodeScript.Hebrew,
            "Hira" => UnicodeScript.Hiragana,
            "Hluw" => UnicodeScript.AnatolianHieroglyphs,
            "Hmng" => UnicodeScript.PahawhHmong,
            "Hmnp" => UnicodeScript.NyiakengPuachueHmong,
            "Hung" => UnicodeScript.OldHungarian,
            "Ital" => UnicodeScript.OldItalic,
            "Java" => UnicodeScript.Javanese,
            "Kali" => UnicodeScript.KayahLi,
            "Kana" => UnicodeScript.Katakana,
            "Kawi" => UnicodeScript.Kawi,
            "Khar" => UnicodeScript.Kharoshthi,
            "Khmr" => UnicodeScript.Khmer,
            "Khoj" => UnicodeScript.Khojki,
            "Kits" => UnicodeScript.KhitanSmallScript,
            "Knda" => UnicodeScript.Kannada,
            "Krai" => UnicodeScript.KiratRai,
            "Kthi" => UnicodeScript.Kaithi,
            "Lana" => UnicodeScript.TaiTham,
            "Laoo" => UnicodeScript.Lao,
            "Latn" => UnicodeScript.Latin,
            "Lepc" => UnicodeScript.Lepcha,
            "Limb" => UnicodeScript.Limbu,
            "Lina" => UnicodeScript.LinearA,
            "Linb" => UnicodeScript.LinearB,
            "Lisu" => UnicodeScript.Lisu,
            "Lyci" => UnicodeScript.Lycian,
            "Lydi" => UnicodeScript.Lydian,
            "Mahj" => UnicodeScript.Mahajani,
            "Maka" => UnicodeScript.Makasar,
            "Mand" => UnicodeScript.Mandaic,
            "Mani" => UnicodeScript.Manichaean,
            "Marc" => UnicodeScript.Marchen,
            "Medf" => UnicodeScript.Medefaidrin,
            "Mend" => UnicodeScript.MendeKikakui,
            "Merc" => UnicodeScript.MeroiticCursive,
            "Mero" => UnicodeScript.MeroiticHieroglyphs,
            "Mlym" => UnicodeScript.Malayalam,
            "Modi" => UnicodeScript.Modi,
            "Mong" => UnicodeScript.Mongolian,
            "Mroo" => UnicodeScript.Mro,
            "Mtei" => UnicodeScript.MeeteiMayek,
            "Mult" => UnicodeScript.Multani,
            "Mymr" => UnicodeScript.Myanmar,
            "Nagm" => UnicodeScript.NagMundari,
            "Nand" => UnicodeScript.Nandinagari,
            "Narb" => UnicodeScript.OldNorthArabian,
            "Nbat" => UnicodeScript.Nabataean,
            "Newa" => UnicodeScript.Newa,
            "Nkoo" => UnicodeScript.Nko,
            "Nshu" => UnicodeScript.Nushu,
            "Ogam" => UnicodeScript.Ogham,
            "Olck" => UnicodeScript.OlChiki,
            "Onao" => UnicodeScript.OlOnal,
            "Orkh" => UnicodeScript.OldTurkic,
            "Orya" => UnicodeScript.Oriya,
            "Osge" => UnicodeScript.Osage,
            "Osma" => UnicodeScript.Osmanya,
            "Ougr" => UnicodeScript.OldUyghur,
            "Palm" => UnicodeScript.Palmyrene,
            "Pauc" => UnicodeScript.PauCinHau,
            "Perm" => UnicodeScript.OldPermic,
            "Phag" => UnicodeScript.PhagsPa,
            "Phli" => UnicodeScript.InscriptionalPahlavi,
            "Phlp" => UnicodeScript.PsalterPahlavi,
            "Phnx" => UnicodeScript.Phoenician,
            "Plrd" => UnicodeScript.Miao,
            "Prti" => UnicodeScript.InscriptionalParthian,
            "Rjng" => UnicodeScript.Rejang,
            "Rohg" => UnicodeScript.HanifiRohingya,
            "Runr" => UnicodeScript.Runic,
            "Samr" => UnicodeScript.Samaritan,
            "Sarb" => UnicodeScript.OldSouthArabian,
            "Saur" => UnicodeScript.Saurashtra,
            "Sgnw" => UnicodeScript.SignWriting,
            "Shaw" => UnicodeScript.Shavian,
            "Shrd" => UnicodeScript.Sharada,
            "Sidd" => UnicodeScript.Siddham,
            "Sind" => UnicodeScript.Khudawadi,
            "Sinh" => UnicodeScript.Sinhala,
            "Sogd" => UnicodeScript.Sogdian,
            "Sogo" => UnicodeScript.OldSogdian,
            "Sora" => UnicodeScript.SoraSompeng,
            "Soyo" => UnicodeScript.Soyombo,
            "Sund" => UnicodeScript.Sundanese,
            "Sunu" => UnicodeScript.Sunuwar,
            "Sylo" => UnicodeScript.SylotiNagri,
            "Syrc" => UnicodeScript.Syriac,
            "Tagb" => UnicodeScript.Tagbanwa,
            "Takr" => UnicodeScript.Takri,
            "Tale" => UnicodeScript.TaiLe,
            "Talu" => UnicodeScript.NewTaiLue,
            "Taml" => UnicodeScript.Tamil,
            "Tang" => UnicodeScript.Tangut,
            "Tavt" => UnicodeScript.TaiViet,
            "Telu" => UnicodeScript.Telugu,
            "Tfng" => UnicodeScript.Tifinagh,
            "Tglg" => UnicodeScript.Tagalog,
            "Thaa" => UnicodeScript.Thaana,
            "Thai" => UnicodeScript.Thai,
            "Tibt" => UnicodeScript.Tibetan,
            "Tirh" => UnicodeScript.Tirhuta,
            "Tnsa" => UnicodeScript.Tangsa,
            "Todr" => UnicodeScript.Todhri,
            "Toto" => UnicodeScript.Toto,
            "Tutg" => UnicodeScript.TuluTigalari,
            "Ugar" => UnicodeScript.Ugaritic,
            "Vaii" => UnicodeScript.Vai,
            "Vith" => UnicodeScript.Vithkuqi,
            "Wara" => UnicodeScript.WarangCiti,
            "Wcho" => UnicodeScript.Wancho,
            "Xpeo" => UnicodeScript.OldPersian,
            "Xsux" => UnicodeScript.Cuneiform,
            "Yezi" => UnicodeScript.Yezidi,
            "Yiii" => UnicodeScript.Yi,
            "Zanb" => UnicodeScript.ZanabazarSquare,
            "Zinh" => UnicodeScript.Inherited,
            "Zyyy" => UnicodeScript.Common,
            "Zzzz" => UnicodeScript.Unknown,
            _ => UnicodeScript.Unknown
        };
    }

    static LineBreakClass ParseLineBreakClass(string value)
    {
        if (Enum.TryParse<LineBreakClass>(value.Trim(), out var result))
            return result;

        return LineBreakClass.XX;
    }

    public List<RangeEntry> BuildRangeEntries()
    {
        var result = new List<RangeEntry>();

        int currentStart = 0;
        var current = props[0];

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var p = props[cp];

            bool same =
                p.bidiClass == current.bidiClass &&
                p.joiningType == current.joiningType &&
                p.joiningGroup == current.joiningGroup;

            if (!same)
            {
                result.Add(new RangeEntry(
                    startCodePoint: currentStart,
                    endCodePoint: cp - 1,
                    bidiClass: current.bidiClass,
                    joiningType: current.joiningType,
                    joiningGroup: current.joiningGroup));

                currentStart = cp;
                current = p;
            }
        }

        result.Add(new RangeEntry(
            startCodePoint: currentStart,
            endCodePoint: MaxCodePoint,
            bidiClass: current.bidiClass,
            joiningType: current.joiningType,
            joiningGroup: current.joiningGroup));

        return result;
    }

    public List<ScriptRangeEntry> BuildScriptRangeEntries()
    {
        var result = new List<ScriptRangeEntry>();

        int currentStart = 0;
        var currentScript = props[0].script;

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var script = props[cp].script;

            if (script != currentScript)
            {
                result.Add(new ScriptRangeEntry(currentStart, cp - 1, currentScript));
                currentStart = cp;
                currentScript = script;
            }
        }

        result.Add(new ScriptRangeEntry(currentStart, MaxCodePoint, currentScript));

        return result;
    }

    public List<LineBreakRangeEntry> BuildLineBreakRangeEntries()
    {
        var result = new List<LineBreakRangeEntry>();

        int currentStart = 0;
        var currentLbc = props[0].lineBreakClass;

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var lbc = props[cp].lineBreakClass;

            if (lbc != currentLbc)
            {
                result.Add(new LineBreakRangeEntry(currentStart, cp - 1, currentLbc));
                currentStart = cp;
                currentLbc = lbc;
            }
        }

        result.Add(new LineBreakRangeEntry(currentStart, MaxCodePoint, currentLbc));

        return result;
    }

    /// <summary>
    /// Build range entries for Extended_Pictographic property.
    /// Only includes ranges where Extended_Pictographic=true.
    /// </summary>
    public List<ExtendedPictographicRangeEntry> BuildExtendedPictographicRangeEntries()
    {
        var result = new List<ExtendedPictographicRangeEntry>();

        int currentStart = -1;
        bool inRange = false;

        for (int cp = 0; cp <= MaxCodePoint; cp++)
        {
            bool ep = props[cp].extendedPictographic;

            if (ep && !inRange)
            {
                // Start a new range
                currentStart = cp;
                inRange = true;
            }
            else if (!ep && inRange)
            {
                // End current range
                result.Add(new ExtendedPictographicRangeEntry(currentStart, cp - 1));
                inRange = false;
            }
        }

        // Don't forget the last range if it extends to MaxCodePoint
        if (inRange)
        {
            result.Add(new ExtendedPictographicRangeEntry(currentStart, MaxCodePoint));
        }

        return result;
    }

    /// <summary>
    /// Build range entries for General_Category property.
    /// </summary>
    public List<GeneralCategoryRangeEntry> BuildGeneralCategoryRangeEntries()
    {
        var result = new List<GeneralCategoryRangeEntry>();

        int currentStart = 0;
        var currentGc = props[0].generalCategory;

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var gc = props[cp].generalCategory;

            if (gc != currentGc)
            {
                result.Add(new GeneralCategoryRangeEntry(currentStart, cp - 1, currentGc));
                currentStart = cp;
                currentGc = gc;
            }
        }

        result.Add(new GeneralCategoryRangeEntry(currentStart, MaxCodePoint, currentGc));

        return result;
    }

    /// <summary>
    /// Build range entries for East_Asian_Width property.
    /// </summary>
    public List<EastAsianWidthRangeEntry> BuildEastAsianWidthRangeEntries()
    {
        var result = new List<EastAsianWidthRangeEntry>();

        int currentStart = 0;
        var currentEaw = props[0].eastAsianWidth;

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var eaw = props[cp].eastAsianWidth;

            if (eaw != currentEaw)
            {
                result.Add(new EastAsianWidthRangeEntry(currentStart, cp - 1, currentEaw));
                currentStart = cp;
                currentEaw = eaw;
            }
        }

        result.Add(new EastAsianWidthRangeEntry(currentStart, MaxCodePoint, currentEaw));

        return result;
    }

    /// <summary>
    /// Build range entries for Grapheme_Cluster_Break property.
    /// </summary>
    public List<GraphemeBreakRangeEntry> BuildGraphemeBreakRangeEntries()
    {
        var result = new List<GraphemeBreakRangeEntry>();

        int currentStart = 0;
        var currentGcb = props[0].graphemeClusterBreak;

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var gcb = props[cp].graphemeClusterBreak;

            if (gcb != currentGcb)
            {
                // Only add non-Other entries to save space
                if (currentGcb != GraphemeClusterBreak.Other)
                    result.Add(new GraphemeBreakRangeEntry(currentStart, cp - 1, currentGcb));
                currentStart = cp;
                currentGcb = gcb;
            }
        }

        // Add final range if not Other
        if (currentGcb != GraphemeClusterBreak.Other)
            result.Add(new GraphemeBreakRangeEntry(currentStart, MaxCodePoint, currentGcb));

        return result;
    }

    /// <summary>
    /// Build range entries for Indic_Conjunct_Break property.
    /// </summary>
    public List<IndicConjunctBreakRangeEntry> BuildIndicConjunctBreakRangeEntries()
    {
        var result = new List<IndicConjunctBreakRangeEntry>();

        int currentStart = 0;
        var currentIncb = props[0].indicConjunctBreak;

        for (int cp = 1; cp <= MaxCodePoint; cp++)
        {
            var incb = props[cp].indicConjunctBreak;

            if (incb != currentIncb)
            {
                // Only add non-None entries to save space
                if (currentIncb != IndicConjunctBreak.None)
                    result.Add(new IndicConjunctBreakRangeEntry(currentStart, cp - 1, currentIncb));
                currentStart = cp;
                currentIncb = incb;
            }
        }

        // Add final range if not None
        if (currentIncb != IndicConjunctBreak.None)
            result.Add(new IndicConjunctBreakRangeEntry(currentStart, MaxCodePoint, currentIncb));

        return result;
    }

    public static List<MirrorEntry> BuildMirrorEntries(string bidiMirroringPath)
    {
        if (string.IsNullOrEmpty(bidiMirroringPath))
            throw new ArgumentNullException(nameof(bidiMirroringPath));

        var result = new List<MirrorEntry>();

        using var reader = new StreamReader(bidiMirroringPath);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length < 2)
                continue;

            string codePart = parts[0].Trim();
            string mirrorPart = parts[1].Trim();

            if (codePart.Length == 0 || mirrorPart.Length == 0)
                continue;

            int codePoint = ParseHexCodePoint(codePart);
            int mirrored = ParseHexCodePoint(mirrorPart);

            if (codePoint < 0 || codePoint > MaxCodePoint)
                continue;
            if (mirrored < 0 || mirrored > MaxCodePoint)
                continue;

            result.Add(new MirrorEntry(codePoint, mirrored));
        }

        result.Sort((a, b) => a.codePoint.CompareTo(b.codePoint));

        return result;
    }

    public static List<BracketEntry> BuildBracketEntries(string bidiBracketsPath)
    {
        if (string.IsNullOrEmpty(bidiBracketsPath))
            throw new ArgumentNullException(nameof(bidiBracketsPath));

        var result = new List<BracketEntry>();

        using var reader = new StreamReader(bidiBracketsPath);

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = StripComment(line);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length < 3)
                continue;

            string codePart = parts[0].Trim();
            string pairedPart = parts[1].Trim();
            string typePart = parts[2].Trim();

            if (codePart.Length == 0 || typePart.Length == 0)
                continue;

            int codePoint = ParseHexCodePoint(codePart);
            if (codePoint < 0 || codePoint > MaxCodePoint)
                continue;

            int pairedCodePoint;

            if (pairedPart.Length == 0 ||
                string.Equals(pairedPart, "<none>", StringComparison.OrdinalIgnoreCase))
            {
                pairedCodePoint = codePoint;
            }
            else
            {
                pairedCodePoint = ParseHexCodePoint(pairedPart);
                if (pairedCodePoint < 0 || pairedCodePoint > MaxCodePoint)
                    continue;
            }

            BidiPairedBracketType bracketType = typePart.ToUpperInvariant() switch
            {
                "O" => BidiPairedBracketType.Open,
                "C" => BidiPairedBracketType.Close,
                "N" => BidiPairedBracketType.None,
                _ => throw new InvalidDataException($"Unknown Bidi_Paired_Bracket_Type '{typePart}'.")
            };

            result.Add(new BracketEntry(codePoint, pairedCodePoint, bracketType));
        }

        result.Sort((a, b) => a.codePoint.CompareTo(b.codePoint));

        return result;
    }
}

#endregion

#region Writer бинарного формата

public static class UnicodeBinaryWriter
{
    const uint Magic = 0x554C5452; // "ULTR"
    const ushort FormatVersion5 = 5;
    const ushort FormatVersion6 = 6;
    const ushort FormatVersion7 = 7;

    /// <summary>
    /// Write format version 7 (with Script, LineBreak, Extended_Pictographic, GeneralCategory, EastAsianWidth, GraphemeBreak, InCB, ScriptExtensions)
    /// </summary>
    public static void WriteBinary(
        string outputPath,
        IReadOnlyList<RangeEntry> ranges,
        IReadOnlyList<MirrorEntry> mirrors,
        IReadOnlyList<BracketEntry> brackets,
        IReadOnlyList<ScriptRangeEntry> scripts,
        IReadOnlyList<LineBreakRangeEntry> lineBreaks,
        IReadOnlyList<ExtendedPictographicRangeEntry> extendedPictographics,
        IReadOnlyList<GeneralCategoryRangeEntry> generalCategories,
        IReadOnlyList<EastAsianWidthRangeEntry> eastAsianWidths,
        IReadOnlyList<GraphemeBreakRangeEntry> graphemeBreaks,
        IReadOnlyList<IndicConjunctBreakRangeEntry> indicConjunctBreaks,
        IReadOnlyList<ScriptExtensionEntry> scriptExtensions,
        int unicodeVersionRaw)
    {
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream);

        // Header placeholder
        long headerPosition = stream.Position;

        writer.Write(Magic);
        writer.Write(FormatVersion7);
        writer.Write((ushort)0); // Reserved
        writer.Write((uint)unicodeVersionRaw);

        // Section offsets placeholder (12 sections * 8 bytes)
        for (int i = 0; i < 24; i++)
            writer.Write((uint)0);

        // Write Range section
        long rangeOffset = stream.Position;
        writer.Write((uint)ranges.Count);
        foreach (var r in ranges)
        {
            writer.Write((uint)r.startCodePoint);
            writer.Write((uint)r.endCodePoint);
            writer.Write((byte)r.bidiClass);
            writer.Write((byte)r.joiningType);
            writer.Write((byte)r.joiningGroup);
            writer.Write((byte)0); // padding
        }
        uint rangeLength = (uint)(stream.Position - rangeOffset);

        // Write Mirror section
        long mirrorOffset = stream.Position;
        writer.Write((uint)mirrors.Count);
        foreach (var m in mirrors)
        {
            writer.Write((uint)m.codePoint);
            writer.Write((uint)m.mirroredCodePoint);
        }
        uint mirrorLength = (uint)(stream.Position - mirrorOffset);

        // Write Bracket section
        long bracketOffset = stream.Position;
        writer.Write((uint)brackets.Count);
        foreach (var b in brackets)
        {
            writer.Write((uint)b.codePoint);
            writer.Write((uint)b.pairedCodePoint);
            writer.Write((byte)b.bracketType);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint bracketLength = (uint)(stream.Position - bracketOffset);

        // Write Script section
        long scriptOffset = stream.Position;
        writer.Write((uint)scripts.Count);
        foreach (var s in scripts)
        {
            writer.Write((uint)s.startCodePoint);
            writer.Write((uint)s.endCodePoint);
            writer.Write((byte)s.script);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint scriptLength = (uint)(stream.Position - scriptOffset);

        // Write LineBreak section
        long lineBreakOffset = stream.Position;
        writer.Write((uint)lineBreaks.Count);
        foreach (var lb in lineBreaks)
        {
            writer.Write((uint)lb.startCodePoint);
            writer.Write((uint)lb.endCodePoint);
            writer.Write((byte)lb.lineBreakClass);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint lineBreakLength = (uint)(stream.Position - lineBreakOffset);

        // Write Extended_Pictographic section
        long extPictOffset = stream.Position;
        writer.Write((uint)extendedPictographics.Count);
        foreach (var ep in extendedPictographics)
        {
            writer.Write((uint)ep.startCodePoint);
            writer.Write((uint)ep.endCodePoint);
        }
        uint extPictLength = (uint)(stream.Position - extPictOffset);

        // Write GeneralCategory section
        long gcOffset = stream.Position;
        writer.Write((uint)generalCategories.Count);
        foreach (var gc in generalCategories)
        {
            writer.Write((uint)gc.startCodePoint);
            writer.Write((uint)gc.endCodePoint);
            writer.Write((byte)gc.generalCategory);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint gcLength = (uint)(stream.Position - gcOffset);

        // Write EastAsianWidth section
        long eawOffset = stream.Position;
        writer.Write((uint)eastAsianWidths.Count);
        foreach (var eaw in eastAsianWidths)
        {
            writer.Write((uint)eaw.startCodePoint);
            writer.Write((uint)eaw.endCodePoint);
            writer.Write((byte)eaw.eastAsianWidth);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint eawLength = (uint)(stream.Position - eawOffset);

        // Write Grapheme_Cluster_Break section
        long gcbOffset = stream.Position;
        writer.Write((uint)graphemeBreaks.Count);
        foreach (var gcb in graphemeBreaks)
        {
            writer.Write((uint)gcb.startCodePoint);
            writer.Write((uint)gcb.endCodePoint);
            writer.Write((byte)gcb.graphemeBreak);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint gcbLength = (uint)(stream.Position - gcbOffset);

        // Write Indic_Conjunct_Break section
        long incbOffset = stream.Position;
        writer.Write((uint)indicConjunctBreaks.Count);
        foreach (var incb in indicConjunctBreaks)
        {
            writer.Write((uint)incb.startCodePoint);
            writer.Write((uint)incb.endCodePoint);
            writer.Write((byte)incb.indicConjunctBreak);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint incbLength = (uint)(stream.Position - incbOffset);

        // Write Script_Extensions section
        long scxOffset = stream.Position;
        writer.Write((uint)scriptExtensions.Count);
        foreach (var scx in scriptExtensions)
        {
            writer.Write((uint)scx.startCodePoint);
            writer.Write((uint)scx.endCodePoint);
            writer.Write((byte)scx.scripts.Length);
            foreach (var script in scx.scripts)
            {
                writer.Write((byte)script);
            }
            // Padding to align to 4 bytes
            int totalBytes = 8 + 1 + scx.scripts.Length; // start + end + count + scripts
            int padding = (4 - (totalBytes % 4)) % 4;
            for (int p = 0; p < padding; p++)
                writer.Write((byte)0);
        }
        uint scxLength = (uint)(stream.Position - scxOffset);

        // Go back and write header with offsets
        stream.Position = headerPosition;

        writer.Write(Magic);
        writer.Write(FormatVersion7);
        writer.Write((ushort)0);
        writer.Write((uint)unicodeVersionRaw);

        writer.Write((uint)rangeOffset);
        writer.Write(rangeLength);
        writer.Write((uint)mirrorOffset);
        writer.Write(mirrorLength);
        writer.Write((uint)bracketOffset);
        writer.Write(bracketLength);
        writer.Write((uint)scriptOffset);
        writer.Write(scriptLength);
        writer.Write((uint)lineBreakOffset);
        writer.Write(lineBreakLength);
        writer.Write((uint)extPictOffset);
        writer.Write(extPictLength);
        writer.Write((uint)gcOffset);
        writer.Write(gcLength);
        writer.Write((uint)eawOffset);
        writer.Write(eawLength);
        writer.Write((uint)gcbOffset);
        writer.Write(gcbLength);
        writer.Write((uint)incbOffset);
        writer.Write(incbLength);
        writer.Write((uint)scxOffset);
        writer.Write(scxLength);

        writer.Flush();
    }

    /// <summary>
    /// Write format version 1 (backward compatible, no Script/LineBreak)
    /// </summary>
    public static void WriteBinaryV1(
        string outputPath,
        IReadOnlyList<RangeEntry> ranges,
        IReadOnlyList<MirrorEntry> mirrors,
        IReadOnlyList<BracketEntry> brackets,
        int unicodeVersionRaw)
    {
        using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream);

        const ushort v1 = 1;

        long headerPosition = stream.Position;

        writer.Write(Magic);
        writer.Write(v1);
        writer.Write((ushort)0);
        writer.Write((uint)unicodeVersionRaw);

        // 3 sections
        for (int i = 0; i < 6; i++)
            writer.Write((uint)0);

        long rangeOffset = stream.Position;
        writer.Write((uint)ranges.Count);
        foreach (var r in ranges)
        {
            writer.Write((uint)r.startCodePoint);
            writer.Write((uint)r.endCodePoint);
            writer.Write((byte)r.bidiClass);
            writer.Write((byte)r.joiningType);
            writer.Write((byte)r.joiningGroup);
            writer.Write((byte)0);
        }
        uint rangeLength = (uint)(stream.Position - rangeOffset);

        long mirrorOffset = stream.Position;
        writer.Write((uint)mirrors.Count);
        foreach (var m in mirrors)
        {
            writer.Write((uint)m.codePoint);
            writer.Write((uint)m.mirroredCodePoint);
        }
        uint mirrorLength = (uint)(stream.Position - mirrorOffset);

        long bracketOffset = stream.Position;
        writer.Write((uint)brackets.Count);
        foreach (var b in brackets)
        {
            writer.Write((uint)b.codePoint);
            writer.Write((uint)b.pairedCodePoint);
            writer.Write((byte)b.bracketType);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        uint bracketLength = (uint)(stream.Position - bracketOffset);

        stream.Position = headerPosition;

        writer.Write(Magic);
        writer.Write(v1);
        writer.Write((ushort)0);
        writer.Write((uint)unicodeVersionRaw);

        writer.Write((uint)rangeOffset);
        writer.Write(rangeLength);
        writer.Write((uint)mirrorOffset);
        writer.Write(mirrorLength);
        writer.Write((uint)bracketOffset);
        writer.Write(bracketLength);

        writer.Flush();
    }
}

#endregion