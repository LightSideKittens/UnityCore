using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore;

/// <summary>
/// Font provider that bridges UniTextFontAsset with the text processing pipeline.
/// Handles font fallback lookup and glyph information retrieval.
/// </summary>
public sealed class UniTextFontProvider
{
    // Registered font assets by ID
    private readonly Dictionary<int, UniTextFontAsset> fontAssets = new();

    // Reverse lookup: font asset instance ID -> our font ID
    private readonly Dictionary<int, int> fontAssetToId = new();

    // Main font asset (ID = 0)
    private UniTextFontAsset mainFontAsset;

    // Current font size
    private float fontSize = 36f;

    // Cached scale factor
    private float fontScale = 1f;

    // Next available font ID for dynamically discovered fonts
    private int nextFontId = 1000;

    // Searched font assets (to prevent infinite recursion in fallback search)
    private static HashSet<int> searchedFontAssets;

    // DEBUG: Enable detailed logging
    public static bool DebugLogging = false;

    /// <summary>
    /// Current font size.
    /// </summary>
    public float FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            UpdateFontScale();
            cachedFontId = -1; // Invalidate cache when font size changes
        }
    }

    /// <summary>
    /// Set font size (method form for compatibility).
    /// </summary>
    public void SetFontSize(float size)
    {
        FontSize = size;
    }

    /// <summary>
    /// Get scale factor for a specific font at a given size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetScale(int fontId, float size)
    {
        var fontAsset = GetFontAsset(fontId);
        float pointSize = fontAsset?.FaceInfo.pointSize ?? 0;
        return pointSize > 0 ? size / pointSize : 1f;
    }

    /// <summary>
    /// Main font asset.
    /// </summary>
    public UniTextFontAsset MainFontAsset => mainFontAsset;

    /// <summary>
    /// Create a font provider with the specified main font asset.
    /// </summary>
    public UniTextFontProvider(UniTextFontAsset fontAsset, float fontSize = 36f)
    {
        if (fontAsset == null)
            throw new ArgumentNullException(nameof(fontAsset));

        this.mainFontAsset = fontAsset;
        this.fontSize = fontSize;

        // Clear font cache when provider is recreated (e.g., after Domain Reload)
        // This prevents stale fontId references
        SharedFontCache.Clear();

        RegisterFontAsset(0, fontAsset);
        UpdateFontScale();

        if (DebugLogging)
        {
            Debug.Log($"[UniTextFontProvider] Created with fontAsset={fontAsset.name}, " +
                $"hasFontData={fontAsset.HasFontData}, fontDataSize={fontAsset.FontData?.Length ?? 0}, " +
                $"glyphLookup={fontAsset.GlyphLookupTable?.Count ?? 0}, charLookup={fontAsset.CharacterLookupTable?.Count ?? 0}");
        }
    }

    /// <summary>
    /// Create a font provider (requires setting font asset separately).
    /// </summary>
    public UniTextFontProvider(float fontSize = 36f)
    {
        this.fontSize = fontSize;
        UpdateFontScale();
    }

    private void UpdateFontScale()
    {
        if (mainFontAsset != null && mainFontAsset.FaceInfo.pointSize > 0)
        {
            fontScale = fontSize / mainFontAsset.FaceInfo.pointSize;
        }
        else
        {
            fontScale = 1f;
        }
    }

    /// <summary>
    /// Register a font asset with a specific ID.
    /// </summary>
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

    /// <summary>
    /// Get font asset by ID.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UniTextFontAsset GetFontAsset(int fontId)
    {
        return fontId == 0 ? mainFontAsset : (fontAssets.TryGetValue(fontId, out var asset) ? asset : mainFontAsset);
    }

    /// <summary>
    /// Get or create font ID for a font asset.
    /// </summary>
    private int GetOrCreateFontId(UniTextFontAsset fontAsset)
    {
        if (fontAsset == null)
            return 0;

        int instanceId = fontAsset.GetInstanceID();
        if (fontAssetToId.TryGetValue(instanceId, out int existingId))
            return existingId;

        // Register new font (this is a fallback font being used for the first time)
        int newId = nextFontId++;
        RegisterFontAsset(newId, fontAsset);
        return newId;
    }

    /// <summary>
    /// Try to get glyph metrics for a codepoint.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphMetrics(int fontId, int codepoint, out GlyphMetrics metrics)
    {
        metrics = default;

        var fontAsset = fontId == 0 ? mainFontAsset : (fontAssets.TryGetValue(fontId, out var f) ? f : null);
        if (fontAsset == null)
            return false;

        var charLookup = fontAsset.CharacterLookupTable;
        if (charLookup == null || !charLookup.TryGetValue((uint)codepoint, out var character))
            return false;

        var glyph = character?.glyph;
        if (glyph == null)
            return false;

        var glyphMetrics = glyph.metrics;
        float pointSize = fontAsset.FaceInfo.pointSize;
        float scale = pointSize > 0 ? fontSize / pointSize : fontScale;

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

    // Cache for fast repeated access to same font
    private int cachedFontId = -1;
    private UniTextFontAsset cachedFontAsset;
    private float cachedScale;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphInfo(int fontId, int codepoint, out uint glyphIndex, out float advance)
    {
        glyphIndex = 0;
        advance = 0;

        // Fast path: use cached font if same fontId
        UniTextFontAsset fontAsset;
        float scale;

        if (fontId == cachedFontId && cachedFontAsset != null)
        {
            fontAsset = cachedFontAsset;
            scale = cachedScale;
        }
        else
        {
            fontAsset = fontId == 0 ? mainFontAsset : (fontAssets.TryGetValue(fontId, out var f) ? f : null);
            if (fontAsset == null)
                return false;

            float pointSize = fontAsset.FaceInfo.pointSize;
            scale = pointSize > 0 ? fontSize / pointSize : fontScale;

            // Cache for next call
            cachedFontId = fontId;
            cachedFontAsset = fontAsset;
            cachedScale = scale;
        }

        var charLookup = fontAsset.CharacterLookupTable;
        if (charLookup == null)
            return false;

        uint unicode = (uint)codepoint;
        if (!charLookup.TryGetValue(unicode, out var character) || character == null)
        {
            // Try dynamic add only if not found
            if (fontAsset.AtlasPopulationMode != UniTextAtlasPopulationMode.Dynamic ||
                !fontAsset.TryAddCharacter(unicode, out character))
                return false;
        }

        var glyph = character.glyph;
        if (glyph == null)
            return false;

        glyphIndex = character.glyphIndex;
        advance = glyph.metrics.horizontalAdvance * scale;
        return true;
    }

    /// <summary>
    /// Get line metrics (ascender, descender, line height) for the main font.
    /// </summary>
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

    /// <summary>
    /// Get line metrics for a specific font size.
    /// </summary>
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
        float scale = faceInfo.pointSize > 0 ? size / faceInfo.pointSize : 1f;
        ascender = faceInfo.ascentLine * scale;
        descender = faceInfo.descentLine * scale;
        lineHeight = faceInfo.lineHeight * scale;

        if (lineHeight <= 0)
            lineHeight = (ascender - descender) * 1.2f;
    }

    /// <summary>
    /// Find which font supports the given codepoint.
    /// Only checks cmap (via HarfBuzz), does NOT add to atlas.
    /// Atlas population happens after shaping, by glyph indices.
    /// </summary>
    public int FindFontForCodepoint(int codepoint, int baseFontId)
    {
        // Fast path: check cache first
        if (SharedFontCache.TryGet(codepoint, baseFontId, out int cachedFontId) &&
            (cachedFontId == 0 || fontAssets.ContainsKey(cachedFontId)))
        {
            if (DebugLogging)
                Debug.Log($"[UniTextFontProvider.FindFontForCodepoint] U+{codepoint:X4} -> cached fontId={cachedFontId}");
            return cachedFontId;
        }

        var baseFont = baseFontId == 0 ? mainFontAsset : (fontAssets.TryGetValue(baseFontId, out var f) ? f : mainFontAsset);
        if (baseFont == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        if (DebugLogging)
            Debug.Log($"[UniTextFontProvider.FindFontForCodepoint] Searching for U+{codepoint:X4} in {baseFont.name}");

        searchedFontAssets ??= new HashSet<int>();
        searchedFontAssets.Clear();
        searchedFontAssets.Add(baseFont.GetInstanceID());

        uint unicode = (uint)codepoint;
        var foundFont = FindFontWithGlyph(unicode, baseFont, true);

        if (foundFont == null)
        {
            if (DebugLogging)
                Debug.Log($"[UniTextFontProvider.FindFontForCodepoint] U+{codepoint:X4} not in base font, checking global fallbacks...");

            var globalFallbacks = UniTextSettings.FallbackFontAssets;
            if (globalFallbacks != null)
            {
                int count = globalFallbacks.Count;
                for (int i = 0; i < count; i++)
                {
                    var fallback = globalFallbacks[i];
                    if (fallback == null || !searchedFontAssets.Add(fallback.GetInstanceID()))
                        continue;

                    foundFont = FindFontWithGlyph(unicode, fallback, true);
                    if (foundFont != null)
                    {
                        if (DebugLogging)
                            Debug.Log($"[UniTextFontProvider.FindFontForCodepoint] U+{codepoint:X4} FOUND in global fallback {foundFont.name}");
                        break;
                    }
                }
            }
        }

        if (foundFont == null)
        {
            if (DebugLogging)
                Debug.LogWarning($"[UniTextFontProvider.FindFontForCodepoint] U+{codepoint:X4} NOT FOUND in any font! Using base font.");
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        int fontId = GetOrCreateFontId(foundFont);
        SharedFontCache.Set(codepoint, baseFontId, fontId);
        return fontId;
    }

    /// <summary>
    /// Check if font has glyph for codepoint using cmap (via HarfBuzz).
    /// Does NOT add to atlas - only checks font coverage.
    /// </summary>
    private UniTextFontAsset FindFontWithGlyph(uint unicode, UniTextFontAsset fontAsset, bool searchFallbacks)
    {
        if (fontAsset == null)
            return null;

        // Check cmap via HarfBuzz (authoritative source for font coverage)
        if (fontAsset.HasFontData)
        {
            uint glyphIndex = HarfBuzzFontValidator.GetGlyphIndex(fontAsset, unicode);
            if (glyphIndex != 0)
            {
                if (DebugLogging)
                    Debug.Log($"[UniTextFontProvider.FindFontWithGlyph] U+{unicode:X4} found in {fontAsset.name} cmap (glyphIndex={glyphIndex})");
                return fontAsset;
            }
        }
        else
        {
            // Fallback to FontEngine for fonts without raw data
            if (fontAsset.LoadFontFace() == UnityEngine.TextCore.LowLevel.FontEngineError.Success)
            {
                uint glyphIndex = UniTextFontEngine.GetGlyphIndex(unicode);
                if (glyphIndex != 0)
                {
                    if (DebugLogging)
                        Debug.Log($"[UniTextFontProvider.FindFontWithGlyph] U+{unicode:X4} found in {fontAsset.name} via FontEngine (glyphIndex={glyphIndex})");
                    return fontAsset;
                }
            }
        }

        if (DebugLogging)
            Debug.Log($"[UniTextFontProvider.FindFontWithGlyph] U+{unicode:X4} NOT in {fontAsset.name} cmap");

        // Search local fallbacks
        if (searchFallbacks && fontAsset.FallbackFontAssetTable != null)
        {
            for (int i = 0; i < fontAsset.FallbackFontAssetTable.Count; i++)
            {
                var fallback = fontAsset.FallbackFontAssetTable[i];
                if (fallback == null || !searchedFontAssets.Add(fallback.GetInstanceID()))
                    continue;

                var result = FindFontWithGlyph(unicode, fallback, true);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    public uint GetGlyphIndex(int fontId, int codepoint)
    {
        if (TryGetGlyphInfo(fontId, codepoint, out uint glyphIndex, out _))
            return glyphIndex;
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphByIndex(int fontId, uint glyphIndex, out float advance)
    {
        advance = 0;

        var fontAsset = fontId == 0 ? mainFontAsset : (fontAssets.TryGetValue(fontId, out var f) ? f : null);
        if (fontAsset == null)
            return false;

        var glyphLookup = fontAsset.GlyphLookupTable;
        if (glyphLookup == null || !glyphLookup.TryGetValue(glyphIndex, out var glyph) || glyph == null)
            return false;

        float pointSize = fontAsset.FaceInfo.pointSize;
        float scale = pointSize > 0 ? fontSize / pointSize : fontScale;
        advance = glyph.metrics.horizontalAdvance * scale;
        return true;
    }

    public Texture2D GetAtlasTexture(int fontId)
        => (fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset)?.AtlasTexture;

    public Material GetMaterial(int fontId)
        => (fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset)?.Material;

    public byte[] GetFontData(int fontId)
    {
        var fontAsset = fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset;
        var data = fontAsset?.FontData;

        if (DebugLogging)
        {
            Debug.Log($"[UniTextFontProvider.GetFontData] fontId={fontId}, " +
                $"fontAsset={fontAsset?.name ?? "null"}, dataSize={data?.Length ?? 0}");
        }

        return data;
    }

    public bool HasFontData(int fontId)
        => (fontAssets.TryGetValue(fontId, out var f) ? f : mainFontAsset)?.HasFontData ?? false;

    /// <summary>
    /// Ensure all glyph indices from shaping results are in the atlas.
    /// This should be called AFTER shaping, before rendering.
    /// </summary>
    public void EnsureGlyphsInAtlas(ReadOnlySpan<ShapedRun> shapedRuns, ReadOnlySpan<ShapedGlyph> shapedGlyphs)
    {
        // Group glyphs by fontId for batch processing
        glyphsByFontBuffer.Clear();

        for (int i = 0; i < shapedRuns.Length; i++)
        {
            ref readonly var run = ref shapedRuns[i];
            int fontId = run.fontId;

            if (!glyphsByFontBuffer.TryGetValue(fontId, out var glyphList))
            {
                glyphList = glyphListPool.Count > 0 ? glyphListPool.Pop() : new List<uint>(64);
                glyphsByFontBuffer[fontId] = glyphList;
            }

            // Add all glyph indices from this run
            int end = run.glyphStart + run.glyphCount;
            for (int g = run.glyphStart; g < end; g++)
            {
                uint glyphIndex = (uint)shapedGlyphs[g].glyphId;
                if (glyphIndex != 0)
                    glyphList.Add(glyphIndex);
            }
        }

        // Add glyphs to atlas for each font
        foreach (var kvp in glyphsByFontBuffer)
        {
            int fontId = kvp.Key;
            var glyphList = kvp.Value;

            var fontAsset = GetFontAsset(fontId);
            if (fontAsset != null && glyphList.Count > 0)
            {
                int added = fontAsset.TryAddGlyphsByIndex(glyphList);

                if (DebugLogging && added > 0)
                    Debug.Log($"[UniTextFontProvider.EnsureGlyphsInAtlas] Added {added} glyphs to fontId={fontId}");
            }

            // Return list to pool
            glyphList.Clear();
            glyphListPool.Push(glyphList);
        }
    }

    // Buffers for EnsureGlyphsInAtlas (reused to avoid allocations)
    private readonly Dictionary<int, List<uint>> glyphsByFontBuffer = new();
    private readonly Stack<List<uint>> glyphListPool = new();
}
