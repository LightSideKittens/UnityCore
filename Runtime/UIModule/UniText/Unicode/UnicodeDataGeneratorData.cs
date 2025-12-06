#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using Debug = UnityEngine.Debug;

public class UnicodeDataGeneratorData : ScriptableObject
{
    [Header("Source Files")]
    public TextAsset derivedBidiClassAsset;
    public TextAsset derivedJoiningTypeAsset;
    public TextAsset arabicShapingAsset;
    public TextAsset bidiBracketsAsset;
    public TextAsset bidiMirroringAsset;
    public TextAsset scriptsAsset;
    public TextAsset lineBreakAsset;
    public TextAsset emojiDataAsset;
    public TextAsset generalCategoryAsset;
    public TextAsset eastAsianWidthAsset;
    public TextAsset graphemeBreakPropertyAsset;
    public TextAsset derivedCorePropertiesAsset;
    public TextAsset scriptExtensionsAsset;

    [Header("Output")]
    public DefaultAsset outputFolder;
    public string outputFileName = "UnicodeData.bytes";
    public bool useFormatV7 = true;

    [Header("Testing")]
    public TestData testing = new();
    
    
    public void OnGUI()
    {
        EditorGUILayout.LabelField("Unicode Data Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Source Files (Required)", EditorStyles.boldLabel);
        
        derivedBidiClassAsset = (TextAsset)EditorGUILayout.ObjectField(
            "DerivedBidiClass.txt",
            derivedBidiClassAsset,
            typeof(TextAsset),
            false);

        derivedJoiningTypeAsset = (TextAsset)EditorGUILayout.ObjectField(
            "DerivedJoiningType.txt",
            derivedJoiningTypeAsset,
            typeof(TextAsset),
            false);

        arabicShapingAsset = (TextAsset)EditorGUILayout.ObjectField(
            "ArabicShaping.txt",
            arabicShapingAsset,
            typeof(TextAsset),
            false);

        bidiBracketsAsset = (TextAsset)EditorGUILayout.ObjectField(
            "BidiBrackets.txt",
            bidiBracketsAsset,
            typeof(TextAsset),
            false);

        bidiMirroringAsset = (TextAsset)EditorGUILayout.ObjectField(
            "BidiMirroring.txt",
            bidiMirroringAsset,
            typeof(TextAsset),
            false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Source Files (Format V2+)", EditorStyles.boldLabel);

        scriptsAsset = (TextAsset)EditorGUILayout.ObjectField(
            "Scripts.txt",
            scriptsAsset,
            typeof(TextAsset),
            false);

        lineBreakAsset = (TextAsset)EditorGUILayout.ObjectField(
            "LineBreak.txt",
            lineBreakAsset,
            typeof(TextAsset),
            false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Source Files (Format V3+)", EditorStyles.boldLabel);

        emojiDataAsset = (TextAsset)EditorGUILayout.ObjectField(
            "emoji-data.txt",
            emojiDataAsset,
            typeof(TextAsset),
            false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Source Files (Format V4)", EditorStyles.boldLabel);

        generalCategoryAsset = (TextAsset)EditorGUILayout.ObjectField(
            "DerivedGeneralCategory.txt",
            generalCategoryAsset,
            typeof(TextAsset),
            false);

        eastAsianWidthAsset = (TextAsset)EditorGUILayout.ObjectField(
            "EastAsianWidth.txt",
            eastAsianWidthAsset,
            typeof(TextAsset),
            false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Source Files (Format V6)", EditorStyles.boldLabel);

        graphemeBreakPropertyAsset = (TextAsset)EditorGUILayout.ObjectField(
            "GraphemeBreakProperty.txt",
            graphemeBreakPropertyAsset,
            typeof(TextAsset),
            false);

        derivedCorePropertiesAsset = (TextAsset)EditorGUILayout.ObjectField(
            "DerivedCoreProperties.txt (InCB)",
            derivedCorePropertiesAsset,
            typeof(TextAsset),
            false);

        scriptExtensionsAsset = (TextAsset)EditorGUILayout.ObjectField(
            "ScriptExtensions.txt",
            scriptExtensionsAsset,
            typeof(TextAsset),
            false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);

        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Output Folder",
            outputFolder,
            typeof(DefaultAsset),
            false);

        outputFileName = EditorGUILayout.TextField("Output File Name", outputFileName);
        useFormatV7 = EditorGUILayout.Toggle("Use Format V7 (Full Unicode Properties)", useFormatV7);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }

        EditorGUILayout.Space();

        bool canGenerate = derivedBidiClassAsset != null &&
                          derivedJoiningTypeAsset != null &&
                          arabicShapingAsset != null &&
                          bidiBracketsAsset != null &&
                          bidiMirroringAsset != null &&
                          outputFolder != null;

        if (useFormatV7)
        {
            canGenerate = canGenerate && scriptsAsset != null && lineBreakAsset != null && 
                         emojiDataAsset != null && generalCategoryAsset != null && 
                         eastAsianWidthAsset != null && graphemeBreakPropertyAsset != null &&
                         derivedCorePropertiesAsset != null && scriptExtensionsAsset != null;
        }

        EditorGUI.BeginDisabledGroup(!canGenerate);
        if (GUILayout.Button("Generate Unicode Data", GUILayout.Height(30)))
        {
            GenerateUnicodeData();
        }
        EditorGUI.EndDisabledGroup();

        if (!canGenerate)
        {
            EditorGUILayout.HelpBox("Assign all required source files including GraphemeBreakProperty.txt and output folder to generate.",
                MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);
        testing.OnGui();
        
        if (GUILayout.Button("Copy", GUILayout.Height(30)))
        {
            var instance = ScriptableObject.CreateInstance<UnicodeDataGeneratorData>();
            instance.derivedBidiClassAsset = instance.derivedBidiClassAsset;
            instance.derivedJoiningTypeAsset = instance.derivedJoiningTypeAsset;
            instance.arabicShapingAsset = instance.arabicShapingAsset;
            instance.bidiBracketsAsset = instance.bidiBracketsAsset;
            instance.bidiMirroringAsset = instance.bidiMirroringAsset;
            instance.scriptsAsset = instance.scriptsAsset;
            instance.lineBreakAsset = instance.lineBreakAsset;
            instance.emojiDataAsset = instance.emojiDataAsset;
            instance.generalCategoryAsset = instance.generalCategoryAsset;
            instance.eastAsianWidthAsset = instance.eastAsianWidthAsset;
            instance.graphemeBreakPropertyAsset = instance.graphemeBreakPropertyAsset;
            instance.testing = testing;
            
            AssetDatabase.CreateAsset(instance, "Assets/UnicodeDataGeneratorData.asset");

        }
    }

    private void GenerateUnicodeData()
    {
        try
        {
            string folderPath = AssetDatabase.GetAssetPath(outputFolder);
            string outputPath = Path.Combine(folderPath, outputFileName);

            // Create temp files
            string tempDir = Path.Combine(Application.temporaryCachePath, "UnicodeGen");
            Directory.CreateDirectory(tempDir);

            string derivedBidiPath = SaveTempFile(tempDir, "DerivedBidiClass.txt", derivedBidiClassAsset);
            string derivedJoiningPath = SaveTempFile(tempDir, "DerivedJoiningType.txt", derivedJoiningTypeAsset);
            string arabicShapingPath = SaveTempFile(tempDir, "ArabicShaping.txt", arabicShapingAsset);
            string bidiBracketsPath = SaveTempFile(tempDir, "BidiBrackets.txt", bidiBracketsAsset);
            string bidiMirroringPath = SaveTempFile(tempDir, "BidiMirroring.txt", bidiMirroringAsset);

            // Build data
            var builder = new UnicodeDataBuilder();
            builder.LoadDerivedBidiClass(derivedBidiPath);
            builder.LoadDerivedJoiningType(derivedJoiningPath);
            builder.LoadArabicShaping(arabicShapingPath);

            var ranges = builder.BuildRangeEntries();
            var mirrors = UnicodeDataBuilder.BuildMirrorEntries(bidiMirroringPath);
            var brackets = UnicodeDataBuilder.BuildBracketEntries(bidiBracketsPath);

            // Unicode version (17.0.0 = 0x110000)
            int unicodeVersion = 0x110000;

            if (useFormatV7 && scriptsAsset != null && lineBreakAsset != null && 
                emojiDataAsset != null && generalCategoryAsset != null && 
                eastAsianWidthAsset != null && graphemeBreakPropertyAsset != null &&
                derivedCorePropertiesAsset != null && scriptExtensionsAsset != null)
            {
                string scriptsPath = SaveTempFile(tempDir, "Scripts.txt", scriptsAsset);
                string lineBreakPath = SaveTempFile(tempDir, "LineBreak.txt", lineBreakAsset);
                string emojiDataPath = SaveTempFile(tempDir, "emoji-data.txt", emojiDataAsset);
                string generalCategoryPath = SaveTempFile(tempDir, "DerivedGeneralCategory.txt", generalCategoryAsset);
                string eastAsianWidthPath = SaveTempFile(tempDir, "EastAsianWidth.txt", eastAsianWidthAsset);
                string graphemeBreakPath = SaveTempFile(tempDir, "GraphemeBreakProperty.txt", graphemeBreakPropertyAsset);
                string derivedCorePropertiesPath = SaveTempFile(tempDir, "DerivedCoreProperties.txt", derivedCorePropertiesAsset);
                string scriptExtensionsPath = SaveTempFile(tempDir, "ScriptExtensions.txt", scriptExtensionsAsset);

                builder.LoadScripts(scriptsPath);
                builder.LoadLineBreak(lineBreakPath);
                builder.LoadEmojiData(emojiDataPath);
                builder.LoadGeneralCategory(generalCategoryPath);
                builder.LoadEastAsianWidth(eastAsianWidthPath);
                builder.LoadGraphemeBreakProperty(graphemeBreakPath);
                builder.LoadIndicConjunctBreak(derivedCorePropertiesPath);
                builder.LoadScriptExtensions(scriptExtensionsPath);

                var scripts = builder.BuildScriptRangeEntries();
                var lineBreaks = builder.BuildLineBreakRangeEntries();
                var extendedPictographics = builder.BuildExtendedPictographicRangeEntries();
                var generalCategories = builder.BuildGeneralCategoryRangeEntries();
                var eastAsianWidths = builder.BuildEastAsianWidthRangeEntries();
                var graphemeBreaks = builder.BuildGraphemeBreakRangeEntries();
                var indicConjunctBreaks = builder.BuildIndicConjunctBreakRangeEntries();
                var scriptExtensions = builder.GetScriptExtensionEntries();

                UnicodeBinaryWriter.WriteBinary(
                    outputPath, ranges, mirrors, brackets, scripts, lineBreaks, 
                    extendedPictographics, generalCategories, eastAsianWidths, graphemeBreaks,
                    indicConjunctBreaks, scriptExtensions, unicodeVersion);

                Debug.Log($"Generated Unicode data (Format V7) with {ranges.Count} ranges, " +
                          $"{mirrors.Count} mirrors, {brackets.Count} brackets, " +
                          $"{scripts.Count} script ranges, {lineBreaks.Count} line break ranges, " +
                          $"{extendedPictographics.Count} Extended_Pictographic ranges, " +
                          $"{generalCategories.Count} GeneralCategory ranges, " +
                          $"{eastAsianWidths.Count} EastAsianWidth ranges, " +
                          $"{graphemeBreaks.Count} GraphemeBreak ranges, " +
                          $"{indicConjunctBreaks.Count} InCB ranges, " +
                          $"{scriptExtensions.Count} ScriptExtension entries.");
            }
            else
            {
                UnicodeBinaryWriter.WriteBinaryV1(
                    outputPath, ranges, mirrors, brackets, unicodeVersion);

                Debug.Log($"Generated Unicode data (Format V1) with {ranges.Count} ranges, " +
                          $"{mirrors.Count} mirrors, {brackets.Count} brackets.");
            }

            // Cleanup temp files
            Directory.Delete(tempDir, true);

            AssetDatabase.Refresh();
            Debug.Log($"Unicode data saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to generate Unicode data: {ex}");
        }
    }

    private string SaveTempFile(string dir, string name, TextAsset asset)
    {
        string path = Path.Combine(dir, name);
        File.WriteAllText(path, asset.text);
        return path;
    }
}
#endif