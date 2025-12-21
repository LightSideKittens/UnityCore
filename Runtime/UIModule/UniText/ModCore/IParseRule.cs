using System.Collections.Generic;


public interface IParseRule
{
    int TryMatch(string text, int index, PooledList<ParsedRange> results);
    void Finalize(int textLength, PooledList<ParsedRange> results);


    void Reset();
}