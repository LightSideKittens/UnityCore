using System;

/// <summary>
/// Результат парсинга — диапазон с привязанным модификатором.
/// Индексы указывают на clean text (после удаления тегов).
/// </summary>
internal readonly struct AttributeSpan : IEquatable<AttributeSpan>
{
    /// <summary>
    /// Начальный индекс в clean text
    /// </summary>
    public readonly int start;

    /// <summary>
    /// Конечный индекс (не включительно)
    /// </summary>
    public readonly int end;

    /// <summary>
    /// Модификатор, применяемый к этому диапазону
    /// </summary>
    public readonly IModifier modifier;

    /// <summary>
    /// Параметр из тега (например, "#FF0000")
    /// </summary>
    public readonly string parameter;

    public int Length => end - start;

    public AttributeSpan(int start, int end, IModifier modifier, string parameter = null)
    {
        this.start = start;
        this.end = end;
        this.modifier = modifier;
        this.parameter = parameter;
    }

    public bool Contains(int index) => index >= start && index < end;
    public bool Overlaps(AttributeSpan other) => start < other.end && end > other.start;

    public bool Equals(AttributeSpan other) =>
        start == other.start && end == other.end && ReferenceEquals(modifier, other.modifier);

    public override bool Equals(object obj) => obj is AttributeSpan other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(start, end, modifier);

    public static bool operator ==(AttributeSpan left, AttributeSpan right) => left.Equals(right);
    public static bool operator !=(AttributeSpan left, AttributeSpan right) => !left.Equals(right);

    public override string ToString() => $"[{start}-{end}] {modifier?.GetType().Name ?? "null"}";
}