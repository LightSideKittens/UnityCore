using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CompositeParseRule : IParseRule
{
    [SerializeReference] public List<IParseRule> rules = new();

    public int TryMatch(string text, int index, PooledList<ParsedRange> results)
    {
        for (var i = 0; i < rules.Count; i++)
        {
            var rule = rules[i];
            if (rule == null) continue;

            var result = rule.TryMatch(text, index, results);
            if (result > index) return result;
        }

        return index;
    }

    public void Finalize(string text, PooledList<ParsedRange> results)
    {
        for (var i = 0; i < rules.Count; i++)
            rules[i]?.Finalize(text, results);
    }

    public void Reset()
    {
        for (var i = 0; i < rules.Count; i++)
            rules[i]?.Reset();
    }
}
