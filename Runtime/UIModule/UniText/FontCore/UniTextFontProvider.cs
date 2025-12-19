using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public sealed class UniTextFontProvider
{
    private readonly Dictionary<int, UniTextFontAsset> fontAssets = new();

    private readonly Dictionary<int, int> fontAssetToId = new();

    private UniTextFontAsset mainFontAsset;

    private float fontSize = 36f;

    private float fontScale = 1f;

    private int nextFontId = 1000;

    private static HashSet<int> searchedFontAssets;


    public float FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            UpdateFontScale();
            cachedFontId = -1;
        }
    }


    public void SetFontSize(float size)
    {
        FontSize = size;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetScale(int fontId, float size)
    {
        var fontAsset = GetFontAsset(fontId);
        var pointSize = fontAsset?.FaceInfo.pointSize ?? 0;
        return pointSize > 0 ? size / pointSize : 1f;
    }


    public UniTextFontAsset MainFontAsset => mainFontAsset;


    public UniTextFontProvider(UniTextFontAsset fontAsset, float fontSize = 36f)
    {
        if (fontAsset == null)
            throw new ArgumentNullException(nameof(fontAsset));

        mainFontAsset = fontAsset;
        this.fontSize = fontSize;

        SharedFontCache.Clear();

        RegisterFontAsset(0, fontAsset);
        UpdateFontScale();
    }


    public UniTextFontProvider(float fontSize = 36f)
    {
        this.fontSize = fontSize;
        UpdateFontScale();
    }

    private void UpdateFontScale()
    {
        if (mainFontAsset != null && mainFontAsset.FaceInfo.pointSize > 0)
            fontScale = fontSize / mainFontAsset.FaceInfo.pointSize;
        else
            fontScale = 1f;
    }


    public void RegisterFontAsset(int fontId, UniTextFontAsset fontAsset)
    {
        if (fontAsset == null) return;

        fontAssets[fontId] = fontAsset;
        fontAssetToId[fontAsset.GetInstanceID()] = fontId;

        if (fontId == 0)
        {
            mainFontAsset = fontAsset;
            UpdateFontScale();
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UniTextFontAsset GetFontAsset(int fontId)
    {
        return fontId == 0 ? mainFontAsset : fontAssets.TryGetValue(fontId, out var asset) ? asset : mainFontAsset;
    }


    private int GetOrCreateFontId(UniTextFontAsset fontAsset)
    {
        if (fontAsset == null)
            return 0;

        var instanceId = fontAsset.GetInstanceID();
        if (fontAssetToId.TryGetValue(instanceId, out var existingId))
            return existingId;

        var newId = nextFontId++;
        RegisterFontAsset(newId, fontAsset);
        return newId;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphMetrics(int fontId, int codepoint, out GlyphMetrics metrics)
    {
        metrics = default;

        var fontAsset = fontId == 0 ? mainFontAsset : fontAssets.TryGetValue(fontId, out var f) ? f : null;
        if (fontAsset == null)
            return false;

        var charLookup = fontAsset.CharacterLookupTable;
        if (charLookup == null || !charLookup.TryGetValue((uint)codepoint, out var character))
            return false;

        var glyph = character?.glyph;
        if (glyph == null)
            return false;

        var glyphMetrics = glyph.metrics;
        var pointSize = fontAsset.FaceInfo.pointSize;
        var scale = pointSize > 0 ? fontSize / pointSize : fontScale;

        metrics = new GlyphMetrics
        {
            width = glyphMetrics.width * scale,
            height = glyphMetrics.height * scale,
            bearingX = glyphMetrics.horizontalBearingX * scale,
            bearingY = glyphMetrics.horizontalBearingY * scale,
            advance = glyphMetrics.horizontalAdvance * scale
        };

        return true;
    }

    private int cachedFontId = -1;
    private UniTextFontAsset cachedFontAsset;
    private float cachedScale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphInfo(int fontId, int codepoint, out uint glyphIndex, out float advance)
    {
        glyphIndex = 0;
        advance = 0;

        UniTextFontAsset fontAsset;
        float scale;

        if (fontId == cachedFontId && cachedFontAsset != null)
        {
            fontAsset = cachedFontAsset;
            scale = cachedScale;
        }
        else
        {
            fontAsset = fontId == 0 ? mainFontAsset : fontAssets.TryGetValue(fontId, out var f) ? f : null;
            if (fontAsset == null)
                return false;

            var pointSize = fontAsset.FaceInfo.pointSize;
            scale = pointSize > 0 ? fontSize / pointSize : fontScale;

            cachedFontId = fontId;
            cachedFontAsset = fontAsset;
            cachedScale = scale;
        }

        var charLookup = fontAsset.CharacterLookupTable;
        if (charLookup == null)
            return false;

        var unicode = (uint)codepoint;
        if (!charLookup.TryGetValue(unicode, out var character) || character == null)
            if (fontAsset.AtlasPopulationMode != UniTextAtlasPopulationMode.Dynamic ||
                !fontAsset.TryAddCharacter(unicode, out character))
                return false;

        var glyph = character.glyph;
        if (glyph == null)
            return false;

        glyphIndex = character.glyphIndex;
        advance = glyph.metrics.horizontalAdvance * scale;
        return true;
    }


    public void GetLineMetrics(out float ascender, out float descender, out float lineHeight)
    {
        if (mainFontAsset == null)
        {
            ascender = fontSize * 0.8f;
            descender = fontSize * 0.2f;
            lineHeight = fontSize;
            return;
        }

        var faceInfo = mainFontAsset.FaceInfo;
        ascender = faceInfo.ascentLine * fontScale;
        descender = faceInfo.descentLine * fontScale;
        lineHeight = faceInfo.lineHeight * fontScale;

        if (lineHeight <= 0)
            lineHeight = (ascender - descender) * 1.2f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetLineMetrics(float size, out float ascender, out float descender, out float lineHeight)
    {
        if (mainFontAsset == null)
        {
            ascender = size * 0.8f;
            descender = size * 0.2f;
            lineHeight = size;
            return;
        }

        var faceInfo = mainFontAsset.FaceInfo;
        var scale = faceInfo.pointSize > 0 ? size / faceInfo.pointSize : 1f;
        ascender = faceInfo.ascentLine * scale;
        descender = faceInfo.descentLine * scale;
        lineHeight = faceInfo.lineHeight * scale;

        if (lineHeight <= 0)
            lineHeight = (ascender - descender) * 1.2f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateTextHeight(float ascender, float descender, int lineCount, float lineHeight,
        float lineSpacing = 0f)
    {
        return ascender - descender + (lineCount - 1) * (lineHeight + lineSpacing);
    }


    public int FindFontForCodepoint(int codepoint, int baseFontId)
    {
        if (SharedFontCache.TryGet(codepoint, baseFontId, out var cachedFontId) &&
            (cachedFontId == 0 || fontAssets.ContainsKey(cachedFontId)))
            return cachedFontId;

        var baseFont = baseFontId == 0 ? mainFontAsset :
            fontAssets.TryGetValue(baseFontId, out var f) ? f : mainFontAsset;
        if (baseFont == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        searchedFontAssets ??= new HashSet<int>();
        searchedFontAssets.Clear();
        searchedFontAssets.Add(baseFont.GetInstanceID());

        var unicode = (uint)codepoint;
        var foundFont = FindFontWithGlyph(unicode, baseFont, true);

        if (foundFont == null)
        {
            var globalFallbacks = UniTextSettings.FallbackFontAssets;
            if (globalFallbacks != null)
            {
                var count = globalFallbacks.Count;
                for (var i = 0; i < count; i++)
                {
                    var fallback = globalFallbacks[i];
                    if (fallback == null || !searchedFontAssets.Add(fallback.GetInstanceID()))
                        continue;

                    foundFont = FindFontWithGlyph(unicode, fallback, true);
                    if (foundFont != null) break;
                }
            }
        }

        if (foundFont == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        var fontId = GetOrCreateFontId(foundFont);
        SharedFontCache.Set(codepoint, baseFontId, fontId);
        return fontId;
    }


    private UniTextFontAsset FindFontWithGlyph(uint unicode, UniTextFontAsset fontAsset, bool searchFallbacks)
    {
        if (fontAsset == null)
            return null;

        if (fontAsset.HasFontData)
        {
            var glyphIndex = HarfBuzzFontValidator.GetGlyphIndex(fontAsset, unicode);
            if (glyphIndex != 0) return fontAsset;
        }
        else
        {
            if (fontAsset.LoadFontFace() == UnityEngine.TextCore.LowLevel.FontEngineError.Success)
            {
                var glyphIndex = UniTextFontEngine.GetGlyphIndex(unicode);
                if (glyphIndex != 0) return fontAsset;
            }
        }

        if (searchFallbacks && fontAsset.FallbackFontAssetTable != null)
            for (var i = 0; i < fontAsset.FallbackFontAssetTable.Count; i++)
            {
                var fallback = fontAsset.FallbackFontAssetTable[i];
                if (fallback == null || !searchedFontAssets.Add(fallback.GetInstanceID()))
                    continue;

                var result = FindFontWithGlyph(unicode, fallback, true);
                if (result != null)
                    return result;
            }

        return null;
    }

    public uint GetGlyphIndex(int fontId, int codepoint)
    {
        if (TryGetGlyphInfo(fontId, codepoint, out var glyphIndex, out _))
            return glyphIndex;
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphByIndex(int fontId, uint glyphIndex, out float advance)
    {
        advance = 0;

        var fontAsset = fontId == 0 ? mainFontAsset : fontAssets.TryGetValue(fontId, out var f) ? f : null;
        if (fontAsset == null)
            return false;

        var glyphLookup = fontAsset.GlyphLookupTable;
        if (glyphLookup == null || !glyphLookup.TryGetValue(glyphIndex, out var glyph) || glyph == null)
            return false;

        var pointSize = fontAsset.FaceInfo.pointSize;
        var scale = pointSize > 0 ? fontSize / pointSize : fontScale;
        advance = glyph.metrics.horizontalAdvance * scale;
        return true;
    }

    public Texture2D GetAtlasTexture(int fontId)
    {
        return (fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset)?.AtlasTexture;
    }

    public Material GetMaterial(int fontId)
    {
        return (fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset)?.Material;
    }

    public byte[] GetFontData(int fontId)
    {
        var fontAsset = fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset;
        var data = fontAsset?.FontData;
        return data;
    }

    public bool HasFontData(int fontId)
    {
        return (fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset)?.HasFontData ?? false;
    }

    public int GetFontDataHash(int fontId)
    {
        var fontAsset = fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset;
        if (fontAsset == null) return 0;

        var hash = fontAsset.FontDataHash;
        if (hash == 0 && fontAsset.HasFontData)
            hash = UniTextFontAsset.ComputeFontDataHash(fontAsset.FontData);
        return hash;
    }


    public void EnsureGlyphsInAtlas(ReadOnlySpan<ShapedRun> shapedRuns, ReadOnlySpan<ShapedGlyph> shapedGlyphs)
    {
        foreach (var kvp in glyphsByFontBuffer)
        {
            kvp.Value.Clear();
            glyphListPool.Push(kvp.Value);
        }

        glyphsByFontBuffer.Clear();

        for (var i = 0; i < shapedRuns.Length; i++)
        {
            ref readonly var run = ref shapedRuns[i];
            var fontId = run.fontId;

            if (!glyphsByFontBuffer.TryGetValue(fontId, out var glyphList))
            {
                glyphList = AcquireGlyphList();
                glyphsByFontBuffer[fontId] = glyphList;
            }

            var end = run.glyphStart + run.glyphCount;
            for (var g = run.glyphStart; g < end; g++)
            {
                var glyphIndex = (uint)shapedGlyphs[g].glyphId;
                if (glyphIndex != 0)
                    glyphList.Add(glyphIndex);
            }
        }

        foreach (var kvp in glyphsByFontBuffer)
        {
            var fontId = kvp.Key;
            var glyphList = kvp.Value;

            var fontAsset = GetFontAsset(fontId);
            if (fontAsset != null && glyphList.Count > 0)
            {
                var added = fontAsset.TryAddGlyphsByIndex(glyphList);
            }
        }
    }

    private static readonly Dictionary<int, List<uint>> glyphsByFontBuffer = new();
    private static readonly Stack<List<uint>> glyphListPool = new();

    private static List<uint> AcquireGlyphList()
    {
        if (glyphListPool.Count > 0)
            return glyphListPool.Pop();
        return new List<uint>(256);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        glyphsByFontBuffer.Clear();
        glyphListPool.Clear();
    }
}