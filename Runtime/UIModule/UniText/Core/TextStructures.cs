using System;


/// <summary>
/// Направление текста
/// </summary>
public enum TextDirection : byte
{
    LeftToRight = 0,
    RightToLeft = 1,
    /// <summary>
    /// Автоматическое определение по первому Strong символу (UAX #9)
    /// </summary>
    Auto = 2
}

// UnicodeScript и LineBreakClass определены в Unicode модуле (UnicodeDataTypes.cs)

/// <summary>
/// Результат shaping для одного run
/// </summary>
public struct ShapedGlyph
{
    public int glyphId; // ID глифа в шрифте
    public int cluster; // Индекс исходного символа
    public float advanceX; // Продвижение по X
    public float advanceY; // Продвижение по Y
    public float offsetX; // Смещение по X
    public float offsetY; // Смещение по Y
}

/// <summary>
/// Диапазон в тексте
/// </summary>
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

    public bool Contains(int index) => index >= start && index < End;
    public bool Overlaps(TextRange other) => start < other.End && End > other.start;

    public bool Equals(TextRange other) => start == other.start && length == other.length;
    public override bool Equals(object obj) => obj is TextRange other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(start, length);

    public static bool operator ==(TextRange left, TextRange right) => left.Equals(right);
    public static bool operator !=(TextRange left, TextRange right) => !left.Equals(right);
}

/// <summary>
/// Run текста после itemization
/// </summary>
public struct TextRun
{
    public TextRange range;
    public byte bidiLevel;
    public UnicodeScript script;
    public int fontId; // ID шрифта (для font fallback)

    public TextDirection Direction => (bidiLevel & 1) == 0
        ? TextDirection.LeftToRight
        : TextDirection.RightToLeft;
}

/// <summary>
/// Run после shaping
/// </summary>
public struct ShapedRun
{
    public TextRange range; // Диапазон в исходном тексте
    public int glyphStart; // Начало глифов в общем массиве
    public int glyphCount; // Количество глифов
    public float width; // Ширина run
    public TextDirection direction;
    public byte bidiLevel; // BiDi level для reordering (UAX #9)
    public int fontId;
}

/// <summary>
/// Строка текста после line breaking
/// </summary>
public struct TextLine
{
    public TextRange range; // Диапазон в исходном тексте
    public int runStart; // Начало runs в массиве
    public int runCount; // Количество runs
    public float width; // Ширина строки
    public float height; // Высота строки
    public float baseline; // Позиция baseline
    public byte paragraphBaseLevel; // Base level параграфа (0=LTR, 1=RTL) для alignment
}

/// <summary>
/// Финальная позиция глифа для рендеринга.
/// Содержит только базовые данные. Атрибуты (color, stylePadding и т.д.)
/// хранятся в отдельных буферах SharedTextBuffers или внутри модификаторов.
/// </summary>
public struct PositionedGlyph
{
    public int glyphId;
    public int cluster; // Индекс исходного символа в clean text (для атрибутов)
    public float x;
    public float y;
    public int fontId;
}

/// <summary>
/// Горизонтальное выравнивание текста
/// </summary>
public enum HorizontalAlignment : byte
{
    Left = 0,
    Center = 1,
    Right = 2
}

/// <summary>
/// Вертикальное выравнивание текста
/// </summary>
public enum VerticalAlignment : byte
{
    Top = 0,
    Middle = 1,
    Bottom = 2
}