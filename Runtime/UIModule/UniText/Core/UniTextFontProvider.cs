using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

/// <summary>
/// Font provider that bridges TMP_FontAsset with the text processing pipeline.
/// Uses TMP_FontAssetUtilities for proper fallback handling (same as TMP itself).
/// </summary>
public sealed class UniTextFontProvider
{
    // Registered font assets by ID
    private readonly Dictionary<int, TMP_FontAsset> fontAssets = new();

    // Reverse lookup: font asset instance ID -> our font ID
    private readonly Dictionary<int, int> fontAssetToId = new();

    // Main font asset (ID = 0)
    private TMP_FontAsset mainFontAsset;

    // Current font size
    private float fontSize = 36f;

    // Cached scale factor
    private float fontScale = 1f;

    // Next available font ID for dynamically discovered fonts
    private int nextFontId = 1000;

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
        if (fontAsset != null && fontAsset.faceInfo.pointSize > 0)
        {
            return size / fontAsset.faceInfo.pointSize;
        }
        return 1f;
    }

    /// <summary>
    /// Main font asset.
    /// </summary>
    public TMP_FontAsset MainFontAsset => mainFontAsset;

    /// <summary>
    /// Create a font provider with the specified main font asset.
    /// </summary>
    public UniTextFontProvider(TMP_FontAsset fontAsset, float fontSize = 36f)
    {
        if (fontAsset == null)
            throw new ArgumentNullException(nameof(fontAsset));

        this.mainFontAsset = fontAsset;
        this.fontSize = fontSize;

        RegisterFontAsset(0, fontAsset);
        UpdateFontScale();
    }

    /// <summary>
    /// Create a font provider using the default font from settings.
    /// </summary>
    public UniTextFontProvider(float fontSize = 36f)
    {
        this.fontSize = fontSize;

        var defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null)
        {
            this.mainFontAsset = defaultFont;
            RegisterFontAsset(0, defaultFont);
        }

        UpdateFontScale();
    }

    private void UpdateFontScale()
    {
        if (mainFontAsset != null && mainFontAsset.faceInfo.pointSize > 0)
        {
            fontScale = fontSize / mainFontAsset.faceInfo.pointSize;
        }
        else
        {
            fontScale = 1f;
        }
    }

    /// <summary>
    /// Register a font asset with a specific ID.
    /// </summary>
    public void RegisterFontAsset(int fontId, TMP_FontAsset fontAsset)
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
    public TMP_FontAsset GetFontAsset(int fontId)
    {
        return fontAssets.TryGetValue(fontId, out var asset) ? asset : mainFontAsset;
    }

    /// <summary>
    /// Get or create font ID for a font asset.
    /// </summary>
    private int GetOrCreateFontId(TMP_FontAsset fontAsset)
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

        // Use TMP's utility to get character (same as TMP does internally)
        var character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(
            (uint)codepoint,
            fontAsset,
            false, // Don't search fallbacks here - we handle that in FindFontForCodepoint
            FontStyles.Normal,
            FontWeight.Regular,
            out _);

        if (character?.glyph == null)
            return false;

        var glyph = character.glyph;
        var glyphMetrics = glyph.metrics;

        // Get scale for the actual font that contains the glyph
        var actualFont = character.textAsset as TMP_FontAsset ?? fontAsset;
        float scale = fontSize / actualFont.faceInfo.pointSize;

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
    /// Uses TMP_FontAssetUtilities - the same method TMP uses internally.
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

        // Use TMP's utility to get character - this is exactly how TMP does it
        var character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(
            (uint)codepoint,
            fontAsset,
            false, // Don't search fallbacks - caller should have used FindFontForCodepoint
            FontStyles.Normal,
            FontWeight.Regular,
            out _);

        if (character?.glyph == null)
            return false;

        // Verify the character is from the expected font
        var actualFont = character.textAsset as TMP_FontAsset;
        if (actualFont != null && actualFont.GetInstanceID() != fontAsset.GetInstanceID())
        {
            // Character was found in a different font (shouldn't happen with includeFallbacks=false)
            return false;
        }

        glyphIndex = character.glyphIndex;

        // Calculate scale for this specific font
        float scale = fontAsset.faceInfo.pointSize > 0
            ? fontSize / fontAsset.faceInfo.pointSize
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

        var faceInfo = mainFontAsset.faceInfo;
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

        float scale = size / mainFontAsset.faceInfo.pointSize;
        var faceInfo = mainFontAsset.faceInfo;
        ascender = faceInfo.ascentLine * scale;
        descender = faceInfo.descentLine * scale;
        lineHeight = faceInfo.lineHeight * scale;

        if (lineHeight <= 0)
            lineHeight = (ascender - descender) * 1.2f;
    }

    /// <summary>
    /// Find font ID that contains the specified codepoint.
    /// Uses TMP_FontAssetUtilities with fallback search - exactly like TMP does.
    /// Returns the font ID for the font that actually contains the glyph.
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

        // Use TMP's utility WITH fallback search - this is exactly how TMP finds glyphs
        var character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(
            (uint)codepoint,
            baseFont,
            true, // includeFallbacks - search all fallback fonts
            FontStyles.Normal,
            FontWeight.Regular,
            out _);

        if (character == null)
        {
            // Not found anywhere - try global fallbacks
            var globalFallbacks = TMP_Settings.fallbackFontAssets;
            if (globalFallbacks != null && globalFallbacks.Count > 0)
            {
                character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(
                    (uint)codepoint,
                    baseFont,
                    globalFallbacks,
                    true,
                    FontStyles.Normal,
                    FontWeight.Regular,
                    out _);
            }
        }

        if (character == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        // character.textAsset tells us which font ACTUALLY contains this glyph
        var actualFont = character.textAsset as TMP_FontAsset;
        if (actualFont == null)
        {
            SharedFontCache.Set(codepoint, baseFontId, baseFontId);
            return baseFontId;
        }

        // Get or create font ID for this font
        int fontId = GetOrCreateFontId(actualFont);

        // Cache the result
        SharedFontCache.Set(codepoint, baseFontId, fontId);
        return fontId;
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

        return fontAsset?.atlasTexture;
    }

    /// <summary>
    /// Get material for a font.
    /// </summary>
    public Material GetMaterial(int fontId)
    {
        if (!fontAssets.TryGetValue(fontId, out var fontAsset))
            fontAsset = mainFontAsset;

        return fontAsset?.material;
    }
}
