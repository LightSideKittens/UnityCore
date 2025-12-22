public interface IParseRule
{
    int TryMatch(string text, int index, PooledList<ParsedRange> results);
    void Finalize(string text, PooledList<ParsedRange> results) {}
    void Reset() {}
}