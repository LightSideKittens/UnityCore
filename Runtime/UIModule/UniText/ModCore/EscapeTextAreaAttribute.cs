using System;
using UnityEngine;

/// <summary>
/// TextArea attribute with optional escape sequence processing.
/// Supports: \n, \r, \t, \\, \uXXXX (Unicode), \xXX (hex byte)
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class EscapeTextAreaAttribute : PropertyAttribute
{
    public int MinLines { get; }
    public int MaxLines { get; }
    public bool ProcessEscapes { get; }

    public EscapeTextAreaAttribute(bool processEscapes = true)
    {
        MinLines = 3;
        MaxLines = 10;
        ProcessEscapes = processEscapes;
    }

    public EscapeTextAreaAttribute(int minLines, int maxLines, bool processEscapes = true)
    {
        MinLines = minLines;
        MaxLines = maxLines;
        ProcessEscapes = processEscapes;
    }
}
