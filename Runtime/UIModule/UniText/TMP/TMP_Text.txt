#define TMP_PRESENT

using System;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;


namespace TMPro
{
    public enum TextAlignmentOptions
    {
        TopLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Top,
        Top = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Top,
        TopRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Top,
        TopJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Top,
        TopFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Top,
        TopGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Top,

        Left = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Middle,
        Center = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Middle,
        Right = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Middle,
        Justified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Middle,
        Flush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Middle,
        CenterGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Middle,

        BottomLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Bottom,
        Bottom = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Bottom,
        BottomRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Bottom,
        BottomJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Bottom,
        BottomFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Bottom,
        BottomGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Bottom,

        BaselineLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Baseline,
        Baseline = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Baseline,
        BaselineRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Baseline,
        BaselineJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Baseline,
        BaselineFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Baseline,
        BaselineGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Baseline,

        MidlineLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Geometry,
        Midline = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Geometry,
        MidlineRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Geometry,
        MidlineJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Geometry,
        MidlineFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Geometry,
        MidlineGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Geometry,

        CaplineLeft = HorizontalAlignmentOptions.Left | VerticalAlignmentOptions.Capline,
        Capline = HorizontalAlignmentOptions.Center | VerticalAlignmentOptions.Capline,
        CaplineRight = HorizontalAlignmentOptions.Right | VerticalAlignmentOptions.Capline,
        CaplineJustified = HorizontalAlignmentOptions.Justified | VerticalAlignmentOptions.Capline,
        CaplineFlush = HorizontalAlignmentOptions.Flush | VerticalAlignmentOptions.Capline,
        CaplineGeoAligned = HorizontalAlignmentOptions.Geometry | VerticalAlignmentOptions.Capline,

        Converted = 0xFFFF
    };

    public enum HorizontalAlignmentOptions
    {
        Left = 0x1,
        Center = 0x2,
        Right = 0x4,
        Justified = 0x8,
        Flush = 0x10,
        Geometry = 0x20
    }

    public enum VerticalAlignmentOptions
    {
        Top = 0x100,
        Middle = 0x200,
        Bottom = 0x400,
        Baseline = 0x800,
        Geometry = 0x1000,
        Capline = 0x2000,
    }


    public enum TextRenderFlags
    {
        DontRender = 0x0,
        Render = 0xFF
    };

    public enum TextOverflowModes
    {
        Overflow = 0,
        Ellipsis = 1,
        Truncate = 3
    };

    public enum TextWrappingModes
    {
        NoWrap = 0,
        Normal = 1,
        PreserveWhitespace = 2,
        PreserveWhitespaceNoWrap = 3
    };

    public enum TextureMappingOptions
    {
        Character = 0,
        Line = 1,
        Paragraph = 2,
        MatchAspect = 3
    };

    [Flags]
    public enum FontStyles
    {
        Normal = 0x0,
        Bold = 0x1,
        Italic = 0x2,
        Underline = 0x4,
        LowerCase = 0x8,
        UpperCase = 0x10,
        SmallCaps = 0x20,
        Strikethrough = 0x40,
        Superscript = 0x80,
        Subscript = 0x100,
        Highlight = 0x200
    };

    public enum FontWeight
    {
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Regular = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        Heavy = 800,
        Black = 900
    };

    public abstract partial class TMP_Text : MaskableGraphic
    {
        protected virtual void LoadFontAsset()
        {
        }


        protected virtual void SetSharedMaterial(Material mat)
        {
        }

        protected virtual Material GetMaterial(Material mat)
        {
            return null;
        }


        protected virtual Material[] GetSharedMaterials()
        {
            return null;
        }

        protected virtual void SetSharedMaterials(Material[] materials)
        {
        }


        protected virtual Material[] GetMaterials(Material[] mats)
        {
            return null;
        }


        protected virtual Material CreateMaterialInstance(Material source)
        {
            Material mat = new(source);
            mat.shaderKeywords = source.shaderKeywords;
            mat.name += " (Instance)";

            return mat;
        }


        protected virtual void SetFaceColor(Color32 color)
        {
        }


        protected virtual void SetOutlineColor(Color32 color)
        {
        }


        protected virtual void SetOutlineThickness(float thickness)
        {
        }

        protected virtual void SetShaderDepth()
        {
        }

        protected virtual void SetCulling()
        {
        }

        internal virtual void UpdateCulling()
        {
        }


        protected virtual float GetPaddingForMaterial()
        {
            ShaderUtilities.GetShaderPropertyIDs();

            if (m_sharedMaterial == null) return 0;

            m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            m_isSDFShader = m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);

            return m_padding;
        }


        protected virtual float GetPaddingForMaterial(Material mat)
        {
            if (mat == null)
                return 0;

            m_padding = ShaderUtilities.GetPadding(mat, m_enableExtraPadding, m_isUsingBold);
            m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
            m_isSDFShader = mat.HasProperty(ShaderUtilities.ID_WeightNormal);

            return m_padding;
        }


        protected bool m_ignoreActiveState;

        public virtual void ForceMeshUpdate(bool ignoreActiveState = false)
        {
        }


        public virtual void UpdateGeometry(Mesh mesh, int index)
        {
        }


        public virtual void UpdateMeshPadding()
        {
        }


        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
            InternalCrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
        }


        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
            InternalCrossFadeAlpha(alpha, duration, ignoreTimeScale);
        }


        protected virtual void InternalCrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale,
            bool useAlpha)
        {
        }


        protected virtual void InternalCrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
        }

        private struct TextBackingContainer
        {
            public uint[] Text => m_Array;

            public int Capacity => m_Array.Length;

            public int Count
            {
                get => m_Index;
                set => m_Index = value;
            }

            private uint[] m_Array;
            private int m_Index;

            public uint this[int index]
            {
                get => m_Array[index];
                set
                {
                    if (index >= m_Array.Length)
                        Resize(index);

                    m_Array[index] = value;
                }
            }

            public TextBackingContainer(int size)
            {
                m_Array = new uint[size];
                m_Index = 0;
            }

            public void Resize(int size)
            {
                size = Mathf.NextPowerOfTwo(size + 1);

                Array.Resize(ref m_Array, size);
            }
        }


        private TMP_Style GetStyle(int hashCode)
        {
            TMP_Style style = null;

            if (m_StyleSheet != null)
            {
                style = m_StyleSheet.GetStyle(hashCode);

                if (style != null)
                    return style;
            }

            if (TMP_Settings.defaultStyleSheet != null)
                style = TMP_Settings.defaultStyleSheet.GetStyle(hashCode);

            return style;
        }

        private void InsertOpeningTextStyle(TMP_Style style, ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            m_TextStyleStackDepth += 1;

            m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);

            uint[] styleDefinition = style.styleOpeningTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth -= 1;
        }

        private void InsertClosingTextStyle(TMP_Style style, ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            m_TextStyleStackDepth += 1;

            m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);

            uint[] styleDefinition = style.styleClosingTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth -= 1;
        }

        private void InsertTextStyleInTextProcessingArray(ref TextProcessingElement[] charBuffer, ref int writeIndex,
            uint[] styleDefinition)
        {
            int styleLength = styleDefinition.Length;

            if (writeIndex + styleLength >= charBuffer.Length)
                ResizeInternalArray(ref charBuffer, writeIndex + styleLength);

            for (int i = 0; i < styleLength; i++)
            {
                uint c = styleDefinition[i];

                if (c == '\\' && i + 1 < styleLength)
                {
                    switch (styleDefinition[i + 1])
                    {
                        case '\\':
                            i += 1;
                            break;
                        case 'n':
                            c = 10;
                            i += 1;
                            break;
                        case 'r':
                            break;
                        case 't':
                            break;
                        case 'u':
                            if (i + 5 < styleLength)
                            {
                                c = GetUTF16(styleDefinition, i + 2);

                                i += 5;
                            }

                            break;
                        case 'U':
                            if (i + 9 < styleLength)
                            {
                                c = GetUTF32(styleDefinition, i + 2);

                                i += 9;
                            }

                            break;
                    }
                }

                if (c == '<')
                {
                    int hashCode = GetMarkupTagHashCode(styleDefinition, i + 1);

                    switch ((MarkupTag)hashCode)
                    {
                        case MarkupTag.NO_PARSE:
                            tag_NoParsing = true;
                            break;
                        case MarkupTag.SLASH_NO_PARSE:
                            tag_NoParsing = false;
                            break;

                        case MarkupTag.BR:
                            if (tag_NoParsing) break;

                            charBuffer[writeIndex].unicode = 10;
                            writeIndex += 1;
                            i += 3;
                            continue;
                        case MarkupTag.CR:
                            if (tag_NoParsing) break;

                            charBuffer[writeIndex].unicode = 13;
                            writeIndex += 1;
                            i += 3;
                            continue;
                        case MarkupTag.NBSP:
                            if (tag_NoParsing) break;

                            charBuffer[writeIndex].unicode = 160;
                            writeIndex += 1;
                            i += 5;
                            continue;
                        case MarkupTag.ZWSP:
                            if (tag_NoParsing) break;

                            charBuffer[writeIndex].unicode = 0x200B;
                            writeIndex += 1;
                            i += 5;
                            continue;
                        case MarkupTag.ZWJ:
                            if (tag_NoParsing) break;

                            charBuffer[writeIndex].unicode = 0x200D;
                            writeIndex += 1;
                            i += 4;
                            continue;
                        case MarkupTag.SHY:
                            if (tag_NoParsing) break;

                            charBuffer[writeIndex].unicode = 0xAD;
                            writeIndex += 1;
                            i += 4;
                            continue;
                        case MarkupTag.STYLE:
                            if (tag_NoParsing) break;

                            if (ReplaceOpeningStyleTag(ref styleDefinition, i, out int offset, ref charBuffer,
                                    ref writeIndex))
                            {
                                int remainChar = styleLength - offset;
                                i = offset;

                                if (writeIndex + remainChar >= charBuffer.Length)
                                    ResizeInternalArray(ref charBuffer, writeIndex + remainChar);

                                continue;
                            }

                            break;
                        case MarkupTag.SLASH_STYLE:
                            if (tag_NoParsing) break;

                            ReplaceClosingStyleTag(ref charBuffer, ref writeIndex);

                            i += 7;
                            continue;
                    }
                }

                charBuffer[writeIndex].unicode = c;
                writeIndex += 1;
            }
        }


        private bool ReplaceOpeningStyleTag(ref TextBackingContainer sourceText, int srcIndex, out int srcOffset,
            ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            int styleHashCode = GetStyleHashCode(ref sourceText, srcIndex + 7, out srcOffset);
            TMP_Style style = GetStyle(styleHashCode);

            if (style == null || srcOffset == 0) return false;

            m_TextStyleStackDepth += 1;

            m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);

            uint[] styleDefinition = style.styleOpeningTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth -= 1;

            return true;
        }


        private bool ReplaceOpeningStyleTag(ref uint[] sourceText, int srcIndex, out int srcOffset,
            ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            int styleHashCode = GetStyleHashCode(ref sourceText, srcIndex + 7, out srcOffset);
            TMP_Style style = GetStyle(styleHashCode);

            if (style == null || srcOffset == 0) return false;

            m_TextStyleStackDepth += 1;

            m_TextStyleStacks[m_TextStyleStackDepth].Push(style.hashCode);

            uint[] styleDefinition = style.styleOpeningTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth -= 1;

            return true;
        }


        private void ReplaceClosingStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            int styleHashCode = m_TextStyleStacks[m_TextStyleStackDepth + 1].Pop();
            TMP_Style style = GetStyle(styleHashCode);

            if (style == null) return;

            m_TextStyleStackDepth += 1;

            uint[] styleDefinition = style.styleClosingTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth -= 1;
        }


        private void InsertOpeningStyleTag(TMP_Style style, ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            if (style == null) return;

            m_TextStyleStacks[0].Push(style.hashCode);

            uint[] styleDefinition = style.styleOpeningTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth = 0;
        }


        private void InsertClosingStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex)
        {
            int styleHashCode = m_TextStyleStacks[0].Pop();
            TMP_Style style = GetStyle(styleHashCode);

            uint[] styleDefinition = style.styleClosingTagArray;

            InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleDefinition);

            m_TextStyleStackDepth = 0;
        }


        private int GetMarkupTagHashCode(uint[] styleDefinition, int readIndex)
        {
            int hashCode = 0;
            int maxReadIndex = readIndex + 16;
            int styleDefinitionLength = styleDefinition.Length;

            for (; readIndex < maxReadIndex && readIndex < styleDefinitionLength; readIndex++)
            {
                uint c = styleDefinition[readIndex];

                if (c == '>' || c == '=' || c == ' ')
                    return hashCode;

                hashCode = ((hashCode << 5) + hashCode) ^ (int)TMP_TextParsingUtilities.ToUpperASCIIFast(c);
            }

            return hashCode;
        }


        private int GetMarkupTagHashCode(TextBackingContainer styleDefinition, int readIndex)
        {
            int hashCode = 0;
            int maxReadIndex = readIndex + 16;
            int styleDefinitionLength = styleDefinition.Capacity;

            for (; readIndex < maxReadIndex && readIndex < styleDefinitionLength; readIndex++)
            {
                uint c = styleDefinition[readIndex];

                if (c == '>' || c == '=' || c == ' ')
                    return hashCode;

                hashCode = ((hashCode << 5) + hashCode) ^ (int)TMP_TextParsingUtilities.ToUpperASCIIFast(c);
            }

            return hashCode;
        }


        private int GetStyleHashCode(ref uint[] text, int index, out int closeIndex)
        {
            int hashCode = 0;
            closeIndex = 0;

            for (int i = index; i < text.Length; i++)
            {
                if (text[i] == 34) continue;

                if (text[i] == 62)
                {
                    closeIndex = i;
                    break;
                }

                hashCode = (hashCode << 5) + hashCode ^ TMP_TextParsingUtilities.ToUpperASCIIFast((char)text[i]);
            }

            return hashCode;
        }


        private int GetStyleHashCode(ref TextBackingContainer text, int index, out int closeIndex)
        {
            int hashCode = 0;
            closeIndex = 0;

            for (int i = index; i < text.Capacity; i++)
            {
                if (text[i] == 34) continue;

                if (text[i] == 62)
                {
                    closeIndex = i;
                    break;
                }

                hashCode = (hashCode << 5) + hashCode ^ TMP_TextParsingUtilities.ToUpperASCIIFast((char)text[i]);
            }

            return hashCode;
        }

        private void ResizeInternalArray<T>(ref T[] array)
        {
            int size = Mathf.NextPowerOfTwo(array.Length + 1);

            Array.Resize(ref array, size);
        }

        private void ResizeInternalArray<T>(ref T[] array, int size)
        {
            size = Mathf.NextPowerOfTwo(size + 1);

            Array.Resize(ref array, size);
        }


        private string InternalTextBackingArrayToString()
        {
            char[] array = new char[textBackingArray.Count];

            for (int i = 0; i < textBackingArray.Capacity; i++)
            {
                char c = (char)textBackingArray[i];

                if (c == 0)
                    break;

                array[i] = c;
            }

            m_IsTextBackingStringDirty = false;

            return new(array);
        }

        internal void InsertNewLine(int i, float baseScale, float currentElementScale, float currentEmScale,
            float boldSpacingAdjustment, float characterSpacingAdjustment, float width, float lineGap,
            ref bool isMaxVisibleDescenderSet, ref float maxVisibleDescender)
        {
            k_InsertNewLineMarker.Begin();

            float baselineAdjustmentDelta = m_maxLineAscender - m_startOfLineAscender;
            if (m_lineOffset > 0 && Math.Abs(baselineAdjustmentDelta) > 0.01f && !m_IsDrivenLineSpacing)
            {
                AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, baselineAdjustmentDelta);
                m_ElementDescender -= baselineAdjustmentDelta;
                m_lineOffset += baselineAdjustmentDelta;
            }

            float lineAscender = m_maxLineAscender - m_lineOffset;
            float lineDescender = m_maxLineDescender - m_lineOffset;

            m_ElementDescender = m_ElementDescender < lineDescender ? m_ElementDescender : lineDescender;
            if (!isMaxVisibleDescenderSet)
                maxVisibleDescender = m_ElementDescender;

            if (m_useMaxVisibleDescender &&
                (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
                isMaxVisibleDescenderSet = true;

            ref var lineInfo = ref m_textInfo.lineInfo[m_lineNumber];
            lineInfo.firstCharacterIndex = m_firstCharacterOfLine;
            lineInfo.firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine =
                m_firstCharacterOfLine > m_firstVisibleCharacterOfLine
                    ? m_firstCharacterOfLine
                    : m_firstVisibleCharacterOfLine;
            lineInfo.lastCharacterIndex = m_lastCharacterOfLine = m_characterCount - 1 > 0 ? m_characterCount - 1 : 0;
            lineInfo.lastVisibleCharacterIndex = m_lastVisibleCharacterOfLine =
                m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine
                    ? m_firstVisibleCharacterOfLine
                    : m_lastVisibleCharacterOfLine;

            lineInfo.characterCount = lineInfo.lastCharacterIndex - lineInfo.firstCharacterIndex + 1;
            lineInfo.visibleCharacterCount = m_lineVisibleCharacterCount;
            lineInfo.visibleSpaceCount = (lineInfo.lastVisibleCharacterIndex + 1 - lineInfo.firstCharacterIndex) -
                                         m_lineVisibleCharacterCount;
            lineInfo.lineExtents.min = new(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x,
                lineDescender);
            lineInfo.lineExtents.max =
                new(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, lineAscender);
            lineInfo.length = lineInfo.lineExtents.max.x;
            lineInfo.width = width;

            float glyphAdjustment = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].adjustedHorizontalAdvance;
            float maxAdvanceOffset =
                (glyphAdjustment * currentElementScale +
                 (m_currentFontAsset.normalSpacingOffset + characterSpacingAdjustment + boldSpacingAdjustment) *
                 currentEmScale + m_cSpacing) * (1 - m_charWidthAdjDelta);
            float adjustedHorizontalAdvance = lineInfo.maxAdvance =
                m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - maxAdvanceOffset;
            m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance = adjustedHorizontalAdvance;

            lineInfo.baseline = 0 - m_lineOffset;
            lineInfo.ascender = lineAscender;
            lineInfo.descender = lineDescender;
            lineInfo.lineHeight = lineAscender - lineDescender + lineGap * baseScale;

            m_firstCharacterOfLine = m_characterCount;
            m_lineVisibleCharacterCount = 0;
            m_lineVisibleSpaceCount = 0;

            SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount - 1);

            m_lineNumber += 1;

            if (m_lineNumber >= m_textInfo.lineInfo.Length)
                ResizeLineExtents(m_lineNumber);

            if (m_lineHeight == TMP_Math.FLOAT_UNSET)
            {
                float ascender = m_textInfo.characterInfo[m_characterCount].adjustedAscender;
                float lineOffsetDelta = 0 - m_maxLineDescender + ascender + (lineGap + m_lineSpacingDelta) * baseScale +
                                        m_lineSpacing * currentEmScale;
                m_lineOffset += lineOffsetDelta;

                m_startOfLineAscender = ascender;
            }
            else
            {
                m_lineOffset += m_lineHeight + m_lineSpacing * currentEmScale;
            }

            m_maxLineAscender = largeNegativeFloat;
            m_maxLineDescender = largePositiveFloat;

            m_xAdvance = 0 + tag_Indent;
            k_InsertNewLineMarker.End();
        }


        internal void SaveWordWrappingState(ref WordWrapState state, int index, int count)
        {
            state.currentFontAsset = m_currentFontAsset;
            state.currentMaterial = m_currentMaterial;
            state.currentMaterialIndex = m_currentMaterialIndex;

            state.previous_WordBreak = index;
            state.total_CharacterCount = count;
            state.visible_CharacterCount = m_lineVisibleCharacterCount;
            state.visibleSpaceCount = m_lineVisibleSpaceCount;

            state.firstCharacterIndex = m_firstCharacterOfLine;
            state.firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine;
            state.lastVisibleCharIndex = m_lastVisibleCharacterOfLine;

            state.fontStyle = m_FontStyleInternal;
            state.italicAngle = m_ItalicAngle;
            state.fontScaleMultiplier = m_fontScaleMultiplier;
            state.currentFontSize = m_currentFontSize;

            state.xAdvance = m_xAdvance;
            state.maxCapHeight = m_maxCapHeight;
            state.maxAscender = m_maxTextAscender;
            state.maxDescender = m_ElementDescender;
            state.startOfLineAscender = m_startOfLineAscender;
            state.maxLineAscender = m_maxLineAscender;
            state.maxLineDescender = m_maxLineDescender;

            state.meshExtents = m_meshExtents;

            state.lineNumber = m_lineNumber;
            state.lineOffset = m_lineOffset;
            state.baselineOffset = m_baselineOffset;
            state.isDrivenLineSpacing = m_IsDrivenLineSpacing;
            state.lastBaseGlyphIndex = m_LastBaseGlyphIndex;

            state.cSpace = m_cSpacing;
            state.mSpace = m_monoSpacing;

            state.horizontalAlignment = m_lineJustification;
            state.marginLeft = m_marginLeft;
            state.marginRight = m_marginRight;

            state.vertexColor = m_htmlColor;
            state.underlineColor = m_underlineColor;
            state.strikethroughColor = m_strikethroughColor;
            state.highlightState = m_HighlightState;

            state.isNonBreakingSpace = m_isNonBreakingSpace;
            state.tagNoParsing = tag_NoParsing;

            state.fxRotation = m_FXRotation;
            state.fxScale = m_FXScale;

            state.basicStyleStack = m_fontStyleStack;
            state.italicAngleStack = m_ItalicAngleStack;
            state.colorStack = m_colorStack;
            state.underlineColorStack = m_underlineColorStack;
            state.strikethroughColorStack = m_strikethroughColorStack;
            state.highlightStateStack = m_HighlightStateStack;
            state.colorGradientStack = m_colorGradientStack;
            state.sizeStack = m_sizeStack;
            state.indentStack = m_indentStack;
            state.fontWeightStack = m_FontWeightStack;

            state.baselineStack = m_baselineOffsetStack;
            state.actionStack = m_actionStack;
            state.materialReferenceStack = m_materialReferenceStack;
            state.lineJustificationStack = m_lineJustificationStack;

            state.spriteAnimationID = m_spriteAnimationID;

            if (m_lineNumber < m_textInfo.lineInfo.Length)
                state.lineInfo = m_textInfo.lineInfo[m_lineNumber];
        }


        internal int RestoreWordWrappingState(ref WordWrapState state)
        {
            int index = state.previous_WordBreak;

            m_currentFontAsset = state.currentFontAsset;
            m_currentMaterial = state.currentMaterial;
            m_currentMaterialIndex = state.currentMaterialIndex;

            m_characterCount = state.total_CharacterCount + 1;
            m_lineVisibleCharacterCount = state.visible_CharacterCount;
            m_lineVisibleSpaceCount = state.visibleSpaceCount;
            m_textInfo.linkCount = state.visible_LinkCount;

            m_firstCharacterOfLine = state.firstCharacterIndex;
            m_firstVisibleCharacterOfLine = state.firstVisibleCharacterIndex;
            m_lastVisibleCharacterOfLine = state.lastVisibleCharIndex;

            m_FontStyleInternal = state.fontStyle;
            m_ItalicAngle = state.italicAngle;
            m_fontScaleMultiplier = state.fontScaleMultiplier;
            m_currentFontSize = state.currentFontSize;

            m_xAdvance = state.xAdvance;
            m_maxCapHeight = state.maxCapHeight;
            m_maxTextAscender = state.maxAscender;
            m_ElementDescender = state.maxDescender;
            m_startOfLineAscender = state.startOfLineAscender;
            m_maxLineAscender = state.maxLineAscender;
            m_maxLineDescender = state.maxLineDescender;

            m_meshExtents = state.meshExtents;

            m_lineNumber = state.lineNumber;
            m_lineOffset = state.lineOffset;
            m_baselineOffset = state.baselineOffset;
            m_IsDrivenLineSpacing = state.isDrivenLineSpacing;
            m_LastBaseGlyphIndex = state.lastBaseGlyphIndex;

            m_cSpacing = state.cSpace;
            m_monoSpacing = state.mSpace;

            m_lineJustification = state.horizontalAlignment;
            m_marginLeft = state.marginLeft;
            m_marginRight = state.marginRight;

            m_htmlColor = state.vertexColor;
            m_underlineColor = state.underlineColor;
            m_strikethroughColor = state.strikethroughColor;
            m_HighlightState = state.highlightState;

            m_isNonBreakingSpace = state.isNonBreakingSpace;
            tag_NoParsing = state.tagNoParsing;

            m_FXRotation = state.fxRotation;
            m_FXScale = state.fxScale;

            m_fontStyleStack = state.basicStyleStack;
            m_ItalicAngleStack = state.italicAngleStack;
            m_colorStack = state.colorStack;
            m_underlineColorStack = state.underlineColorStack;
            m_strikethroughColorStack = state.strikethroughColorStack;
            m_HighlightStateStack = state.highlightStateStack;
            m_colorGradientStack = state.colorGradientStack;
            m_sizeStack = state.sizeStack;
            m_indentStack = state.indentStack;
            m_FontWeightStack = state.fontWeightStack;

            m_baselineOffsetStack = state.baselineStack;
            m_actionStack = state.actionStack;
            m_materialReferenceStack = state.materialReferenceStack;
            m_lineJustificationStack = state.lineJustificationStack;

            m_spriteAnimationID = state.spriteAnimationID;

            if (m_lineNumber < m_textInfo.lineInfo.Length)
                m_textInfo.lineInfo[m_lineNumber] = state.lineInfo;

            return index;
        }


        protected virtual void SaveGlyphVertexInfo(float padding, float style_padding, Color32 vertexColor)
        {
            #region Setup Mesh Vertices

            m_textInfo.characterInfo[m_characterCount].vertex_BL.position =
                m_textInfo.characterInfo[m_characterCount].bottomLeft;
            m_textInfo.characterInfo[m_characterCount].vertex_TL.position =
                m_textInfo.characterInfo[m_characterCount].topLeft;
            m_textInfo.characterInfo[m_characterCount].vertex_TR.position =
                m_textInfo.characterInfo[m_characterCount].topRight;
            m_textInfo.characterInfo[m_characterCount].vertex_BR.position =
                m_textInfo.characterInfo[m_characterCount].bottomRight;

            #endregion


            #region Setup Vertex Colors

            vertexColor.a = m_fontColor32.a < vertexColor.a ? m_fontColor32.a : vertexColor.a;

#if TEXTCORE_FONT_ENGINE_1_5_OR_NEWER
            bool isColorGlyph =
                ((GlyphRasterModes)m_currentFontAsset.m_AtlasRenderMode & GlyphRasterModes.RASTER_MODE_COLOR) ==
                GlyphRasterModes.RASTER_MODE_COLOR;
#else
            bool isColorGlyph = false;
#endif

            if (!m_enableVertexGradient || isColorGlyph)
            {
                vertexColor = isColorGlyph ? new(255, 255, 255, vertexColor.a) : vertexColor;

                m_textInfo.characterInfo[m_characterCount].vertex_BL.color = vertexColor;
                m_textInfo.characterInfo[m_characterCount].vertex_TL.color = vertexColor;
                m_textInfo.characterInfo[m_characterCount].vertex_TR.color = vertexColor;
                m_textInfo.characterInfo[m_characterCount].vertex_BR.color = vertexColor;
            }
            else
            {
                if (!m_overrideHtmlColors && m_colorStack.index > 1)
                {
                    m_textInfo.characterInfo[m_characterCount].vertex_BL.color = vertexColor;
                    m_textInfo.characterInfo[m_characterCount].vertex_TL.color = vertexColor;
                    m_textInfo.characterInfo[m_characterCount].vertex_TR.color = vertexColor;
                    m_textInfo.characterInfo[m_characterCount].vertex_BR.color = vertexColor;
                }
                else
                {
                    if (m_fontColorGradientPreset != null)
                    {
                        m_textInfo.characterInfo[m_characterCount].vertex_BL.color =
                            m_fontColorGradientPreset.bottomLeft * vertexColor;
                        m_textInfo.characterInfo[m_characterCount].vertex_TL.color =
                            m_fontColorGradientPreset.topLeft * vertexColor;
                        m_textInfo.characterInfo[m_characterCount].vertex_TR.color =
                            m_fontColorGradientPreset.topRight * vertexColor;
                        m_textInfo.characterInfo[m_characterCount].vertex_BR.color =
                            m_fontColorGradientPreset.bottomRight * vertexColor;
                    }
                    else
                    {
                        m_textInfo.characterInfo[m_characterCount].vertex_BL.color =
                            m_fontColorGradient.bottomLeft * vertexColor;
                        m_textInfo.characterInfo[m_characterCount].vertex_TL.color =
                            m_fontColorGradient.topLeft * vertexColor;
                        m_textInfo.characterInfo[m_characterCount].vertex_TR.color =
                            m_fontColorGradient.topRight * vertexColor;
                        m_textInfo.characterInfo[m_characterCount].vertex_BR.color =
                            m_fontColorGradient.bottomRight * vertexColor;
                    }
                }
            }

            if (m_colorGradientPreset != null && !isColorGlyph)
            {
                if (m_colorGradientPresetIsTinted)
                {
                    m_textInfo.characterInfo[m_characterCount].vertex_BL.color *= m_colorGradientPreset.bottomLeft;
                    m_textInfo.characterInfo[m_characterCount].vertex_TL.color *= m_colorGradientPreset.topLeft;
                    m_textInfo.characterInfo[m_characterCount].vertex_TR.color *= m_colorGradientPreset.topRight;
                    m_textInfo.characterInfo[m_characterCount].vertex_BR.color *= m_colorGradientPreset.bottomRight;
                }
                else
                {
                    m_textInfo.characterInfo[m_characterCount].vertex_BL.color =
                        m_colorGradientPreset.bottomLeft.MinAlpha(vertexColor);
                    m_textInfo.characterInfo[m_characterCount].vertex_TL.color =
                        m_colorGradientPreset.topLeft.MinAlpha(vertexColor);
                    m_textInfo.characterInfo[m_characterCount].vertex_TR.color =
                        m_colorGradientPreset.topRight.MinAlpha(vertexColor);
                    m_textInfo.characterInfo[m_characterCount].vertex_BR.color =
                        m_colorGradientPreset.bottomRight.MinAlpha(vertexColor);
                }
            }

            #endregion

            if (!m_isSDFShader)
                style_padding = 0f;


            #region Setup UVs

            Glyph altGlyph = m_textInfo.characterInfo[m_characterCount].alternativeGlyph;
            GlyphRect glyphRect = altGlyph?.glyphRect ?? m_cached_TextElement.m_Glyph.glyphRect;

            Vector2 uv0;
            uv0.x = (glyphRect.x - padding - style_padding) / m_currentFontAsset.m_AtlasWidth;
            uv0.y = (glyphRect.y - padding - style_padding) / m_currentFontAsset.m_AtlasHeight;

            Vector2 uv1;
            uv1.x = uv0.x;
            uv1.y = (glyphRect.y + padding + style_padding + glyphRect.height) / m_currentFontAsset.m_AtlasHeight;

            Vector2 uv2;
            uv2.x = (glyphRect.x + padding + style_padding + glyphRect.width) / m_currentFontAsset.m_AtlasWidth;
            uv2.y = uv1.y;

            Vector2 uv3;
            uv3.x = uv2.x;
            uv3.y = uv0.y;

            m_textInfo.characterInfo[m_characterCount].vertex_BL.uv = uv0;
            m_textInfo.characterInfo[m_characterCount].vertex_TL.uv = uv1;
            m_textInfo.characterInfo[m_characterCount].vertex_TR.uv = uv2;
            m_textInfo.characterInfo[m_characterCount].vertex_BR.uv = uv3;

            #endregion Setup UVs


            #region Setup Normals & Tangents

            #endregion end Normals & Tangents
        }


        protected virtual void FillCharacterVertexBuffers(int i)
        {
            int materialIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
            ref var meshInfo = ref m_textInfo.meshInfo[materialIndex];
            int index_X4 = m_textInfo.meshInfo[materialIndex].vertexCount;

            if (index_X4 >= meshInfo.vertices.Length)
                meshInfo.ResizeMeshInfo(Mathf.NextPowerOfTwo((index_X4 + 4) / 4));

            TMP_CharacterInfo[] characterInfoArray = m_textInfo.characterInfo;
            m_textInfo.characterInfo[i].vertexIndex = index_X4;

            var chInfo = characterInfoArray[i];
            var BL = chInfo.vertex_BL;
            var TL = chInfo.vertex_TL;
            var TR = chInfo.vertex_TR;
            var BR = chInfo.vertex_BR;
            var i0 = 0 + index_X4;
            var i1 = 1 + index_X4;
            var i2 = 2 + index_X4;
            var i3 = 3 + index_X4;

            var verts = meshInfo.vertices;
            verts[i0] = BL.position;
            verts[i1] = TL.position;
            verts[i2] = TR.position;
            verts[i3] = BR.position;


            var uvs0 = meshInfo.uvs0;
            uvs0[i0] = BL.uv;
            uvs0[i1] = TL.uv;
            uvs0[i2] = TR.uv;
            uvs0[i3] = BR.uv;

            var uvs2 = meshInfo.uvs2;
            uvs2[i0] = BL.uv2;
            uvs2[i1] = TL.uv2;
            uvs2[i2] = TR.uv2;
            uvs2[i3] = BR.uv2;

            var colors = meshInfo.colors32;
            colors[i0] = m_ConvertToLinearSpace ? BL.color.GammaToLinear() : BL.color;
            colors[i1] = m_ConvertToLinearSpace ? TL.color.GammaToLinear() : TL.color;
            colors[i2] = m_ConvertToLinearSpace ? TR.color.GammaToLinear() : TR.color;
            colors[i3] = m_ConvertToLinearSpace ? BR.color.GammaToLinear() : BR.color;

            meshInfo.vertexCount = index_X4 + 4;
        }


        protected virtual void DrawUnderlineMesh(Vector3 start, Vector3 end, ref int index, float startScale,
            float endScale, float maxScale, float sdfScale, Color32 underlineColor)
        {
            GetUnderlineSpecialCharacter(m_fontAsset);

            if (m_Underline.character == null)
            {
                if (!TMP_Settings.warningsDisabled)
                    Debug.LogWarning(
                        "Unable to add underline or strikethrough since the character [0x5F] used by these features is not present in the Font Asset assigned to this text object.",
                        this);

                return;
            }

            int underlineMaterialIndex = m_Underline.materialIndex;

            int verticesCount = index + 12;

            if (verticesCount > m_textInfo.meshInfo[underlineMaterialIndex].vertices.Length)
            {
                m_textInfo.meshInfo[underlineMaterialIndex].ResizeMeshInfo(verticesCount / 4);
            }

            start.y = Mathf.Min(start.y, end.y);
            end.y = Mathf.Min(start.y, end.y);

            GlyphMetrics underlineGlyphMetrics = m_Underline.character.glyph.metrics;
            GlyphRect underlineGlyphRect = m_Underline.character.glyph.glyphRect;

            float segmentWidth = underlineGlyphMetrics.width / 2 * maxScale;

            if (end.x - start.x < underlineGlyphMetrics.width * maxScale)
            {
                segmentWidth = (end.x - start.x) / 2f;
            }

            float startPadding = m_padding * startScale / maxScale;
            float endPadding = m_padding * endScale / maxScale;

            float underlineThickness = m_Underline.fontAsset.faceInfo.underlineThickness;

            #region UNDERLINE VERTICES

            Vector3[] vertices = m_textInfo.meshInfo[underlineMaterialIndex].vertices;

            vertices[index + 0] = start + new Vector3(0, 0 - (underlineThickness + m_padding) * maxScale, 0);
            vertices[index + 1] = start + new Vector3(0, m_padding * maxScale, 0);
            vertices[index + 2] = vertices[index + 1] + new Vector3(segmentWidth, 0, 0);
            vertices[index + 3] = vertices[index + 0] + new Vector3(segmentWidth, 0, 0);

            vertices[index + 4] = vertices[index + 3];
            vertices[index + 5] = vertices[index + 2];
            vertices[index + 6] = end + new Vector3(-segmentWidth, m_padding * maxScale, 0);
            vertices[index + 7] = end + new Vector3(-segmentWidth, -(underlineThickness + m_padding) * maxScale, 0);

            vertices[index + 8] = vertices[index + 7];
            vertices[index + 9] = vertices[index + 6];
            vertices[index + 10] = end + new Vector3(0, m_padding * maxScale, 0);
            vertices[index + 11] = end + new Vector3(0, -(underlineThickness + m_padding) * maxScale, 0);

            #endregion

            #region HANDLE UV0

            Vector4[] uvs0 = m_textInfo.meshInfo[underlineMaterialIndex].uvs0;

            int atlasWidth = m_Underline.fontAsset.atlasWidth;
            int atlasHeight = m_Underline.fontAsset.atlasHeight;

            float xScale = Mathf.Abs(sdfScale);

            Vector4 uv0 = new((underlineGlyphRect.x - startPadding) / atlasWidth,
                (underlineGlyphRect.y - m_padding) / atlasHeight, 0, xScale);
            Vector4 uv1 = new(uv0.x, (underlineGlyphRect.y + underlineGlyphRect.height + m_padding) / atlasHeight, 0,
                xScale);
            Vector4 uv2 = new((underlineGlyphRect.x - startPadding + (float)underlineGlyphRect.width / 2) / atlasWidth,
                uv1.y, 0, xScale);
            Vector4 uv3 = new(uv2.x, uv0.y, 0, xScale);
            Vector4 uv4 = new((underlineGlyphRect.x + endPadding + (float)underlineGlyphRect.width / 2) / atlasWidth,
                uv1.y, 0, xScale);
            Vector4 uv5 = new(uv4.x, uv0.y, 0, xScale);
            Vector4 uv6 = new((underlineGlyphRect.x + endPadding + underlineGlyphRect.width) / atlasWidth, uv1.y, 0,
                xScale);
            Vector4 uv7 = new(uv6.x, uv0.y, 0, xScale);

            uvs0[0 + index] = uv0;
            uvs0[1 + index] = uv1;
            uvs0[2 + index] = uv2;
            uvs0[3 + index] = uv3;

            uvs0[4 + index] = new(uv2.x - uv2.x * 0.001f, uv0.y, 0, xScale);
            uvs0[5 + index] = new(uv2.x - uv2.x * 0.001f, uv1.y, 0, xScale);
            uvs0[6 + index] = new(uv2.x + uv2.x * 0.001f, uv1.y, 0, xScale);
            uvs0[7 + index] = new(uv2.x + uv2.x * 0.001f, uv0.y, 0, xScale);

            uvs0[8 + index] = uv5;
            uvs0[9 + index] = uv4;
            uvs0[10 + index] = uv6;
            uvs0[11 + index] = uv7;

            #endregion

            #region HANDLE UV2 - SDF SCALE

            float min_UvX = 0;
            float max_UvX = (vertices[index + 2].x - start.x) / (end.x - start.x);

            Vector2[] uvs2 = m_textInfo.meshInfo[underlineMaterialIndex].uvs2;

            uvs2[0 + index] = new(0, 0);
            uvs2[1 + index] = new(0, 1);
            uvs2[2 + index] = new(max_UvX, 1);
            uvs2[3 + index] = new(max_UvX, 0);

            min_UvX = (vertices[index + 4].x - start.x) / (end.x - start.x);
            max_UvX = (vertices[index + 6].x - start.x) / (end.x - start.x);

            uvs2[4 + index] = new(min_UvX, 0);
            uvs2[5 + index] = new(min_UvX, 1);
            uvs2[6 + index] = new(max_UvX, 1);
            uvs2[7 + index] = new(max_UvX, 0);

            min_UvX = (vertices[index + 8].x - start.x) / (end.x - start.x);

            uvs2[8 + index] = new(min_UvX, 0);
            uvs2[9 + index] = new(min_UvX, 1);
            uvs2[10 + index] = new(1, 1);
            uvs2[11 + index] = new(1, 0);

            #endregion

            #region UNDERLINE VERTEX COLORS

            underlineColor.a = m_fontColor32.a < underlineColor.a ? m_fontColor32.a : underlineColor.a;

            Color32[] colors32 = m_textInfo.meshInfo[underlineMaterialIndex].colors32;
            colors32[0 + index] = underlineColor;
            colors32[1 + index] = underlineColor;
            colors32[2 + index] = underlineColor;
            colors32[3 + index] = underlineColor;

            colors32[4 + index] = underlineColor;
            colors32[5 + index] = underlineColor;
            colors32[6 + index] = underlineColor;
            colors32[7 + index] = underlineColor;

            colors32[8 + index] = underlineColor;
            colors32[9 + index] = underlineColor;
            colors32[10 + index] = underlineColor;
            colors32[11 + index] = underlineColor;

            #endregion

            index += 12;
        }


        protected virtual void DrawTextHighlight(Vector3 start, Vector3 end, ref int index, Color32 highlightColor)
        {
            if (m_Underline.character == null)
            {
                GetUnderlineSpecialCharacter(m_fontAsset);

                if (m_Underline.character == null)
                {
                    if (!TMP_Settings.warningsDisabled)
                        Debug.LogWarning(
                            "Unable to add highlight since the primary Font Asset doesn't contain the underline character.",
                            this);

                    return;
                }
            }

            int underlineMaterialIndex = m_Underline.materialIndex;

            int verticesCount = index + 4;

            if (verticesCount > m_textInfo.meshInfo[underlineMaterialIndex].vertices.Length)
            {
                m_textInfo.meshInfo[underlineMaterialIndex].ResizeMeshInfo(verticesCount / 4);
            }

            #region HIGHLIGHT VERTICES

            Vector3[] vertices = m_textInfo.meshInfo[underlineMaterialIndex].vertices;

            vertices[index + 0] = start;
            vertices[index + 1] = new(start.x, end.y, 0);
            vertices[index + 2] = end;
            vertices[index + 3] = new(end.x, start.y, 0);

            #endregion

            #region HANDLE UV0

            Vector4[] uvs0 = m_textInfo.meshInfo[underlineMaterialIndex].uvs0;

            int atlasWidth = m_Underline.fontAsset.atlasWidth;
            int atlasHeight = m_Underline.fontAsset.atlasHeight;
            GlyphRect glyphRect = m_Underline.character.glyph.glyphRect;

            Vector2 uvGlyphCenter = new((glyphRect.x + (float)glyphRect.width / 2) / atlasWidth,
                (glyphRect.y + (float)glyphRect.height / 2) / atlasHeight);
            Vector2 uvTexelSize = new(1.0f / atlasWidth, 1.0f / atlasHeight);

            uvs0[index + 0] = uvGlyphCenter - uvTexelSize;
            uvs0[index + 1] = uvGlyphCenter + new Vector2(-uvTexelSize.x, uvTexelSize.y);
            uvs0[index + 2] = uvGlyphCenter + uvTexelSize;
            uvs0[index + 3] = uvGlyphCenter + new Vector2(uvTexelSize.x, -uvTexelSize.y);

            #endregion

            #region HANDLE UV2 - SDF SCALE

            Vector2[] uvs2 = m_textInfo.meshInfo[underlineMaterialIndex].uvs2;
            Vector2 customUV = new(0, 1);
            uvs2[index + 0] = customUV;
            uvs2[index + 1] = customUV;
            uvs2[index + 2] = customUV;
            uvs2[index + 3] = customUV;

            #endregion

            #region HIGHLIGHT VERTEX COLORS

            highlightColor.a = m_fontColor32.a < highlightColor.a ? m_fontColor32.a : highlightColor.a;

            Color32[] colors32 = m_textInfo.meshInfo[underlineMaterialIndex].colors32;
            colors32[index + 0] = highlightColor;
            colors32[index + 1] = highlightColor;
            colors32[index + 2] = highlightColor;
            colors32[index + 3] = highlightColor;

            #endregion

            index += 4;
        }


        protected void LoadDefaultSettings()
        {
            if (m_fontSize == -99 || m_isWaitingOnResourceLoad)
            {
                m_rectTransform = rectTransform;

                if (m_rectTransform.sizeDelta == new Vector2(100, 100))
                    m_rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProUITextContainerSize;

                m_TextWrappingMode = TMP_Settings.textWrappingMode;

                m_ActiveFontFeatures = new(TMP_Settings.fontFeatures);

                m_enableExtraPadding = TMP_Settings.enableExtraPadding;
                m_tintAllSprites = TMP_Settings.enableTintAllSprites;
                m_parseCtrlCharacters = TMP_Settings.enableParseEscapeCharacters;
                m_fontSize = m_fontSizeBase = TMP_Settings.defaultFontSize;
                m_fontSizeMin = m_fontSize * TMP_Settings.defaultTextAutoSizingMinRatio;
                m_fontSizeMax = m_fontSize * TMP_Settings.defaultTextAutoSizingMaxRatio;
                m_isWaitingOnResourceLoad = false;
                raycastTarget = TMP_Settings.enableRaycastTarget;
            }
            else
            {
                if ((int)m_textAlignment < 0xFF)
                    m_textAlignment = TMP_Compatibility.ConvertTextAlignmentEnumValues(m_textAlignment);

                if (m_ActiveFontFeatures.Count == 1 && m_ActiveFontFeatures[0] == 0)
                {
                    m_ActiveFontFeatures.Clear();

                    if (m_enableKerning)
                        m_ActiveFontFeatures.Add(OTL_FeatureTag.kern);
                }
            }

            if (m_textAlignment != TextAlignmentOptions.Converted)
            {
                m_HorizontalAlignment = (HorizontalAlignmentOptions)((int)m_textAlignment & 0xFF);
                m_VerticalAlignment = (VerticalAlignmentOptions)((int)m_textAlignment & 0xFF00);
                m_textAlignment = TextAlignmentOptions.Converted;
            }
        }


        protected void GetSpecialCharacters(TMP_FontAsset fontAsset)
        {
            GetEllipsisSpecialCharacter(fontAsset);

            GetUnderlineSpecialCharacter(fontAsset);
        }


        protected void GetEllipsisSpecialCharacter(TMP_FontAsset fontAsset)
        {
            TMP_Character character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(0x2026, fontAsset, false,
                m_FontStyleInternal, m_FontWeightInternal, out _);

            if (character == null)
            {
                if (fontAsset.m_FallbackFontAssetTable != null && fontAsset.m_FallbackFontAssetTable.Count > 0)
                    character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(0x2026, fontAsset,
                        fontAsset.m_FallbackFontAssetTable, true, m_FontStyleInternal, m_FontWeightInternal, out _);
            }

            if (character == null)
            {
                if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
                    character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(0x2026, fontAsset,
                        TMP_Settings.fallbackFontAssets, true, m_FontStyleInternal, m_FontWeightInternal, out _);
            }

            if (character == null)
            {
                if (TMP_Settings.defaultFontAsset != null)
                    character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(0x2026, TMP_Settings.defaultFontAsset,
                        true, m_FontStyleInternal, m_FontWeightInternal, out _);
            }

            if (character != null)
                m_Ellipsis = new(character, 0);
        }

        protected void GetUnderlineSpecialCharacter(TMP_FontAsset fontAsset)
        {
            TMP_Character character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(0x5F, fontAsset, false,
                FontStyles.Normal, FontWeight.Regular, out _);

            if (character != null)
                m_Underline = new(character, 0);
        }

        internal TMP_TextElement GetTextElement(uint unicode, TMP_FontAsset fontAsset, FontStyles fontStyle,
            FontWeight fontWeight, out bool isUsingAlternativeTypeface)
        {
            TMP_Character character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, fontAsset, true,
                fontStyle, fontWeight, out isUsingAlternativeTypeface);

            if (character != null)
            {
                fontAsset.AddCharacterToLookupCache(unicode, character, fontStyle, fontWeight,
                    isUsingAlternativeTypeface);

                return character;
            }

            if (fontAsset.instanceID != m_fontAsset.instanceID)
            {
                character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, m_fontAsset, false, fontStyle,
                    fontWeight, out isUsingAlternativeTypeface);

                if (character != null)
                {
                    fontAsset.AddCharacterToLookupCache(unicode, character, fontStyle, fontWeight,
                        isUsingAlternativeTypeface);

                    return character;
                }

                if (m_fontAsset.m_FallbackFontAssetTable != null && m_fontAsset.m_FallbackFontAssetTable.Count > 0)
                    character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, fontAsset,
                        m_fontAsset.m_FallbackFontAssetTable, true, fontStyle, fontWeight,
                        out isUsingAlternativeTypeface);

                if (character != null)
                {
                    fontAsset.AddCharacterToLookupCache(unicode, character, fontStyle, fontWeight,
                        isUsingAlternativeTypeface);

                    return character;
                }
            }

            if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
                character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, fontAsset,
                    TMP_Settings.fallbackFontAssets, true, fontStyle, fontWeight, out isUsingAlternativeTypeface);

            if (character != null)
            {
                fontAsset.AddCharacterToLookupCache(unicode, character, fontStyle, fontWeight,
                    isUsingAlternativeTypeface);

                return character;
            }

            if (TMP_Settings.defaultFontAsset != null)
                character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, TMP_Settings.defaultFontAsset,
                    true, fontStyle, fontWeight, out isUsingAlternativeTypeface);

            if (character != null)
            {
                fontAsset.AddCharacterToLookupCache(unicode, character, fontStyle, fontWeight,
                    isUsingAlternativeTypeface);

                return character;
            }

            if (fontStyle != FontStyles.Normal || fontWeight != FontWeight.Regular)
            {
                character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, fontAsset, true,
                    FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);

                if (character != null)
                {
                    fontAsset.AddCharacterToLookupCache(unicode, character, FontStyles.Normal, FontWeight.Regular,
                        isUsingAlternativeTypeface);

                    return character;
                }

                if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
                    character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, fontAsset,
                        TMP_Settings.fallbackFontAssets, true, FontStyles.Normal, FontWeight.Regular,
                        out isUsingAlternativeTypeface);

                if (character != null)
                {
                    fontAsset.AddCharacterToLookupCache(unicode, character, FontStyles.Normal, FontWeight.Regular,
                        isUsingAlternativeTypeface);

                    return character;
                }

                if (TMP_Settings.defaultFontAsset != null)
                    character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, TMP_Settings.defaultFontAsset,
                        true, FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);

                if (character != null)
                {
                    fontAsset.AddCharacterToLookupCache(unicode, character, FontStyles.Normal, FontWeight.Regular,
                        isUsingAlternativeTypeface);

                    return character;
                }
            }

            return null;
        }


        public virtual void ClearMesh()
        {
        }


        public virtual void ClearMesh(bool uploadGeometry)
        {
        }

        protected void DoMissingGlyphCallback(int unicode, int stringIndex, TMP_FontAsset fontAsset)
        {
            OnMissingCharacter?.Invoke(unicode, stringIndex, m_text, fontAsset, this);
        }

        internal virtual void InternalUpdate()
        {
        }

        protected uint HexToInt(char hex)
        {
            switch (hex)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': break;
                case 'a': return 10;
                case 'b': return 11;
                case 'c': return 12;
                case 'd': return 13;
                case 'e': return 14;
                case 'f': break;
            }

            return 15;
        }

        private bool IsValidUTF16(TextBackingContainer text, int index)
        {
            for (int i = 0; i < 4; i++)
            {
                uint c = text[index + i];
                if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
                    return false;
            }

            return true;
        }

        private uint GetUTF16(uint[] text, int i)
        {
            uint unicode = 0;
            unicode += HexToInt((char)text[i]) << 12;
            unicode += HexToInt((char)text[i + 1]) << 8;
            unicode += HexToInt((char)text[i + 2]) << 4;
            unicode += HexToInt((char)text[i + 3]);
            return unicode;
        }

        private uint GetUTF16(TextBackingContainer text, int i)
        {
            uint unicode = 0;
            unicode += HexToInt((char)text[i]) << 12;
            unicode += HexToInt((char)text[i + 1]) << 8;
            unicode += HexToInt((char)text[i + 2]) << 4;
            unicode += HexToInt((char)text[i + 3]);
            return unicode;
        }

        private bool IsValidUTF32(TextBackingContainer text, int index)
        {
            for (int i = 0; i < 8; i++)
            {
                uint c = text[index + i];
                if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
                    return false;
            }

            return true;
        }

        private uint GetUTF32(uint[] text, int i)
        {
            uint unicode = 0;
            unicode += HexToInt((char)text[i]) << 28;
            unicode += HexToInt((char)text[i + 1]) << 24;
            unicode += HexToInt((char)text[i + 2]) << 20;
            unicode += HexToInt((char)text[i + 3]) << 16;
            unicode += HexToInt((char)text[i + 4]) << 12;
            unicode += HexToInt((char)text[i + 5]) << 8;
            unicode += HexToInt((char)text[i + 6]) << 4;
            unicode += HexToInt((char)text[i + 7]);
            return unicode;
        }

        private uint GetUTF32(TextBackingContainer text, int i)
        {
            uint unicode = 0;
            unicode += HexToInt((char)text[i]) << 28;
            unicode += HexToInt((char)text[i + 1]) << 24;
            unicode += HexToInt((char)text[i + 2]) << 20;
            unicode += HexToInt((char)text[i + 3]) << 16;
            unicode += HexToInt((char)text[i + 4]) << 12;
            unicode += HexToInt((char)text[i + 5]) << 8;
            unicode += HexToInt((char)text[i + 6]) << 4;
            unicode += HexToInt((char)text[i + 7]);
            return unicode;
        }


        protected Color32 HexCharsToColor(char[] hexChars, int tagCount)
        {
            if (tagCount == 4)
            {
                byte r = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
                byte g = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
                byte b = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));

                return new(r, g, b, 255);
            }

            if (tagCount == 5)
            {
                byte r = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
                byte g = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
                byte b = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
                byte a = (byte)(HexToInt(hexChars[4]) * 16 + HexToInt(hexChars[4]));

                return new(r, g, b, a);
            }

            if (tagCount == 7)
            {
                byte r = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
                byte g = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
                byte b = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));

                return new(r, g, b, 255);
            }

            if (tagCount == 9)
            {
                byte r = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
                byte g = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
                byte b = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
                byte a = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));

                return new(r, g, b, a);
            }

            if (tagCount == 10)
            {
                byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
                byte g = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
                byte b = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));

                return new(r, g, b, 255);
            }

            if (tagCount == 11)
            {
                byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
                byte g = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
                byte b = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
                byte a = (byte)(HexToInt(hexChars[10]) * 16 + HexToInt(hexChars[10]));

                return new(r, g, b, a);
            }

            if (tagCount == 13)
            {
                byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
                byte g = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
                byte b = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));

                return new(r, g, b, 255);
            }

            if (tagCount == 15)
            {
                byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
                byte g = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
                byte b = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
                byte a = (byte)(HexToInt(hexChars[13]) * 16 + HexToInt(hexChars[14]));

                return new(r, g, b, a);
            }

            return new(255, 255, 255, 255);
        }


        protected Color32 HexCharsToColor(char[] hexChars, int startIndex, int length)
        {
            if (length == 7)
            {
                byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
                byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
                byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));

                return new(r, g, b, 255);
            }

            if (length == 9)
            {
                byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
                byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
                byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
                byte a = (byte)(HexToInt(hexChars[startIndex + 7]) * 16 + HexToInt(hexChars[startIndex + 8]));

                return new(r, g, b, a);
            }

            return s_colorWhite;
        }


        private int GetAttributeParameters(char[] chars, int startIndex, int length, ref float[] parameters)
        {
            int endIndex = startIndex;
            int attributeCount = 0;

            while (endIndex < startIndex + length)
            {
                parameters[attributeCount] = ConvertToFloat(chars, startIndex, length, out endIndex);

                length -= (endIndex - startIndex) + 1;
                startIndex = endIndex + 1;

                attributeCount += 1;
            }

            return attributeCount;
        }


        protected float ConvertToFloat(char[] chars, int startIndex, int length)
        {
            return ConvertToFloat(chars, startIndex, length, out _);
        }


        protected float ConvertToFloat(char[] chars, int startIndex, int length, out int lastIndex)
        {
            if (startIndex == 0)
            {
                lastIndex = 0;
                return Int16.MinValue;
            }

            int endIndex = startIndex + length;

            bool isIntegerValue = true;
            float decimalPointMultiplier = 0;

            int valueSignMultiplier = 1;
            if (chars[startIndex] == '+')
            {
                valueSignMultiplier = 1;
                startIndex += 1;
            }
            else if (chars[startIndex] == '-')
            {
                valueSignMultiplier = -1;
                startIndex += 1;
            }

            float value = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                uint c = chars[i];

                if (c >= '0' && c <= '9' || c == '.')
                {
                    if (c == '.')
                    {
                        isIntegerValue = false;
                        decimalPointMultiplier = 0.1f;
                        continue;
                    }

                    if (isIntegerValue)
                        value = value * 10 + (c - 48) * valueSignMultiplier;
                    else
                    {
                        value = value + (c - 48) * decimalPointMultiplier * valueSignMultiplier;
                        decimalPointMultiplier *= 0.1f;
                    }

                    continue;
                }

                if (c == ',')
                {
                    if (i + 1 < endIndex && chars[i + 1] == ' ')
                        lastIndex = i + 1;
                    else
                        lastIndex = i;

                    if (value > 32767)
                        return Int16.MinValue;

                    return value;
                }
            }

            lastIndex = endIndex;

            if (value > 32767)
                return Int16.MinValue;

            return value;
        }

        private void ClearMarkupTagAttributes()
        {
            int length = m_xmlAttribute.Length;
            for (int i = 0; i < length; i++)
                m_xmlAttribute[i] = new();
        }
    }
}