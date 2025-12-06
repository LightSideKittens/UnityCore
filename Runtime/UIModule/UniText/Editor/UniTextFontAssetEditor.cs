using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for UniTextFontAsset.
/// Handles font byte extraction from source Font asset.
/// </summary>
[CustomEditor(typeof(UniTextFontAsset))]
public class UniTextFontAssetEditor : Editor
{
    private SerializedProperty fontDataProp;
    private SerializedProperty sourceFontProp;
    private SerializedProperty sourceFontFilePathProp;
    private SerializedProperty atlasPopulationModeProp;
    private SerializedProperty faceInfoProp;
    private SerializedProperty atlasWidthProp;
    private SerializedProperty atlasHeightProp;
    private SerializedProperty atlasPaddingProp;
    private SerializedProperty atlasRenderModeProp;
    private SerializedProperty atlasTexturesProp;
    private SerializedProperty materialProp;
    private SerializedProperty fallbackFontAssetTableProp;

    private bool showFaceInfo;
    private bool showAtlasSettings = true;
    private bool showFallbacks;

    private void OnEnable()
    {
        fontDataProp = serializedObject.FindProperty("fontData");
        sourceFontProp = serializedObject.FindProperty("sourceFont");
        sourceFontFilePathProp = serializedObject.FindProperty("sourceFontFilePath");
        atlasPopulationModeProp = serializedObject.FindProperty("atlasPopulationMode");
        faceInfoProp = serializedObject.FindProperty("faceInfo");
        atlasWidthProp = serializedObject.FindProperty("atlasWidth");
        atlasHeightProp = serializedObject.FindProperty("atlasHeight");
        atlasPaddingProp = serializedObject.FindProperty("atlasPadding");
        atlasRenderModeProp = serializedObject.FindProperty("atlasRenderMode");
        atlasTexturesProp = serializedObject.FindProperty("atlasTextures");
        materialProp = serializedObject.FindProperty("material");
        fallbackFontAssetTableProp = serializedObject.FindProperty("fallbackFontAssetTable");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var fontAsset = (UniTextFontAsset)target;

        EditorGUILayout.Space();

        // Source Font Section
        DrawSourceFontSection(fontAsset);

        EditorGUILayout.Space();

        // Font Data Status
        DrawFontDataStatus(fontAsset);

        EditorGUILayout.Space();

        // Face Info
        DrawFaceInfoSection();

        EditorGUILayout.Space();

        // Atlas Settings
        DrawAtlasSettingsSection();

        EditorGUILayout.Space();

        // Material
        EditorGUILayout.PropertyField(materialProp);

        EditorGUILayout.Space();

        // Fallback Fonts
        DrawFallbacksSection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSourceFontSection(UniTextFontAsset fontAsset)
    {
        EditorGUILayout.LabelField("Source Font", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sourceFontProp, new GUIContent("Source Font File"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            var font = sourceFontProp.objectReferenceValue as Font;
            if (font != null)
            {
                ExtractFontBytes(fontAsset, font);
            }
        }

        EditorGUILayout.PropertyField(atlasPopulationModeProp);

        // Manual extraction button
        EditorGUILayout.Space(5);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Extract Font Bytes", GUILayout.Height(25)))
            {
                var font = sourceFontProp.objectReferenceValue as Font;
                if (font != null)
                {
                    ExtractFontBytes(fontAsset, font);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No source font assigned.", "OK");
                }
            }

            if (GUILayout.Button("Load from File...", GUILayout.Height(25)))
            {
                LoadFontFromFile(fontAsset);
            }
        }
    }

    private void DrawFontDataStatus(UniTextFontAsset fontAsset)
    {
        EditorGUILayout.LabelField("Font Data Status", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            var hasData = fontAsset.HasFontData;
            var statusColor = hasData ? Color.green : Color.red;
            var statusText = hasData
                ? $"✓ Font data loaded ({fontAsset.FontData.Length:N0} bytes)"
                : "✗ No font data - TEXT WILL NOT RENDER!";

            var style = new GUIStyle(EditorStyles.label) { normal = { textColor = statusColor } };
            EditorGUILayout.LabelField(statusText, style);
        }

        if (fontAsset.HasFontData)
        {
            EditorGUILayout.HelpBox(
                "Raw font bytes are available for HarfBuzz shaping. " +
                "This data will be included in the build.",
                MessageType.Info);
        }
        else
        {
            var sourceFont = sourceFontProp.objectReferenceValue as Font;
            if (sourceFont != null)
            {
                EditorGUILayout.HelpBox(
                    "Source font is assigned but font bytes are not extracted! " +
                    "Click 'Extract Font Bytes' to fix this.",
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No font data. Assign a Source Font (bytes will be extracted automatically) " +
                    "or use 'Load from File...' to load a .ttf/.otf file.",
                    MessageType.Warning);
            }
        }
    }

    private void DrawFaceInfoSection()
    {
        showFaceInfo = EditorGUILayout.Foldout(showFaceInfo, "Face Info", true);
        if (showFaceInfo)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(faceInfoProp, true);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawAtlasSettingsSection()
    {
        showAtlasSettings = EditorGUILayout.Foldout(showAtlasSettings, "Atlas Settings", true);
        if (showAtlasSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(atlasWidthProp);
            EditorGUILayout.PropertyField(atlasHeightProp);
            EditorGUILayout.PropertyField(atlasPaddingProp);
            EditorGUILayout.PropertyField(atlasRenderModeProp);
            EditorGUILayout.PropertyField(atlasTexturesProp, true);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawFallbacksSection()
    {
        showFallbacks = EditorGUILayout.Foldout(showFallbacks, "Fallback Font Assets", true);
        if (showFallbacks)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(fallbackFontAssetTableProp, true);
            EditorGUI.indentLevel--;
        }
    }

    private void ExtractFontBytes(UniTextFontAsset fontAsset, Font font)
    {
        // Get the asset path of the font
        string fontPath = AssetDatabase.GetAssetPath(font);
        if (string.IsNullOrEmpty(fontPath))
        {
            EditorUtility.DisplayDialog("Error",
                "Cannot find font asset path. Make sure the font is imported in the project.",
                "OK");
            return;
        }

        // Read the font file bytes
        string fullPath = Path.GetFullPath(fontPath);
        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error",
                $"Font file not found at: {fullPath}",
                "OK");
            return;
        }

        try
        {
            byte[] fontBytes = File.ReadAllBytes(fullPath);

            Undo.RecordObject(fontAsset, "Extract Font Bytes");
            fontAsset.SetFontData(fontBytes);
            fontAsset.UpdateFromSourceFont();
            EditorUtility.SetDirty(fontAsset);

            Debug.Log($"UniTextFontAsset: Extracted {fontBytes.Length:N0} bytes from '{font.name}'");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error",
                $"Failed to read font file: {ex.Message}",
                "OK");
        }
    }

    private void LoadFontFromFile(UniTextFontAsset fontAsset)
    {
        string path = EditorUtility.OpenFilePanel("Select Font File", "", "ttf,otf,ttc");
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            byte[] fontBytes = File.ReadAllBytes(path);

            Undo.RecordObject(fontAsset, "Load Font from File");
            fontAsset.SetFontData(fontBytes);

            // Store the source path
            var sourceFontFilePathProp = serializedObject.FindProperty("sourceFontFilePath");
            sourceFontFilePathProp.stringValue = path;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(fontAsset);

            Debug.Log($"UniTextFontAsset: Loaded {fontBytes.Length:N0} bytes from '{Path.GetFileName(path)}'");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error",
                $"Failed to read font file: {ex.Message}",
                "OK");
        }
    }

    private static void CreateFontAssetFromFont()
    {
        var font = Selection.activeObject as Font;
        if (font == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a Font asset first.", "OK");
            return;
        }

        // Get font bytes
        string fontPath = AssetDatabase.GetAssetPath(font);
        string fullPath = Path.GetFullPath(fontPath);

        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error", "Cannot find font file.", "OK");
            return;
        }

        byte[] fontBytes = File.ReadAllBytes(fullPath);

        // Create asset
        var fontAsset = UniTextFontAsset.CreateFontAsset(fontBytes);
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create font asset.", "OK");
            return;
        }

        fontAsset.sourceFont = font;

        // Save asset
        string directory = Path.GetDirectoryName(fontPath);
        string assetPath = Path.Combine(directory, font.name + " UniText.asset").Replace("\\", "/");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(fontAsset, assetPath);

        // Save sub-assets (material, textures)
        if (fontAsset.Material != null)
        {
            fontAsset.Material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.Material, fontAsset);
        }

        if (fontAsset.AtlasTextures != null)
        {
            for (int i = 0; i < fontAsset.AtlasTextures.Length; i++)
            {
                if (fontAsset.AtlasTextures[i] != null)
                {
                    fontAsset.AtlasTextures[i].name = font.name + " Atlas " + i;
                    AssetDatabase.AddObjectToAsset(fontAsset.AtlasTextures[i], fontAsset);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = fontAsset;
        EditorGUIUtility.PingObject(fontAsset);

        Debug.Log($"Created UniTextFontAsset: {assetPath}");
    }

    /// <summary>
    /// Create font asset. Auto-populates sourceFont if a Font is selected.
    /// </summary>
    [MenuItem("Assets/Create/UniText/Font Asset", false, 100)]
    private static void CreateFontAsset()
    {
        var selectedFont = Selection.activeObject as Font;

        if (selectedFont != null)
        {
            // Create from selected Font with all SubAssets
            CreateFontAssetFromFont();
        }
        else
        {
            // Create empty asset with default Material and Texture
            CreateEmptyFontAssetWithSubAssets();
        }
    }

    private static void CreateEmptyFontAssetWithSubAssets()
    {
        var fontAsset = ScriptableObject.CreateInstance<UniTextFontAsset>();

        // Create default atlas texture
        var texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
        texture.name = "Atlas";
        fontAsset.AtlasTextures = new[] { texture };

        // Create default material
        var shader = Shader.Find("UniText/Mobile/Distance Field");
        if (shader == null)
            shader = Shader.Find("GUI/Text Shader");

        if (shader != null)
        {
            var material = new Material(shader);
            material.name = "Material";
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_TextureWidth", 1024);
            material.SetFloat("_TextureHeight", 1024);
            material.SetFloat("_GradientScale", 10);
            fontAsset.Material = material;
        }

        // Determine save path
        string directory = GetSelectedDirectory();
        string assetPath = Path.Combine(directory, "New UniTextFontAsset.asset").Replace("\\", "/");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        // Save main asset
        AssetDatabase.CreateAsset(fontAsset, assetPath);

        // Add SubAssets
        if (fontAsset.Material != null)
            AssetDatabase.AddObjectToAsset(fontAsset.Material, fontAsset);

        if (texture != null)
            AssetDatabase.AddObjectToAsset(texture, fontAsset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = fontAsset;
        EditorGUIUtility.PingObject(fontAsset);

        Debug.Log($"Created UniTextFontAsset: {assetPath}");
    }

    private static string GetSelectedDirectory()
    {
        string path = "Assets";

        foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                path = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(path))
                break;
        }

        return path;
    }
}
