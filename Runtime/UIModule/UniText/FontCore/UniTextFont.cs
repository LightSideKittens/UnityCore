using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;


public enum UniTextAtlasPopulationMode
{
    Static = 0,
    Dynamic = 1
}


[Serializable]
public class UniTextFont : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private byte[] fontData;
    [SerializeField] private int fontDataHash;
    [SerializeField] private string sourceFontFilePath;
    [SerializeField] private float italicStyle = 30;
    [SerializeField] private UniTextAtlasPopulationMode atlasPopulationMode = UniTextAtlasPopulationMode.Dynamic;
    [SerializeField] internal FaceInfo faceInfo;
    [SerializeField] internal List<Glyph> glyphTable = new();
    [SerializeField] internal List<UniTextCharacter> characterTable = new();
    [SerializeField] internal Texture2D[] atlasTextures;
    [SerializeField] internal int atlasTextureIndex;
    [SerializeField] internal int atlasWidth = 1024;
    [SerializeField] internal int atlasHeight = 1024;
    [SerializeField] internal int atlasPadding = 9;
    [SerializeField] internal GlyphRenderMode atlasRenderMode = GlyphRenderMode.SDFAA;
    [SerializeField] private List<GlyphRect> usedGlyphRects = new();
    [SerializeField] private List<GlyphRect> freeGlyphRects = new();
    [SerializeField] internal bool clearDynamicDataOnBuild = true;

    #endregion

    #region Runtime Fields

    internal Dictionary<uint, Glyph> glyphLookupDictionary;
    internal Dictionary<uint, UniTextCharacter> characterLookupDictionary;

    private List<UniTextCharacter> charactersToAdd = new();
    private HashSet<uint> charactersToAddLookup = new();
    private List<uint> glyphIndexList = new();
    private List<uint> glyphIndexListNewlyAdded = new();

    private string cachedTempFontPath;

    private bool tempFileCreated;

    private int cachedFaceIndex = -1;
    private int cachedInstanceId = -1;

    private static int currentlyLoadedFontInstanceId = 0;

    #endregion

    public int GetCachedInstanceId()
    {
        if (cachedInstanceId < 0)
            cachedInstanceId = GetInstanceID();
        return cachedInstanceId;
    }

    #region Properties

    public byte[] FontData => fontData;

    public float ItalicStyle => italicStyle;

    public int FontDataHash => fontDataHash;
    
    public bool HasFontData => fontData != null && fontData.Length > 0;


    public static int ComputeFontDataHash(byte[] data)
    {
        if (data == null || data.Length == 0) return 0;
        unchecked
        {
            var hash = -2128831035;
            var len = data.Length;
            var step = len > 4096 ? len / 1024 : 1;
            for (var i = 0; i < len; i += step)
                hash = (hash ^ data[i]) * 16777619;
            return (hash ^ len) * 16777619;
        }
    }


    public FaceInfo FaceInfo
    {
        get => faceInfo;
        internal set => faceInfo = value;
    }


    public UniTextAtlasPopulationMode AtlasPopulationMode
    {
        get => atlasPopulationMode;
        set => atlasPopulationMode = value;
    }


    public Texture2D AtlasTexture
    {
        get
        {
            if (atlasTextures != null && atlasTextures.Length > 0)
                return atlasTextures[0];
            return null;
        }
    }


    public Texture2D[] AtlasTextures
    {
        get => atlasTextures;
        set => atlasTextures = value;
    }


    public int AtlasWidth => atlasWidth;


    public int AtlasHeight => atlasHeight;


    public int AtlasPadding => atlasPadding;


    public GlyphRenderMode AtlasRenderMode => atlasRenderMode;


    public Dictionary<uint, Glyph> GlyphLookupTable
    {
        get
        {
            if (glyphLookupDictionary == null)
                ReadFontAssetDefinition();
            return glyphLookupDictionary;
        }
    }


    public Dictionary<uint, UniTextCharacter> CharacterLookupTable
    {
        get
        {
            if (characterLookupDictionary == null)
                ReadFontAssetDefinition();
            return characterLookupDictionary;
        }
    }


    #endregion

    #region Initialization

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

        for (var i = 0; i < glyphTable.Count; i++)
        {
            var glyph = glyphTable[i];
            var index = glyph.index;

            if (glyphLookupDictionary.TryAdd(index, glyph))
            {
                glyphIndexList.Add(index);
            }
        }
    }

    private void InitializeCharacterLookupDictionary()
    {
        characterLookupDictionary ??= new Dictionary<uint, UniTextCharacter>();
        characterLookupDictionary.Clear();

        if (characterTable == null) return;

        for (var i = 0; i < characterTable.Count; i++)
        {
            var character = characterTable[i];
            var unicode = character.unicode;

            if (characterLookupDictionary.TryAdd(unicode, character))
            {
                if (glyphLookupDictionary.TryGetValue(character.glyphIndex, out var glyph))
                    character.glyph = glyph;
            }
        }
    }
    
    private void AddSynthesizedCharacters()
    {
        var fontLoaded = LoadFontFace() == FontEngineError.Success;

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
        var cp = (uint)unicode;

        if (characterLookupDictionary.ContainsKey(cp))
            return;

        Glyph glyph;

        if (fontLoaded)
            if (UniTextFontEngine.GetGlyphIndex(cp) != 0)
            {
                if (!addImmediately) return;

                var glyphLoadFlags = GlyphLoadFlags.LOAD_NO_BITMAP;
                if (FontEngine.TryGetGlyphWithUnicodeValue(cp, glyphLoadFlags, out glyph))
                {
                    var character = new UniTextCharacter(cp, glyph);
                    characterLookupDictionary.Add(cp, character);
                }

                return;
            }

        glyph = new Glyph(0, new GlyphMetrics(0, 0, 0, 0, 0), GlyphRect.zero, 1.0f, 0);
        var synthCharacter = new UniTextCharacter(cp, glyph);
        characterLookupDictionary.Add(cp, synthCharacter);
    }

    #endregion

    #region Font Loading

    public FontEngineError LoadFontFace()
    {
        var myInstanceId = GetInstanceID();

        if (cachedFaceIndex < 0)
        {
            var accessor = PathAccessor.Get<int>(faceInfo, "m_FaceIndex");
            cachedFaceIndex = accessor.Get(faceInfo);
        }

        var pointSize = faceInfo.pointSize > 0 ? faceInfo.pointSize : 90;
        var ptSize = (int)pointSize;

        if (fontData != null && fontData.Length > 0)
        {
            var tempPath = GetOrCreateTempFontFile();

            if (!string.IsNullOrEmpty(tempPath))
            {
                var result = FontEngine.LoadFontFace(tempPath, ptSize, cachedFaceIndex);

                if (result == FontEngineError.Success)
                {
                    currentlyLoadedFontInstanceId = myInstanceId;
                    return FontEngineError.Success;
                }
            }
        }

        if (!string.IsNullOrEmpty(sourceFontFilePath))
        {
            var result = FontEngine.LoadFontFace(sourceFontFilePath, ptSize, cachedFaceIndex);

            if (result == FontEngineError.Success)
            {
                currentlyLoadedFontInstanceId = myInstanceId;
                return FontEngineError.Success;
            }
        }

        return FontEngineError.Invalid_File;
    }

    private string GetOrCreateTempFontFile()
    {
        if (tempFileCreated && !string.IsNullOrEmpty(cachedTempFontPath))
            return cachedTempFontPath;

        if (!string.IsNullOrEmpty(cachedTempFontPath) && System.IO.File.Exists(cachedTempFontPath))
        {
            tempFileCreated = true;
            return cachedTempFontPath;
        }

        if (fontData == null || fontData.Length == 0)
            return null;

        try
        {
            var tempDir = System.IO.Path.GetTempPath();
            var fileName = $"unitext_font_{GetInstanceID()}_{fontData.Length}.ttf";
            cachedTempFontPath = System.IO.Path.Combine(tempDir, fileName);

            System.IO.File.WriteAllBytes(cachedTempFontPath, fontData);
            tempFileCreated = true;

            return cachedTempFontPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"UniTextFontAsset [{name}]: Failed to create temp font file: {ex.Message}");
            cachedTempFontPath = null;
            return null;
        }
    }

    private void OnDestroy()
    {
        if (tempFileCreated && !string.IsNullOrEmpty(cachedTempFontPath))
        {
            try
            {
                if (System.IO.File.Exists(cachedTempFontPath))
                {
                    System.IO.File.Delete(cachedTempFontPath);
                }
            }
            catch { }
        }
    }

    #endregion

    #region Dynamic Character Loading

    public bool TryAddCharacter(uint unicode)
    {
        if (atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
            return false;

        return TryAddCharacterInternal(unicode);
    }

    private bool TryAddCharacterInternal(uint unicode)
    {
        uint glyphIndex = 0;

        if (HasFontData) glyphIndex = HarfBuzzFontValidator.GetGlyphIndex(this, unicode);

        if (glyphIndex == 0 && LoadFontFace() == FontEngineError.Success)
            glyphIndex = UniTextFontEngine.GetGlyphIndex(unicode);

        if (glyphIndex == 0)
        {
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

            if (glyphIndex == 0) return false;
        }

        if (LoadFontFace() != FontEngineError.Success) return false;

        if (characterLookupDictionary.ContainsKey(unicode))
        {
            return true;
        }

        UniTextCharacter character;
        
        if (glyphLookupDictionary.TryGetValue(glyphIndex, out var existingGlyph))
        {
            character = new UniTextCharacter(unicode, glyphIndex)
            {
                glyph = existingGlyph
            };
            characterTable.Add(character);
            characterLookupDictionary[unicode] = character;
            return true;
        }

        var result = TryAddGlyphToAtlas(unicode, glyphIndex, out character);
        return result;
    }

    private bool TryAddGlyphToAtlas(uint unicode, uint glyphIndex, out UniTextCharacter character)
    {
        character = null;

        if (LoadFontFace() != FontEngineError.Success)
            return false;

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

        if (atlasTextures[atlasTextureIndex].width <= 1 || atlasTextures[atlasTextureIndex].height <= 1)
        {
            atlasTextures[atlasTextureIndex].Reinitialize(atlasWidth, atlasHeight);
            UniTextFontEngine.ResetAtlasTexture(atlasTextures[atlasTextureIndex]);

            ClearGlyphDataOnly();
        }

        var success = UniTextFontEngine.TryAddGlyphToTexture(
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
            return TryAddGlyphToNewAtlasTexture(unicode, glyphIndex, out character);
        }

        glyph.atlasIndex = atlasTextureIndex;

        glyphTable.Add(glyph);
        glyphLookupDictionary.Add(glyphIndex, glyph);
        glyphIndexList.Add(glyphIndex);

        character = new UniTextCharacter(unicode, glyphIndex)
        {
            glyph = glyph
        };
        characterTable.Add(character);
        characterLookupDictionary.Add(unicode, character);

        atlasTextures[atlasTextureIndex].Apply(false, false);

        return true;
    }

    private bool TryAddGlyphToNewAtlasTexture(uint unicode, uint glyphIndex, out UniTextCharacter character)
    {
        character = null;

        if (LoadFontFace() != FontEngineError.Success)
            return false;

        atlasTextureIndex++;
        Array.Resize(ref atlasTextures, atlasTextureIndex + 1);

        var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
            ? TextureFormat.RGBA32
            : TextureFormat.Alpha8;
        atlasTextures[atlasTextureIndex]
            = new Texture2D(atlasWidth, atlasHeight, texFormat, false);
        UniTextFontEngine.ResetAtlasTexture(atlasTextures[atlasTextureIndex]);
        freeGlyphRects.Clear();
        freeGlyphRects.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));
        usedGlyphRects.Clear();

        var success = UniTextFontEngine.TryAddGlyphToTexture(
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
            glyph = glyph
        };
        characterTable.Add(character);
        characterLookupDictionary.Add(unicode, character);

        atlasTextures[atlasTextureIndex].Apply(false, false);

        return true;
    }

    public int TryAddGlyphsByIndex(List<uint> glyphIndices)
    {
        if (glyphIndices == null || glyphIndices.Count == 0)
            return 0;

        if (atlasPopulationMode != UniTextAtlasPopulationMode.Dynamic)
            return 0;

        if (glyphLookupDictionary == null)
            ReadFontAssetDefinition();

        if (LoadFontFace() != FontEngineError.Success)
            return 0;

        var addedCount = 0;
        var count = glyphIndices.Count;
        for (var i = 0; i < count; i++)
        {
            var glyphIndex = glyphIndices[i];

            if (glyphIndex == 0)
                continue;

            if (glyphLookupDictionary.ContainsKey(glyphIndex))
                continue;

            if (TryAddGlyphToAtlasByIndex(glyphIndex))
                addedCount++;
        }

        return addedCount;
    }

    private bool TryAddGlyphToAtlasByIndex(uint glyphIndex)
    {
        var loadResult = LoadFontFace();
        if (loadResult != FontEngineError.Success) return false;

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

        if (atlasTextures[atlasTextureIndex].width <= 1 || atlasTextures[atlasTextureIndex].height <= 1)
        {
            atlasTextures[atlasTextureIndex].Reinitialize(atlasWidth, atlasHeight);
            UniTextFontEngine.ResetAtlasTexture(atlasTextures[atlasTextureIndex]);

            ClearGlyphDataOnly();
        }

        var success = UniTextFontEngine.TryAddGlyphToTexture(
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
            Debug.Log($"[UniTextFontAsset] Atlas {atlasTextureIndex} full, creating new atlas for glyph {glyphIndex}. freeRects={freeGlyphRects.Count}, usedRects={usedGlyphRects.Count}");
            return TryAddGlyphToNewAtlasByIndex(glyphIndex);
        }

        glyph.atlasIndex = atlasTextureIndex;

        glyphTable.Add(glyph);
        glyphLookupDictionary ??= new Dictionary<uint, Glyph>();
        glyphLookupDictionary[glyphIndex] = glyph;
        glyphIndexList ??= new List<uint>();
        glyphIndexList.Add(glyphIndex);

        atlasTextures[atlasTextureIndex].Apply(false, false);

        return true;
    }

    private bool TryAddGlyphToNewAtlasByIndex(uint glyphIndex)
    {
        if (LoadFontFace() != FontEngineError.Success)
        {
            Debug.LogError($"[UniTextFontAsset] TryAddGlyphToNewAtlasByIndex: LoadFontFace failed!");
            return false;
        }

        var newAtlasIndex = atlasTextureIndex + 1;
        Debug.Log(
            $"[UniTextFontAsset] Creating new atlas #{newAtlasIndex} ({atlasWidth}x{atlasHeight}) for glyph {glyphIndex}");

        atlasTextureIndex = newAtlasIndex;
        Array.Resize(ref atlasTextures, atlasTextureIndex + 1);

        var texFormat = atlasRenderMode == GlyphRenderMode.COLOR || atlasRenderMode == GlyphRenderMode.COLOR_HINTED
            ? TextureFormat.RGBA32
            : TextureFormat.Alpha8;
        var newAtlasTexture = new Texture2D(atlasWidth, atlasHeight, texFormat, false);
        newAtlasTexture.name = name + " Atlas " + atlasTextureIndex;
        atlasTextures[atlasTextureIndex] = newAtlasTexture;
        UniTextFontEngine.ResetAtlasTexture(newAtlasTexture);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.AssetDatabase.AddObjectToAsset(newAtlasTexture, this);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        freeGlyphRects.Clear();
        freeGlyphRects.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));
        usedGlyphRects.Clear();

        var success = UniTextFontEngine.TryAddGlyphToTexture(
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
            Debug.LogError(
                $"[UniTextFontAsset] Failed to add glyph {glyphIndex} even to new atlas #{atlasTextureIndex}!");
            return false;
        }

        glyph.atlasIndex = atlasTextureIndex;
        Debug.Log($"[UniTextFontAsset] Successfully added glyph {glyphIndex} to new atlas #{atlasTextureIndex}");

        glyphTable.Add(glyph);
        glyphLookupDictionary ??= new Dictionary<uint, Glyph>();
        glyphLookupDictionary[glyphIndex] = glyph;
        glyphIndexList ??= new List<uint>();
        glyphIndexList.Add(glyphIndex);

        atlasTextures[atlasTextureIndex].Apply(false, false);

        return true;
    }
    
    #endregion

    #region Static Creation Methods

    public static UniTextFont CreateFontAsset(byte[] fontBytes, int samplingPointSize = 90, int atlasPadding = 9,
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

        var fontAsset = CreateInstance<UniTextFont>();
        fontAsset.fontData = fontBytes;
        fontAsset.fontDataHash = ComputeFontDataHash(fontBytes);
        fontAsset.faceInfo = FontEngine.GetFaceInfo();
        fontAsset.atlasPopulationMode = UniTextAtlasPopulationMode.Dynamic;
        fontAsset.atlasWidth = atlasWidth;
        fontAsset.atlasHeight = atlasHeight;
        fontAsset.atlasPadding = atlasPadding;
        fontAsset.atlasRenderMode = renderMode;

        var texFormat = renderMode == GlyphRenderMode.COLOR || renderMode == GlyphRenderMode.COLOR_HINTED
            ? TextureFormat.RGBA32
            : TextureFormat.Alpha8;
        var texture = new Texture2D(1, 1, texFormat, false);
        fontAsset.atlasTextures = new[] { texture };

        fontAsset.freeGlyphRects = new List<GlyphRect> { new(0, 0, atlasWidth - 1, atlasHeight - 1) };
        fontAsset.usedGlyphRects = new List<GlyphRect>();

        fontAsset.ReadFontAssetDefinition();

        return fontAsset;
    }

    #endregion

    #region Dynamic Data Management

    public bool ClearDynamicDataOnBuild
    {
        get => clearDynamicDataOnBuild;
        set => clearDynamicDataOnBuild = value;
    }


    private void ClearGlyphDataOnly()
    {
        glyphTable?.Clear();
        characterTable?.Clear();
        glyphLookupDictionary?.Clear();
        characterLookupDictionary?.Clear();
        glyphIndexList?.Clear();
        glyphIndexListNewlyAdded?.Clear();
        charactersToAdd?.Clear();
        charactersToAddLookup?.Clear();
        usedGlyphRects?.Clear();
        freeGlyphRects?.Clear();
        freeGlyphRects?.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));

        Debug.Log($"UniTextFontAsset [{name}]: Glyph data cleared due to atlas reset. Glyphs will be re-rendered.");
    }


    public void ClearDynamicData()
    {
        glyphTable?.Clear();
        characterTable?.Clear();

        glyphLookupDictionary?.Clear();
        characterLookupDictionary?.Clear();
        glyphIndexList?.Clear();
        glyphIndexListNewlyAdded?.Clear();
        charactersToAdd?.Clear();
        charactersToAddLookup?.Clear();

        usedGlyphRects?.Clear();
        freeGlyphRects?.Clear();
        freeGlyphRects?.Add(new GlyphRect(0, 0, atlasWidth - 1, atlasHeight - 1));

        if (atlasTextures != null && atlasTextures.Length > 0)
        {
            for (var i = 1; i < atlasTextures.Length; i++)
                if (atlasTextures[i] != null)
                {
#if UNITY_EDITOR
                    DestroyImmediate(atlasTextures[i], true);
#else
                    Destroy(atlasTextures[i]);
#endif
                }

            var firstTexture = atlasTextures[0];
            if (firstTexture != null)
            {
                var texFormat = atlasRenderMode == GlyphRenderMode.COLOR ||
                                atlasRenderMode == GlyphRenderMode.COLOR_HINTED
                    ? TextureFormat.RGBA32
                    : TextureFormat.Alpha8;
                firstTexture.Reinitialize(1, 1, texFormat, false);
                UniTextFontEngine.ResetAtlasTexture(firstTexture);
                firstTexture.Apply(false, false);
            }

            if (atlasTextures.Length > 1)
                atlasTextures = new[] { firstTexture };
        }

        atlasTextureIndex = 0;

        if (currentlyLoadedFontInstanceId == GetInstanceID())
            currentlyLoadedFontInstanceId = 0;

        Debug.Log($"UniTextFontAsset [{name}]: Dynamic data cleared. Atlas will regenerate at runtime.");
    }

    #endregion

    #region Editor Support

#if UNITY_EDITOR
    
    [SerializeField] public Font sourceFont;
    
    public event Action Changed;

    private void OnValidate()
    {
        Changed?.Invoke();
    }

    public void SetFontData(byte[] data)
    {
        fontData = data;
        fontDataHash = ComputeFontDataHash(data);

        if (data != null && data.Length > 0)
            if (FontEngine.LoadFontFace(data, faceInfo.pointSize > 0 ? faceInfo.pointSize : 90, 0) ==
                FontEngineError.Success)
                faceInfo = FontEngine.GetFaceInfo();
    }

    public void UpdateFromSourceFont()
    {
        if (sourceFont == null) return;

        if (FontEngine.LoadFontFace(sourceFont, faceInfo.pointSize > 0 ? faceInfo.pointSize : 90, 0) ==
            FontEngineError.Success) faceInfo = FontEngine.GetFaceInfo();
    }
#endif

    #endregion

    public int ComputeFontDataHash()
    {
        return fontDataHash = ComputeFontDataHash(fontData);
    }
}


[Serializable]
public class UniTextCharacter
{
    public uint unicode;
    public uint glyphIndex;
    [NonSerialized] public Glyph glyph;
    
    public UniTextCharacter()
    {
    }

    public UniTextCharacter(uint unicode, uint glyphIndex)
    {
        this.unicode = unicode;
        this.glyphIndex = glyphIndex;
    }

    public UniTextCharacter(uint unicode, Glyph glyph)
    {
        this.unicode = unicode;
        this.glyph = glyph;
        glyphIndex = glyph?.index ?? 0;
    }
}