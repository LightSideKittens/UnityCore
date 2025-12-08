using UnityEngine;

/// <summary>
/// MonoBehaviour to run parser tests from Unity Editor.
/// Attach to any GameObject and use context menu to run tests.
/// </summary>
public class ParserTestRunner : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("Enable debug logging in parsers during tests")]
    public bool enableParserLogging = false;

    [ContextMenu("Run All Parser Tests")]
    public void RunAllTests()
    {
        ParserTests.RunAllTests();
    }

    [ContextMenu("Run Quick Demo")]
    public void RunQuickDemo()
    {
        Debug.Log("═══════════════════════════════════════════════════════════════");
        Debug.Log("                    PARSER QUICK DEMO");
        Debug.Log("═══════════════════════════════════════════════════════════════");

        // Setup
        var registry = new ModifierRegistry { DebugLogging = true };
        int boldId = registry.Register(new StubItemizeModifier("Bold"));
        int colorId = registry.Register(new StubRenderModifier("Color"));
        int errorId = registry.Register(new StubRenderModifier("Error"));

        // HTML Parser demo
        Debug.Log("\n--- HTML Parser Demo ---");
        var htmlParser = new HtmlParser { DebugLogging = enableParserLogging };
        htmlParser.RegisterTag("b", boldId);
        htmlParser.RegisterTag("color", colorId);

        string htmlInput = "<b>Hello</b> <color=#FF0000>World</color>!";
        Debug.Log($"Input: {htmlInput}");

        var htmlResult = htmlParser.Parse(htmlInput);

        Debug.Log($"Output: {htmlResult.displayText.ToString()}");
        Debug.Log($"Spans: {htmlResult.SpanCount}");
        for (int i = 0; i < htmlResult.SpanCount; i++)
        {
            var span = htmlResult.spans[i];
            var content = span.GetContent(htmlResult.displayText);
            var value = span.GetValue(htmlResult.originalText);
            var mod = registry.Get(span.modifierId);
            Debug.Log($"  [{span.start}..{span.end}] \"{content.ToString()}\" -> {mod?.Name}" +
                     (value.IsEmpty ? "" : $" (value: {value.ToString()})"));
        }

        // Keyword Parser demo
        Debug.Log("\n--- Keyword Parser Demo ---");
        var keywordParser = new KeywordParser { DebugLogging = enableParserLogging };
        keywordParser.RegisterKeyword("ERROR", errorId);

        string logInput = "2024-01-15 ERROR: Connection failed. Retrying...";
        Debug.Log($"Input: {logInput}");

        var keywordResult = keywordParser.Parse(logInput);

        Debug.Log($"Output: {keywordResult.displayText.ToString()}");
        Debug.Log($"Text modified: {keywordResult.textModified}");
        Debug.Log($"Spans: {keywordResult.SpanCount}");
        for (int i = 0; i < keywordResult.SpanCount; i++)
        {
            var span = keywordResult.spans[i];
            var content = span.GetContent(keywordResult.displayText);
            var mod = registry.Get(span.modifierId);
            Debug.Log($"  [{span.start}..{span.end}] \"{content.ToString()}\" -> {mod?.Name}");
        }

        Debug.Log("\n═══════════════════════════════════════════════════════════════");
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("UniText/Run Parser Tests")]
    private static void RunTestsFromMenu()
    {
        ParserTests.RunAllTests();
    }

    [UnityEditor.MenuItem("UniText/Run Parser Demo")]
    private static void RunDemoFromMenu()
    {
        var runner = new ParserTestRunner();
        runner.enableParserLogging = true;
        runner.RunQuickDemo();
    }
#endif
}
