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
    public float GetScale(int fontId, float size)
    {
        var fontAsset = GetFontAsset(fontId);
        if (fontAsset != null && fontAsset.FaceInfo.pointSize > 0)
        {
            return size / fontAsset.FaceInfo.pointSize;
        }
        return 1f;
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

        RegisterFontAsset(0, fontAsset);
        UpdateFontScale();
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
    public UniTextFontAsset GetFontAsset(int fontId)
    {
        if (fontAssets.TryGetValue(fontId, out var asset))
            return asset;

        return mainFontAsset;
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

        if (!fontAssets.TryGetValue(fontId, out var fontAsset))
            fontAsset = mainFontAsset;

        if (fontAsset == null)
            return false;

        var charLookup = fontAsset.CharacterLookupTable;
        if (charLookup == null || !charLookup.TryGetValue((uint)codepoint, out var character))
            return false;

        if (character.glyph == null)
            return false;

        var glyph = character.glyph;
        var glyphMetrics = glyph.metrics;

        float scale = fontSize / fontAsset.FaceInfo.pointSize;

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

    /// <summary>
    /// Try to get glyph info (index and advance) for a codepoint.
    /// IMPORTANT: Does NOT search fallback fonts - use FindFontForCodepoint first!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetGlyphInfo(int fontId, int codepoint, out uint glyphIndex, out float advance)
    {
        glyphIndex = 0;
        advance = 0;

        if (!fontAssets.TryGetValue(fontId, out var fontAsset))
            fontAsset = mainFontAsset;

        if (fontAsset == null)
            return false;

        var charLookup = fontAsset.CharacterLookupTable;
        if (charLookup == null)
            return false;

        // Try to get existing character
        if (!charLookup.TryGetValue((uint)codepoint, out var character))
        {
            // Try to add character dynamically
            if (fontAsset.AtlasPopulationMode == UniTextAtlasPopulationMode.Dynamic)
            {
                if (!fontAsset.TryAddCharacter((uint)codepoint, out character))
                    return false;
            }
            else
            {
                return false;
            }
        }

        if (character?.glyph == null)
            return false;

        glyphIndex = character.glyphIndex;

        // Calculate scale for this specific font
        float scale = fontAsset.FaceInfo.pointSize > 0
            ? fontSize / fontAsset.FaceInfo.pointSize
            : fontScale;

        advance = character.glyph.metrics.horizontalAdvance * scale;
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
    public void GetLineMetrics(float size, out float ascender, out float descender, out float lineHeight)
    {
        if (mainFontAsset == null)
        {
            ascender = size * 0.8f;
            descender = size * 0.2f;
            lineHeight = size;
            return;
        }

        float scale = size / mainFontAsset.FaceInfo.pointSize;
        var faceInfo = mainFontAsset.FaceInfo;
        ascender = faceInfo.ascentLine * scale;
        descender = faceInfo.descentLine * scale;
        lineHeight = faceInfo.lineHeight * scale;

        if (lineHeight <= 0)
            lineHeight = (ascender - descender) * 1.2f;
    }

    /// <summary>
    /// Find font ID that contains the specified codepoint.
    /// Searches: base font → font's fallbacks → global fallbacks (UniTextSettings).
    /// Uses SharedFontCache for performance.
    /// </summary>
    public int FindFontForCodepoint(int codepoint, int baseFontId)
    {
        // Check cache first
        if (SharedFontCache.TryGet(codepoint, baseFontId, out int cachedFontId))
            return cachedFontId;

        // Get the base font asset
        if (!fontAssets.TryGetValue(baseFontId, out var baseFont))
            baseFont = mainFontAsset;

        if (baseFont == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        // Initialize search tracking
        searchedFontAssets ??= new HashSet<int>();
        searchedFontAssets.Clear();
        searchedFontAssets.Add(baseFont.GetInstanceID());

        // 1. Search in base font and its fallbacks
        var foundFont = FindCharacterInFontAsset((uint)codepoint, baseFont, true);

        // 2. Search in global fallback fonts from UniTextSettings
        if (foundFont == null)
        {
            var globalFallbacks = UniTextSettings.FallbackFontAssets;
            if (globalFallbacks != null && globalFallbacks.Count > 0)
            {
                for (int i = 0; i < globalFallbacks.Count; i++)
                {
                    var fallback = globalFallbacks[i];
                    if (fallback == null)
                        continue;

                    int fallbackId = fallback.GetInstanceID();
                    if (!searchedFontAssets.Add(fallbackId))
                        continue; // Already searched

                    foundFont = FindCharacterInFontAsset((uint)codepoint, fallback, true);
                    if (foundFont != null)
                        break;
                }
            }
        }

        if (foundFont == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        // Get or create font ID for this font
        int fontId = GetOrCreateFontId(foundFont);

        // Cache the result
        SharedFontCache.Set(codepoint, baseFontId, fontId);
        return fontId;
    }

    /// <summary>
    /// Find character in font asset, optionally searching fallbacks.
    /// Returns the font asset that contains the character.
    /// </summary>
    private UniTextFontAsset FindCharacterInFontAsset(uint unicode, UniTextFontAsset fontAsset, bool searchFallbacks)
    {
        if (fontAsset == null)
            return null;

        var charLookup = fontAsset.CharacterLookupTable;

        // Check if character exists in this font
        if (charLookup != null && charLookup.ContainsKey(unicode))
            return fontAsset;

        // Try to add character dynamically
        if (fontAsset.AtlasPopulationMode == UniTextAtlasPopulationMode.Dynamic)
        {
            if (fontAsset.TryAddCharacter(unicode, out _))
                return fontAsset;
        }

        // Search fallback fonts
        if (searchFallbacks && fontAsset.FallbackFontAssetTable != null)
        {
            for (int i = 0; i < fontAsset.FallbackFontAssetTable.Count; i++)
            {
                var fallback = fontAsset.FallbackFontAssetTable[i];
                if (fallback == null)
                    continue;

                int fallbackId = fallback.GetInstanceID();
                if (!searchedFontAssets.Add(fallbackId))
                    continue; // Already searched this font

                var result = FindCharacterInFontAsset(unicode, fallback, true);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Get glyph ID for a codepoint (for mesh generation).
    /// </summary>
    public uint GetGlyphIndex(int fontId, int codepoint)
    {
        if (TryGetGlyphInfo(fontId, codepoint, out uint glyphIndex, out _))
            return glyphIndex;
        return 0;
    }

    /// <summary>
    /// Get atlas texture for a font.
    /// </summary>
    public Texture2D GetAtlasTexture(int fontId)
    {
        if (!fontAssets.TryGetValue(fontId, out var fontAsset))
            fontAsset = mainFontAsset;

        return fontAsset?.AtlasTexture;
    }

    /// <summary>
    /// Get material for a font.
    /// </summary>
    public Material GetMaterial(int fontId)
    {
        if (!fontAssets.TryGetValue(fontId, out var fontAsset))
            fontAsset = mainFontAsset;

        return fontAsset?.Material;
    }
}
