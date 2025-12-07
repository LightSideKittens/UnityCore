using System;
using System.Collections.Generic;
using HarfBuzzSharp;

/// <summary>
/// Utility class to validate font glyph coverage using HarfBuzz.
/// Uses HarfBuzz to read cmap table directly, bypassing Unity FontEngine issues.
/// </summary>
public static class HarfBuzzFontValidator
{
    // Cache Font objects per font asset to avoid recreating them
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

    /// <summary>
    /// Get glyph index for a codepoint using HarfBuzz.
    /// Returns 0 if the codepoint is not in the font.
    /// </summary>
    public static uint GetGlyphIndex(UniTextFontAsset fontAsset, uint codepoint)
    {
        if (fontAsset == null || !fontAsset.HasFontData)
            return 0;

        int instanceId = fontAsset.GetInstanceID();

        // Get or create cached font
        if (!fontCache.TryGetValue(instanceId, out var cache))
        {
            cache = CreateCache(fontAsset);
            if (cache == null)
                return 0;
            fontCache[instanceId] = cache;
        }

        // Use Font.GetGlyph to get glyph index from cmap
        // Returns 0 if codepoint is not in the font
        if (cache.font.TryGetGlyph(codepoint, out uint glyphIndex))
            return glyphIndex;

        return 0;
    }

    private static ValidatorCache CreateCache(UniTextFontAsset fontAsset)
    {
        byte[] fontData = fontAsset.FontData;
        if (fontData == null || fontData.Length == 0)
            return null;

        try
        {
            var cache = new ValidatorCache();
            cache.dataLength = fontData.Length;

            // Copy to unmanaged memory to prevent GC relocation
            cache.unmanagedData = System.Runtime.InteropServices.Marshal.AllocHGlobal(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, cache.unmanagedData, fontData.Length);

            // Create blob from unmanaged pointer
            cache.blob = new Blob(cache.unmanagedData, fontData.Length, MemoryMode.ReadOnly);
            cache.face = new Face(cache.blob, 0);
            cache.font = new Font(cache.face);

            return cache;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[HarfBuzzFontValidator] Failed to create cache for {fontAsset.name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Clear cached fonts for a specific font asset.
    /// </summary>
    public static void ClearCache(int fontAssetInstanceId)
    {
        if (fontCache.TryGetValue(fontAssetInstanceId, out var cache))
        {
            cache.Dispose();
            fontCache.Remove(fontAssetInstanceId);
        }
    }

    /// <summary>
    /// Clear all cached fonts.
    /// </summary>
    public static void ClearAllCaches()
    {
        foreach (var cache in fontCache.Values)
            cache.Dispose();
        fontCache.Clear();
    }
}
