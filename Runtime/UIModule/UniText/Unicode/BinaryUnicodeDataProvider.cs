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


public readonly struct DefaultIgnorableRangeEntry
{
    public readonly int startCodePoint;
    public readonly int endCodePoint;

    public DefaultIgnorableRangeEntry(int startCodePoint, int endCodePoint)
    {
        this.startCodePoint = startCodePoint;
        this.endCodePoint = endCodePoint;
    }
}


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
    private const uint Magic = 0x554C5452;
    private const ushort FormatVersion1 = 1;
    private const ushort FormatVersion2 = 2;
    private const ushort FormatVersion3 = 3;
    private const ushort FormatVersion4 = 4;
    private const ushort FormatVersion5 = 5;
    private const ushort FormatVersion6 = 6;
    private const ushort FormatVersion7 = 7;
    private const ushort FormatVersion8 = 8;

    private const int BmpSize = 65536;

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
    private readonly DefaultIgnorableRangeEntry[] defaultIgnorableRanges;

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

        using var stream = new MemoryStream(data, false);
        using var reader = new BinaryReader(stream);

        var fileMagic = reader.ReadUInt32();
        if (fileMagic != Magic)
            throw new InvalidDataException("Invalid Unicode data blob: magic mismatch.");

        FormatVersion = reader.ReadUInt16();
        if (FormatVersion != FormatVersion1 && FormatVersion != FormatVersion2 &&
            FormatVersion != FormatVersion3 && FormatVersion != FormatVersion4 &&
            FormatVersion != FormatVersion5 && FormatVersion != FormatVersion6 &&
            FormatVersion != FormatVersion7 && FormatVersion != FormatVersion8)
            throw new InvalidDataException($"Unsupported Unicode data format version: {FormatVersion}.");

        reader.ReadUInt16();

        var unicodeVersion = reader.ReadUInt32();
        UnicodeVersionRaw = unchecked((int)unicodeVersion);

        var rangeOffset = reader.ReadUInt32();
        var rangeLength = reader.ReadUInt32();
        var mirrorOffset = reader.ReadUInt32();
        var mirrorLength = reader.ReadUInt32();
        var bracketOffset = reader.ReadUInt32();
        var bracketLength = reader.ReadUInt32();

        uint scriptOffset = 0, scriptLength = 0;
        uint lineBreakOffset = 0, lineBreakLength = 0;

        if (FormatVersion >= FormatVersion2)
        {
            scriptOffset = reader.ReadUInt32();
            scriptLength = reader.ReadUInt32();
            lineBreakOffset = reader.ReadUInt32();
            lineBreakLength = reader.ReadUInt32();
        }

        uint extPictOffset = 0, extPictLength = 0;

        if (FormatVersion >= FormatVersion3)
        {
            extPictOffset = reader.ReadUInt32();
            extPictLength = reader.ReadUInt32();
        }

        uint gcOffset = 0, gcLength = 0;
        uint eawOffset = 0, eawLength = 0;

        if (FormatVersion >= FormatVersion4)
        {
            gcOffset = reader.ReadUInt32();
            gcLength = reader.ReadUInt32();
            eawOffset = reader.ReadUInt32();
            eawLength = reader.ReadUInt32();
        }

        uint gcbOffset = 0, gcbLength = 0;

        if (FormatVersion >= FormatVersion5)
        {
            gcbOffset = reader.ReadUInt32();
            gcbLength = reader.ReadUInt32();
        }

        uint incbOffset = 0, incbLength = 0;

        if (FormatVersion >= FormatVersion6)
        {
            incbOffset = reader.ReadUInt32();
            incbLength = reader.ReadUInt32();
        }

        uint scxOffset = 0, scxLength = 0;

        if (FormatVersion >= FormatVersion7)
        {
            scxOffset = reader.ReadUInt32();
            scxLength = reader.ReadUInt32();
        }

        uint diOffset = 0, diLength = 0;

        if (FormatVersion >= FormatVersion8)
        {
            diOffset = reader.ReadUInt32();
            diLength = reader.ReadUInt32();
        }

        if (rangeOffset == 0 || rangeLength == 0)
            throw new InvalidDataException("Unicode data blob is missing Range section.");

        stream.Position = rangeOffset;
        var rangeCount = reader.ReadUInt32();
        ranges = new RangeEntry[rangeCount];

        for (uint i = 0; i < rangeCount; i++)
        {
            var start = reader.ReadUInt32();
            var end = reader.ReadUInt32();
            var bidi = reader.ReadByte();
            var jt = reader.ReadByte();
            var jg = reader.ReadByte();
            reader.ReadByte();

            ranges[i] = new RangeEntry(
                unchecked((int)start),
                unchecked((int)end),
                (BidiClass)bidi,
                (JoiningType)jt,
                (JoiningGroup)jg);
        }

        if (mirrorOffset != 0 && mirrorLength != 0)
        {
            stream.Position = mirrorOffset;
            var mirrorCount = reader.ReadUInt32();
            mirrors = new MirrorEntry[mirrorCount];

            for (uint i = 0; i < mirrorCount; i++)
            {
                var cp = reader.ReadUInt32();
                var mirrored = reader.ReadUInt32();

                mirrors[i] = new MirrorEntry(
                    unchecked((int)cp),
                    unchecked((int)mirrored));
            }
        }
        else
        {
            mirrors = Array.Empty<MirrorEntry>();
        }

        if (bracketOffset != 0 && bracketLength != 0)
        {
            stream.Position = bracketOffset;
            var bracketCount = reader.ReadUInt32();
            brackets = new BracketEntry[bracketCount];

            for (uint i = 0; i < bracketCount; i++)
            {
                var cp = reader.ReadUInt32();
                var paired = reader.ReadUInt32();
                var bpt = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                brackets[i] = new BracketEntry(
                    unchecked((int)cp),
                    unchecked((int)paired),
                    (BidiPairedBracketType)bpt);
            }
        }
        else
        {
            brackets = Array.Empty<BracketEntry>();
        }

        if (scriptOffset != 0 && scriptLength != 0)
        {
            stream.Position = scriptOffset;
            var scriptCount = reader.ReadUInt32();
            scriptRanges = new ScriptRangeEntry[scriptCount];

            for (uint i = 0; i < scriptCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var script = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                scriptRanges[i] = new ScriptRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    (UnicodeScript)script);
            }
        }
        else
        {
            scriptRanges = Array.Empty<ScriptRangeEntry>();
        }

        if (lineBreakOffset != 0 && lineBreakLength != 0)
        {
            stream.Position = lineBreakOffset;
            var lineBreakCount = reader.ReadUInt32();
            lineBreakRanges = new LineBreakRangeEntry[lineBreakCount];

            for (uint i = 0; i < lineBreakCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var lbc = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                lineBreakRanges[i] = new LineBreakRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    (LineBreakClass)lbc);
            }
        }
        else
        {
            lineBreakRanges = Array.Empty<LineBreakRangeEntry>();
        }

        if (extPictOffset != 0 && extPictLength != 0)
        {
            stream.Position = extPictOffset;
            var extPictCount = reader.ReadUInt32();
            extendedPictographicRanges = new ExtendedPictographicRangeEntry[extPictCount];

            for (uint i = 0; i < extPictCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();

                extendedPictographicRanges[i] = new ExtendedPictographicRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end));
            }
        }
        else
        {
            extendedPictographicRanges = Array.Empty<ExtendedPictographicRangeEntry>();
        }

        if (gcOffset != 0 && gcLength != 0)
        {
            stream.Position = gcOffset;
            var gcCount = reader.ReadUInt32();
            generalCategoryRanges = new GeneralCategoryRangeEntry[gcCount];

            for (uint i = 0; i < gcCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var gc = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                generalCategoryRanges[i] = new GeneralCategoryRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    (GeneralCategory)gc);
            }
        }
        else
        {
            generalCategoryRanges = Array.Empty<GeneralCategoryRangeEntry>();
        }

        if (eawOffset != 0 && eawLength != 0)
        {
            stream.Position = eawOffset;
            var eawCount = reader.ReadUInt32();
            eastAsianWidthRanges = new EastAsianWidthRangeEntry[eawCount];

            for (uint i = 0; i < eawCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var eaw = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                eastAsianWidthRanges[i] = new EastAsianWidthRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    (EastAsianWidth)eaw);
            }
        }
        else
        {
            eastAsianWidthRanges = Array.Empty<EastAsianWidthRangeEntry>();
        }

        if (gcbOffset != 0 && gcbLength != 0)
        {
            stream.Position = gcbOffset;
            var gcbCount = reader.ReadUInt32();
            graphemeBreakRanges = new GraphemeBreakRangeEntry[gcbCount];

            for (uint i = 0; i < gcbCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var gcb = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                graphemeBreakRanges[i] = new GraphemeBreakRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    (GraphemeClusterBreak)gcb);
            }
        }
        else
        {
            graphemeBreakRanges = Array.Empty<GraphemeBreakRangeEntry>();
        }

        if (incbOffset != 0 && incbLength != 0)
        {
            stream.Position = incbOffset;
            var incbCount = reader.ReadUInt32();
            indicConjunctBreakRanges = new IndicConjunctBreakRangeEntry[incbCount];

            for (uint i = 0; i < incbCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var incb = reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadByte();

                indicConjunctBreakRanges[i] = new IndicConjunctBreakRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    (IndicConjunctBreak)incb);
            }
        }
        else
        {
            indicConjunctBreakRanges = Array.Empty<IndicConjunctBreakRangeEntry>();
        }

        if (scxOffset != 0 && scxLength != 0)
        {
            stream.Position = scxOffset;
            var scxCount = reader.ReadUInt32();
            scriptExtensionRanges = new ScriptExtensionRangeEntry[scxCount];

            for (uint i = 0; i < scxCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();
                var scriptCount = reader.ReadByte();

                var scripts = new UnicodeScript[scriptCount];
                for (var j = 0; j < scriptCount; j++) scripts[j] = (UnicodeScript)reader.ReadByte();

                var totalBytes = 8 + 1 + scriptCount;
                var padding = (4 - totalBytes % 4) % 4;
                for (var p = 0; p < padding; p++)
                    reader.ReadByte();

                scriptExtensionRanges[i] = new ScriptExtensionRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end),
                    scripts);
            }
        }
        else
        {
            scriptExtensionRanges = Array.Empty<ScriptExtensionRangeEntry>();
        }

        if (diOffset != 0 && diLength != 0)
        {
            stream.Position = diOffset;
            var diCount = reader.ReadUInt32();
            defaultIgnorableRanges = new DefaultIgnorableRangeEntry[diCount];

            for (uint i = 0; i < diCount; i++)
            {
                var start = reader.ReadUInt32();
                var end = reader.ReadUInt32();

                defaultIgnorableRanges[i] = new DefaultIgnorableRangeEntry(
                    unchecked((int)start),
                    unchecked((int)end));
            }
        }
        else
        {
            defaultIgnorableRanges = Array.Empty<DefaultIgnorableRangeEntry>();
        }

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
        foreach (var range in ranges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++)
            {
                bmpBidiClass[cp] = range.bidiClass;
                bmpJoiningType[cp] = range.joiningType;
            }
        }

        foreach (var range in scriptRanges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++) bmpScript[cp] = range.script;
        }

        foreach (var range in lineBreakRanges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++) bmpLineBreak[cp] = range.lineBreakClass;
        }

        foreach (var range in generalCategoryRanges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++) bmpGeneralCategory[cp] = range.generalCategory;
        }

        foreach (var range in eastAsianWidthRanges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++) bmpEastAsianWidth[cp] = range.eastAsianWidth;
        }

        foreach (var range in graphemeBreakRanges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++) bmpGraphemeBreak[cp] = range.graphemeBreak;
        }

        foreach (var range in indicConjunctBreakRanges)
        {
            var start = Math.Max(0, range.startCodePoint);
            var end = Math.Min(BmpSize - 1, range.endCodePoint);
            for (var cp = start; cp <= end; cp++) bmpIndicConjunctBreak[cp] = range.indicConjunctBreak;
        }
    }

    public BidiClass GetBidiClass(int codePoint)
    {
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
        if ((uint)codePoint < BmpSize)
            return bmpScript[codePoint];

        var entry = FindScriptRange(codePoint);
        return entry?.script ?? UnicodeScript.Unknown;
    }

    public LineBreakClass GetLineBreakClass(int codePoint)
    {
        if ((uint)codePoint < BmpSize)
            return bmpLineBreak[codePoint];

        var entry = FindLineBreakRange(codePoint);
        return entry?.lineBreakClass ?? LineBreakClass.XX;
    }

    public bool IsExtendedPictographic(int codePoint)
    {
        return FindExtendedPictographicRange(codePoint) != null;
    }

    public GeneralCategory GetGeneralCategory(int codePoint)
    {
        if ((uint)codePoint < BmpSize)
            return bmpGeneralCategory[codePoint];

        var entry = FindGeneralCategoryRange(codePoint);
        return entry?.generalCategory ?? GeneralCategory.Cn;
    }

    public EastAsianWidth GetEastAsianWidth(int codePoint)
    {
        if ((uint)codePoint < BmpSize)
            return bmpEastAsianWidth[codePoint];

        var entry = FindEastAsianWidthRange(codePoint);
        return entry?.eastAsianWidth ?? EastAsianWidth.N;
    }


    public bool IsUnambiguousHyphen(int codePoint)
    {
        return GetLineBreakClass(codePoint) == LineBreakClass.HH;
    }


    public bool IsDottedCircle(int codePoint)
    {
        return codePoint == UnicodeData.DottedCircle;
    }


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
        if ((uint)codePoint < BmpSize)
            return bmpGraphemeBreak[codePoint];

        var entry = FindGraphemeBreakRange(codePoint);
        return entry?.graphemeBreak ?? GraphemeClusterBreak.Other;
    }

    public IndicConjunctBreak GetIndicConjunctBreak(int codePoint)
    {
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

        var script = GetScript(codePoint);
        return new[] { script };
    }

    public bool HasScriptExtension(int codePoint, UnicodeScript script)
    {
        var entry = FindScriptExtensionRange(codePoint);
        if (entry != null)
        {
            foreach (var s in entry.Value.scripts)
                if (s == script)
                    return true;
            return false;
        }

        return GetScript(codePoint) == script;
    }


    public bool IsDefaultIgnorable(int codePoint)
    {
        if (defaultIgnorableRanges != null && defaultIgnorableRanges.Length > 0)
            return FindDefaultIgnorableRange(codePoint) != null;

        var lbc = GetLineBreakClass(codePoint);
        if (lbc == LineBreakClass.ZW || lbc == LineBreakClass.ZWJ)
            return true;

        var gc = GetGeneralCategory(codePoint);
        return gc == GeneralCategory.Cf ||
               gc == GeneralCategory.Mn ||
               gc == GeneralCategory.Me;
    }

    private DefaultIgnorableRangeEntry? FindDefaultIgnorableRange(int codePoint)
    {
        var lo = 0;
        var hi = defaultIgnorableRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            var entry = defaultIgnorableRanges[mid];

            if (codePoint < entry.startCodePoint)
                hi = mid - 1;
            else if (codePoint > entry.endCodePoint)
                lo = mid + 1;
            else
                return entry;
        }

        return null;
    }

    private RangeEntry? FindRange(int codePoint)
    {
        var lo = 0;
        var hi = ranges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = mirrors.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = brackets.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = scriptRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = lineBreakRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = extendedPictographicRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = generalCategoryRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = eastAsianWidthRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = graphemeBreakRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = indicConjunctBreakRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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
        var lo = 0;
        var hi = scriptExtensionRanges.Length - 1;

        while (lo <= hi)
        {
            var mid = (lo + hi) >> 1;
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