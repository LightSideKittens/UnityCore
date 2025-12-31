using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public sealed class UniTextFontProvider
{
    private readonly FastIntDictionary<UniTextFont> fontAssets = new();

    private UniTextFonts fontsAsset;
    private UniTextAppearance appearance;
    private UniTextFont mainFont;
    private int mainFontId;

    private float fontSize = 36f;
    private float fontScale = 1f;

    [ThreadStatic] private static HashSet<int> searchedFontAssets;


    public float FontSize
    {
        get => fontSize;
        set
        {
            fontSize = value;
            UpdateFontScale();
        }
    }

    public void SetFontSize(float size)
    {
        FontSize = size;
    }

    public UniTextFont MainFont => mainFont;
    public int MainFontId => mainFontId;

    public UniTextAppearance Appearance => appearance;


    public UniTextFontProvider(UniTextFonts fonts, UniTextAppearance appearance, float fontSize = 36f)
    {
        if (fonts == null || fonts.MainFont == null)
            throw new ArgumentNullException(nameof(fonts));

        this.fontsAsset = fonts;
        this.appearance = appearance;
        this.mainFont = fonts.MainFont;
        this.fontSize = fontSize;

        mainFontId = GetFontId(mainFont);
        RegisterFontAsset(mainFontId, mainFont);
        UpdateFontScale();

        for (int i = 0; i < fonts.fonts.Count; i++)
        {
            fonts.fonts[i].GetCachedInstanceId();
        }
    }

    private void UpdateFontScale()
    {
        var pointSize = mainFont.FaceInfo.pointSize;
        fontScale = pointSize > 0 ? fontSize / pointSize : 1f;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFontId(UniTextFont font)
    {
        if (font == null) return 0;
        var hash = font.FontDataHash;
        if (hash == 0 && font.HasFontData)
            hash = font.ComputeFontDataHash();
        return hash != 0 ? hash : font.GetCachedInstanceId();
    }

    public void RegisterFontAsset(int fontId, UniTextFont font)
    {
        if (font == null || fontId == 0) return;
        fontAssets[fontId] = font;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UniTextFont GetFontAsset(int fontId)
    {
        if (fontId == mainFontId)
            return mainFont;

        if (fontAssets.TryGetValue(fontId, out var asset))
            return asset;

        return mainFont;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetLineMetrics(float size, out float ascender, out float descender, out float lineHeight)
    {
        var faceInfo = mainFont.FaceInfo;
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

    public int FindFontForCodepoint(int codepoint)
    {
        if (SharedFontCache.TryGet(codepoint, mainFontId, out var cachedFontId))
        {
            if (cachedFontId == mainFontId || fontAssets.ContainsKey(cachedFontId))
                return cachedFontId;
        }

        searchedFontAssets ??= new HashSet<int>();
        searchedFontAssets.Clear();

        var unicode = (uint)codepoint;
        var foundFont = fontsAsset?.FindFontForCodepoint(unicode, searchedFontAssets);

        if (foundFont == null)
        {
            SharedFontCache.Set(codepoint, mainFontId, mainFontId);
            return mainFontId;
        }

        var fontId = GetFontId(foundFont);
        if (!fontAssets.ContainsKey(fontId))
            RegisterFontAsset(fontId, foundFont);

        SharedFontCache.Set(codepoint, mainFontId, fontId);
        return fontId;
    }

    private int cachedMaterialFontId = -1;
    private Material cachedMaterial;

    public Material GetMaterial(int fontId)
    {
        if (fontId == cachedMaterialFontId && cachedMaterial != null)
            return cachedMaterial;

        var fontAsset = GetFontAsset(fontId);
        var mat = appearance?.GetMaterial(fontAsset);
        cachedMaterialFontId = fontId;
        cachedMaterial = mat;
        return mat;
    }

    public void InvalidateMaterialCache()
    {
        cachedMaterialFontId = -1;
        cachedMaterial = null;
    }

    public byte[] GetFontData(int fontId)
    {
        return GetFontAsset(fontId)?.FontData;
    }

    public int GetFontDataHash(int fontId)
    {
        return fontId;
    }


    public void EnsureGlyphsInAtlas(ReadOnlySpan<ShapedRun> shapedRuns, ReadOnlySpan<ShapedGlyph> shapedGlyphs)
    {
        glyphsByFontBuffer ??= new FastIntDictionary<List<uint>>();
        glyphListPool ??= new Stack<List<uint>>();

        foreach (var kvp in glyphsByFontBuffer)
        {
            kvp.Value.Clear();
            glyphListPool.Push(kvp.Value);
        }

        glyphsByFontBuffer.ClearFast();

        for (var i = 0; i < shapedRuns.Length; i++)
        {
            ref readonly var run = ref shapedRuns[i];
            var fontId = run.fontId;

            if (!glyphsByFontBuffer.TryGetValue(fontId, out var glyphList))
            {
                glyphList = AcquireGlyphList();
                if (glyphList.Capacity < shapedGlyphs.Length)
                    glyphList.Capacity = shapedGlyphs.Length;
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

            if (glyphList.Count > 0)
            {
                var fontAsset = GetFontAsset(fontId);
                fontAsset.TryAddGlyphsByIndex(glyphList);
            }
        }
    }

    [ThreadStatic] private static FastIntDictionary<List<uint>> glyphsByFontBuffer;
    [ThreadStatic] private static Stack<List<uint>> glyphListPool;

    private static List<uint> AcquireGlyphList()
    {
        glyphListPool ??= new Stack<List<uint>>();
        if (glyphListPool.Count > 0)
            return glyphListPool.Pop();
        return new List<uint>(256);
    }
    
    public void EnsureCodepointsInAtlas(ReadOnlySpan<uint> codepoints)
    {
        if (codepoints.Length == 0) return;

        for (var i = 0; i < codepoints.Length; i++)
        {
            var codepoint = codepoints[i];
            var fontId = FindFontForCodepoint((int)codepoint);
            var font = GetFontAsset(fontId);
            font?.TryAddCharacter(codepoint);
        }
    }
}
