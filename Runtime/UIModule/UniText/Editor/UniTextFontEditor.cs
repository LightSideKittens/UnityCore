using System.IO;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(UniTextFont))]
public class UniTextFontEditor : Editor
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
    private SerializedProperty italicStyleProp;

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
        italicStyleProp = serializedObject.FindProperty("italicStyle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var fontAsset = (UniTextFont)target;

        BeginSection("Source Font");
        DrawSourceFontContent(fontAsset);
        EndSection();

        BeginSection("Font Data Status");
        DrawFontDataStatusContent(fontAsset);
        EndSection();

        BeginSection("Face Info");
        DrawFlatProperties(faceInfoProp);
        EditorGUILayout.PropertyField(italicStyleProp);
        EndSection();

        BeginSection("Atlas Settings");
        EditorGUILayout.PropertyField(atlasWidthProp);
        EditorGUILayout.PropertyField(atlasHeightProp);
        EditorGUILayout.PropertyField(atlasPaddingProp);
        EditorGUILayout.PropertyField(atlasRenderModeProp);
        EndSection();

        BeginSection("Dynamic Data");
        DrawDynamicDataContent(fontAsset);
        EndSection();

        serializedObject.ApplyModifiedProperties();

        var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.LabelField("Made with ❤️ by Light Side", style);
        EditorGUILayout.Space(-4);
    }

    private void BeginSection(string label)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    private void EndSection()
    {
        EditorGUILayout.EndVertical();
    }

    private void DrawFlatProperties(SerializedProperty property)
    {
        var iterator = property.Copy();
        var endProperty = property.GetEndProperty();

        iterator.NextVisible(true);

        while (!SerializedProperty.EqualContents(iterator, endProperty))
        {
            EditorGUILayout.PropertyField(iterator, true);
            if (!iterator.NextVisible(false))
                break;
        }
    }

    private void DrawSourceFontContent(UniTextFont uniTextFont)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sourceFontProp, new GUIContent("Source Font File"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            var font = sourceFontProp.objectReferenceValue as Font;
            if (font != null)
            {
                ExtractFontBytes(uniTextFont, font);
            }
        }

        EditorGUILayout.PropertyField(atlasPopulationModeProp);

        EditorGUILayout.Space(5);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Extract Font Bytes", GUILayout.Height(25)))
            {
                var font = sourceFontProp.objectReferenceValue as Font;
                if (font != null)
                {
                    ExtractFontBytes(uniTextFont, font);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No source font assigned.", "OK");
                }
            }

            if (GUILayout.Button("Load from File...", GUILayout.Height(25)))
            {
                LoadFontFromFile(uniTextFont);
            }
        }
    }

    private void DrawFontDataStatusContent(UniTextFont font)
    {
        var hasData = font.HasFontData;
        var statusColor = hasData ? new Color(0.33f, 1f, 0.39f) : new Color(1f, 0.35f, 0.28f);
        var statusText = hasData
            ? $"✓ Font data loaded ({font.FontData.Length:N0} bytes)"
            : "✗ No font data - TEXT WILL NOT RENDER!";

        var statusStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = statusColor } };
        EditorGUILayout.LabelField(statusText, statusStyle);

        if (hasData)
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

    private void DrawDynamicDataContent(UniTextFont font)
    {
        var glyphTableProp = serializedObject.FindProperty("glyphTable");
        var characterTableProp = serializedObject.FindProperty("characterTable");
        var clearDynamicDataOnBuildProp = serializedObject.FindProperty("clearDynamicDataOnBuild");

        int glyphCount = glyphTableProp?.arraySize ?? 0;
        int charCount = characterTableProp?.arraySize ?? 0;
        int atlasCount = atlasTexturesProp?.arraySize ?? 0;

        EditorGUILayout.LabelField("Statistics", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Glyphs: {glyphCount}  |  Characters: {charCount}  |  Atlas textures: {atlasCount}");

        if (font.AtlasTexture != null)
        {
            var tex = font.AtlasTexture;
            long sizeBytes = tex.width * tex.height;
            if (tex.format == TextureFormat.RGBA32)
                sizeBytes *= 4;
            string sizeStr = sizeBytes > 1024 * 1024
                ? $"{sizeBytes / (1024f * 1024f):F1} MB"
                : $"{sizeBytes / 1024f:F1} KB";
            EditorGUILayout.LabelField($"Atlas size: {tex.width}x{tex.height} ({sizeStr})");
        }

        EditorGUILayout.Space(5);

        if (clearDynamicDataOnBuildProp != null)
        {
            EditorGUILayout.PropertyField(clearDynamicDataOnBuildProp,
                new GUIContent("Clear On Build",
                    "When enabled, all dynamically generated glyphs and atlas data will be cleared before build. " +
                    "The atlas will regenerate at runtime as characters are needed. " +
                    "This significantly reduces build size."));
        }

        EditorGUILayout.Space(5);

        GUI.backgroundColor = new Color(1f, 0.47f, 0.47f);
        if (GUILayout.Button("Clear Dynamic Data", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear Dynamic Data",
                $"This will clear {glyphCount} glyphs, {charCount} characters, and reset the atlas texture.\n\n" +
                "The atlas will regenerate at runtime as characters are requested.\n\n" +
                "Are you sure?", "Clear", "Cancel"))
            {
                Undo.RecordObject(font, "Clear Dynamic Data");
                font.ClearDynamicData();
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssets();
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void ExtractFontBytes(UniTextFont uniTextFont, Font font)
    {
        string fontPath = AssetDatabase.GetAssetPath(font);
        if (string.IsNullOrEmpty(fontPath))
        {
            EditorUtility.DisplayDialog("Error",
                "Cannot find font asset path. Make sure the font is imported in the project.",
                "OK");
            return;
        }

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

            Undo.RecordObject(uniTextFont, "Extract Font Bytes");
            uniTextFont.SetFontData(fontBytes);
            uniTextFont.UpdateFromSourceFont();
            EditorUtility.SetDirty(uniTextFont);

            Debug.Log($"UniTextFontAsset: Extracted {fontBytes.Length:N0} bytes from '{font.name}'");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error",
                $"Failed to read font file: {ex.Message}",
                "OK");
        }
    }

    private void LoadFontFromFile(UniTextFont font)
    {
        string path = EditorUtility.OpenFilePanel("Select Font File", "", "ttf,otf,ttc");
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            byte[] fontBytes = File.ReadAllBytes(path);

            Undo.RecordObject(font, "Load Font from File");
            font.SetFontData(fontBytes);

            var sourceFontFilePathProp = serializedObject.FindProperty("sourceFontFilePath");
            sourceFontFilePathProp.stringValue = path;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(font);

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

        string fontPath = AssetDatabase.GetAssetPath(font);
        string fullPath = Path.GetFullPath(fontPath);

        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error", "Cannot find font file.", "OK");
            return;
        }

        byte[] fontBytes = File.ReadAllBytes(fullPath);

        var fontAsset = UniTextFont.CreateFontAsset(fontBytes);
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create font asset.", "OK");
            return;
        }

        fontAsset.sourceFont = font;

        string directory = Path.GetDirectoryName(fontPath);
        string assetPath = Path.Combine(directory, font.name + " UniText.asset").Replace("\\", "/");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(fontAsset, assetPath);

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


    [MenuItem("Assets/Create/UniText/Font Asset", false, 100)]
    private static void CreateFontAsset()
    {
        var selectedFont = Selection.activeObject as Font;

        if (selectedFont != null)
        {
            CreateFontAssetFromFont();
        }
        else
        {
            CreateEmptyFontAssetWithSubAssets();
        }
    }

    private static void CreateEmptyFontAssetWithSubAssets()
    {
        var fontAsset = ScriptableObject.CreateInstance<UniTextFont>();

        var texture = new Texture2D(1, 1, TextureFormat.Alpha8, false);
        texture.name = "Atlas";
        fontAsset.AtlasTextures = new[] { texture };

        string directory = GetSelectedDirectory();
        string assetPath = Path.Combine(directory, "New UniTextFontAsset.asset").Replace("\\", "/");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(fontAsset, assetPath);

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
