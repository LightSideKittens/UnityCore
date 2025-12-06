using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Runs conformance tests for UAX #14 Line Breaking Algorithm
/// using official LineBreakTest.txt from Unicode
/// </summary>
public sealed class LineBreakConformanceRunner
{
    private readonly LineBreakAlgorithm algorithm;

    public LineBreakConformanceRunner(IUnicodeDataProvider dataProvider)
    {
        if (dataProvider == null)
            throw new ArgumentNullException(nameof(dataProvider));
        
        algorithm = new LineBreakAlgorithm(dataProvider);
    }

    public LineBreakConformanceRunner(LineBreakAlgorithm algorithm)
    {
        this.algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
    }

    /// <summary>
    /// Run tests from LineBreakTest.txt content
    /// Format: ÷ 0020 × 0041 ÷ # comment
    /// ÷ = break allowed, × = break prohibited
    /// </summary>
    public LineBreakConformanceSummary RunTests(string fileContent, int maxFailuresToLog = 20)
    {
        var summary = new LineBreakConformanceSummary();

        if (string.IsNullOrEmpty(fileContent))
        {
            summary.sampleFailures = "LineBreakTest content is empty or null.";
            return summary;
        }

        var failures = new List<LineBreakConformanceFailure>();

        using var reader = new System.IO.StringReader(fileContent);
        string? line;
        int lineNumber = 0;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            // Strip comment
            int hashIndex = line.IndexOf('#');
            if (hashIndex >= 0)
                line = line.Substring(0, hashIndex);

            line = line.Trim();
            if (line.Length == 0)
                continue;

            // Parse test case
            if (!TryParseTestCase(line, out int[] codePoints, out bool[] expectedBreaks, out string? parseError))
            {
                summary.skippedTests++;
                if (parseError != null)
                    AddFailure(failures, lineNumber, line, parseError, maxFailuresToLog);
                continue;
            }

            summary.totalEvaluatedTests++;

            // Get actual break opportunities using LineBreakAlgorithm
            bool[] actualBreaks;
            try
            {
                actualBreaks = algorithm.GetBreakOpportunities(codePoints);
            }
            catch (Exception ex)
            {
                summary.failedTests++;
                AddFailure(failures, lineNumber, line, $"Exception: {ex.Message}", maxFailuresToLog);
                continue;
            }

            // Compare
            if (!CompareBreaks(expectedBreaks, actualBreaks, out string errorMessage))
            {
                summary.failedTests++;
                AddFailure(failures, lineNumber, line, errorMessage, maxFailuresToLog);
                continue;
            }

            summary.passedTests++;
        }

        summary.sampleFailures = BuildSampleFailuresText(failures, maxFailuresToLog);
        return summary;
    }

    /// <summary>
    /// Parse a test line like: ÷ 0020 × 0041 ÷
    /// </summary>
    private bool TryParseTestCase(string line, out int[] codePoints, out bool[] breaks, out string? error)
    {
        codePoints = Array.Empty<int>();
        breaks = Array.Empty<bool>();
        error = null;

        var cpList = new List<int>();
        var breakList = new List<bool>();

        // Tokenize by spaces
        string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        bool expectBreakMarker = true;

        foreach (string token in tokens)
        {
            if (token == "÷" || token == "\u00F7")
            {
                if (!expectBreakMarker)
                {
                    error = "Unexpected ÷ marker";
                    return false;
                }
                breakList.Add(true); // break allowed
                expectBreakMarker = false;
            }
            else if (token == "×" || token == "\u00D7")
            {
                if (!expectBreakMarker)
                {
                    error = "Unexpected × marker";
                    return false;
                }
                breakList.Add(false); // break prohibited
                expectBreakMarker = false;
            }
            else
            {
                // Should be hex codepoint
                if (expectBreakMarker)
                {
                    error = $"Expected break marker, got '{token}'";
                    return false;
                }

                if (!int.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp))
                {
                    error = $"Invalid hex codepoint: '{token}'";
                    return false;
                }

                cpList.Add(cp);
                expectBreakMarker = true;
            }
        }

        if (cpList.Count == 0)
        {
            error = "No codepoints found";
            return false;
        }

        // breaks array should have length = codepoints + 1 (before first, between each, after last)
        if (breakList.Count != cpList.Count + 1)
        {
            error = $"Break count mismatch: expected {cpList.Count + 1}, got {breakList.Count}";
            return false;
        }

        codePoints = cpList.ToArray();
        breaks = breakList.ToArray();
        return true;
    }

    private bool CompareBreaks(bool[] expected, bool[] actual, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (expected.Length != actual.Length)
        {
            errorMessage = $"Length mismatch: expected {expected.Length}, actual {actual.Length}";
            return false;
        }

        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                string expectedStr = expected[i] ? "÷ (break)" : "× (no break)";
                string actualStr = actual[i] ? "÷ (break)" : "× (no break)";
                errorMessage = $"Mismatch at position {i}: expected {expectedStr}, actual {actualStr}";
                return false;
            }
        }

        return true;
    }

    private static void AddFailure(List<LineBreakConformanceFailure> failures, int lineNumber, 
        string rawInput, string message, int maxFailuresToLog)
    {
        if (failures.Count >= maxFailuresToLog)
            return;

        failures.Add(new LineBreakConformanceFailure
        {
            lineNumber = lineNumber,
            rawInput = rawInput,
            message = message
        });
    }

    private static string BuildSampleFailuresText(List<LineBreakConformanceFailure> failures, int maxFailuresToLog)
    {
        if (failures == null || failures.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        int count = Math.Min(failures.Count, maxFailuresToLog);

        for (int i = 0; i < count; i++)
        {
            var failure = failures[i];
            sb.Append("- Line ").Append(failure.lineNumber).Append(": ").Append(failure.message).AppendLine();
            sb.Append("  Input: ").Append(failure.rawInput).AppendLine();
        }

        return sb.ToString();
    }
}

internal sealed class LineBreakConformanceFailure
{
    public int lineNumber;
    public string? rawInput;
    public string? message;
}

public struct LineBreakConformanceSummary
{
    public int totalEvaluatedTests;
    public int passedTests;
    public int failedTests;
    public int skippedTests;
    public string? sampleFailures;
}