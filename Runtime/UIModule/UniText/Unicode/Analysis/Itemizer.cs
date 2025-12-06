using System;

/// <summary>
/// Itemizer — разбивает текст на runs.
/// Run создаётся при изменении любого из:
/// - BiDi level
/// - Script
/// - Font
/// </summary>
public sealed class Itemizer
{
    public Itemizer(IUnicodeDataProvider unicodeData)
    {
        // Kept for compatibility, but we use static UnicodeData.Provider
    }

    /// <summary>
    /// Создать Itemizer с использованием статического UnicodeData.
    /// </summary>
    public Itemizer()
    {
    }

    /// <summary>
    /// Itemize текст и записать runs в предоставленные буферы.
    /// </summary>
    public void Itemize(
        ReadOnlySpan<int> codepoints,
        ReadOnlySpan<byte> bidiLevels,
        UnicodeScript[] scripts,
        UniTextFontProvider fontProvider,
        int baseFontId,
        TextRun[] runs,
        ref int runCount)
    {
        runCount = 0;

        if (codepoints.IsEmpty)
            return;

        int runStart = 0;
        byte currentLevel = bidiLevels[0];
        var currentScript = scripts[0];
        int currentFontId = fontProvider?.FindFontForCodepoint(codepoints[0], baseFontId) ?? baseFontId;

        for (int i = 1; i < codepoints.Length; i++)
        {
            bool needBreak = false;

            if (bidiLevels[i] != currentLevel)
                needBreak = true;

            if (scripts[i] != currentScript)
                needBreak = true;

            int fontId = fontProvider?.FindFontForCodepoint(codepoints[i], baseFontId) ?? baseFontId;
            if (fontId != currentFontId)
                needBreak = true;

            if (needBreak)
            {
                EnsureCapacity(ref runs, runCount + 1);
                runs[runCount++] = new TextRun
                {
                    range = new TextRange(runStart, i - runStart),
                    bidiLevel = currentLevel,
                    script = currentScript,
                    fontId = currentFontId,
                    attributeSnapshot = 0
                };

                runStart = i;
                currentLevel = bidiLevels[i];
                currentScript = scripts[i];
                currentFontId = fontId;
            }
        }

        // Last run
        EnsureCapacity(ref runs, runCount + 1);
        runs[runCount++] = new TextRun
        {
            range = new TextRange(runStart, codepoints.Length - runStart),
            bidiLevel = currentLevel,
            script = currentScript,
            fontId = currentFontId,
            attributeSnapshot = 0
        };
    }

    private static void EnsureCapacity(ref TextRun[] array, int required)
    {
        if (array.Length >= required) return;
        Array.Resize(ref array, Math.Max(required, array.Length * 2));
    }
}
