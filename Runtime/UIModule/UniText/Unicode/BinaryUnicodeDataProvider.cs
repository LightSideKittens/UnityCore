using System;
using System.IO;


public readonly struct RangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly BidiClass bidiClass;
    public readonly JoiningType joiningType;
    public readonly JoiningGroup joiningGroup;

    public RangeEntry(
        int startCodePoint,
        int endCodePoint,
        BidiClass bidiClass,
        JoiningType joiningType,
        JoiningGroup joiningGroup)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.bidiClass = bidiClass;
        this.joiningType = joiningType;
        this.joiningGroup = joiningGroup;
    }

    public bool Contains(int codePoint)
    {
        return codePoint >= startCodePoint && codePoint <= endCodePoint;
    }
}


public readonly struct MirrorEntry
{
    public readonly int codePoint;
    public readonly int mirroredCodePoint;

    public MirrorEntry(int codePoint, int mirroredCodePoint)
    {
        this.codePoint = codePoint;
        this.mirroredCodePoint = mirroredCodePoint;
    }
}


public readonly struct BracketEntry
{
    public readonly int codePoint;
    public readonly int pairedCodePoint;
    public readonly BidiPairedBracketType bracketType;

    public BracketEntry(int codePoint, int pairedCodePoint, BidiPairedBracketType bracketType)
    {
        this.codePoint = codePoint;
        this.pairedCodePoint = pairedCodePoint;
        this.bracketType = bracketType;
    }
}


public readonly struct ScriptRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly UnicodeScript script;

    public ScriptRangeEntry(int startCodePoint, int endCodePoint, UnicodeScript script)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.script = script;
    }
}


public readonly struct LineBreakRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly LineBreakClass lineBreakClass;

    public LineBreakRangeEntry(int startCodePoint, int endCodePoint, LineBreakClass lineBreakClass)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.lineBreakClass = lineBreakClass;
    }
}


/// <summary>
/// Range entry for Extended_Pictographic property (from emoji-data.txt)
/// </summary>
public readonly struct ExtendedPictographicRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;

    public ExtendedPictographicRangeEntry(int startCodePoint, int endCodePoint)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
    }
}


/// <summary>
/// Range entry for General_Category property (from DerivedGeneralCategory.txt)
/// </summary>
public readonly struct GeneralCategoryRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly GeneralCategory generalCategory;

    public GeneralCategoryRangeEntry(int startCodePoint, int endCodePoint, GeneralCategory generalCategory)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.generalCategory = generalCategory;
    }
}


/// <summary>
/// Range entry for East_Asian_Width property (from EastAsianWidth.txt)
/// </summary>
public readonly struct EastAsianWidthRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly EastAsianWidth eastAsianWidth;

    public EastAsianWidthRangeEntry(int startCodePoint, int endCodePoint, EastAsianWidth eastAsianWidth)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.eastAsianWidth = eastAsianWidth;
    }
}


/// <summary>
/// Range entry for Grapheme_Cluster_Break property (from GraphemeBreakProperty.txt)
/// </summary>
public readonly struct GraphemeBreakRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly GraphemeClusterBreak graphemeBreak;

    public GraphemeBreakRangeEntry(int startCodePoint, int endCodePoint, GraphemeClusterBreak graphemeBreak)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.graphemeBreak = graphemeBreak;
    }
}

/// <summary>
/// Range entry for Indic_Conjunct_Break property (from DerivedCoreProperties.txt)
/// </summary>
public readonly struct IndicConjunctBreakRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly IndicConjunctBreak indicConjunctBreak;

    public IndicConjunctBreakRangeEntry(int startCodePoint, int endCodePoint, IndicConjunctBreak indicConjunctBreak)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.indicConjunctBreak = indicConjunctBreak;
    }
}

/// <summary>
/// Range entry for Script_Extensions property (from ScriptExtensions.txt)
/// </summary>
public readonly struct ScriptExtensionRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;
    public readonly UnicodeScript[] scripts;

    public ScriptExtensionRangeEntry(int startCodePoint, int endCodePoint, UnicodeScript[] scripts)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
        this.scripts = scripts;
    }
}


public sealed class BinaryUnicodeDataProvider : IUnicodeDataProvider
{
    private const uint Magic = 0x554C5452; // "ULTR"
    private const ushort FormatVersion1 = 1;
    private const ushort FormatVersion2 = 2;
    private const ushort FormatVersion3 = 3;
    private const ushort FormatVersion4 = 4;
    private const ushort FormatVersion5 = 5;
    private const ushort FormatVersion6 = 6;
    private const ushort FormatVersion7 = 7;
    
    private const int BmpSize = 65536; // 0x0000–0xFFFF

    private readonly RangeEntry[] ranges;
    private readonly MirrorEntry[] mirrors;
    private readonly BracketEntry[] brackets;
    private readonly ScriptRangeEntry[] scriptRanges;
    private readonly LineBreakRangeEntry[] lineBreakRanges;
    private readonly ExtendedPictographicRangeEntry[] extendedPictographicRanges;
    private readonly GeneralCategoryRangeEntry[] generalCategoryRanges;
    private readonly EastAsianWidthRangeEntry[] eastAsianWidthRanges;
    private readonly GraphemeBreakRangeEntry[] graphemeBreakRanges;
    private readonly IndicConjunctBreakRangeEntry[] indicConjunctBreakRanges;
    private readonly ScriptExtensionRangeEntry[] scriptExtensionRanges;
    
    // BMP direct lookup tables (O(1) for code points 0x0000–0xFFFF)
    private readonly BidiClass[] bmpBidiClass;
    private readonly JoiningType[] bmpJoiningType;
    private readonly UnicodeScript[] bmpScript;
    private readonly LineBreakClass[] bmpLineBreak;
    private readonly GeneralCategory[] bmpGeneralCategory;
    private readonly EastAsianWidth[] bmpEastAsianWidth;
    private readonly GraphemeClusterBreak[] bmpGraphemeBreak;
    private readonly IndicConjunctBreak[] bmpIndicConjunctBreak;

    public int UnicodeVersionRaw { get; }
    public ushort FormatVersion { get; }

    public BinaryUnicodeDataProvider(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        using var stream = new MemoryStream(data, writable: false);
        using var reader = new BinaryReader(stream);

        uint fileMagic = reader.ReadUInt32();
        if (fileMagic != Magic)
            throw new InvalidDataException("Invalid Unicode data blob: magic mismatch.");

        FormatVersion = reader.ReadUInt16();
        if (FormatVersion != FormatVersion1 && FormatVersion != FormatVersion2 && 
            FormatVersion != FormatVersion3 && FormatVersion != FormatVersion4 &&
            FormatVersion != FormatVersion5 && FormatVersion != FormatVersion6 &&
            FormatVersion != FormatVersion7)
            throw new InvalidDataException($"Unsupported Unicode data format version: {FormatVersion}.");

        reader.ReadUInt16(); // Reserved

        uint unicodeVersion = reader.ReadUInt32();
        UnicodeVersionRaw = unchecked((int)unicodeVersion);

        // Read section offsets (format v1)
        uint rangeOffset = reader.ReadUInt32();
        uint rangeLength = reader.ReadUInt32();
        uint mirrorOffset = reader.ReadUInt32();
        uint mirrorLength = reader.ReadUInt32();
        uint bracketOffset = reader.ReadUInt32();
        uint bracketLength = reader.ReadUInt32();

        // Format v2 adds Script and LineBreak sections
        uint scriptOffset = 0, scriptLength = 0;
        uint lineBreakOffset = 0, lineBreakLength = 0;
        
        if (FormatVersion >= FormatVersion2)
        {
            scriptOffset = reader.ReadUInt32();
            scriptLength = reader.ReadUInt32();
            lineBreakOffset = reader.ReadUInt32();
            lineBreakLength = reader.ReadUInt32();
        }

        // Format v3 adds Extended_Pictographic section
        uint extPictOffset = 0, extPictLength = 0;
        
        if (FormatVersion >= FormatVersion3)
        {
            extPictOffset = reader.ReadUInt32();
            extPictLength = reader.ReadUInt32();
        }

        // Format v4 adds GeneralCategory and EastAsianWidth sections
        uint gcOffset = 0, gcLength = 0;
        uint eawOffset = 0, eawLength = 0;
        
        if (FormatVersion >= FormatVersion4)
        {
            gcOffset = reader.ReadUInt32();
            gcLength = reader.ReadUInt32();
            eawOffset = reader.ReadUInt32();
            eawLength = reader.ReadUInt32();
        }

        // Format v5 adds Grapheme_Cluster_Break section
        uint gcbOffset = 0, gcbLength = 0;
        
        if (FormatVersion >= FormatVersion5)
        {
            gcbOffset = reader.ReadUInt32();
            gcbLength = reader.ReadUInt32();
        }

        // Format v6 adds Indic_Conjunct_Break section
        uint incbOffset = 0, incbLength = 0;
        
        if (FormatVersion >= FormatVersion6)
        {
            incbOffset = reader.ReadUInt32();
            incbLength = reader.ReadUInt32();
        }

        // Format v7 adds Script_Extensions section
        uint scxOffset = 0, scxLength = 0;
        
        if (FormatVersion >= FormatVersion7)
        {
            scxOffset = reader.ReadUInt32();
            scxLength = reader.ReadUInt32();
        }

        // Read Range section
        if (rangeOffset == 0 || rangeLength == 0)
            throw new InvalidDataException("Unicode data blob is missing Range section.");

        stream.Position = rangeOffset;
        uint rangeCount = reader.ReadUInt32();
        ranges = new RangeEntry[rangeCount];

        for (uint i = 0; i < rangeCount; i++)
        {
            uint start = reader.ReadUInt32();
            uint end = reader.ReadUInt32();
            byte bidi = reader.ReadByte();
            byte jt = reader.ReadByte();
            byte jg = reader.ReadByte();
            reader.ReadByte(); // padding

            ranges[i] = new RangeEntry(
                startCodePoint: unchecked((int)start),
                endCodePoint: unchecked((int)end),
                bidiClass: (BidiClass)bidi,
                joiningType: (JoiningType)jt,
                joiningGroup: (JoiningGroup)jg);
        }

        // Read Mirror section
        if (mirrorOffset != 0 && mirrorLength != 0)
        {
            stream.Position = mirrorOffset;
            uint mirrorCount = reader.ReadUInt32();
            mirrors = new MirrorEntry[mirrorCount];

            for (uint i = 0; i < mirrorCount; i++)
            {
                uint cp = reader.ReadUInt32();
                uint mirrored = reader.ReadUInt32();

                mirrors[i] = new MirrorEntry(
                    codePoint: unchecked((int)cp),
                    mirroredCodePoint: unchecked((int)mirrored));
            }
        }
        else
        {
            mirrors = Array.Empty<MirrorEntry>();
        }

        // Read Bracket section
        if (bracketOffset != 0 && bracketLength != 0)
        {
            stream.Position = bracketOffset;
            uint bracketCount = reader.ReadUInt32();
            brackets = new BracketEntry[bracketCount];

            for (uint i = 0; i < bracketCount; i++)
            {
                uint cp = reader.ReadUInt32();
                uint paired = reader.ReadUInt32();
                byte bpt = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                brackets[i] = new BracketEntry(
                    codePoint: unchecked((int)cp),
                    pairedCodePoint: unchecked((int)paired),
                    bracketType: (BidiPairedBracketType)bpt);
            }
        }
        else
        {
            brackets = Array.Empty<BracketEntry>();
        }

        // Read Script section (format v2)
        if (scriptOffset != 0 && scriptLength != 0)
        {
            stream.Position = scriptOffset;
            uint scriptCount = reader.ReadUInt32();
            scriptRanges = new ScriptRangeEntry[scriptCount];

            for (uint i = 0; i < scriptCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte script = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                scriptRanges[i] = new ScriptRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    script: (UnicodeScript)script);
            }
        }
        else
        {
            scriptRanges = Array.Empty<ScriptRangeEntry>();
        }

        // Read LineBreak section (format v2)
        if (lineBreakOffset != 0 && lineBreakLength != 0)
        {
            stream.Position = lineBreakOffset;
            uint lineBreakCount = reader.ReadUInt32();
            lineBreakRanges = new LineBreakRangeEntry[lineBreakCount];

            for (uint i = 0; i < lineBreakCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte lbc = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                lineBreakRanges[i] = new LineBreakRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    lineBreakClass: (LineBreakClass)lbc);
            }
        }
        else
        {
            lineBreakRanges = Array.Empty<LineBreakRangeEntry>();
        }

        // Read Extended_Pictographic section (format v3)
        if (extPictOffset != 0 && extPictLength != 0)
        {
            stream.Position = extPictOffset;
            uint extPictCount = reader.ReadUInt32();
            extendedPictographicRanges = new ExtendedPictographicRangeEntry[extPictCount];

            for (uint i = 0; i < extPictCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();

                extendedPictographicRanges[i] = new ExtendedPictographicRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end));
            }
        }
        else
        {
            extendedPictographicRanges = Array.Empty<ExtendedPictographicRangeEntry>();
        }

        // Read GeneralCategory section (format v4)
        if (gcOffset != 0 && gcLength != 0)
        {
            stream.Position = gcOffset;
            uint gcCount = reader.ReadUInt32();
            generalCategoryRanges = new GeneralCategoryRangeEntry[gcCount];

            for (uint i = 0; i < gcCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte gc = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                generalCategoryRanges[i] = new GeneralCategoryRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    generalCategory: (GeneralCategory)gc);
            }
        }
        else
        {
            generalCategoryRanges = Array.Empty<GeneralCategoryRangeEntry>();
        }

        // Read EastAsianWidth section (format v4)
        if (eawOffset != 0 && eawLength != 0)
        {
            stream.Position = eawOffset;
            uint eawCount = reader.ReadUInt32();
            eastAsianWidthRanges = new EastAsianWidthRangeEntry[eawCount];

            for (uint i = 0; i < eawCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte eaw = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                eastAsianWidthRanges[i] = new EastAsianWidthRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    eastAsianWidth: (EastAsianWidth)eaw);
            }
        }
        else
        {
            eastAsianWidthRanges = Array.Empty<EastAsianWidthRangeEntry>();
        }

        // Read Grapheme_Cluster_Break section (format v5)
        if (gcbOffset != 0 && gcbLength != 0)
        {
            stream.Position = gcbOffset;
            uint gcbCount = reader.ReadUInt32();
            graphemeBreakRanges = new GraphemeBreakRangeEntry[gcbCount];

            for (uint i = 0; i < gcbCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte gcb = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                graphemeBreakRanges[i] = new GraphemeBreakRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    graphemeBreak: (GraphemeClusterBreak)gcb);
            }
        }
        else
        {
            graphemeBreakRanges = Array.Empty<GraphemeBreakRangeEntry>();
        }

        // Read Indic_Conjunct_Break section (format v6)
        if (incbOffset != 0 && incbLength != 0)
        {
            stream.Position = incbOffset;
            uint incbCount = reader.ReadUInt32();
            indicConjunctBreakRanges = new IndicConjunctBreakRangeEntry[incbCount];

            for (uint i = 0; i < incbCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte incb = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                indicConjunctBreakRanges[i] = new IndicConjunctBreakRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    indicConjunctBreak: (IndicConjunctBreak)incb);
            }
        }
        else
        {
            indicConjunctBreakRanges = Array.Empty<IndicConjunctBreakRangeEntry>();
        }

        // Read Script_Extensions section (format v7)
        if (scxOffset != 0 && scxLength != 0)
        {
            stream.Position = scxOffset;
            uint scxCount = reader.ReadUInt32();
            scriptExtensionRanges = new ScriptExtensionRangeEntry[scxCount];

            for (uint i = 0; i < scxCount; i++)
            {
                uint start = reader.ReadUInt32();
                uint end = reader.ReadUInt32();
                byte scriptCount = reader.ReadByte();
                
                var scripts = new UnicodeScript[scriptCount];
                for (int j = 0; j < scriptCount; j++)
                {
                    scripts[j] = (UnicodeScript)reader.ReadByte();
                }
                
                // Read padding to align to 4 bytes
                int totalBytes = 8 + 1 + scriptCount; // start + end + count + scripts
                int padding = (4 - (totalBytes % 4)) % 4;
                for (int p = 0; p < padding; p++)
                    reader.ReadByte();

                scriptExtensionRanges[i] = new ScriptExtensionRangeEntry(
                    startCodePoint: unchecked((int)start),
                    endCodePoint: unchecked((int)end),
                    scripts: scripts);
            }
        }
        else
        {
            scriptExtensionRanges = Array.Empty<ScriptExtensionRangeEntry>();
        }
        
        // Initialize BMP lookup tables for O(1) access
        bmpBidiClass = new BidiClass[BmpSize];
        bmpJoiningType = new JoiningType[BmpSize];
        bmpScript = new UnicodeScript[BmpSize];
        bmpLineBreak = new LineBreakClass[BmpSize];
        bmpGeneralCategory = new GeneralCategory[BmpSize];
        bmpEastAsianWidth = new EastAsianWidth[BmpSize];
        bmpGraphemeBreak = new GraphemeClusterBreak[BmpSize];
        bmpIndicConjunctBreak = new IndicConjunctBreak[BmpSize];
        
        InitializeBmpTables();
    }
    
    private void InitializeBmpTables()
    {
        // Fill BidiClass and JoiningType from ranges
        foreach (var range in ranges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpBidiClass[cp] = range.bidiClass;
                bmpJoiningType[cp] = range.joiningType;
            }
        }
        
        // Fill Script
        foreach (var range in scriptRanges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpScript[cp] = range.script;
            }
        }
        
        // Fill LineBreak
        foreach (var range in lineBreakRanges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpLineBreak[cp] = range.lineBreakClass;
            }
        }
        
        // Fill GeneralCategory
        foreach (var range in generalCategoryRanges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpGeneralCategory[cp] = range.generalCategory;
            }
        }
        
        // Fill EastAsianWidth
        foreach (var range in eastAsianWidthRanges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpEastAsianWidth[cp] = range.eastAsianWidth;
            }
        }
        
        // Fill GraphemeBreak
        foreach (var range in graphemeBreakRanges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpGraphemeBreak[cp] = range.graphemeBreak;
            }
        }
        
        // Fill IndicConjunctBreak
        foreach (var range in indicConjunctBreakRanges)
        {
            int start = Math.Max(0, range.startCodePoint);
            int end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (int cp = start; cp <= end; cp++)
            {
                bmpIndicConjunctBreak[cp] = range.indicConjunctBreak;
            }
        }
    }

    public BidiClass GetBidiClass(int codePoint)
    {
        // Fast path for BMP (99%+ of real text)
        if ((uint)codePoint < BmpSize)
            return bmpBidiClass[codePoint];
            
        var entry = FindRange(codePoint);
        return entry?.bidiClass ?? BidiClass.LeftToRight;
    }

    public bool IsBidiMirrored(int codePoint)
    {
        return FindMirror(codePoint) != null;
    }

    public int GetBidiMirroringGlyph(int codePoint)
    {
        var mirror = FindMirror(codePoint);
        return mirror?.mirroredCodePoint ?? codePoint;
    }

    public BidiPairedBracketType GetBidiPairedBracketType(int codePoint)
    {
        var bracket = FindBracket(codePoint);
        return bracket?.bracketType ?? BidiPairedBracketType.None;
    }

    public int GetBidiPairedBracket(int codePoint)
    {
        var bracket = FindBracket(codePoint);
        return bracket?.pairedCodePoint ?? codePoint;
    }

    public JoiningType GetJoiningType(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpJoiningType[codePoint];
            
        var entry = FindRange(codePoint);
        return entry?.joiningType ?? JoiningType.NonJoining;
    }

    public JoiningGroup GetJoiningGroup(int codePoint)
    {
        var entry = FindRange(codePoint);
        return entry?.joiningGroup ?? JoiningGroup.NoJoiningGroup;
    }

    public UnicodeScript GetScript(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpScript[codePoint];
            
        var entry = FindScriptRange(codePoint);
        return entry?.script ?? UnicodeScript.Unknown;
    }

    public LineBreakClass GetLineBreakClass(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpLineBreak[codePoint];
            
        var entry = FindLineBreakRange(codePoint);
        return entry?.lineBreakClass ?? LineBreakClass.XX; // XX = Unknown
    }

    public bool IsExtendedPictographic(int codePoint)
    {
        return FindExtendedPictographicRange(codePoint) != null;
    }

    public GeneralCategory GetGeneralCategory(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpGeneralCategory[codePoint];
            
        var entry = FindGeneralCategoryRange(codePoint);
        return entry?.generalCategory ?? GeneralCategory.Cn; // Cn = Not assigned
    }

    public EastAsianWidth GetEastAsianWidth(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpEastAsianWidth[codePoint];
            
        var entry = FindEastAsianWidthRange(codePoint);
        return entry?.eastAsianWidth ?? EastAsianWidth.N; // N = Neutral
    }

    /// <summary>
    /// Check if codepoint is an Unambiguous Hyphen (HH class per UAX #14).
    /// These characters provide break opportunity after, except word-initially.
    /// 
    /// Per UAX #14 Table 1 (Unicode 17.0.0), HH class includes:
    /// ARMENIAN HYPHEN, HEBREW MAQAF, CANADIAN SYLLABICS HYPHEN,
    /// HYPHEN, FIGURE DASH, EN DASH, DOUBLE OBLIQUE HYPHEN, DOUBLE HYPHEN,
    /// OBLIQUE HYPHEN, GARAY HYPHEN, YEZIDI HYPHENATION MARK.
    /// 
    /// All these codepoints are correctly marked as HH in LineBreak.txt (Unicode 17.0.0).
    /// See: https://www.unicode.org/Public/17.0.0/ucd/LineBreak.txt
    /// </summary>
    public bool IsUnambiguousHyphen(int codePoint)
    {
        return GetLineBreakClass(codePoint) == LineBreakClass.HH;
    }
    
    /// <summary>
    /// U+25CC DOTTED CIRCLE - placeholder character for displaying combining marks in isolation.
    /// This is a stable Unicode codepoint that will not change.
    /// Used for LB28a Brahmic script rules per UAX #14.
    /// </summary>
    private const int DottedCircle = 0x25CC;

    /// <summary>
    /// Check if codepoint is U+25CC DOTTED CIRCLE.
    /// This is a placeholder character used to display combining marks in isolation.
    /// </summary>
    public bool IsDottedCircle(int codePoint)
    {
        return codePoint == DottedCircle;
    }
    
    /// <summary>
    /// Check if codepoint belongs to a Brahmic script for LB28a rules.
    /// Per UAX #14, $Brahmic = [\p{sc=Bali}\p{sc=Batk}\p{sc=Bugi}\p{sc=Java}\p{sc=Kali}\p{sc=Maka}
    ///                         \p{sc=Mand}\p{sc=Modi}\p{sc=Nag}\p{sc=Sund}\p{sc=Tale}\p{sc=Talu}
    ///                         \p{sc=Takr}\p{sc=Tibt}]
    /// </summary>
    public bool IsBrahmicForLB28a(int codePoint)
    {
        var script = GetScript(codePoint);
        return script == UnicodeScript.Balinese ||
               script == UnicodeScript.Batak ||
               script == UnicodeScript.Buginese ||
               script == UnicodeScript.Javanese ||
               script == UnicodeScript.KayahLi ||
               script == UnicodeScript.Makasar ||
               script == UnicodeScript.Mandaic ||
               script == UnicodeScript.Modi ||
               script == UnicodeScript.Nandinagari ||
               script == UnicodeScript.Sundanese ||
               script == UnicodeScript.TaiLe ||
               script == UnicodeScript.NewTaiLue ||
               script == UnicodeScript.Takri ||
               script == UnicodeScript.Tibetan;
    }

    public GraphemeClusterBreak GetGraphemeClusterBreak(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpGraphemeBreak[codePoint];
            
        var entry = FindGraphemeBreakRange(codePoint);
        return entry?.graphemeBreak ?? GraphemeClusterBreak.Other;
    }

    public IndicConjunctBreak GetIndicConjunctBreak(int codePoint)
    {
        // Fast path for BMP
        if ((uint)codePoint < BmpSize)
            return bmpIndicConjunctBreak[codePoint];
            
        var entry = FindIndicConjunctBreakRange(codePoint);
        return entry?.indicConjunctBreak ?? IndicConjunctBreak.None;
    }

    public UnicodeScript[] GetScriptExtensions(int codePoint)
    {
        var entry = FindScriptExtensionRange(codePoint);
        if (entry != null)
            return entry.Value.scripts;
        
        // Default: return array with single Script value
        var script = GetScript(codePoint);
        return new[] { script };
    }

    public bool HasScriptExtension(int codePoint, UnicodeScript script)
    {
        var entry = FindScriptExtensionRange(codePoint);
        if (entry != null)
        {
            foreach (var s in entry.Value.scripts)
            {
                if (s == script)
                    return true;
            }
            return false;
        }
        
        // Default: check against single Script value
        return GetScript(codePoint) == script;
    }

    private RangeEntry? FindRange(int codePoint)
    {
        int lo = 0;
        int hi = ranges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = ranges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private MirrorEntry? FindMirror(int codePoint)
    {
        int lo = 0;
        int hi = mirrors.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = mirrors[mid];

            if (codePoint < entry.codePoint)
                hi = mid - 1;
            else if (codePoint > entry.codePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private BracketEntry? FindBracket(int codePoint)
    {
        int lo = 0;
        int hi = brackets.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = brackets[mid];

            if (codePoint < entry.codePoint)
                hi = mid - 1;
            else if (codePoint > entry.codePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private ScriptRangeEntry? FindScriptRange(int codePoint)
    {
        int lo = 0;
        int hi = scriptRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = scriptRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private LineBreakRangeEntry? FindLineBreakRange(int codePoint)
    {
        int lo = 0;
        int hi = lineBreakRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = lineBreakRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private ExtendedPictographicRangeEntry? FindExtendedPictographicRange(int codePoint)
    {
        int lo = 0;
        int hi = extendedPictographicRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = extendedPictographicRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private GeneralCategoryRangeEntry? FindGeneralCategoryRange(int codePoint)
    {
        int lo = 0;
        int hi = generalCategoryRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = generalCategoryRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private EastAsianWidthRangeEntry? FindEastAsianWidthRange(int codePoint)
    {
        int lo = 0;
        int hi = eastAsianWidthRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = eastAsianWidthRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private GraphemeBreakRangeEntry? FindGraphemeBreakRange(int codePoint)
    {
        int lo = 0;
        int hi = graphemeBreakRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = graphemeBreakRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private IndicConjunctBreakRangeEntry? FindIndicConjunctBreakRange(int codePoint)
    {
        int lo = 0;
        int hi = indicConjunctBreakRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = indicConjunctBreakRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private ScriptExtensionRangeEntry? FindScriptExtensionRange(int codePoint)
    {
        int lo = 0;
        int hi = scriptExtensionRanges.Length - 1;

        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            var entry = scriptExtensionRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }
}