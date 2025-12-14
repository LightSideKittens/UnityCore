/// <summary>
/// Найденный диапазон с информацией о тегах для удаления
/// </summary>
public struct ParsedRange
{
    public int start;
    public int end;
    public int tagStart;
    public int tagEnd;
    public int closeTagStart;
    public int closeTagEnd;
    public string parameter;
    /// <summary>
    /// Строка для вставки вместо тега (self-closing). null = обычный тег.
    /// </summary>
    public string insertString;

    public bool HasTags => tagStart >= 0;
    public bool IsSelfClosing => insertString != null;

    public ParsedRange(int start, int end, string parameter)
    {
        this.start = start;
        this.end = end;
        tagStart = -1;
        tagEnd = -1;
        closeTagStart = -1;
        closeTagEnd = -1;
        this.parameter = parameter;
        insertString = null;
    }

    public ParsedRange(int tagStart, int tagEnd, int closeTagStart, int closeTagEnd, string parameter = null)
    {
        this.tagStart = tagStart;
        this.tagEnd = tagEnd;
        this.closeTagStart = closeTagStart;
        this.closeTagEnd = closeTagEnd;
        this.parameter = parameter;
        insertString = null;
        start = tagEnd;
        end = closeTagStart;
    }

    public static ParsedRange SelfClosing(int tagStart, int tagEnd, string insertString, string parameter)
    {
        return new ParsedRange
        {
            tagStart = tagStart,
            tagEnd = tagEnd,
            closeTagStart = tagEnd,
            closeTagEnd = tagEnd,
            start = tagStart,
            end = tagStart,
            parameter = parameter,
            insertString = insertString
        };
    }
}