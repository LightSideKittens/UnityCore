using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

/// <summary>
/// HarfBuzz shaping engine for OpenType complex script support.
/// </summary>
public sealed class HarfBuzzShapingEngine : IShapingEngine, IDisposable
{
    private ShapedGlyph[] outputBuffer = new ShapedGlyph[256];
    private readonly Dictionary<int, HarfBuzzFontCache> fontCache = new();

    // Buffer pool to avoid creating new Buffer for each Shape call
    private readonly Stack<Buffer> bufferPool = new(4);

    private int lastClearedSessionId;

    /// <summary>
    /// Cached HarfBuzz objects for a font. Blob/Face/Font are kept alive together.
    /// Uses unmanaged memory to avoid GC relocation issues with Blob.FromStream.
    /// See: https://github.com/mono/SkiaSharp/issues/2323
    /// </summary>
    private sealed class HarfBuzzFontCache : IDisposable
    {
        private readonly IntPtr unmanagedData;
        private readonly int dataLength;
        public readonly Blob blob;
        public readonly Face face;
        public readonly Font font;
        public readonly int upem;

        public HarfBuzzFontCache(byte[] fontData)
        {
            // Allocate unmanaged memory to prevent GC from moving the data
            // This fixes the issue where Blob.FromStream uses fixed() incorrectly
            dataLength = fontData.Length;
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

            // Free unmanaged memory after HarfBuzz objects are disposed
            if (unmanagedData != IntPtr.Zero)
                Marshal.FreeHGlobal(unmanagedData);
        }
    }

    public void Dispose()
    {
        foreach (var entry in fontCache.Values)
            entry.Dispose();
        fontCache.Clear();

        // Dispose pooled buffers
        while (bufferPool.Count > 0)
            bufferPool.Pop().Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Buffer AcquireBuffer()
    {
        if (bufferPool.Count > 0)
        {
            var buffer = bufferPool.Pop();
            buffer.ClearContents();
            return buffer;
        }
        return new Buffer();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReleaseBuffer(Buffer buffer)
    {
        if (buffer != null && bufferPool.Count < 8) // Keep max 8 buffers in pool
            bufferPool.Push(buffer);
        else
            buffer?.Dispose();
    }

    public void RegisterFont(int fontId, byte[] fontData)
    {
        if (fontData == null || fontData.Length == 0)
            throw new ArgumentException("Font data is empty", nameof(fontData));

        // Dispose old entry if exists
        if (fontCache.TryGetValue(fontId, out var oldEntry))
            oldEntry.Dispose();

        fontCache[fontId] = new HarfBuzzFontCache(fontData);
    }

    public bool HasFont(int fontId) => fontCache.ContainsKey(fontId);

    public void ClearFontCache()
    {
        foreach (var entry in fontCache.Values)
            entry.Dispose();
        fontCache.Clear();
    }

    private bool TryAutoRegisterFont(int fontId, UniTextFontProvider fontProvider)
    {
        if (fontProvider == null) return false;

        byte[] fontData = fontProvider.GetFontData(fontId);
        if (fontData == null || fontData.Length == 0)
            return false;

        try
        {
            RegisterFont(fontId, fontData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        int length = codepoints.Length;
        if (length == 0)
            return new ShapingResult(ReadOnlySpan<ShapedGlyph>.Empty, 0);

        // Get or create cached font
        if (!fontCache.TryGetValue(fontId, out var fontEntry))
        {
            if (!TryAutoRegisterFont(fontId, fontProvider))
                return FallbackShape(codepoints, fontProvider, fontId, direction);
            fontEntry = fontCache[fontId];
        }

        // Use pooled buffer instead of creating new one each call
        var buffer = AcquireBuffer();
        try
        {
            buffer.ContentType = ContentType.Unicode;

            for (int i = 0; i < length; i++)
                buffer.Add(codepoints[i], i);

            buffer.Direction = direction == TextDirection.RightToLeft
                ? Direction.RightToLeft
                : Direction.LeftToRight;
            buffer.Script = MapScript(script);
            buffer.GuessSegmentProperties();

            fontEntry.font.Shape(buffer);

            // Use Span-based methods to avoid array allocations
            var glyphInfos = buffer.GetGlyphInfoSpan();
            var glyphPositions = buffer.GetGlyphPositionSpan();
            int glyphCount = glyphInfos.Length;

            if (outputBuffer.Length < glyphCount)
                outputBuffer = new ShapedGlyph[Math.Max(glyphCount, outputBuffer.Length * 2)];

            float totalAdvance = 0;
            bool hasFontProvider = fontProvider != null;
            float fontSize = hasFontProvider ? fontProvider.FontSize : 36f;
            float offsetScale = fontSize / fontEntry.upem;

            for (int i = 0; i < glyphCount; i++)
            {
                ref readonly var info = ref glyphInfos[i];
                ref readonly var pos = ref glyphPositions[i];

                uint harfBuzzGlyphId = info.Codepoint;

                // Use HarfBuzz advance values (properly scaled)
                float advanceX = pos.XAdvance * offsetScale;
                float advanceY = pos.YAdvance * offsetScale;

                outputBuffer[i] = new ShapedGlyph
                {
                    glyphId = (int)harfBuzzGlyphId,
                    cluster = (int)info.Cluster,
                    advanceX = advanceX,
                    advanceY = advanceY,
                    offsetX = pos.XOffset * offsetScale,
                    offsetY = pos.YOffset * offsetScale
                };

                totalAdvance += advanceX;
            }

            return new ShapingResult(outputBuffer.AsSpan(0, glyphCount), totalAdvance);
        }
        finally
        {
            ReleaseBuffer(buffer);
        }
    }

    private ShapingResult FallbackShape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        TextDirection direction)
    {
        int length = codepoints.Length;
        if (outputBuffer.Length < length)
            outputBuffer = new ShapedGlyph[Math.Max(length, outputBuffer.Length * 2)];

        float totalAdvance = 0;
        var unicodeData = UnicodeData.Provider;
        bool isRtl = direction == TextDirection.RightToLeft;
        bool hasFontProvider = fontProvider != null;
        bool checkMirroring = isRtl && unicodeData != null;

        for (int i = 0; i < length; i++)
        {
            int codepoint = codepoints[i];

            if (checkMirroring && unicodeData.IsBidiMirrored(codepoint))
            {
                int mirrored = unicodeData.GetBidiMirroringGlyph(codepoint);
                if (mirrored != 0 && mirrored != codepoint)
                    codepoint = mirrored;
            }

            uint glyphIndex = 0;
            float advance = 0;
            if (hasFontProvider)
                fontProvider.TryGetGlyphInfo(fontId, codepoint, out glyphIndex, out advance);

            int outputIndex = isRtl ? (length - 1 - i) : i;
            outputBuffer[outputIndex] = new ShapedGlyph
            {
                glyphId = (int)glyphIndex,
                cluster = i,
                advanceX = advance,
                advanceY = 0,
                offsetX = 0,
                offsetY = 0
            };

            totalAdvance += advance;
        }

        return new ShapingResult(outputBuffer.AsSpan(0, length), totalAdvance);
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
