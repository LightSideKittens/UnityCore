/// <summary>
/// Найденный диапазон с информацией о тегах для удаления
/// </summary>
public struct ParsedRange
{
    /// <summary>
    /// Начало контента в исходном тексте (после открывающего тега)
    /// </summary>
    public int start;

    /// <summary>
    /// Конец контента в исходном тексте (перед закрывающим тегом)
    /// </summary>
    public int end;

    /// <summary>
    /// Начало открывающего тега (для удаления)
    /// </summary>
    public int tagStart;

    /// <summary>
    /// Конец открывающего тега (не включительно)
    /// </summary>
    public int tagEnd;

    /// <summary>
    /// Начало закрывающего тега (для удаления)
    /// </summary>
    public int closeTagStart;

    /// <summary>
    /// Конец закрывающего тега (не включительно)
    /// </summary>
    public int closeTagEnd;

    /// <summary>
    /// Параметр из тега (например, "#FF0000" из color=#FF0000)
    /// </summary>
    public string parameter;

    /// <summary>
    /// Есть ли теги для удаления
    /// </summary>
    public bool HasTags => tagStart >= 0;

    public ParsedRange(int start, int end, string parameter)
    {
        this.start = start;
        this.end = end;
        tagStart = -1;
        tagEnd = -1;
        closeTagStart = -1;
        closeTagEnd = -1;
        this.parameter = parameter;
    }

    public ParsedRange(int tagStart, int tagEnd, int closeTagStart, int closeTagEnd, string parameter = null)
    {
        this.tagStart = tagStart;
        this.tagEnd = tagEnd;
        this.closeTagStart = closeTagStart;
        this.closeTagEnd = closeTagEnd;
        this.parameter = parameter;

        // Контент между тегами
        start = tagEnd;
        end = closeTagStart;
    }
}