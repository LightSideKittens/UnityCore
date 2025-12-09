using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace TMPro
{
    [Serializable]
    public class FaceInfo_Legacy
    {
        public string Name;
        public float PointSize;
        public float Scale;

        public int CharacterCount;

        public float LineHeight;
        public float Baseline;
        public float Ascender;
        public float CapHeight;
        public float Descender;
        public float CenterLine;

        public float SuperscriptOffset;
        public float SubscriptOffset;
        public float SubSize;

        public float Underline;
        public float UnderlineThickness;

        public float strikethrough;
        public float strikethroughThickness;

        public float TabWidth;

        public float Padding;
        public float AtlasWidth;
        public float AtlasHeight;
    }


    [Serializable]
    public class TMP_Glyph : TMP_TextElement_Legacy
    {
        public static TMP_Glyph Clone(TMP_Glyph source)
        {
            TMP_Glyph copy = new();

            copy.id = source.id;
            copy.x = source.x;
            copy.y = source.y;
            copy.width = source.width;
            copy.height = source.height;
            copy.xOffset = source.xOffset;
            copy.yOffset = source.yOffset;
            copy.xAdvance = source.xAdvance;
            copy.scale = source.scale;

            return copy;
        }
    }


    [Serializable]
    public struct FontAssetCreationSettings
    {
        public string sourceFontFileName;
        public string sourceFontFileGUID;
        public int faceIndex;
        public int pointSizeSamplingMode;
        public int pointSize;
        public int padding;
        public int paddingMode;
        public int packingMode;
        public int atlasWidth;
        public int atlasHeight;
        public int characterSetSelectionMode;
        public string characterSequence;
        public string referencedFontAssetGUID;
        public string referencedTextAssetGUID;
        public int fontStyle;
        public float fontStyleModifier;
        public int renderMode;
        public bool includeFontFeatures;

        internal FontAssetCreationSettings(string sourceFontFileGUID, int pointSize, int pointSizeSamplingMode,
            int padding, int packingMode, int atlasWidth, int atlasHeight, int characterSelectionMode,
            string characterSet, int renderMode)
        {
            sourceFontFileName = string.Empty;
            this.sourceFontFileGUID = sourceFontFileGUID;
            faceIndex = 0;
            this.pointSize = pointSize;
            this.pointSizeSamplingMode = pointSizeSamplingMode;
            this.padding = padding;
            paddingMode = 2;
            this.packingMode = packingMode;
            this.atlasWidth = atlasWidth;
            this.atlasHeight = atlasHeight;
            characterSequence = characterSet;
            characterSetSelectionMode = characterSelectionMode;
            this.renderMode = renderMode;

            referencedFontAssetGUID = string.Empty;
            referencedTextAssetGUID = string.Empty;
            fontStyle = 0;
            fontStyleModifier = 0;
            includeFontFeatures = false;
        }
    }

    [Serializable]
    public struct TMP_FontWeightPair
    {
        public TMP_FontAsset regularTypeface;
        public TMP_FontAsset italicTypeface;
    }


    public struct KerningPairKey
    {
        public uint ascii_Left;
        public uint ascii_Right;
        public uint key;

        public KerningPairKey(uint ascii_left, uint ascii_right)
        {
            ascii_Left = ascii_left;
            ascii_Right = ascii_right;
            key = (ascii_right << 16) + ascii_left;
        }
    }

    [Serializable]
    public struct GlyphValueRecord_Legacy
    {
        public float xPlacement;
        public float yPlacement;
        public float xAdvance;
        public float yAdvance;

        internal GlyphValueRecord_Legacy(GlyphValueRecord valueRecord)
        {
            xPlacement = valueRecord.xPlacement;
            yPlacement = valueRecord.yPlacement;
            xAdvance = valueRecord.xAdvance;
            yAdvance = valueRecord.yAdvance;
        }

        public static GlyphValueRecord_Legacy operator +(GlyphValueRecord_Legacy a, GlyphValueRecord_Legacy b)
        {
            GlyphValueRecord_Legacy c;
            c.xPlacement = a.xPlacement + b.xPlacement;
            c.yPlacement = a.yPlacement + b.yPlacement;
            c.xAdvance = a.xAdvance + b.xAdvance;
            c.yAdvance = a.yAdvance + b.yAdvance;

            return c;
        }
    }

    [Serializable]
    public class KerningPair
    {
        public uint firstGlyph
        {
            get { return m_FirstGlyph; }
            set { m_FirstGlyph = value; }
        }

        [FormerlySerializedAs("AscII_Left")] [SerializeField]
        private uint m_FirstGlyph;

        public GlyphValueRecord_Legacy firstGlyphAdjustments
        {
            get { return m_FirstGlyphAdjustments; }
        }

        [SerializeField] private GlyphValueRecord_Legacy m_FirstGlyphAdjustments;

        public uint secondGlyph
        {
            get { return m_SecondGlyph; }
            set { m_SecondGlyph = value; }
        }

        [FormerlySerializedAs("AscII_Right")] [SerializeField]
        private uint m_SecondGlyph;

        public GlyphValueRecord_Legacy secondGlyphAdjustments
        {
            get { return m_SecondGlyphAdjustments; }
        }

        [SerializeField] private GlyphValueRecord_Legacy m_SecondGlyphAdjustments;

        [FormerlySerializedAs("XadvanceOffset")]
        public float xOffset;

        internal static KerningPair empty = new(0, new(), 0, new());

        public bool ignoreSpacingAdjustments
        {
            get { return m_IgnoreSpacingAdjustments; }
        }

        [SerializeField] private bool m_IgnoreSpacingAdjustments;

        public KerningPair()
        {
            m_FirstGlyph = 0;
            m_FirstGlyphAdjustments = new();

            m_SecondGlyph = 0;
            m_SecondGlyphAdjustments = new();
        }

        public KerningPair(uint left, uint right, float offset)
        {
            firstGlyph = left;
            m_SecondGlyph = right;
            xOffset = offset;
        }

        public KerningPair(uint firstGlyph, GlyphValueRecord_Legacy firstGlyphAdjustments, uint secondGlyph,
            GlyphValueRecord_Legacy secondGlyphAdjustments)
        {
            m_FirstGlyph = firstGlyph;
            m_FirstGlyphAdjustments = firstGlyphAdjustments;
            m_SecondGlyph = secondGlyph;
            m_SecondGlyphAdjustments = secondGlyphAdjustments;
        }

        internal void ConvertLegacyKerningData()
        {
            m_FirstGlyphAdjustments.xAdvance = xOffset;
        }
    }

    [Serializable]
    public class KerningTable
    {
        public List<KerningPair> kerningPairs;

        public KerningTable()
        {
            kerningPairs = new();
        }


        public void AddKerningPair()
        {
            if (kerningPairs.Count == 0)
            {
                kerningPairs.Add(new(0, 0, 0));
            }
            else
            {
                uint left = kerningPairs.Last().firstGlyph;
                uint right = kerningPairs.Last().secondGlyph;
                float xoffset = kerningPairs.Last().xOffset;

                kerningPairs.Add(new(left, right, xoffset));
            }
        }


        public int AddKerningPair(uint first, uint second, float offset)
        {
            int index = kerningPairs.FindIndex(item => item.firstGlyph == first && item.secondGlyph == second);

            if (index == -1)
            {
                kerningPairs.Add(new(first, second, offset));
                return 0;
            }

            return -1;
        }


        public int AddGlyphPairAdjustmentRecord(uint first, GlyphValueRecord_Legacy firstAdjustments, uint second,
            GlyphValueRecord_Legacy secondAdjustments)
        {
            int index = kerningPairs.FindIndex(item => item.firstGlyph == first && item.secondGlyph == second);

            if (index == -1)
            {
                kerningPairs.Add(new(first, firstAdjustments, second, secondAdjustments));
                return 0;
            }

            return -1;
        }

        public void RemoveKerningPair(int left, int right)
        {
            int index = kerningPairs.FindIndex(item => item.firstGlyph == left && item.secondGlyph == right);

            if (index != -1)
                kerningPairs.RemoveAt(index);
        }


        public void RemoveKerningPair(int index)
        {
            kerningPairs.RemoveAt(index);
        }


        public void SortKerningPairs()
        {
            if (kerningPairs.Count > 0)
                kerningPairs = kerningPairs.OrderBy(s => s.firstGlyph).ThenBy(s => s.secondGlyph).ToList();
        }
    }


    public static class TMP_FontUtilities
    {
        private static List<int> k_searchedFontAssets;


        public static TMP_FontAsset SearchForCharacter(TMP_FontAsset font, uint unicode, out TMP_Character character)
        {
            if (k_searchedFontAssets == null)
                k_searchedFontAssets = new();

            k_searchedFontAssets.Clear();

            return SearchForCharacterInternal(font, unicode, out character);
        }


        public static TMP_FontAsset SearchForCharacter(List<TMP_FontAsset> fonts, uint unicode,
            out TMP_Character character)
        {
            return SearchForCharacterInternal(fonts, unicode, out character);
        }


        private static TMP_FontAsset SearchForCharacterInternal(TMP_FontAsset font, uint unicode,
            out TMP_Character character)
        {
            character = null;

            if (font == null) return null;

            if (font.characterLookupTable.TryGetValue(unicode, out character))
            {
                if (character.textAsset != null)
                    return font;

                font.characterLookupTable.Remove(unicode);
            }

            if (font.fallbackFontAssetTable != null && font.fallbackFontAssetTable.Count > 0)
            {
                for (int i = 0; i < font.fallbackFontAssetTable.Count && character == null; i++)
                {
                    TMP_FontAsset temp = font.fallbackFontAssetTable[i];
                    if (temp == null) continue;

                    int id = temp.GetInstanceID();

                    if (k_searchedFontAssets.Contains(id)) continue;

                    k_searchedFontAssets.Add(id);

                    temp = SearchForCharacterInternal(temp, unicode, out character);

                    if (temp != null)
                        return temp;
                }
            }

            return null;
        }


        private static TMP_FontAsset SearchForCharacterInternal(List<TMP_FontAsset> fonts, uint unicode,
            out TMP_Character character)
        {
            character = null;

            if (fonts != null && fonts.Count > 0)
            {
                for (int i = 0; i < fonts.Count; i++)
                {
                    TMP_FontAsset fontAsset = SearchForCharacterInternal(fonts[i], unicode, out character);

                    if (fontAsset != null)
                        return fontAsset;
                }
            }

            return null;
        }
    }
}