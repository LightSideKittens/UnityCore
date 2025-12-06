using System;

/// <summary>
/// Анализатор скриптов (UAX #24).
/// Определяет Unicode Script для каждого codepoint.
/// </summary>
public sealed class ScriptAnalyzer
{
    private readonly IUnicodeDataProvider dataProvider;

    public ScriptAnalyzer(IUnicodeDataProvider dataProvider)
    {
        this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }

    /// <summary>
    /// Анализировать скрипты и записать результат в предоставленный буфер.
    /// </summary>
    public void Analyze(ReadOnlySpan<int> codepoints, UnicodeScript[] result)
    {
        if (result.Length < codepoints.Length)
            throw new ArgumentException("Result buffer too small", nameof(result));

        int length = codepoints.Length;

        // Первый проход: определяем скрипт каждого codepoint
        for (int i = 0; i < length; i++)
        {
            result[i] = dataProvider.GetScript(codepoints[i]);
        }

        // Второй проход: разрешаем Common и Inherited
        ResolveInheritedScripts(result, length);
    }

    /// <summary>
    /// Разрешение Common и Inherited скриптов.
    /// По UAX #24, эти скрипты должны наследовать от окружающего контекста.
    /// </summary>
    private static void ResolveInheritedScripts(UnicodeScript[] scripts, int length)
    {
        UnicodeScript lastRealScript = UnicodeScript.Unknown;

        // Forward pass: наследуем от предыдущего
        for (int i = 0; i < length; i++)
        {
            var script = scripts[i];

            if (script == UnicodeScript.Common || script == UnicodeScript.Inherited)
            {
                if (lastRealScript != UnicodeScript.Unknown)
                {
                    scripts[i] = lastRealScript;
                }
            }
            else
            {
                lastRealScript = script;
            }
        }

        // Backward pass: для оставшихся Common/Inherited в начале
        lastRealScript = UnicodeScript.Unknown;
        for (int i = length - 1; i >= 0; i--)
        {
            var script = scripts[i];

            if (script == UnicodeScript.Common || script == UnicodeScript.Inherited)
            {
                if (lastRealScript != UnicodeScript.Unknown)
                {
                    scripts[i] = lastRealScript;
                }
            }
            else
            {
                lastRealScript = script;
            }
        }
    }
}
