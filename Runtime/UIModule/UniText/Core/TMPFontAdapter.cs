using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Информация о глифе для рендеринга через TMP.
/// </summary>
public struct TMPGlyphRenderInfo
{
    public int glyphIndex;
    public int atlasIndex;

    public float width;
    public float height;
    public float bearingX;
    public float bearingY;
    public float advance;

    // UV rect from glyphRect (pixel coordinates in atlas)
    public float uvX;
    public float uvY;
    public float uvWidth;
    public float uvHeight;

    public int atlasWidth;
    public int atlasHeight;

    /// <summary>
    /// Шрифт-источник, из которого получен глиф (может отличаться от основного при fallback).
    /// </summary>
    public TMP_FontAsset sourceFontAsset;

    public void GetNormalizedUV(out Vector2 uvMin, out Vector2 uvMax)
    {
        uvMin = new Vector2(uvX / atlasWidth, uvY / atlasHeight);
        uvMax = new Vector2((uvX + uvWidth) / atlasWidth, (uvY + uvHeight) / atlasHeight);
    }

    /// <summary>
    /// Получить материал для рендеринга этого глифа.
    /// </summary>
    public Material GetMaterial()
    {
        return sourceFontAsset != null ? sourceFontAsset.material : null;
    }
}

/// <summary>
/// Font provider для TMP шрифтов.
/// Работает напрямую с TMP_FontAsset без лишних обёрток.
/// </summary>
public sealed class TMPFontProvider
{
    private readonly TMP_FontAsset[] fonts;
    private readonly int defaultFontId;
    private float fontSize = 36f;

    public TMPFontProvider(TMP_FontAsset defaultFont, params TMP_FontAsset[] fallbackFonts)
    {
        if (defaultFont == null)
            throw new ArgumentNullException(nameof(defaultFont));

        int totalCount = 1 + (fallbackFonts?.Length ?? 0);
        fonts = new TMP_FontAsset[totalCount];

        fonts[0] = defaultFont;
        defaultFontId = 0;

        if (fallbackFonts != null)
        {
            for (int i = 0; i < fallbackFonts.Length; i++)
            {
                fonts[i + 1] = fallbackFonts[i];
            }
        }
    }

    /// <summary>
    /// Текущий размер шрифта для расчёта метрик.
    /// </summary>
    public float FontSize
    {
        get => fontSize;
        set => fontSize = Mathf.Max(1f, value);
    }

    /// <summary>
    /// Установить размер шрифта.
    /// </summary>
    public void SetFontSize(float size)
    {
        fontSize = Mathf.Max(1f, size);
    }

    /// <summary>
    /// Получить TMP_FontAsset по ID.
    /// </summary>
    public TMP_FontAsset GetFont(int fontId)
    {
        if (fontId >= 0 && fontId < fonts.Length && fonts[fontId] != null)
            return fonts[fontId];

        return fonts[defaultFontId];
    }

    /// <summary>
    /// Проверить, есть ли глиф для codepoint в шрифте.
    /// </summary>
    public bool HasGlyph(int fontId, int codepoint)
    {
        var font = GetFont(fontId);
        return TryGetCharacter(font, (uint)codepoint, out _, out _);
    }

    /// <summary>
    /// Найти шрифт для codepoint (font fallback).
    /// </summary>
    public int FindFontForCodepoint(int codepoint, int preferredFontId)
    {
        if (preferredFontId >= 0 && preferredFontId < fonts.Length)
        {
            var preferred = fonts[preferredFontId];
            if (preferred != null && TryGetCharacter(preferred, (uint)codepoint, out _, out _))
                return preferredFontId;
        }

        for (int i = 0; i < fonts.Length; i++)
        {
            if (i == preferredFontId) continue;

            var font = fonts[i];
            if (font != null && TryGetCharacter(font, (uint)codepoint, out _, out _))
                return i;
        }

        return defaultFontId;
    }

    /// <summary>
    /// Получить метрики глифа.
    /// </summary>
    public bool TryGetGlyphMetrics(int fontId, int codepoint, out GlyphMetrics metrics)
    {
        var font = GetFont(fontId);
        if (TryGetCharacter(font, (uint)codepoint, out var character, out var sourceFontAsset))
        {
            var glyph = character.glyph;
            var faceInfo = sourceFontAsset.faceInfo;
            float scale = fontSize / faceInfo.pointSize * faceInfo.scale;

            metrics = new GlyphMetrics
            {
                width = glyph.metrics.width * scale,
                height = glyph.metrics.height * scale,
                bearingX = glyph.metrics.horizontalBearingX * scale,
                bearingY = glyph.metrics.horizontalBearingY * scale,
                advance = glyph.metrics.horizontalAdvance * scale
            };
            return true;
        }

        metrics = default;
        return false;
    }

    /// <summary>
    /// Получить информацию о глифе для рендеринга.
    /// </summary>
    public bool TryGetGlyphRenderInfo(int fontId, int codepoint, out TMPGlyphRenderInfo info)
    {
        var font = GetFont(fontId);
        if (TryGetCharacter(font, (uint)codepoint, out var character, out var sourceFontAsset))
        {
            var glyph = character.glyph;
            var faceInfo = sourceFontAsset.faceInfo;
            float scale = fontSize / faceInfo.pointSize * faceInfo.scale;

            info = new TMPGlyphRenderInfo
            {
                glyphIndex = (int)glyph.index,
                atlasIndex = glyph.atlasIndex,

                width = glyph.metrics.width * scale,
                height = glyph.metrics.height * scale,
                bearingX = glyph.metrics.horizontalBearingX * scale,
                bearingY = glyph.metrics.horizontalBearingY * scale,
                advance = glyph.metrics.horizontalAdvance * scale,

                uvX = glyph.glyphRect.x,
                uvY = glyph.glyphRect.y,
                uvWidth = glyph.glyphRect.width,
                uvHeight = glyph.glyphRect.height,

                atlasWidth = sourceFontAsset.atlasWidth,
                atlasHeight = sourceFontAsset.atlasHeight,

                sourceFontAsset = sourceFontAsset
            };
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// Получить метрики линии.
    /// </summary>
    public void GetLineMetrics(float size, out float ascender, out float descender, out float lineHeight)
    {
        var defaultFont = fonts[defaultFontId];
        if (defaultFont != null)
        {
            var faceInfo = defaultFont.faceInfo;
            float scale = size / faceInfo.pointSize * faceInfo.scale;

            ascender = faceInfo.ascentLine * scale;
            descender = faceInfo.descentLine * scale;
            lineHeight = faceInfo.lineHeight * scale;
        }
        else
        {
            ascender = size * 0.8f;
            descender = size * -0.2f;
            lineHeight = size * 1.2f;
        }
    }

    /// <summary>
    /// Получить scale для размера шрифта.
    /// </summary>
    public float GetScale(int fontId, float size)
    {
        var font = GetFont(fontId);
        var faceInfo = font.faceInfo;
        return size / faceInfo.pointSize * faceInfo.scale;
    }

    /// <summary>
    /// Найти символ с полной поддержкой TMP fallback chain.
    /// </summary>
    private static bool TryGetCharacter(TMP_FontAsset font, uint unicode, out TMP_Character character, out TMP_FontAsset sourceFontAsset)
    {
        // 1. Поиск в основном шрифте и его fallback chain
        character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(
            unicode, font, true, FontStyles.Normal, FontWeight.Regular, out _);

        if (character != null)
        {
            sourceFontAsset = character.textAsset as TMP_FontAsset ?? font;
            return true;
        }

        // 2. Глобальные fallback из TMP_Settings
        var globalFallbacks = TMP_Settings.fallbackFontAssets;
        if (globalFallbacks != null && globalFallbacks.Count > 0)
        {
            character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(
                unicode, font, globalFallbacks, true,
                FontStyles.Normal, FontWeight.Regular, out _);

            if (character != null)
            {
                sourceFontAsset = character.textAsset as TMP_FontAsset ?? font;
                return true;
            }
        }

        // 3. Default шрифт из TMP_Settings
        var defaultFontAsset = TMP_Settings.defaultFontAsset;
        if (defaultFontAsset != null && defaultFontAsset != font)
        {
            character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(
                unicode, defaultFontAsset, true, FontStyles.Normal, FontWeight.Regular, out _);

            if (character != null)
            {
                sourceFontAsset = character.textAsset as TMP_FontAsset ?? defaultFontAsset;
                return true;
            }
        }

        sourceFontAsset = null;
        return false;
    }

    /// <summary>
    /// Материал default шрифта.
    /// </summary>
    public Material DefaultMaterial => fonts[defaultFontId]?.material;

    /// <summary>
    /// Default шрифт.
    /// </summary>
    public TMP_FontAsset DefaultFont => fonts[defaultFontId];
}
