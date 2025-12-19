using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;


public static class UniTextFontEngine
{
    private delegate uint GetGlyphIndexDelegate(uint unicode);

    private delegate void ResetAtlasTextureDelegate(Texture2D texture);

    private delegate bool TryAddGlyphToTextureDelegate(
        uint glyphIndex, int padding, GlyphPackingMode packingMode,
        List<GlyphRect> freeGlyphRects, List<GlyphRect> usedGlyphRects,
        GlyphRenderMode renderMode, Texture2D texture, out Glyph glyph);

    private delegate bool TryAddGlyphsToTextureDelegate(
        List<uint> glyphIndexes, int padding, GlyphPackingMode packingMode,
        List<GlyphRect> freeGlyphRects, List<GlyphRect> usedGlyphRects,
        GlyphRenderMode renderMode, Texture2D texture, out Glyph[] glyphs);

    private static readonly GetGlyphIndexDelegate s_GetGlyphIndex;
    private static readonly ResetAtlasTextureDelegate s_ResetAtlasTexture;
    private static readonly TryAddGlyphToTextureDelegate s_TryAddGlyphToTexture;
    private static readonly TryAddGlyphsToTextureDelegate s_TryAddGlyphsToTexture;

    static UniTextFontEngine()
    {
        var type = typeof(FontEngine);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        s_GetGlyphIndex = (GetGlyphIndexDelegate)Delegate.CreateDelegate(typeof(GetGlyphIndexDelegate),
            type.GetMethod("GetGlyphIndex", flags, null, new[] { typeof(uint) }, null));

        s_ResetAtlasTexture = (ResetAtlasTextureDelegate)Delegate.CreateDelegate(typeof(ResetAtlasTextureDelegate),
            type.GetMethod("ResetAtlasTexture", flags, null, new[] { typeof(Texture2D) }, null));

        s_TryAddGlyphToTexture = (TryAddGlyphToTextureDelegate)Delegate.CreateDelegate(
            typeof(TryAddGlyphToTextureDelegate),
            type.GetMethod("TryAddGlyphToTexture", flags, null, new[]
            {
                typeof(uint), typeof(int), typeof(GlyphPackingMode),
                typeof(List<GlyphRect>), typeof(List<GlyphRect>),
                typeof(GlyphRenderMode), typeof(Texture2D), typeof(Glyph).MakeByRefType()
            }, null));

        s_TryAddGlyphsToTexture = (TryAddGlyphsToTextureDelegate)Delegate.CreateDelegate(
            typeof(TryAddGlyphsToTextureDelegate),
            type.GetMethod("TryAddGlyphsToTexture", flags, null, new[]
            {
                typeof(List<uint>), typeof(int), typeof(GlyphPackingMode),
                typeof(List<GlyphRect>), typeof(List<GlyphRect>),
                typeof(GlyphRenderMode), typeof(Texture2D), typeof(Glyph[]).MakeByRefType()
            }, null));
    }

    public static uint GetGlyphIndex(uint unicode)
    {
        return s_GetGlyphIndex(unicode);
    }

    public static void ResetAtlasTexture(Texture2D texture)
    {
        s_ResetAtlasTexture(texture);
    }

    public static bool TryAddGlyphToTexture(
        uint glyphIndex, int padding, GlyphPackingMode packingMode,
        List<GlyphRect> freeGlyphRects, List<GlyphRect> usedGlyphRects,
        GlyphRenderMode renderMode, Texture2D texture, out Glyph glyph)
    {
        return s_TryAddGlyphToTexture(glyphIndex, padding, packingMode, freeGlyphRects, usedGlyphRects, renderMode,
            texture, out glyph);
    }

    public static bool TryAddGlyphsToTexture(
        List<uint> glyphIndexes, int padding, GlyphPackingMode packingMode,
        List<GlyphRect> freeGlyphRects, List<GlyphRect> usedGlyphRects,
        GlyphRenderMode renderMode, Texture2D texture, out Glyph[] glyphs)
    {
        return s_TryAddGlyphsToTexture(glyphIndexes, padding, packingMode, freeGlyphRects, usedGlyphRects, renderMode,
            texture, out glyphs);
    }
}