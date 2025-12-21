using System;
using System.Collections.Generic;
using HarfBuzzSharp;


public static class HarfBuzzFontValidator
{
    private static readonly Dictionary<int, ValidatorCache> fontCache = new();

    private class ValidatorCache : IDisposable
    {
        public Blob blob;
        public Face face;
        public Font font;
        public IntPtr unmanagedData;
        public int dataLength;

        public void Dispose()
        {
            font?.Dispose();
            face?.Dispose();
            blob?.Dispose();
            if (unmanagedData != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(unmanagedData);
                unmanagedData = IntPtr.Zero;
            }
        }
    }


    public static uint GetGlyphIndex(UniTextFont font, uint codepoint)
    {
        if (font == null || !font.HasFontData)
            return 0;

        var instanceId = font.GetInstanceID();

        if (!fontCache.TryGetValue(instanceId, out var cache))
        {
            cache = CreateCache(font);
            if (cache == null)
                return 0;
            fontCache[instanceId] = cache;
        }

        if (cache.font.TryGetGlyph(codepoint, out var glyphIndex))
            return glyphIndex;

        return 0;
    }

    private static ValidatorCache CreateCache(UniTextFont font)
    {
        var fontData = font.FontData;
        if (fontData == null || fontData.Length == 0)
            return null;

        try
        {
            var cache = new ValidatorCache();
            cache.dataLength = fontData.Length;

            cache.unmanagedData = System.Runtime.InteropServices.Marshal.AllocHGlobal(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, cache.unmanagedData, fontData.Length);

            cache.blob = new Blob(cache.unmanagedData, fontData.Length, MemoryMode.ReadOnly);
            cache.face = new Face(cache.blob, 0);
            cache.font = new Font(cache.face);

            return cache;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(
                $"[HarfBuzzFontValidator] Failed to create cache for {font.name}: {ex.Message}");
            return null;
        }
    }


    public static void ClearCache(int fontAssetInstanceId)
    {
        if (fontCache.TryGetValue(fontAssetInstanceId, out var cache))
        {
            cache.Dispose();
            fontCache.Remove(fontAssetInstanceId);
        }
    }


    public static void ClearAllCaches()
    {
        foreach (var cache in fontCache.Values)
            cache.Dispose();
        fontCache.Clear();
    }
}