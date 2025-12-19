using System;


public sealed class ScriptAnalyzer
{
    private readonly IUnicodeDataProvider dataProvider;

    public ScriptAnalyzer(IUnicodeDataProvider dataProvider)
    {
        this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }


    public ScriptAnalyzer()
    {
        dataProvider = UnicodeData.Provider ?? throw new InvalidOperationException(
            "UnicodeData not initialized. Call UnicodeData.EnsureInitialized() first.");
    }


    public void Analyze(ReadOnlySpan<int> codepoints, UnicodeScript[] result)
    {
        if (result.Length < codepoints.Length)
            throw new ArgumentException("Result buffer too small", nameof(result));

        var length = codepoints.Length;

        for (var i = 0; i < length; i++) result[i] = dataProvider.GetScript(codepoints[i]);

        ResolveInheritedScripts(result, length);
    }


    private static void ResolveInheritedScripts(UnicodeScript[] scripts, int length)
    {
        var lastRealScript = UnicodeScript.Unknown;

        for (var i = 0; i < length; i++)
        {
            var script = scripts[i];

            if (script == UnicodeScript.Common || script == UnicodeScript.Inherited)
            {
                if (lastRealScript != UnicodeScript.Unknown) scripts[i] = lastRealScript;
            }
            else
            {
                lastRealScript = script;
            }
        }

        lastRealScript = UnicodeScript.Unknown;
        for (var i = length - 1; i >= 0; i--)
        {
            var script = scripts[i];

            if (script == UnicodeScript.Common || script == UnicodeScript.Inherited)
            {
                if (lastRealScript != UnicodeScript.Unknown) scripts[i] = lastRealScript;
            }
            else
            {
                lastRealScript = script;
            }
        }
    }
}