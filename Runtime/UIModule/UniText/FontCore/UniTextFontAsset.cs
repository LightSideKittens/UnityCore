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

    /// <summary>
    /// Clear dynamic data (glyphs, characters, atlas) on build.
    /// </summary>
    [SerializeField]
    internal bool clearDynamicDataOnBuild = true;

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

    // Cached faceIndex to avoid reflection
    private int cachedFaceIndex = -1;

    // Static tracking of currently loaded font in FontEngine (FontEngine is global!)
    private static int currentlyLoadedFontInstanceId = 0;

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
    /// FontEngine is GLOBAL - only one font can be loaded at a time.
    /// NOTE: We ALWAYS reload because external code (Unity Editor, TMP, etc.) may have
    /// loaded a different font into FontEngine without our knowledge.
    /// </summary>
    public FontEngineError LoadFontFace()
    {
        int myInstanceId = GetInstanceID();

        // Cache faceIndex to avoid reflection on every call
        if (cachedFaceIndex < 0)
        {
            var accessor = PathAccessor.Get<int>(faceInfo, "m_FaceIndex");
            cachedFaceIndex = accessor.Get(faceInfo);
        }

        float pointSize = faceInfo.pointSize > 0 ? faceInfo.pointSize : 90;
        int ptSize = (int)pointSize;

        if (DebugLogging)
            Debug.Log($"[UniTextFontAsset.LoadFontFace] Loading {name}: fontData={fontData?.Length ?? 0} bytes, sourcePath={sourceFontFilePath ?? "null"}, ptSize={ptSize}");

        // Try loading from raw bytes via temp file (LoadFontFace(byte[]) has issues)
        if (fontData != null && fontData.Length > 0)
        {
            string tempPath = GetOrCreateTempFontFile();
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.LoadFontFace] {name} tempPath={tempPath}");

            if (!string.IsNullOrEmpty(tempPath))
            {
                var result = FontEngine.LoadFontFace(tempPath, ptSize, cachedFaceIndex);
                if (DebugLogging)
                    Debug.Log($"[UniTextFontAsset.LoadFontFace] {name} FontEngine.LoadFontFace(tempPath) result={result}");

                if (result == FontEngineError.Success)
                {
                    currentlyLoadedFontInstanceId = myInstanceId;
                    return FontEngineError.Success;
                }
            }
        }

        // Fallback to file path (for DynamicOS mode)
        if (!string.IsNullOrEmpty(sourceFontFilePath))
        {
            var result = FontEngine.LoadFontFace(sourceFontFilePath, ptSize, cachedFaceIndex);
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.LoadFontFace] {name} FontEngine.LoadFontFace(sourcePath) result={result}");

            if (result == FontEngineError.Success)
            {
                currentlyLoadedFontInstanceId = myInstanceId;
                return FontEngineError.Success;
            }
        }

        if (DebugLogging)
            Debug.LogWarning($"[UniTextFontAsset.LoadFontFace] {name} FAILED to load!");

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
        // Fast path: if we already created the file this session, skip File.Exists check
        if (tempFileCreated && !string.IsNullOrEmpty(cachedTempFontPath))
            return cachedTempFontPath;

        // Slower path: verify file exists (only needed on first call or after domain reload)
        if (!string.IsNullOrEmpty(cachedTempFontPath) && System.IO.File.Exists(cachedTempFontPath))
        {
            tempFileCreated = true; // Mark as verified
            return cachedTempFontPath;
        }

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

    // DEBUG: Enable detailed logging
    public static bool DebugLogging = false;

    private bool TryAddCharacterInternal(uint unicode, out UniTextCharacter character)
    {
        character = null;

        // First, check using HarfBuzz (reliable, per-font)
        uint glyphIndex = 0;

        if (HasFontData)
        {
            glyphIndex = HarfBuzzFontValidator.GetGlyphIndex(this, unicode);
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4} in {name}: HarfBuzz.GetGlyphIndex returned {glyphIndex}");
        }

        // Fallback to FontEngine for fonts without raw data
        if (glyphIndex == 0 && LoadFontFace() == FontEngineError.Success)
        {
            glyphIndex = UniTextFontEngine.GetGlyphIndex(unicode);
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4} in {name}: FontEngine.GetGlyphIndex returned {glyphIndex}");
        }

        if (glyphIndex == 0)
        {
            // Handle special cases
            uint specialCodepoint = unicode switch
            {
                UnicodeData.NoBreakSpace => UnicodeData.Space,
                UnicodeData.SoftHyphen => UnicodeData.Hyphen,
                UnicodeData.NonBreakingHyphen => UnicodeData.Hyphen,
                _ => 0
            };

            if (specialCodepoint != 0)
            {
                if (HasFontData)
                    glyphIndex = HarfBuzzFontValidator.GetGlyphIndex(this, specialCodepoint);
                if (glyphIndex == 0 && LoadFontFace() == FontEngineError.Success)
                    glyphIndex = UniTextFontEngine.GetGlyphIndex(specialCodepoint);
            }

            if (glyphIndex == 0)
            {
                if (DebugLogging)
                    Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4}: glyph not found in font {name}");
                return false;
            }
        }

        // Need to load font face for atlas operations
        if (LoadFontFace() != FontEngineError.Success)
        {
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4}: LoadFontFace failed for atlas");
            return false;
        }

        // Check if character already exists in lookup
        if (characterLookupDictionary.ContainsKey(unicode))
        {
            character = characterLookupDictionary[unicode];
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4}: already in characterLookup, reusing");
            return true;
        }

        // Check if glyph already exists
        if (glyphLookupDictionary.TryGetValue(glyphIndex, out var existingGlyph))
        {
            if (DebugLogging)
                Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4}: glyph {glyphIndex} already in atlas, reusing");

            character = new UniTextCharacter(unicode, glyphIndex)
            {
                fontAsset = this,
                glyph = existingGlyph
            };
            characterTable.Add(character);
            characterLookupDictionary[unicode] = character; // Use indexer to avoid duplicate key exception
            return true;
        }

        if (DebugLogging)
            Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4}: adding glyph {glyphIndex} to atlas...");

        // Add new glyph to atlas
        bool result = TryAddGlyphToAtlas(unicode, glyphIndex, out character);

        if (DebugLogging)
            Debug.Log($"[UniTextFontAsset.TryAddCharacterInternal] U+{unicode:X4}: TryAddGlyphToAtlas returned {result}");

        return result;
    }

    private bool TryAddGlyphToAtlas(uint unicode, uint glyphIndex, out UniTextCharacter character)
    {
        character = null;

        // CRITICAL: FontEngine is GLOBAL! Must ensure THIS font is loaded before rendering glyph.
        if (LoadFontFace() != FontEngineError.Success)
            return false;

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

        // CRITICAL: Ensure correct font is loaded before rendering
        if (LoadFontFace() != FontEngineError.Success)
            return false;

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
    /// Try to add a glyph to atlas by glyph index (from shaping result).
    /// This is the correct way to populate atlas - after shaping, using glyph indices.
    /// </summary>
    public bool TryAddGlyphByIndex(uint glyphIndex)
    {
        if (atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
            return false;

        // Already in atlas?
        if (glyphLookupDictionary != null && glyphLookupDictionary.ContainsKey(glyphIndex))
            return true;

        // Need to load font for atlas operations
        if (LoadFontFace() != FontEngineError.Success)
            return false;

        return TryAddGlyphToAtlasByIndex(glyphIndex);
    }

    /// <summary>
    /// Try to add multiple glyphs to atlas by glyph indices (batch operation).
    /// </summary>
    public int TryAddGlyphsByIndex(ReadOnlySpan<uint> glyphIndices)
    {
        if (atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
            return 0;

        if (LoadFontFace() != FontEngineError.Success)
            return 0;

        int addedCount = 0;
        for (int i = 0; i < glyphIndices.Length; i++)
        {
            uint glyphIndex = glyphIndices[i];

            // Skip if already in atlas or zero (missing glyph)
            if (glyphIndex == 0)
                continue;

            if (glyphLookupDictionary != null && glyphLookupDictionary.ContainsKey(glyphIndex))
                continue;

            if (TryAddGlyphToAtlasByIndex(glyphIndex))
                addedCount++;
        }

        return addedCount;
    }

    /// <summary>
    /// Try to add multiple glyphs to atlas by glyph indices (List overload for compatibility).
    /// </summary>
    public int TryAddGlyphsByIndex(List<uint> glyphIndices)
    {
        if (glyphIndices == null || glyphIndices.Count == 0)
            return 0;

        if (atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
            return 0;

        if (LoadFontFace() != FontEngineError.Success)
            return 0;

        int addedCount = 0;
        int count = glyphIndices.Count;
        for (int i = 0; i < count; i++)
        {
            uint glyphIndex = glyphIndices[i];

            if (glyphIndex == 0)
                continue;

            if (glyphLookupDictionary != null && glyphLookupDictionary.ContainsKey(glyphIndex))
                continue;

            if (TryAddGlyphToAtlasByIndex(glyphIndex))
                addedCount++;
        }

        return addedCount;
    }

    private bool TryAddGlyphToAtlasByIndex(uint glyphIndex)
    {
        // CRITICAL: FontEngine is GLOBAL! Must ensure THIS font is loaded before rendering glyph.
        // Without this, glyph may be rendered from wrong font if another font was loaded between calls.
        var loadResult = LoadFontFace();
        if (loadResult != FontEngineError.Success)
        {
            if (DebugLogging)
                Debug.LogWarning($"[UniTextFontAsset.TryAddGlyphToAtlasByIndex] {name} LoadFontFace FAILED: {loadResult}");
            return false;
        }

        if (DebugLogging)
            Debug.Log($"[UniTextFontAsset.TryAddGlyphToAtlasByIndex] {name} adding glyphIndex={glyphIndex}");

        // Ensure atlas texture exists
        if (atlasTextures == null || atlasTextures.Length == 0)
        {
            atlasTextures = new Texture2D[1];
            var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
                ? TextureFormat.RGBA32
                : TextureFormat.Alpha8;
            atlasTextures[0] = new Texture2D(atlasWidth, atlasHeight, texFormat, false);
            UniTextFontEngine.ResetAtlasTexture(atlasTextures[0]);

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

        // Try to add glyph (FontEngine uses currently loaded font)
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
                return TryAddGlyphToNewAtlasByIndex(glyphIndex);
            return false;
        }

        glyph.atlasIndex = atlasTextureIndex;

        // Add to glyph tables only (no character mapping - we don't know the codepoint)
        glyphTable.Add(glyph);
        glyphLookupDictionary ??= new Dictionary<uint, Glyph>();
        glyphLookupDictionary[glyphIndex] = glyph;
        glyphIndexList ??= new List<uint>();
        glyphIndexList.Add(glyphIndex);

        atlasTextures[atlasTextureIndex].Apply(false, false);

        if (DebugLogging)
            Debug.Log($"[UniTextFontAsset.TryAddGlyphToAtlasByIndex] Added glyphIndex={glyphIndex} to atlas");

        return true;
    }

    private bool TryAddGlyphToNewAtlasByIndex(uint glyphIndex)
    {
        // CRITICAL: Ensure correct font is loaded before rendering
        if (LoadFontFace() != FontEngineError.Success)
            return false;

        atlasTextureIndex++;
        Array.Resize(ref atlasTextures, atlasTextureIndex + 1);

        var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
            ? TextureFormat.RGBA32
            : TextureFormat.Alpha8;
        atlasTextures[atlasTextureIndex] = new Texture2D(atlasWidth, atlasHeight, texFormat, false);
        UniTextFontEngine.ResetAtlasTexture(atlasTextures[atlasTextureIndex]);

        freeGlyphRects.Clear();
        freeGlyphRects.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));

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
        glyphLookupDictionary ??= new Dictionary<uint, Glyph>();
        glyphLookupDictionary[glyphIndex] = glyph;
        glyphIndexList ??= new List<uint>();
        glyphIndexList.Add(glyphIndex);

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

    #region Dynamic Data Management

    /// <summary>
    /// Clear dynamic data on build setting.
    /// </summary>
    public bool ClearDynamicDataOnBuild
    {
        get => clearDynamicDataOnBuild;
        set => clearDynamicDataOnBuild = value;
    }

    /// <summary>
    /// Clears all dynamically generated data (glyphs, characters, atlas textures).
    /// Atlas will be regenerated at runtime as characters are requested.
    /// </summary>
    public void ClearDynamicData()
    {
        // Clear glyph and character tables
        glyphTable?.Clear();
        characterTable?.Clear();

        // Clear lookup dictionaries
        glyphLookupDictionary?.Clear();
        characterLookupDictionary?.Clear();
        glyphIndexList?.Clear();
        glyphIndexListNewlyAdded?.Clear();
        charactersToAdd?.Clear();
        charactersToAddLookup?.Clear();

        // Clear packing rects
        usedGlyphRects?.Clear();
        freeGlyphRects?.Clear();
        freeGlyphRects?.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));

        // Reset atlas to minimal 1x1 texture
        if (atlasTextures != null && atlasTextures.Length > 0)
        {
            // Destroy additional atlas textures (keep only first one)
            for (int i = 1; i < atlasTextures.Length; i++)
            {
                if (atlasTextures[i] != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(atlasTextures[i], true);
#else
                    Destroy(atlasTextures[i]);
#endif
                }
            }

            // Reset first texture to 1x1
            var firstTexture = atlasTextures[0];
            if (firstTexture != null)
            {
                var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
                    ? TextureFormat.RGBA32
                    : TextureFormat.Alpha8;
                firstTexture.Reinitialize(1, 1, texFormat, false);
                UniTextFontEngine.ResetAtlasTexture(firstTexture);
                firstTexture.Apply(false, false);
            }

            // Keep only first texture
            if (atlasTextures.Length > 1)
                atlasTextures = new[] { firstTexture };
        }

        atlasTextureIndex = 0;

        // Reset font loading tracking (if this font was loaded, invalidate the cache)
        if (currentlyLoadedFontInstanceId == GetInstanceID())
            currentlyLoadedFontInstanceId = 0;

        Debug.Log($"UniTextFontAsset [{name}]: Dynamic data cleared. Atlas will regenerate at runtime.");
    }

    #endregion

    #region Editor Support

#if UNITY_EDITOR
    /// <summary>
    /// Source font (Editor only - for extracting bytes).
    /// </summary>
    [SerializeField]
    public Font sourceFont;

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
