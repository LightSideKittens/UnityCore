using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;


public sealed class HarfBuzzShapingEngine : IShapingEngine, IDisposable
{
    private static readonly FastIntDictionary<HarfBuzzFontCache> fontCache = new();
    private static readonly FastIntDictionary<int> instanceIdToFontHash = new();
    private static readonly object fontCacheLock = new();

    [ThreadStatic] private static ShapedGlyph[] outputBuffer;
    [ThreadStatic] private static Stack<Buffer> bufferPool;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        lock (fontCacheLock)
        {
            foreach (var kvp in fontCache)
                kvp.Value?.Dispose();
            fontCache.Clear();
            instanceIdToFontHash.Clear();
        }
        outputBuffer = null;
        bufferPool = null;
    }

    private static ShapedGlyph[] OutputBuffer => outputBuffer ??= new ShapedGlyph[256];
    private static Stack<Buffer> BufferPool => bufferPool ??= new Stack<Buffer>(4);

    private sealed class HarfBuzzFontCache : IDisposable
    {
        private readonly IntPtr unmanagedData;
        public readonly Blob blob;
        public readonly Face face;
        public readonly Font font;
        public readonly int upem;

        public HarfBuzzFontCache(byte[] fontData)
        {
            var dataLength = fontData.Length;
            unmanagedData = Marshal.AllocHGlobal(dataLength);
            Marshal.Copy(fontData, 0, unmanagedData, dataLength);

            blob = new Blob(unmanagedData, dataLength, MemoryMode.ReadOnly);
            face = new Face(blob, 0);
            font = new Font(face);
            font.SetFunctionsOpenType();
            upem = face.UnitsPerEm > 0 ? face.UnitsPerEm : 1000;
        }

        public void Dispose()
        {
            font?.Dispose();
            face?.Dispose();
            blob?.Dispose();
            if (unmanagedData != IntPtr.Zero)
                Marshal.FreeHGlobal(unmanagedData);
        }
    }

    public void Dispose()
    {
        lock (fontCacheLock)
        {
            foreach (var kvp in fontCache)
                kvp.Value.Dispose();
            fontCache.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Buffer AcquireBuffer()
    {
        var pool = BufferPool;
        if (pool.Count > 0)
        {
            var buffer = pool.Pop();
            buffer.ClearContents();
            return buffer;
        }

        return new Buffer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReleaseBuffer(Buffer buffer)
    {
        var pool = BufferPool;
        if (buffer != null && pool.Count < 8)
            pool.Push(buffer);
        else
            buffer?.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static HarfBuzzFontCache GetOrCreateCacheByInstanceId(UniTextFont font)
    {
        if (font == null || !font.HasFontData)
            return null;

        var instanceId = font.GetCachedInstanceId();

        lock (fontCacheLock)
        {
            if (instanceIdToFontHash.TryGetValue(instanceId, out var fontHash))
            {
                if (fontCache.TryGetValue(fontHash, out var cached))
                    return cached;
            }

            var fontData = font.FontData;
            if (fontData == null || fontData.Length == 0)
                return null;

            fontHash = fontData.GetHashCode();
            if (!fontCache.TryGetValue(fontHash, out var entry))
            {
                entry = new HarfBuzzFontCache(fontData);
                fontCache[fontHash] = entry;
            }

            instanceIdToFontHash[instanceId] = fontHash;
            return entry;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetGlyphIndex(UniTextFont font, uint codepoint)
    {
        var cache = GetOrCreateCacheByInstanceId(font);
        if (cache == null)
            return 0;

        return cache.font.TryGetGlyph(codepoint, out var glyphIndex) ? glyphIndex : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetGlyphInfo(UniTextFont font, uint codepoint, float fontSize,
        out uint glyphIndex, out float advance)
    {
        glyphIndex = 0;
        advance = 0;

        var cache = GetOrCreateCacheByInstanceId(font);
        if (cache == null)
            return false;

        if (!cache.font.TryGetGlyph(codepoint, out glyphIndex))
            return false;

        var advanceUnits = cache.font.GetHorizontalGlyphAdvance(glyphIndex);
        advance = advanceUnits * fontSize / cache.upem;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetUnitsPerEm(UniTextFont font)
    {
        var cache = GetOrCreateCacheByInstanceId(font);
        return cache?.upem ?? 1000;
    }

    public static void ClearCache(int fontAssetInstanceId)
    {
        lock (fontCacheLock)
        {
            if (instanceIdToFontHash.TryGetValue(fontAssetInstanceId, out var fontHash))
            {
                instanceIdToFontHash.Remove(fontAssetInstanceId);

                var stillUsed = false;
                foreach (var kvp in instanceIdToFontHash)
                {
                    if (kvp.Value == fontHash)
                    {
                        stillUsed = true;
                        break;
                    }
                }

                if (!stillUsed && fontCache.TryGetValue(fontHash, out var cache))
                {
                    cache.Dispose();
                    fontCache.Remove(fontHash);
                }
            }
        }
    }

    public static void ClearAllCaches()
    {
        lock (fontCacheLock)
        {
            foreach (var kvp in fontCache)
                kvp.Value?.Dispose();
            fontCache.Clear();
            instanceIdToFontHash.Clear();
        }
    }

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        var length = codepoints.Length;
        if (length == 0)
            return new ShapingResult(ReadOnlySpan<ShapedGlyph>.Empty, 0);

        var fontHash = fontProvider?.GetFontDataHash(fontId) ?? 0;
        if (fontHash == 0)
            return FallbackShape(codepoints, fontProvider, fontId, direction);

        HarfBuzzFontCache fontEntry;
        lock (fontCacheLock)
        {
            if (!fontCache.TryGetValue(fontHash, out fontEntry))
            {
                var fontData = fontProvider.GetFontData(fontId);
                if (fontData == null || fontData.Length == 0)
                    return FallbackShape(codepoints, fontProvider, fontId, direction);

                fontEntry = new HarfBuzzFontCache(fontData);
                fontCache[fontHash] = fontEntry;
            }
        }

        var buffer = AcquireBuffer();
        try
        {
            buffer.Direction = direction == TextDirection.RightToLeft ? Direction.RightToLeft : Direction.LeftToRight;
            buffer.Script = MapScript(script);
            buffer.ContentType = ContentType.Unicode;
            buffer.AddCodepoints(codepoints);

            fontEntry.font.Shape(buffer);

            var glyphInfos = buffer.GetGlyphInfoSpan();
            var glyphPositions = buffer.GetGlyphPositionSpan();
            var glyphCount = glyphInfos.Length;

            var outBuf = OutputBuffer;
            if (outBuf.Length < glyphCount)
            {
                outBuf = new ShapedGlyph[Math.Max(glyphCount, outBuf.Length * 2)];
                outputBuffer = outBuf;
            }

            float totalAdvance = 0;
            var fontSize = fontProvider?.FontSize ?? 36f;
            var offsetScale = fontSize / fontEntry.upem;

            for (var i = 0; i < glyphCount; i++)
            {
                ref readonly var info = ref glyphInfos[i];
                ref readonly var pos = ref glyphPositions[i];

                var advanceX = pos.XAdvance * offsetScale;
                outBuf[i] = new ShapedGlyph
                {
                    glyphId = (int)info.Codepoint,
                    cluster = (int)info.Cluster,
                    advanceX = advanceX,
                    advanceY = pos.YAdvance * offsetScale,
                    offsetX = pos.XOffset * offsetScale,
                    offsetY = pos.YOffset * offsetScale
                };
                totalAdvance += advanceX;
            }

            return new ShapingResult(outBuf.AsSpan(0, glyphCount), totalAdvance);
        }
        finally
        {
            ReleaseBuffer(buffer);
        }
    }

    private static ShapingResult FallbackShape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        TextDirection direction)
    {
        var length = codepoints.Length;
        var outBuf = OutputBuffer;
        if (outBuf.Length < length)
        {
            outBuf = new ShapedGlyph[Math.Max(length, outBuf.Length * 2)];
            outputBuffer = outBuf;
        }

        float totalAdvance = 0;
        var unicodeData = UnicodeData.Provider;
        var isRtl = direction == TextDirection.RightToLeft;
        var checkMirroring = isRtl && unicodeData != null;

        var font = fontProvider.GetFontAsset(fontId);
        var fontSize = fontProvider.FontSize;

        for (var i = 0; i < length; i++)
        {
            var codepoint = codepoints[i];

            if (checkMirroring && unicodeData.IsBidiMirrored(codepoint))
            {
                var mirrored = unicodeData.GetBidiMirroringGlyph(codepoint);
                if (mirrored != 0 && mirrored != codepoint)
                    codepoint = mirrored;
            }

            uint glyphIndex;
            float advance;

            TryGetGlyphInfo(font, (uint)codepoint, fontSize, out glyphIndex, out advance);

            var outputIndex = isRtl ? length - 1 - i : i;
            outBuf[outputIndex] = new ShapedGlyph
            {
                glyphId = (int)glyphIndex,
                cluster = i,
                advanceX = advance
            };
            totalAdvance += advance;
        }

        return new ShapingResult(outBuf.AsSpan(0, length), totalAdvance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Script MapScript(UnicodeScript script)
    {
        return script switch
        {
            UnicodeScript.Arabic => Script.Arabic,
            UnicodeScript.Armenian => Script.Armenian,
            UnicodeScript.Bengali => Script.Bengali,
            UnicodeScript.Cyrillic => Script.Cyrillic,
            UnicodeScript.Devanagari => Script.Devanagari,
            UnicodeScript.Georgian => Script.Georgian,
            UnicodeScript.Greek => Script.Greek,
            UnicodeScript.Gujarati => Script.Gujarati,
            UnicodeScript.Gurmukhi => Script.Gurmukhi,
            UnicodeScript.Han => Script.Han,
            UnicodeScript.Hangul => Script.Hangul,
            UnicodeScript.Hebrew => Script.Hebrew,
            UnicodeScript.Hiragana => Script.Hiragana,
            UnicodeScript.Kannada => Script.Kannada,
            UnicodeScript.Katakana => Script.Katakana,
            UnicodeScript.Khmer => Script.Khmer,
            UnicodeScript.Lao => Script.Lao,
            UnicodeScript.Latin => Script.Latin,
            UnicodeScript.Malayalam => Script.Malayalam,
            UnicodeScript.Myanmar => Script.Myanmar,
            UnicodeScript.Oriya => Script.Oriya,
            UnicodeScript.Sinhala => Script.Sinhala,
            UnicodeScript.Tamil => Script.Tamil,
            UnicodeScript.Telugu => Script.Telugu,
            UnicodeScript.Thai => Script.Thai,
            UnicodeScript.Tibetan => Script.Tibetan,
            _ => Script.Common
        };
    }
}