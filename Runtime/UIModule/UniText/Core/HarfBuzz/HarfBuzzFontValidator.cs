using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using HarfBuzzSharp;


public static class HarfBuzzFontValidator
{
    private static readonly ConcurrentDictionary<int, ValidatorCache> fontCache = new();

    private sealed class ValidatorCache : IDisposable
    {
        public readonly Blob blob;
        public readonly Face face;
        public readonly Font font;
        public readonly int upem;
        private readonly IntPtr unmanagedData;

        public ValidatorCache(byte[] fontData)
        {
            unmanagedData = System.Runtime.InteropServices.Marshal.AllocHGlobal(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, unmanagedData, fontData.Length);

            blob = new Blob(unmanagedData, fontData.Length, MemoryMode.ReadOnly);
            face = new Face(blob, 0);
            font = new Font(face);
            upem = face.UnitsPerEm > 0 ? face.UnitsPerEm : 1000;
        }

        public void Dispose()
        {
            font?.Dispose();
            face?.Dispose();
            blob?.Dispose();
            if (unmanagedData != IntPtr.Zero)
                System.Runtime.InteropServices.Marshal.FreeHGlobal(unmanagedData);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ValidatorCache GetOrCreateCache(UniTextFont font)
    {
        if (font == null || !font.HasFontData)
            return null;

        var instanceId = font.GetCachedInstanceId();
        if (fontCache.TryGetValue(instanceId, out var cache))
            return cache;

        var fontData = font.FontData;
        if (fontData == null || fontData.Length == 0)
            return null;

        try
        {
            cache = new ValidatorCache(fontData);
            fontCache.TryAdd(instanceId, cache);
            return cache;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(
                $"[HarfBuzzFontValidator] Failed to create cache for {font.name}: {ex.Message}");
            return null;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetGlyphIndex(UniTextFont font, uint codepoint)
    {
        var cache = GetOrCreateCache(font);
        if (cache == null)
            return 0;

        if (cache.font.TryGetGlyph(codepoint, out var glyphIndex))
            return glyphIndex;

        return 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetGlyphHorizontalAdvance(UniTextFont font, uint glyphIndex)
    {
        var cache = GetOrCreateCache(font);
        if (cache == null)
            return 0;

        return cache.font.GetHorizontalGlyphAdvance(glyphIndex);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetGlyphInfo(UniTextFont font, uint codepoint, float fontSize,
        out uint glyphIndex, out float advance)
    {
        glyphIndex = 0;
        advance = 0;

        var cache = GetOrCreateCache(font);
        if (cache == null)
            return false;

        if (!cache.font.TryGetGlyph(codepoint, out glyphIndex))
            return false;

        var advanceUnits = cache.font.GetHorizontalGlyphAdvance(glyphIndex);
        advance = advanceUnits * fontSize / cache.upem;
        return true;
    }


    public static int GetUnitsPerEm(UniTextFont font)
    {
        var cache = GetOrCreateCache(font);
        return cache?.upem ?? 1000;
    }


    public static void ClearCache(int fontAssetInstanceId)
    {
        if (fontCache.TryRemove(fontAssetInstanceId, out var cache))
            cache.Dispose();
    }


    public static void ClearAllCaches()
    {
        foreach (var kvp in fontCache)
            if (fontCache.TryRemove(kvp.Key, out var cache))
                cache.Dispose();
    }
}