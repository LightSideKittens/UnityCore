using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

/// <summary>
/// HarfBuzz shaping engine for OpenType complex script support.
/// Cache keyed by pre-computed font data hash - handles multiple font providers with same fontId.
/// </summary>
public sealed class HarfBuzzShapingEngine : IShapingEngine, IDisposable
{
    private ShapedGlyph[] outputBuffer = new ShapedGlyph[256];
    private readonly Dictionary<int, HarfBuzzFontCache> fontCache = new();
    private readonly Stack<Buffer> bufferPool = new(4);

    public static bool DebugLogging;

    /// <summary>
    /// Cached HarfBuzz objects for a font.
    /// Uses unmanaged memory to avoid GC relocation issues.
    /// </summary>
    private sealed class HarfBuzzFontCache : IDisposable
    {
        private readonly IntPtr unmanagedData;
        public readonly Blob blob;
        public readonly Face face;
        public readonly Font font;
        public readonly int upem;

        public HarfBuzzFontCache(byte[] fontData)
        {
            int dataLength = fontData.Length;
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
        foreach (var entry in fontCache.Values)
            entry.Dispose();
        fontCache.Clear();

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
        if (buffer != null && bufferPool.Count < 8)
            bufferPool.Push(buffer);
        else
            buffer?.Dispose();
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

        // Get pre-computed hash from font asset (computed at Editor time)
        int fontHash = fontProvider?.GetFontDataHash(fontId) ?? 0;
        if (fontHash == 0)
            return FallbackShape(codepoints, fontProvider, fontId, direction);

        if (DebugLogging)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[HarfBuzz] fontHash={fontHash:X8}, script={script}, dir={direction}, len={length}: ");
            for (int j = 0; j < length && j < 10; j++)
                sb.Append($"U+{codepoints[j]:X4} ");
            if (length > 10) sb.Append("...");
            UnityEngine.Debug.Log(sb.ToString());
        }

        // Get or create cached font by hash
        if (!fontCache.TryGetValue(fontHash, out var fontEntry))
        {
            byte[] fontData = fontProvider.GetFontData(fontId);
            if (fontData == null || fontData.Length == 0)
                return FallbackShape(codepoints, fontProvider, fontId, direction);

            fontEntry = new HarfBuzzFontCache(fontData);
            fontCache[fontHash] = fontEntry;

            if (DebugLogging)
                UnityEngine.Debug.Log($"[HarfBuzz] Registered font hash={fontHash:X8}, upem={fontEntry.upem}");
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
            int glyphCount = glyphInfos.Length;

            if (outputBuffer.Length < glyphCount)
                outputBuffer = new ShapedGlyph[Math.Max(glyphCount, outputBuffer.Length * 2)];

            float totalAdvance = 0;
            float fontSize = fontProvider?.FontSize ?? 36f;
            float offsetScale = fontSize / fontEntry.upem;

            for (int i = 0; i < glyphCount; i++)
            {
                ref readonly var info = ref glyphInfos[i];
                ref readonly var pos = ref glyphPositions[i];

                float advanceX = pos.XAdvance * offsetScale;
                outputBuffer[i] = new ShapedGlyph
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
            fontProvider?.TryGetGlyphInfo(fontId, codepoint, out glyphIndex, out advance);

            int outputIndex = isRtl ? (length - 1 - i) : i;
            outputBuffer[outputIndex] = new ShapedGlyph
            {
                glyphId = (int)glyphIndex,
                cluster = i,
                advanceX = advance
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
