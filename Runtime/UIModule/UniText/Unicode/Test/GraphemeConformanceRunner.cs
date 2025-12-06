using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Conformance test runner for UAX #29 Grapheme Cluster Boundaries.
/// Validates GraphemeBreaker against GraphemeBreakTest.txt
/// </summary>
public sealed class GraphemeConformanceRunner
{
    private readonly GraphemeBreaker breaker;

    public GraphemeConformanceRunner(GraphemeBreaker breaker)
    {
        this.breaker = breaker ?? throw new ArgumentNullException(nameof(breaker));
    }

    /// <summary>
    /// Run tests from GraphemeBreakTest.txt
    /// Format: ÷ 0020 × 0308 ÷ 0020 ÷  # comment
    /// ÷ = break, × = no break
    /// </summary>
    public GraphemeConformanceSummary RunTests(string testFileContent, int maxFailuresToLog = 20)
    {
        var summary = new GraphemeConformanceSummary();

        if (string.IsNullOrEmpty(testFileContent))
        {
            summary.sampleFailures = "Test file content is empty or null.";
            return summary;
        }

        var failures = new StringBuilder();
        int failureCount = 0;

        using var reader = new System.IO.StringReader(testFileContent);
        string? line;
        int lineNumber = 0;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            // Strip comment
            int hash = line.IndexOf('#');
            if (hash >= 0)
                line = line.Substring(0, hash);

            line = line.Trim();
            if (line.Length == 0)
                continue;

            summary.totalTests++;

            // Parse test case
            if (!TryParseTestCase(line, out int[] codepoints, out bool[] expectedBreaks))
            {
                summary.skippedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Failed to parse '{line}'");
                continue;
            }

            // Run test
            try
            {
                var actualBreaks = breaker.GetBreakOpportunities(codepoints);

                // Compare
                bool passed = true;
                for (int i = 0; i < actualBreaks.Length && i < expectedBreaks.Length; i++)
                {
                    if (actualBreaks[i] != expectedBreaks[i])
                    {
                        passed = false;
                        if (failureCount++ < maxFailuresToLog)
                        {
                            string expected = expectedBreaks[i] ? "÷" : "×";
                            string actual = actualBreaks[i] ? "÷" : "×";
                            failures.AppendLine($"Line {lineNumber}: Position {i} - expected {expected}, got {actual}");
                            failures.AppendLine($"  Input: {FormatCodepoints(codepoints)}");
                        }
                        break;
                    }
                }

                if (passed)
                    summary.passedTests++;
                else
                    summary.failedTests++;
            }
            catch (Exception ex)
            {
                summary.failedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Exception - {ex.Message}");
            }
        }

        summary.sampleFailures = failures.ToString();
        return summary;
    }

    private bool TryParseTestCase(string line, out int[] codepoints, out bool[] breaks)
    {
        codepoints = Array.Empty<int>();
        breaks = Array.Empty<bool>();

        var codepointList = new List<int>();
        var breakList = new List<bool>();

        // Parse format: ÷ 0020 × 0308 ÷ 0020 ÷
        // Each token is either ÷ (break), × (no break), or hex codepoint
        
        string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string token in tokens)
        {
            if (token == "÷")
            {
                breakList.Add(true);
            }
            else if (token == "×")
            {
                breakList.Add(false);
            }
            else
            {
                // Hex codepoint
                if (!int.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp))
                    return false;
                codepointList.Add(cp);
            }
        }

        // Validate: breaks should be codepoints + 1
        if (breakList.Count != codepointList.Count + 1)
            return false;

        codepoints = codepointList.ToArray();
        breaks = breakList.ToArray();
        return true;
    }

    private static string FormatCodepoints(int[] codepoints)
    {
        var sb = new StringBuilder();
        foreach (int cp in codepoints)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append($"U+{cp:X4}");
        }
        return sb.ToString();
    }
}

public struct GraphemeConformanceSummary
{
    public int totalTests;
    public int passedTests;
    public int failedTests;
    public int skippedTests;
    public string? sampleFailures;
}