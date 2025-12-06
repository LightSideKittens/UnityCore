using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Population mode for font atlas.
/// </summary>
public enum UniTextAtlasPopulationMode
{
    Static = 0,
    Dynamic = 1
}

/// <summary>
/// Font asset for UniText with raw bytes support for HarfBuzz shaping.
/// Replaces TMP_FontAsset for full Unicode compliance.
/// </summary>
[Serializable]
public class UniTextFontAsset : ScriptableObject
{
    #region Serialized Fields

    /// <summary>
    /// Raw font bytes for HarfBuzz and FontEngine.
    /// This is the primary font data used at runtime.
    /// </summary>
    [SerializeField]
    private byte[] fontData;

    /// <summary>
    /// Source font file path (for DynamicOS mode).
    /// </summary>
    [SerializeField]
    private string sourceFontFilePath;

    /// <summary>
    /// Atlas population mode.
    /// </summary>
    [SerializeField]
    private UniTextAtlasPopulationMode atlasPopulationMode = UniTextAtlasPopulationMode.Dynamic;

    /// <summary>
    /// Font face information.
    /// </summary>
    [SerializeField]
    internal FaceInfo faceInfo;

    /// <summary>
    /// Glyph table.
    /// </summary>
    [SerializeField]
    internal List<Glyph> glyphTable = new();

    /// <summary>
    /// Character table.
    /// </summary>
    [SerializeField]
    internal List<UniTextCharacter> characterTable = new();

    /// <summary>
    /// Atlas textures.
    /// </summary>
    [SerializeField]
    internal Texture2D[] atlasTextures;

    /// <summary>
    /// Current atlas texture index.
    /// </summary>
    [SerializeField]
    internal int atlasTextureIndex;

    /// <summary>
    /// Atlas width.
    /// </summary>
    [SerializeField]
    internal int atlasWidth = 1024;

    /// <summary>
    /// Atlas height.
    /// </summary>
    [SerializeField]
    internal int atlasHeight = 1024;

    /// <summary>
    /// Atlas padding.
    /// </summary>
    [SerializeField]
    internal int atlasPadding = 9;

    /// <summary>
    /// Atlas render mode.
    /// </summary>
    [SerializeField]
    internal GlyphRenderMode atlasRenderMode = GlyphRenderMode.SDFAA;

    /// <summary>
    /// Used glyph rects for atlas packing.
    /// </summary>
    [SerializeField]
    private List<GlyphRect> usedGlyphRects = new();

    /// <summary>
    /// Free glyph rects for atlas packing.
    /// </summary>
    [SerializeField]
    private List<GlyphRect> freeGlyphRects = new();

    /// <summary>
    /// Material for rendering.
    /// </summary>
    [SerializeField]
    internal Material material;

    /// <summary>
    /// Fallback font assets.
    /// </summary>
    [SerializeField]
    internal List<UniTextFontAsset> fallbackFontAssetTable;

    /// <summary>
    /// Multi-atlas support.
    /// </summary>
    [SerializeField]
    private bool isMultiAtlasTexturesEnabled = true;

    #endregion

    #region Runtime Fields

    // Lookup dictionaries (built at runtime)
    internal Dictionary<uint, Glyph> glyphLookupDictionary;
    internal Dictionary<uint, UniTextCharacter> characterLookupDictionary;

    // Working lists for dynamic character addition
    private List<UniTextCharacter> charactersToAdd = new();
    private HashSet<uint> charactersToAddLookup = new();
    private List<uint> glyphIndexList = new();
    private List<uint> glyphIndexListNewlyAdded = new();

    // Cached temp font file path (for runtime loading)
    private string cachedTempFontPath;

    // Flag to track if we need cleanup
    private bool tempFileCreated;

    #endregion

    #region Properties

    /// <summary>
    /// Raw font bytes (for HarfBuzz shaping).
    /// </summary>
    public byte[] FontData => fontData;

    /// <summary>
    /// Has raw font data available.
    /// </summary>
    public bool HasFontData => fontData != null && fontData.Length > 0;

    /// <summary>
    /// Font face information.
    /// </summary>
    public FaceInfo FaceInfo
    {
        get => faceInfo;
        internal set => faceInfo = value;
    }

    /// <summary>
    /// Atlas population mode.
    /// </summary>
    public UniTextAtlasPopulationMode AtlasPopulationMode
    {
        get => atlasPopulationMode;
        set => atlasPopulationMode = value;
    }

    /// <summary>
    /// Main atlas texture.
    /// </summary>
    public Texture2D AtlasTexture
    {
        get
        {
            if (atlasTextures != null && atlasTextures.Length > 0)
                return atlasTextures[0];
            return null;
        }
    }

    /// <summary>
    /// All atlas textures.
    /// </summary>
    public Texture2D[] AtlasTextures
    {
        get => atlasTextures;
        set => atlasTextures = value;
    }

    /// <summary>
    /// Atlas width.
    /// </summary>
    public int AtlasWidth => atlasWidth;

    /// <summary>
    /// Atlas height.
    /// </summary>
    public int AtlasHeight => atlasHeight;

    /// <summary>
    /// Atlas padding.
    /// </summary>
    public int AtlasPadding => atlasPadding;

    /// <summary>
    /// Render mode.
    /// </summary>
    public GlyphRenderMode AtlasRenderMode => atlasRenderMode;

    /// <summary>
    /// Material for rendering.
    /// </summary>
    public Material Material
    {
        get => material;
        set => material = value;
    }

    /// <summary>
    /// Glyph lookup table.
    /// </summary>
    public Dictionary<uint, Glyph> GlyphLookupTable
    {
        get
        {
            if (glyphLookupDictionary == null)
                ReadFontAssetDefinition();
            return glyphLookupDictionary;
        }
    }

    /// <summary>
    /// Character lookup table.
    /// </summary>
    public Dictionary<uint, UniTextCharacter> CharacterLookupTable
    {
        get
        {
            if (characterLookupDictionary == null)
                ReadFontAssetDefinition();
            return characterLookupDictionary;
        }
    }

    /// <summary>
    /// Fallback font assets.
    /// </summary>
    public List<UniTextFontAsset> FallbackFontAssetTable
    {
        get => fallbackFontAssetTable;
        set => fallbackFontAssetTable = value;
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize lookup dictionaries from serialized data.
    /// </summary>
    public void ReadFontAssetDefinition()
    {
        InitializeGlyphLookupDictionary();
        InitializeCharacterLookupDictionary();
        AddSynthesizedCharacters();
    }

    private void InitializeGlyphLookupDictionary()
    {
        glyphLookupDictionary ??= new Dictionary<uint, Glyph>();
        glyphLookupDictionary.Clear();

        glyphIndexList ??= new List<uint>();
        glyphIndexList.Clear();

        if (glyphTable == null) return;

        for (int i = 0; i < glyphTable.Count; i++)
        {
            var glyph = glyphTable[i];
            uint index = glyph.index;

            if (!glyphLookupDictionary.ContainsKey(index))
            {
                glyphLookupDictionary.Add(index, glyph);
                glyphIndexList.Add(index);
            }
        }
    }

    private void InitializeCharacterLookupDictionary()
    {
        characterLookupDictionary ??= new Dictionary<uint, UniTextCharacter>();
        characterLookupDictionary.Clear();

        if (characterTable == null) return;

        for (int i = 0; i < characterTable.Count; i++)
        {
            var character = characterTable[i];
            uint unicode = character.unicode;

            if (!characterLookupDictionary.ContainsKey(unicode))
            {
                characterLookupDictionary.Add(unicode, character);
                character.fontAsset = this;

                if (glyphLookupDictionary.TryGetValue(character.glyphIndex, out var glyph))
                    character.glyph = glyph;
            }
        }
    }

    /// <summary>
    /// Add synthesized characters for control characters.
    /// </summary>
    private void AddSynthesizedCharacters()
    {
        bool fontLoaded = LoadFontFace() == FontEngineError.Success;

        // Tab, line breaks, zero-width characters
        AddSynthesizedCharacter(UnicodeData.Tab, fontLoaded, true);
        AddSynthesizedCharacter(UnicodeData.LineFeed, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.CarriageReturn, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.ZeroWidthSpace, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.LeftToRightMark, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.RightToLeftMark, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.LineSeparator, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.ParagraphSeparator, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.WordJoiner, fontLoaded);
        AddSynthesizedCharacter(UnicodeData.ArabicLetterMark, fontLoaded);
    }

    private void AddSynthesizedCharacter(int unicode, bool fontLoaded, bool addImmediately = false)
    {
        uint cp = (uint)unicode;

        if (characterLookupDictionary.ContainsKey(cp))
            return;

        Glyph glyph;

        if (fontLoaded)
        {
            if (UniTextFontEngine.GetGlyphIndex(cp) != 0)
            {
                if (!addImmediately) return;

                var glyphLoadFlags = GlyphLoadFlags.LOAD_NO_BITMAP;
                if (FontEngine.TryGetGlyphWithUnicodeValue(cp, glyphLoadFlags, out glyph))
                {
                    var character = new UniTextCharacter(cp, this, glyph);
                    characterLookupDictionary.Add(cp, character);
                }
                return;
            }
        }

        // Create zero-width glyph for control characters
        glyph = new Glyph(0, new UnityEngine.TextCore.GlyphMetrics(0, 0, 0, 0, 0), GlyphRect.zero, 1.0f, 0);
        var synthCharacter = new UniTextCharacter(cp, this, glyph);
        characterLookupDictionary.Add(cp, synthCharacter);
    }

    #endregion

    #region Font Loading

    /// <summary>
    /// Load font face into FontEngine.
    /// </summary>
    public FontEngineError LoadFontFace()
    {
        var accessor = PathAccessor.Get<int>(faceInfo, "m_FaceIndex");
        float pointSize = faceInfo.pointSize > 0 ? faceInfo.pointSize : 90;
        int faceIndex = accessor.Get(faceInfo);
        int ptSize = (int)pointSize;
        
        // Try loading from raw bytes via temp file (LoadFontFace(byte[]) has issues)
        if (fontData != null && fontData.Length > 0)
        {
            string tempPath = GetOrCreateTempFontFile();
            if (!string.IsNullOrEmpty(tempPath))
            {
                if (FontEngine.LoadFontFace(tempPath, ptSize, faceIndex) == FontEngineError.Success)
                    return FontEngineError.Success;
            }
        }

        // Fallback to file path (for DynamicOS mode)
        if (!string.IsNullOrEmpty(sourceFontFilePath))
        {
            if (FontEngine.LoadFontFace(sourceFontFilePath, ptSize, faceIndex) == FontEngineError.Success)
                return FontEngineError.Success;
        }

        return FontEngineError.Invalid_File;
    }

    /// <summary>
    /// Unload font face from FontEngine.
    /// Note: FontEngine is global, this just signals intent to unload.
    /// </summary>
    public void UnloadFontFace()
    {
        // FontEngine doesn't have an unload method - it just gets overwritten by next LoadFontFace
    }

    /// <summary>
    /// Get or create temporary font file from fontData bytes.
    /// FontEngine.LoadFontFace(byte[]) has issues - it returns Success but glyphs don't work.
    /// Workaround: save bytes to temp file and load via path.
    /// </summary>
    private string GetOrCreateTempFontFile()
    {
        // Return cached path if file still exists
        if (!string.IsNullOrEmpty(cachedTempFontPath) && System.IO.File.Exists(cachedTempFontPath))
            return cachedTempFontPath;

        if (fontData == null || fontData.Length == 0)
            return null;

        try
        {
            // Create temp file with unique name based on asset instance
            string tempDir = System.IO.Path.GetTempPath();
            string fileName = $"unitext_font_{GetInstanceID()}_{fontData.Length}.ttf";
            cachedTempFontPath = System.IO.Path.Combine(tempDir, fileName);

            // Write bytes to temp file
            System.IO.File.WriteAllBytes(cachedTempFontPath, fontData);
            tempFileCreated = true;

            return cachedTempFontPath;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"UniTextFontAsset [{name}]: Failed to create temp font file: {ex.Message}");
            cachedTempFontPath = null;
            return null;
        }
    }

    /// <summary>
    /// Cleanup temp file when asset is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (tempFileCreated && !string.IsNullOrEmpty(cachedTempFontPath))
        {
            try
            {
                if (System.IO.File.Exists(cachedTempFontPath))
                    System.IO.File.Delete(cachedTempFontPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #endregion

    #region Character Lookup

    /// <summary>
    /// Check if font has a specific character.
    /// </summary>
    public bool HasCharacter(int codepoint)
    {
        if (characterLookupDictionary == null)
            ReadFontAssetDefinition();

        return characterLookupDictionary != null && characterLookupDictionary.ContainsKey((uint)codepoint);
    }

    /// <summary>
    /// Check if font has a character, optionally searching fallbacks.
    /// </summary>
    public bool HasCharacter(uint unicode, bool searchFallbacks = false, bool tryAddCharacter = false)
    {
        if (characterLookupDictionary == null)
            ReadFontAssetDefinition();

        if (characterLookupDictionary.ContainsKey(unicode))
            return true;

        // Try to add character dynamically
        if (tryAddCharacter && atlasPopulationMode == UniTextAtlasPopulationMode.Dynamic)
        {
            if (TryAddCharacterInternal(unicode, out _))
                return true;
        }

        // Search fallback fonts
        if (searchFallbacks && fallbackFontAssetTable != null)
        {
            for (int i = 0; i < fallbackFontAssetTable.Count; i++)
            {
                var fallback = fallbackFontAssetTable[i];
                if (fallback != null && fallback.HasCharacter(unicode, true, tryAddCharacter))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get glyph index for a unicode codepoint.
    /// </summary>
    public uint GetGlyphIndex(uint unicode)
    {
        if (characterLookupDictionary != null && characterLookupDictionary.TryGetValue(unicode, out var character))
            return character.glyphIndex;

        if (LoadFontFace() == FontEngineError.Success)
            return UniTextFontEngine.GetGlyphIndex(unicode);

        return 0;
    }

    #endregion

    #region Dynamic Character Loading

    /// <summary>
    /// Try to add a single character to the font asset.
    /// </summary>
    public bool TryAddCharacter(uint unicode, out UniTextCharacter character)
    {
        character = null;

        if (atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
            return false;

        return TryAddCharacterInternal(unicode, out character);
    }

    private bool TryAddCharacterInternal(uint unicode, out UniTextCharacter character)
    {
        character = null;

        if (LoadFontFace() != FontEngineError.Success)
            return false;

        uint glyphIndex = UniTextFontEngine.GetGlyphIndex(unicode);
        if (glyphIndex == 0)
        {
            // Handle special cases
            switch (unicode)
            {
                case UnicodeData.NoBreakSpace:
                    glyphIndex = UniTextFontEngine.GetGlyphIndex(UnicodeData.Space);
                    break;
                case UnicodeData.SoftHyphen:
                case UnicodeData.NonBreakingHyphen:
                    glyphIndex = UniTextFontEngine.GetGlyphIndex(UnicodeData.Hyphen);
                    break;
            }

            if (glyphIndex == 0)
                return false;
        }

        // Check if glyph already exists
        if (glyphLookupDictionary.TryGetValue(glyphIndex, out var existingGlyph))
        {
            character = new UniTextCharacter(unicode, glyphIndex)
            {
                fontAsset = this,
                glyph = existingGlyph
            };
            characterTable.Add(character);
            characterLookupDictionary.Add(unicode, character);
            return true;
        }

        // Add new glyph to atlas
        return TryAddGlyphToAtlas(unicode, glyphIndex, out character);
    }

    private bool TryAddGlyphToAtlas(uint unicode, uint glyphIndex, out UniTextCharacter character)
    {
        character = null;

        // Ensure atlas texture exists
        if (atlasTextures == null || atlasTextures.Length == 0)
        {
            atlasTextures = new Texture2D[1];
            var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
                ? TextureFormat.RGBA32
                : TextureFormat.Alpha8;
            atlasTextures[0] = new Texture2D(atlasWidth, atlasHeight, texFormat, false);
            UniTextFontEngine.ResetAtlasTexture(atlasTextures[0]);

            // Initialize free rects
            freeGlyphRects ??= new List<GlyphRect>();
            freeGlyphRects.Clear();
            freeGlyphRects.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));
            usedGlyphRects ??= new List<GlyphRect>();
            usedGlyphRects.Clear();
        }

        // Resize if needed
        if (atlasTextures[atlasTextureIndex].width <= 1 || atlasTextures[atlasTextureIndex].height <= 1)
        {
            atlasTextures[atlasTextureIndex].Reinitialize(atlasWidth, atlasHeight);
            UniTextFontEngine.ResetAtlasTexture(atlasTextures[atlasTextureIndex]);
        }

        // Try to add glyph
        bool success = UniTextFontEngine.TryAddGlyphToTexture(
            glyphIndex,
            atlasPadding,
            GlyphPackingMode.BestShortSideFit,
            freeGlyphRects,
            usedGlyphRects,
            atlasRenderMode,
            atlasTextures[atlasTextureIndex],
            out var glyph);

        if (!success || glyph == null)
        {
            // Try multi-atlas if enabled
            if (isMultiAtlasTexturesEnabled)
                return TryAddGlyphToNewAtlasTexture(unicode, glyphIndex, out character);
            return false;
        }

        glyph.atlasIndex = atlasTextureIndex;

        // Add to tables
        glyphTable.Add(glyph);
        glyphLookupDictionary.Add(glyphIndex, glyph);
        glyphIndexList.Add(glyphIndex);

        character = new UniTextCharacter(unicode, glyphIndex)
        {
            fontAsset = this,
            glyph = glyph
        };
        characterTable.Add(character);
        characterLookupDictionary.Add(unicode, character);

        // Apply atlas changes
        atlasTextures[atlasTextureIndex].Apply(false, false);

        return true;
    }

    private bool TryAddGlyphToNewAtlasTexture(uint unicode, uint glyphIndex, out UniTextCharacter character)
    {
        character = null;

        // Create new atlas texture
        atlasTextureIndex++;
        Array.Resize(ref atlasTextures, atlasTextureIndex + 1);

        var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
            ? TextureFormat.RGBA32
            : TextureFormat.Alpha8;
        atlasTextures[atlasTextureIndex] = new Texture2D(atlasWidth, atlasHeight, texFormat, false);
        UniTextFontEngine.ResetAtlasTexture(atlasTextures[atlasTextureIndex]);

        // Reset free rects for new atlas
        freeGlyphRects.Clear();
        freeGlyphRects.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));

        // Try again with new atlas
        bool success = UniTextFontEngine.TryAddGlyphToTexture(
            glyphIndex,
            atlasPadding,
            GlyphPackingMode.BestShortSideFit,
            freeGlyphRects,
            usedGlyphRects,
            atlasRenderMode,
            atlasTextures[atlasTextureIndex],
            out var glyph);

        if (!success || glyph == null)
            return false;

        glyph.atlasIndex = atlasTextureIndex;

        glyphTable.Add(glyph);
        glyphLookupDictionary.Add(glyphIndex, glyph);
        glyphIndexList.Add(glyphIndex);

        character = new UniTextCharacter(unicode, glyphIndex)
        {
            fontAsset = this,
            glyph = glyph
        };
        characterTable.Add(character);
        characterLookupDictionary.Add(unicode, character);

        atlasTextures[atlasTextureIndex].Apply(false, false);

        return true;
    }

    /// <summary>
    /// Try to add multiple characters.
    /// </summary>
    public bool TryAddCharacters(string characters, out string missingCharacters)
    {
        missingCharacters = "";

        if (string.IsNullOrEmpty(characters) || atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
        {
            missingCharacters = characters;
            return false;
        }

        if (LoadFontFace() != FontEngineError.Success)
        {
            missingCharacters = characters;
            return false;
        }

        var missing = new System.Text.StringBuilder();

        for (int i = 0; i < characters.Length; i++)
        {
            uint unicode;
            if (char.IsHighSurrogate(characters[i]) && i + 1 < characters.Length && char.IsLowSurrogate(characters[i + 1]))
            {
                unicode = (uint)char.ConvertToUtf32(characters[i], characters[i + 1]);
                i++;
            }
            else
            {
                unicode = characters[i];
            }

            if (!characterLookupDictionary.ContainsKey(unicode))
            {
                if (!TryAddCharacterInternal(unicode, out _))
                    missing.Append(char.ConvertFromUtf32((int)unicode));
            }
        }

        missingCharacters = missing.ToString();
        return missingCharacters.Length == 0;
    }

    #endregion

    #region Static Creation Methods

    /// <summary>
    /// Create font asset from raw font bytes.
    /// </summary>
    public static UniTextFontAsset CreateFontAsset(byte[] fontBytes, int samplingPointSize = 90, int atlasPadding = 9,
        GlyphRenderMode renderMode = GlyphRenderMode.SDFAA, int atlasWidth = 1024, int atlasHeight = 1024)
    {
        if (fontBytes == null || fontBytes.Length == 0)
        {
            Debug.LogError("UniTextFontAsset: Cannot create font asset from null or empty byte array.");
            return null;
        }

        if (FontEngine.LoadFontFace(fontBytes, samplingPointSize, 0) != FontEngineError.Success)
        {
            Debug.LogError("UniTextFontAsset: Failed to load font face from byte array.");
            return null;
        }

        var fontAsset = CreateInstance<UniTextFontAsset>();
        fontAsset.fontData = fontBytes;
        fontAsset.faceInfo = FontEngine.GetFaceInfo();
        fontAsset.atlasPopulationMode = UniTextAtlasPopulationMode.Dynamic;
        fontAsset.atlasWidth = atlasWidth;
        fontAsset.atlasHeight = atlasHeight;
        fontAsset.atlasPadding = atlasPadding;
        fontAsset.atlasRenderMode = renderMode;

        // Create initial atlas texture
        var texFormat = renderMode == GlyphRenderMode.COLOR || renderMode == GlyphRenderMode.COLOR_HINTED
            ? TextureFormat.RGBA32
            : TextureFormat.Alpha8;
        var texture = new Texture2D(1, 1, texFormat, false);
        fontAsset.atlasTextures = new[] { texture };

        // Create material
        var shader = Shader.Find("UniText/Mobile/Distance Field");
        if (shader == null)
            shader = Shader.Find("GUI/Text Shader");

        if (shader != null)
        {
            fontAsset.material = new Material(shader);
            fontAsset.material.SetTexture("_MainTex", texture);
            fontAsset.material.SetFloat("_TextureWidth", atlasWidth);
            fontAsset.material.SetFloat("_TextureHeight", atlasHeight);
            fontAsset.material.SetFloat("_GradientScale", atlasPadding + 1);
        }

        // Initialize free rects
        fontAsset.freeGlyphRects = new List<GlyphRect> { new(0, 0, atlasWidth - 1, atlasHeight - 1) };
        fontAsset.usedGlyphRects = new List<GlyphRect>();

        fontAsset.ReadFontAssetDefinition();

        return fontAsset;
    }

    #endregion

    #region Editor Support

#if UNITY_EDITOR
    /// <summary>
    /// Source font (Editor only - for extracting bytes).
    /// </summary>
    [SerializeField]
    internal Font sourceFont;

    /// <summary>
    /// Set raw font data from bytes (called by Editor).
    /// </summary>
    public void SetFontData(byte[] data)
    {
        fontData = data;

        if (data != null && data.Length > 0)
        {
            // Reload face info from new data
            if (FontEngine.LoadFontFace(data, faceInfo.pointSize > 0 ? faceInfo.pointSize : 90, 0) == FontEngineError.Success)
            {
                faceInfo = FontEngine.GetFaceInfo();
            }
        }
    }

    /// <summary>
    /// Update font asset from source font.
    /// </summary>
    public void UpdateFromSourceFont()
    {
        if (sourceFont == null) return;

        // Load font to get face info
        if (FontEngine.LoadFontFace(sourceFont, faceInfo.pointSize > 0 ? faceInfo.pointSize : 90, 0) == FontEngineError.Success)
        {
            faceInfo = FontEngine.GetFaceInfo();
        }
    }
#endif

    #endregion
}

/// <summary>
/// Character info for UniTextFontAsset.
/// </summary>
[Serializable]
public class UniTextCharacter
{
    /// <summary>
    /// Unicode value.
    /// </summary>
    public uint unicode;

    /// <summary>
    /// Glyph index in the font.
    /// </summary>
    public uint glyphIndex;

    /// <summary>
    /// Reference to the glyph (runtime only).
    /// </summary>
    [NonSerialized]
    public Glyph glyph;

    /// <summary>
    /// Reference to the font asset (runtime only).
    /// </summary>
    [NonSerialized]
    public UniTextFontAsset fontAsset;

    public UniTextCharacter() { }

    public UniTextCharacter(uint unicode, uint glyphIndex)
    {
        this.unicode = unicode;
        this.glyphIndex = glyphIndex;
    }

    public UniTextCharacter(uint unicode, UniTextFontAsset fontAsset, Glyph glyph)
    {
        this.unicode = unicode;
        this.fontAsset = fontAsset;
        this.glyph = glyph;
        this.glyphIndex = glyph?.index ?? 0;
    }
}
