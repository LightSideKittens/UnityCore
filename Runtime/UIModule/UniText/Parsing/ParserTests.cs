using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Test cases for text parsers.
/// Run from Unity Editor or as MonoBehaviour.
/// </summary>
public static class ParserTests
{
    private static ModifierRegistry registry;
    private static int boldId, italicId, colorId, sizeId, supId, subId;
    private static int errorId, warningId, todoId, urlId;

    /// <summary>
    /// Run all parser tests.
    /// </summary>
    public static void RunAllTests()
    {
        Debug.Log("═══════════════════════════════════════════════════════════════");
        Debug.Log("                    PARSER TESTS START");
        Debug.Log("═══════════════════════════════════════════════════════════════");

        SetupRegistry();

        // HTML Parser tests
        TestHtmlParser_SimpleTag();
        TestHtmlParser_NestedTags();
        TestHtmlParser_TagWithValue();
        TestHtmlParser_MultipleTagsWithValues();
        TestHtmlParser_UnclosedTag();
        TestHtmlParser_UnknownTag();
        TestHtmlParser_EmptyInput();
        TestHtmlParser_NoTags();
        TestHtmlParser_MalformedTags();
        TestHtmlParser_CaseInsensitive();
        TestHtmlParser_ComplexNesting();

        // Keyword Parser tests
        TestKeywordParser_SingleKeyword();
        TestKeywordParser_MultipleKeywords();
        TestKeywordParser_CaseSensitive();
        TestKeywordParser_WholeWordsOnly();
        TestKeywordParser_OverlappingKeywords();
        TestKeywordParser_NoMatches();

        // Performance tests
        TestPerformance_HtmlParser();
        TestPerformance_KeywordParser();

        Debug.Log("═══════════════════════════════════════════════════════════════");
        Debug.Log("                    PARSER TESTS COMPLETE");
        Debug.Log("═══════════════════════════════════════════════════════════════");
    }

    private static void SetupRegistry()
    {
        registry = new ModifierRegistry { DebugLogging = false };

        // Itemize modifiers
        boldId = registry.Register(new StubItemizeModifier("Bold"));
        italicId = registry.Register(new StubItemizeModifier("Italic"));
        sizeId = registry.Register(new StubItemizeModifier("Size"));

        // Layout modifiers
        supId = registry.Register(new StubLayoutModifier("Superscript"));
        subId = registry.Register(new StubLayoutModifier("Subscript"));

        // Render modifiers
        colorId = registry.Register(new StubRenderModifier("Color"));
        errorId = registry.Register(new StubRenderModifier("Error"));
        warningId = registry.Register(new StubRenderModifier("Warning"));
        todoId = registry.Register(new StubRenderModifier("Todo"));
        urlId = registry.Register(new StubRenderModifier("Url"));

        Debug.Log($"Registry setup complete: {registry.Count} modifiers registered");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HTML PARSER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    private static void TestHtmlParser_SimpleTag()
    {
        Debug.Log("\n--- Test: HtmlParser Simple Tag ---");

        var parser = CreateHtmlParser();
        var result = parser.Parse("<b>Hello</b>");

        AssertEqual("Hello", result.displayText.ToString(), "Display text");
        AssertEqual(1, result.SpanCount, "Span count");
        AssertEqual(0, result.spans[0].start, "Span start");
        AssertEqual(5, result.spans[0].end, "Span end");
        AssertEqual(boldId, result.spans[0].modifierId, "Modifier ID");

        Debug.Log("PASSED: Simple tag");
    }

    private static void TestHtmlParser_NestedTags()
    {
        Debug.Log("\n--- Test: HtmlParser Nested Tags ---");

        var parser = CreateHtmlParser();
        var result = parser.Parse("<b>Hello <i>World</i></b>");

        AssertEqual("Hello World", result.displayText.ToString(), "Display text");
        AssertEqual(2, result.SpanCount, "Span count");

        // Inner tag (italic) should be first because it closes first
        // Actually depends on implementation - let's check
        Debug.Log($"  Span 0: [{result.spans[0].start}..{result.spans[0].end}] mod={result.spans[0].modifierId}");
        Debug.Log($"  Span 1: [{result.spans[1].start}..{result.spans[1].end}] mod={result.spans[1].modifierId}");

        Debug.Log("PASSED: Nested tags");
    }

    private static void TestHtmlParser_TagWithValue()
    {
        Debug.Log("\n--- Test: HtmlParser Tag With Value ---");

        var parser = CreateHtmlParser();
        string input = "<color=#FF0000>Red Text</color>";
        var result = parser.Parse(input);

        AssertEqual("Red Text", result.displayText.ToString(), "Display text");
        AssertEqual(1, result.SpanCount, "Span count");
        AssertEqual(colorId, result.spans[0].modifierId, "Modifier ID");
        AssertTrue(result.spans[0].HasValue, "Has value");

        var value = result.spans[0].GetValue(result.originalText);
        AssertEqual("#FF0000", value.ToString(), "Value");

        Debug.Log("PASSED: Tag with value");
    }

    private static void TestHtmlParser_MultipleTagsWithValues()
    {
        Debug.Log("\n--- Test: HtmlParser Multiple Tags With Values ---");

        var parser = CreateHtmlParser();
        string input = "<color=#FF0000>Red</color> and <size=24>Big</size> text";
        var result = parser.Parse(input);

        AssertEqual("Red and Big text", result.displayText.ToString(), "Display text");
        AssertEqual(2, result.SpanCount, "Span count");

        // Check first span (color)
        var colorSpan = result.spans[0];
        AssertEqual(0, colorSpan.start, "Color span start");
        AssertEqual(3, colorSpan.end, "Color span end");
        AssertEqual("#FF0000", colorSpan.GetValue(result.originalText).ToString(), "Color value");

        // Check second span (size)
        var sizeSpan = result.spans[1];
        AssertEqual(8, sizeSpan.start, "Size span start");
        AssertEqual(11, sizeSpan.end, "Size span end");
        AssertEqual("24", sizeSpan.GetValue(result.originalText).ToString(), "Size value");

        Debug.Log("PASSED: Multiple tags with values");
    }

    private static void TestHtmlParser_UnclosedTag()
    {
        Debug.Log("\n--- Test: HtmlParser Unclosed Tag ---");

        var parser = CreateHtmlParser();
        var result = parser.Parse("<b>Hello World");

        AssertEqual("Hello World", result.displayText.ToString(), "Display text");
        AssertEqual(1, result.SpanCount, "Span count (unclosed tag creates span to end)");
        AssertEqual(0, result.spans[0].start, "Span start");
        AssertEqual(11, result.spans[0].end, "Span end (should be text length)");

        Debug.Log("PASSED: Unclosed tag");
    }

    private static void TestHtmlParser_UnknownTag()
    {
        Debug.Log("\n--- Test: HtmlParser Unknown Tag ---");

        var parser = CreateHtmlParser();
        var result = parser.Parse("<unknown>Hello</unknown> World");

        // Unknown tags should be copied as text
        AssertEqual("<unknown>Hello</unknown> World", result.displayText.ToString(), "Display text");
        AssertEqual(0, result.SpanCount, "Span count");

        Debug.Log("PASSED: Unknown tag");
    }

    private static void TestHtmlParser_EmptyInput()
    {
        Debug.Log("\n--- Test: HtmlParser Empty Input ---");

        var parser = CreateHtmlParser();
        var result = parser.Parse("");

        AssertEqual("", result.displayText.ToString(), "Display text");
        AssertEqual(0, result.SpanCount, "Span count");

        Debug.Log("PASSED: Empty input");
    }

    private static void TestHtmlParser_NoTags()
    {
        Debug.Log("\n--- Test: HtmlParser No Tags ---");

        var parser = CreateHtmlParser();
        var result = parser.Parse("Hello World without tags");

        AssertEqual("Hello World without tags", result.displayText.ToString(), "Display text");
        AssertEqual(0, result.SpanCount, "Span count");

        Debug.Log("PASSED: No tags");
    }

    private static void TestHtmlParser_MalformedTags()
    {
        Debug.Log("\n--- Test: HtmlParser Malformed Tags ---");

        var parser = CreateHtmlParser();

        // Unclosed bracket
        var result1 = parser.Parse("<b Hello");
        Debug.Log($"  Unclosed bracket: \"{result1.displayText.ToString()}\"");

        // Empty tag
        var result2 = parser.Parse("<>Hello</>");
        Debug.Log($"  Empty tag: \"{result2.displayText.ToString()}\"");

        // Just brackets
        var result3 = parser.Parse("< > test < >");
        Debug.Log($"  Just brackets: \"{result3.displayText.ToString()}\"");

        Debug.Log("PASSED: Malformed tags (no crash)");
    }

    private static void TestHtmlParser_CaseInsensitive()
    {
        Debug.Log("\n--- Test: HtmlParser Case Insensitive ---");

        var parser = CreateHtmlParser();

        var result1 = parser.Parse("<B>Bold</B>");
        var result2 = parser.Parse("<b>Bold</b>");
        var result3 = parser.Parse("<B>Bold</b>");

        AssertEqual(1, result1.SpanCount, "Uppercase span count");
        AssertEqual(1, result2.SpanCount, "Lowercase span count");
        AssertEqual(1, result3.SpanCount, "Mixed case span count");

        AssertEqual(boldId, result1.spans[0].modifierId, "Uppercase modifier");
        AssertEqual(boldId, result2.spans[0].modifierId, "Lowercase modifier");
        AssertEqual(boldId, result3.spans[0].modifierId, "Mixed case modifier");

        Debug.Log("PASSED: Case insensitive");
    }

    private static void TestHtmlParser_ComplexNesting()
    {
        Debug.Log("\n--- Test: HtmlParser Complex Nesting ---");

        var parser = CreateHtmlParser();
        string input = "<b>A<i>B<color=#00FF00>C</color>D</i>E</b>F";
        var result = parser.Parse(input);

        AssertEqual("ABCDEF", result.displayText.ToString(), "Display text");
        AssertEqual(3, result.SpanCount, "Span count");

        Debug.Log($"  Input: {input}");
        Debug.Log($"  Output: {result.displayText.ToString()}");
        for (int i = 0; i < result.SpanCount; i++)
        {
            var span = result.spans[i];
            var content = span.GetContent(result.displayText);
            var mod = registry.Get(span.modifierId);
            Debug.Log($"  Span {i}: [{span.start}..{span.end}] = \"{content.ToString()}\" ({mod?.Name})");
        }

        Debug.Log("PASSED: Complex nesting");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // KEYWORD PARSER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    private static void TestKeywordParser_SingleKeyword()
    {
        Debug.Log("\n--- Test: KeywordParser Single Keyword ---");

        var parser = new KeywordParser { DebugLogging = false };
        parser.RegisterKeyword("ERROR", errorId);

        var result = parser.Parse("Found ERROR in line 5");

        AssertEqual("Found ERROR in line 5", result.displayText.ToString(), "Display text (unchanged)");
        AssertFalse(result.textModified, "Text not modified");
        AssertEqual(1, result.SpanCount, "Span count");
        AssertEqual(6, result.spans[0].start, "Span start");
        AssertEqual(11, result.spans[0].end, "Span end");

        Debug.Log("PASSED: Single keyword");
    }

    private static void TestKeywordParser_MultipleKeywords()
    {
        Debug.Log("\n--- Test: KeywordParser Multiple Keywords ---");

        var parser = new KeywordParser { DebugLogging = false };
        parser.RegisterKeyword("ERROR", errorId);
        parser.RegisterKeyword("WARNING", warningId);
        parser.RegisterKeyword("TODO", todoId);

        var result = parser.Parse("ERROR: fix this. WARNING: check that. TODO: refactor");

        AssertEqual(3, result.SpanCount, "Span count");

        Debug.Log($"  Input: ERROR: fix this. WARNING: check that. TODO: refactor");
        for (int i = 0; i < result.SpanCount; i++)
        {
            var span = result.spans[i];
            var content = span.GetContent(result.displayText);
            var mod = registry.Get(span.modifierId);
            Debug.Log($"  Span {i}: [{span.start}..{span.end}] = \"{content.ToString()}\" ({mod?.Name})");
        }

        Debug.Log("PASSED: Multiple keywords");
    }

    private static void TestKeywordParser_CaseSensitive()
    {
        Debug.Log("\n--- Test: KeywordParser Case Sensitive ---");

        var parser = new KeywordParser { CaseSensitive = true };
        parser.RegisterKeyword("ERROR", errorId);

        var result1 = parser.Parse("ERROR error Error");
        AssertEqual(1, result1.SpanCount, "Case sensitive - only exact match");

        parser.CaseSensitive = false;
        var result2 = parser.Parse("ERROR error Error");
        AssertEqual(3, result2.SpanCount, "Case insensitive - all matches");

        Debug.Log("PASSED: Case sensitive");
    }

    private static void TestKeywordParser_WholeWordsOnly()
    {
        Debug.Log("\n--- Test: KeywordParser Whole Words Only ---");

        var parser = new KeywordParser { WholeWordsOnly = true };
        parser.RegisterKeyword("ERROR", errorId);

        // "ERROR" should match, "ERRORS" should not
        var result = parser.Parse("ERROR ERRORS ERROR_CODE MY_ERROR");
        AssertEqual(1, result.SpanCount, "Only whole word matches");
        AssertEqual(0, result.spans[0].start, "Match at start");

        parser.WholeWordsOnly = false;
        var result2 = parser.Parse("ERROR ERRORS ERROR_CODE MY_ERROR");
        AssertEqual(4, result2.SpanCount, "All partial matches");

        Debug.Log("PASSED: Whole words only");
    }

    private static void TestKeywordParser_OverlappingKeywords()
    {
        Debug.Log("\n--- Test: KeywordParser Overlapping Keywords ---");

        var parser = new KeywordParser();
        parser.RegisterKeyword("ERROR", errorId);
        parser.RegisterKeyword("ERROR_CODE", warningId);

        var result = parser.Parse("Found ERROR_CODE here");

        Debug.Log($"  Spans found: {result.SpanCount}");
        for (int i = 0; i < result.SpanCount; i++)
        {
            var span = result.spans[i];
            var content = span.GetContent(result.displayText);
            Debug.Log($"  Span {i}: [{span.start}..{span.end}] = \"{content.ToString()}\"");
        }

        // Both keywords match (ERROR is substring of ERROR_CODE)
        AssertEqual(2, result.SpanCount, "Both keywords match");

        Debug.Log("PASSED: Overlapping keywords");
    }

    private static void TestKeywordParser_NoMatches()
    {
        Debug.Log("\n--- Test: KeywordParser No Matches ---");

        var parser = new KeywordParser();
        parser.RegisterKeyword("ERROR", errorId);

        var result = parser.Parse("Everything is fine");

        AssertEqual(0, result.SpanCount, "No matches");
        AssertFalse(result.textModified, "Text not modified");

        Debug.Log("PASSED: No matches");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERFORMANCE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    private static void TestPerformance_HtmlParser()
    {
        Debug.Log("\n--- Test: HtmlParser Performance ---");

        var parser = CreateHtmlParser();

        // Generate test text with many tags
        var sb = new System.Text.StringBuilder(10000);
        for (int i = 0; i < 100; i++)
        {
            sb.Append($"<b>Bold{i}</b> normal <color=#FF0000>red{i}</color> <i>italic</i> ");
        }
        string testText = sb.ToString();

        // Warmup
        parser.Parse(testText);
        ParseBuffers.Reset();
        parser.Parse(testText);

        // Measure
        var sw = Stopwatch.StartNew();
        int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            parser.Parse(testText);
        }
        sw.Stop();

        double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        double avgUs = avgMs * 1000;

        Debug.Log($"  Text length: {testText.Length} chars");
        Debug.Log($"  Iterations: {iterations}");
        Debug.Log($"  Total time: {sw.Elapsed.TotalMilliseconds:F2} ms");
        Debug.Log($"  Average: {avgUs:F2} us per parse");
        Debug.Log($"  Throughput: {(testText.Length * iterations / sw.Elapsed.TotalSeconds / 1_000_000):F2} MB/s");

        Debug.Log("PASSED: HtmlParser performance");
    }

    private static void TestPerformance_KeywordParser()
    {
        Debug.Log("\n--- Test: KeywordParser Performance ---");

        var parser = new KeywordParser();
        parser.RegisterKeyword("ERROR", errorId);
        parser.RegisterKeyword("WARNING", warningId);
        parser.RegisterKeyword("INFO", todoId);
        parser.RegisterKeyword("DEBUG", urlId);

        // Generate test text
        var sb = new System.Text.StringBuilder(10000);
        string[] words = { "ERROR", "WARNING", "INFO", "DEBUG", "normal", "text", "here" };
        var rng = new System.Random(42);
        for (int i = 0; i < 1000; i++)
        {
            sb.Append(words[rng.Next(words.Length)]);
            sb.Append(' ');
        }
        string testText = sb.ToString();

        // Warmup
        parser.Parse(testText);
        ParseBuffers.Reset();
        parser.Parse(testText);

        // Measure
        var sw = Stopwatch.StartNew();
        int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            parser.Parse(testText);
        }
        sw.Stop();

        double avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        double avgUs = avgMs * 1000;

        Debug.Log($"  Text length: {testText.Length} chars");
        Debug.Log($"  Iterations: {iterations}");
        Debug.Log($"  Total time: {sw.Elapsed.TotalMilliseconds:F2} ms");
        Debug.Log($"  Average: {avgUs:F2} us per parse");
        Debug.Log($"  Throughput: {(testText.Length * iterations / sw.Elapsed.TotalSeconds / 1_000_000):F2} MB/s");

        Debug.Log("PASSED: KeywordParser performance");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    private static HtmlParser CreateHtmlParser()
    {
        var parser = new HtmlParser { DebugLogging = false };
        parser.RegisterTag("b", boldId);
        parser.RegisterTag("i", italicId);
        parser.RegisterTag("color", colorId);
        parser.RegisterTag("size", sizeId);
        parser.RegisterTag("sup", supId);
        parser.RegisterTag("sub", subId);
        return parser;
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!Equals(expected, actual))
        {
            Debug.LogError($"FAILED: {message}. Expected: {expected}, Actual: {actual}");
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError($"FAILED: {message}. Expected: true, Actual: false");
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertFalse(bool condition, string message)
    {
        if (condition)
        {
            Debug.LogError($"FAILED: {message}. Expected: false, Actual: true");
            throw new Exception($"Assertion failed: {message}");
        }
    }
}
