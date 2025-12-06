using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Tekst;

/// <summary>
/// Validates Script property implementation against UAX #24 (Unicode Script Property).
/// 
/// Provides two test methods:
/// 1. RunDataTests: Validates IUnicodeDataProvider.GetScript() against Scripts.txt
/// 2. RunAnalyzerTests: Validates ScriptAnalyzer against ScriptAnalyzerTest.txt
/// 
/// Reference: https://www.unicode.org/reports/tr24/ (Unicode 17.0.0)
/// </summary>
public sealed class ScriptConformanceRunner
{
    private readonly IUnicodeDataProvider dataProvider;
    
    public ScriptConformanceRunner(IUnicodeDataProvider dataProvider)
    {
        this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
    }
    
    #region Data Tests - Scripts.txt validation
    
    /// <summary>
    /// Validate Script property data against Scripts.txt
    /// Format: 0000..001F ; Common # comment
    /// </summary>
    public ScriptConformanceSummary RunDataTests(string scriptsFileContent, int maxFailuresToLog = 20)
    {
        var summary = new ScriptConformanceSummary();
        
        if (string.IsNullOrEmpty(scriptsFileContent))
        {
            summary.sampleFailures = "Scripts.txt content is empty or null.";
            return summary;
        }
        
        var failures = new StringBuilder();
        int failureCount = 0;
        
        foreach (var (lineNumber, rangePart, scriptPart) in ParseDataFile(scriptsFileContent))
        {
            if (!TryParseScript(scriptPart, out UnicodeScript expectedScript))
            {
                summary.skippedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Unknown script '{scriptPart}'");
                continue;
            }
            
            if (!TryParseRange(rangePart, out int start, out int end))
            {
                summary.skippedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Invalid range '{rangePart}'");
                continue;
            }
            
            foreach (int cp in GetSamplePoints(start, end))
            {
                summary.totalEvaluatedTests++;
                
                try
                {
                    var actual = dataProvider.GetScript(cp);
                    if (actual == expectedScript)
                        summary.passedTests++;
                    else
                    {
                        summary.failedTests++;
                        if (failureCount++ < maxFailuresToLog)
                            failures.AppendLine($"U+{cp:X4}: expected {expectedScript}, got {actual}");
                    }
                }
                catch (Exception ex)
                {
                    summary.failedTests++;
                    if (failureCount++ < maxFailuresToLog)
                        failures.AppendLine($"U+{cp:X4}: Exception - {ex.Message}");
                }
            }
        }
        
        summary.sampleFailures = failures.ToString();
        return summary;
    }
    
    #endregion
    
    #region Analyzer Tests - ScriptAnalyzerTest.txt validation
    
    /// <summary>
    /// Validate ScriptAnalyzer against ScriptAnalyzerTest.txt
    /// Format: 0041 0042 ; Latin Latin # comment
    /// </summary>
    public ScriptAnalyzerTestSummary RunAnalyzerTests(IScriptAnalyzer analyzer, string testFileContent, int maxFailuresToLog = 20)
    {
        if (analyzer == null)
            throw new ArgumentNullException(nameof(analyzer));
        
        var summary = new ScriptAnalyzerTestSummary();
        
        if (string.IsNullOrEmpty(testFileContent))
        {
            summary.sampleFailures = "Test file content is empty or null.";
            return summary;
        }
        
        var failures = new StringBuilder();
        int failureCount = 0;
        
        foreach (var (lineNumber, codepointsPart, scriptsPart) in ParseDataFile(testFileContent))
        {
            summary.totalTests++;
            
            // Parse codepoints
            if (!TryParseCodepoints(codepointsPart, out int[] codepoints))
            {
                summary.failedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Invalid codepoints '{codepointsPart}'");
                continue;
            }
            
            // Parse expected scripts
            if (!TryParseScriptList(scriptsPart, out UnicodeScript[] expectedScripts))
            {
                summary.failedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Invalid scripts '{scriptsPart}'");
                continue;
            }
            
            // Validate lengths match
            if (codepoints.Length != expectedScripts.Length)
            {
                summary.failedTests++;
                if (failureCount++ < maxFailuresToLog)
                    failures.AppendLine($"Line {lineNumber}: Length mismatch - {codepoints.Length} codepoints vs {expectedScripts.Length} scripts");
                continue;
            }
            
            // Run analyzer
            try
            {
                var resultBuffer = new ScriptResultBuffer();
                analyzer.Analyze(codepoints, resultBuffer);
                var actualScripts = resultBuffer.Scripts;
                
                if (actualScripts.Length != expectedScripts.Length)
                {
                    summary.failedTests++;
                    if (failureCount++ < maxFailuresToLog)
                        failures.AppendLine($"Line {lineNumber}: Result length {actualScripts.Length}, expected {expectedScripts.Length}");
                    continue;
                }
                
                bool passed = true;
                for (int i = 0; i < actualScripts.Length; i++)
                {
                    if (actualScripts[i] != expectedScripts[i])
                    {
                        passed = false;
                        if (failureCount++ < maxFailuresToLog)
                            failures.AppendLine($"Line {lineNumber}: Index {i} (U+{codepoints[i]:X4}) - expected {expectedScripts[i]}, got {actualScripts[i]}");
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
    
    #endregion
    
    #region Parsing helpers
    
    private IEnumerable<(int lineNumber, string field1, string field2)> ParseDataFile(string content)
    {
        using var reader = new System.IO.StringReader(content);
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
            
            // Split by semicolon
            int semi = line.IndexOf(';');
            if (semi < 0)
                continue;
            
            string field1 = line.Substring(0, semi).Trim();
            string field2 = line.Substring(semi + 1).Trim();
            
            if (field1.Length > 0 && field2.Length > 0)
                yield return (lineNumber, field1, field2);
        }
    }
    
    private bool TryParseRange(string rangePart, out int start, out int end)
    {
        start = end = 0;
        
        int dots = rangePart.IndexOf("..", StringComparison.Ordinal);
        if (dots >= 0)
        {
            return int.TryParse(rangePart.Substring(0, dots), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out start)
                && int.TryParse(rangePart.Substring(dots + 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out end)
                && start >= 0 && end >= start;
        }
        
        if (int.TryParse(rangePart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out start))
        {
            end = start;
            return start >= 0;
        }
        
        return false;
    }
    
    private bool TryParseScript(string name, out UnicodeScript script)
    {
        // Convert "Old_Italic" -> "OldItalic"
        var sb = new StringBuilder();
        foreach (var part in name.Split('_'))
        {
            if (part.Length == 0) continue;
            sb.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
                sb.Append(part.Substring(1).ToLowerInvariant());
        }
        
        return Enum.TryParse(sb.ToString(), out script);
    }
    
    private bool TryParseCodepoints(string input, out int[] codepoints)
    {
        var list = new List<int>();
        foreach (var hex in input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int cp))
            {
                codepoints = Array.Empty<int>();
                return false;
            }
            list.Add(cp);
        }
        codepoints = list.ToArray();
        return true;
    }
    
    private bool TryParseScriptList(string input, out UnicodeScript[] scripts)
    {
        var list = new List<UnicodeScript>();
        foreach (var name in input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!TryParseScript(name, out UnicodeScript script))
            {
                scripts = Array.Empty<UnicodeScript>();
                return false;
            }
            list.Add(script);
        }
        scripts = list.ToArray();
        return true;
    }
    
    private IEnumerable<int> GetSamplePoints(int start, int end)
    {
        int size = end - start + 1;
        
        if (size <= 10)
        {
            for (int i = start; i <= end; i++)
                yield return i;
            yield break;
        }
        
        // For large ranges: boundaries + samples
        yield return start;
        yield return end;
        yield return start + 1;
        yield return end - 1;
        yield return start + size / 2;
        
        int step = size / 5;
        for (int i = 1; i < 5; i++)
            yield return start + i * step;
    }
    
    #endregion
    
    #region Backward Compatibility
    
    /// <summary>
    /// Alias for RunDataTests (backward compatibility)
    /// </summary>
    public ScriptConformanceSummary RunTests(string scriptsFileContent, int maxFailuresToLog = 20)
        => RunDataTests(scriptsFileContent, maxFailuresToLog);
    
    #endregion
}

#region Summary Types

public struct ScriptConformanceSummary
{
    public int totalEvaluatedTests;
    public int passedTests;
    public int failedTests;
    public int skippedTests;
    public string? sampleFailures;
}

public struct ScriptAnalyzerTestSummary
{
    public int totalTests;
    public int passedTests;
    public int failedTests;
    public string? sampleFailures;
}

#endregion