using System;


public enum TextDirection : byte
{
    LeftToRight = 0,
    RightToLeft = 1,

    Auto = 2
}


public struct ShapedGlyph
{
    public int glyphId;
    public int cluster;
    public float advanceX;
    public float advanceY;
    public float offsetX;
    public float offsetY;
}


public readonly struct TextRange : IEquatable<TextRange>
{
    public readonly int start;
    public readonly int length;

    public int End => start + length;

    public TextRange(int start, int length)
    {
        this.start = start;
        this.length = length;
    }

    public bool Contains(int index)
    {
        return index >= start && index < End;
    }

    public bool Overlaps(TextRange other)
    {
        return start < other.End && End > other.start;
    }

    public bool Equals(TextRange other)
    {
        return start == other.start && length == other.length;
    }

    public override bool Equals(object obj)
    {
        return obj is TextRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(start, length);
    }

    public static bool operator ==(TextRange left, TextRange right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextRange left, TextRange right)
    {
        return !left.Equals(right);
    }
}


public struct TextRun
{
    public TextRange range;
    public byte bidiLevel;
    public UnicodeScript script;
    public int fontId;

    public TextDirection Direction => (bidiLevel & 1) == 0
        ? TextDirection.LeftToRight
        : TextDirection.RightToLeft;
}


public struct ShapedRun
{
    public TextRange range;
    public int glyphStart;
    public int glyphCount;
    public float width;
    public TextDirection direction;
    public byte bidiLevel;
    public int fontId;
}


public struct TextLine
{
    public TextRange range;
    public int runStart;
    public int runCount;
    public float width;
    public float height;
    public float baseline;
    public byte paragraphBaseLevel;
    public float startMargin;
}


public struct PositionedGlyph
{
    public int glyphId;
    public int cluster;
    public float x;
    public float y;
    public int fontId;
    public int shapedGlyphIndex;

    public float left;
    public float top;
    public float right;
    public float bottom;
}


public struct CachedGlyphData
{
    public int rectX;
    public int rectY;
    public int rectWidth;
    public int rectHeight;
    public float bearingX;
    public float bearingY;
    public float width;
    public float height;
    public bool isValid;
}


public enum HorizontalAlignment : byte
{
    Left = 0,
    Center = 1,
    Right = 2
}


public enum VerticalAlignment : byte
{
    Top = 0,
    Middle = 1,
    Bottom = 2
}