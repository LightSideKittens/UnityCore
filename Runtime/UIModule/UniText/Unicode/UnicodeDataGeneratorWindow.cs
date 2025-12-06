#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Serializable]
public class TestData
    {
        [SerializeField] private int maxFailuresToLog = 10;
        [SerializeField] private TextAsset unicodeDataAsset;
        [SerializeField] private TextAsset bidiCharacterTestAsset;
        [SerializeField] private TextAsset lineBreakTestAsset;
        [SerializeField] private TextAsset scriptsAsset;
        [SerializeField] private TextAsset scriptAnalyzerTestAsset;
        [SerializeField] private TextAsset graphemeBreakTestAsset;

        public void RunBidiCharacterTests()
        {
            if (unicodeDataAsset == null || bidiCharacterTestAsset == null)
            {
                Debug.LogError("Assign unicodeDataAsset and bidiCharacterTestAsset.");
                return;
            }

            try
            {
                var provider = new BinaryUnicodeDataProvider(unicodeDataAsset.bytes);
                var engine = new BidiEngine(provider);
                var runner = new BidiConformanceRunner(engine);
                var summary = runner.RunBidiCharacterTests(bidiCharacterTestAsset.text, maxFailuresToLog);

                LogSummary("BidiCharacterTest", summary.passedTests, summary.failedTests, 
                    summary.skippedTests, summary.sampleFailures);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while running BidiCharacterTest: {ex}");
            }
        }

        public void RunLineBreakTests()
        {
            if (unicodeDataAsset == null || lineBreakTestAsset == null)
            {
                Debug.LogError("Assign unicodeDataAsset and lineBreakTestAsset.");
                return;
            }

            try
            {
                var provider = new BinaryUnicodeDataProvider(unicodeDataAsset.bytes);
                var runner = new LineBreakConformanceRunner(provider);
                var summary = runner.RunTests(lineBreakTestAsset.text, maxFailuresToLog);

                LogSummary("LineBreakTest", summary.passedTests, summary.failedTests,
                    summary.skippedTests, summary.sampleFailures);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while running LineBreakTest: {ex}");
            }
        }

        public void RunScriptTests()
        {
            if (unicodeDataAsset == null)
            {
                Debug.LogError("Assign unicodeDataAsset.");
                return;
            }

            if (scriptsAsset == null && scriptAnalyzerTestAsset == null)
            {
                Debug.LogError("Assign scriptsAsset and/or scriptAnalyzerTestAsset.");
                return;
            }

            try
            {
                var provider = new BinaryUnicodeDataProvider(unicodeDataAsset.bytes);
                var runner = new ScriptConformanceRunner(provider);

                // 1. Test data (Scripts.txt)
                if (scriptsAsset != null)
                {
                    var dataSummary = runner.RunDataTests(scriptsAsset.text, maxFailuresToLog);
                    LogSummary("ScriptDataTest", dataSummary.passedTests, dataSummary.failedTests,
                        dataSummary.skippedTests, dataSummary.sampleFailures);
                }

                // 2. Test analyzer (ScriptAnalyzerTest.txt)
                if (scriptAnalyzerTestAsset != null)
                {
                    var analyzer = new Tekst.ScriptAnalyzer(provider);
                    var analyzerSummary = runner.RunAnalyzerTests(analyzer, scriptAnalyzerTestAsset.text, maxFailuresToLog);
                    LogSummary("ScriptAnalyzerTest", analyzerSummary.passedTests, analyzerSummary.failedTests,
                        0, analyzerSummary.sampleFailures);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while running ScriptTests: {ex}");
            }
        }

        public void RunGraphemeTests()
        {
            if (unicodeDataAsset == null || graphemeBreakTestAsset == null)
            {
                Debug.LogError("Assign unicodeDataAsset and graphemeBreakTestAsset.");
                return;
            }

            try
            {
                var provider = new BinaryUnicodeDataProvider(unicodeDataAsset.bytes);
                var breaker = new GraphemeBreaker(provider);
                var runner = new GraphemeConformanceRunner(breaker);
                var summary = runner.RunTests(graphemeBreakTestAsset.text, maxFailuresToLog);

                LogSummary("GraphemeBreakTest", summary.passedTests, summary.failedTests,
                    summary.skippedTests, summary.sampleFailures);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while running GraphemeTests: {ex}");
            }
        }

        public void RunAllTests()
        {
            if (unicodeDataAsset == null)
            {
                Debug.LogError("Assign unicodeDataAsset first.");
                return;
            }

            Debug.Log("=== Running All Unicode Conformance Tests ===");

            if (bidiCharacterTestAsset != null)
                RunBidiCharacterTests();
            else
                Debug.LogWarning("Skipping BiDi tests: bidiCharacterTestAsset not assigned");

            if (lineBreakTestAsset != null)
                RunLineBreakTests();
            else
                Debug.LogWarning("Skipping LineBreak tests: lineBreakTestAsset not assigned");

            if (scriptsAsset != null || scriptAnalyzerTestAsset != null)
                RunScriptTests();
            else
                Debug.LogWarning("Skipping Script tests: scriptsAsset and scriptAnalyzerTestAsset not assigned");

            if (graphemeBreakTestAsset != null)
                RunGraphemeTests();
            else
                Debug.LogWarning("Skipping Grapheme tests: graphemeBreakTestAsset not assigned");

            Debug.Log("=== All Tests Complete ===");
        }

        private void LogSummary(string testName, int passed, int failed, int skipped, string? sampleFailures)
        {
            string log = $"{testName}: Passed={passed}, Failed={failed}, Skipped={skipped}";
            
            if (failed > 0 && !string.IsNullOrEmpty(sampleFailures))
            {
                log += $"\nSample Failures:\n{sampleFailures}";
                Debug.LogWarning(log);
            }
            else
            {
                Debug.Log(log);
            }

            // Save to file
            string filePath = Path.Combine(Application.persistentDataPath, $"{testName}Results.txt");
            File.WriteAllText(filePath, log);
        }

        public void OnGui()
        {
            maxFailuresToLog = EditorGUILayout.IntField("Max Failures To Log", maxFailuresToLog);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test Data Assets", EditorStyles.miniBoldLabel);

            unicodeDataAsset = (TextAsset)EditorGUILayout.ObjectField(
                "UnicodeData.bytes",
                unicodeDataAsset,
                typeof(TextAsset),
                false);

            bidiCharacterTestAsset = (TextAsset)EditorGUILayout.ObjectField(
                "BidiCharacterTest.txt",
                bidiCharacterTestAsset,
                typeof(TextAsset),
                false);

            lineBreakTestAsset = (TextAsset)EditorGUILayout.ObjectField(
                "LineBreakTest.txt",
                lineBreakTestAsset,
                typeof(TextAsset),
                false);

            scriptsAsset = (TextAsset)EditorGUILayout.ObjectField(
                "Scripts.txt",
                scriptsAsset,
                typeof(TextAsset),
                false);

            scriptAnalyzerTestAsset = (TextAsset)EditorGUILayout.ObjectField(
                "ScriptAnalyzerTest.txt",
                scriptAnalyzerTestAsset,
                typeof(TextAsset),
                false);

            graphemeBreakTestAsset = (TextAsset)EditorGUILayout.ObjectField(
                "GraphemeBreakTest.txt",
                graphemeBreakTestAsset,
                typeof(TextAsset),
                false);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Run All Tests"))
            {
                var sw = new Stopwatch();
                sw.Start();
                RunAllTests();
                Debug.Log(sw.ElapsedTicks);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(unicodeDataAsset == null || bidiCharacterTestAsset == null);
            if (GUILayout.Button("BiDi"))
                RunBidiCharacterTests();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(unicodeDataAsset == null || lineBreakTestAsset == null);
            if (GUILayout.Button("LineBreak"))
                RunLineBreakTests();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(unicodeDataAsset == null || (scriptsAsset == null && scriptAnalyzerTestAsset == null));
            if (GUILayout.Button("Script"))
                RunScriptTests();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(unicodeDataAsset == null || graphemeBreakTestAsset == null);
            if (GUILayout.Button("Grapheme"))
                RunGraphemeTests();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }
    }

public class UnicodeDataGeneratorWindow : EditorWindow
{
    public UnicodeDataGeneratorData data;

    [MenuItem("Tools/RTL/Unicode Data Generator")]
    public static void ShowWindow()
    {
        UnicodeDataGeneratorWindow window = GetWindow<UnicodeDataGeneratorWindow>();
        window.titleContent = new GUIContent("Unicode Data Generator");
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        data = EditorGUILayout.ObjectField("Data Generator", data, typeof(UnicodeDataGeneratorData), false) as UnicodeDataGeneratorData;

        if (data != null)
        {
            data.OnGUI();
        }
    }

}
#endif