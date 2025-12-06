using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace TMPro
{
    public abstract partial class TMP_Text
    {
        private static ProfilerMarker generateTextMarker = new("TMP.GenerateText");
        private static ProfilerMarker generateTextPhaseIMarker = new("TMP GenerateText - Phase I");
        private static ProfilerMarker parseMarkupTextMarker = new("TMP Parse Markup Text");
        private static ProfilerMarker characterLookupMarker = new("TMP Lookup Character & Glyph Data");
        private static ProfilerMarker handleGposFeaturesMarker = new("TMP Handle GPOS Features");
        private static ProfilerMarker calculateVerticesPositionMarker = new("TMP Calculate Vertices Position");
        private static ProfilerMarker computeTextMetricsMarker = new("TMP Compute Text Metrics");
        private static ProfilerMarker handleVisibleCharacterMarker = new("TMP Handle Visible Character");
        private static ProfilerMarker handleWhiteSpacesMarker = new("TMP Handle White Space & Control Character");
        private static ProfilerMarker handleHorizontalLineBreakingMarker = new("TMP Handle Horizontal Line Breaking");
        private static ProfilerMarker handleVerticalLineBreakingMarker = new("TMP Handle Vertical Line Breaking");
        private static ProfilerMarker saveGlyphVertexDataMarker = new("TMP Save Glyph Vertex Data");
        private static ProfilerMarker computeCharacterAdvanceMarker = new("TMP Compute Character Advance");
        private static ProfilerMarker handleCarriageReturnMarker = new("TMP Handle Carriage Return");
        private static ProfilerMarker handleLineTerminationMarker = new("TMP Handle Line Termination");
        private static ProfilerMarker saveTextExtentMarker = new("TMP Save Text Extent");
        private static ProfilerMarker saveProcessingStatesMarker = new("TMP Save Processing States");
        private static ProfilerMarker generateTextPhaseIiMarker = new("TMP GenerateText - Phase II");
        private static ProfilerMarker generateTextPhaseIiiMarker = new("TMP GenerateText - Phase III");
        private static ProfilerMarker setArraySizesMarker = new("TMP.SetArraySizes");

        protected TMP_SubMeshUI[] subTextObjects = new TMP_SubMeshUI[8];

        protected Vector3[] rectTransformCorners = new Vector3[4];
        protected CanvasRenderer cachedCanvasRenderer;
        protected Canvas cachedCanvas;
        protected float canvasScaleFactor;
        protected bool shouldUpdateCulling;

        private Dictionary<int, int> materialIndexPairs = new();

        public new CanvasRenderer CanvasRenderer
        {
            get
            {
                if (cachedCanvasRenderer == null) cachedCanvasRenderer = GetComponent<CanvasRenderer>();

                return cachedCanvasRenderer;
            }
        }

        protected void OnPreRenderCanvas()
        {
            if (!m_isAwake || (!IsActive() && !m_ignoreActiveState))
                return;

            if (cachedCanvas == null)
            {
                cachedCanvas = canvas;
                if (cachedCanvas == null) return;
            }


            if (_havePropertiesChanged || m_isLayoutDirty)
            {
                if (m_fontAsset == null)
                {
                    Debug.LogWarning("Please assign a Font Asset to this " + transform.name + " gameobject.", this);
                    return;
                }

                if (checkPaddingRequired)
                    UpdateMeshPadding();

                ParseInputText();
                TMP_FontAsset.UpdateFontAssetsInUpdateQueue();

                if (m_enableAutoSizing)
                    m_fontSize = Mathf.Clamp(m_fontSizeBase, m_fontSizeMin, m_fontSizeMax);

                m_maxFontSize = m_fontSizeMax;
                m_minFontSize = m_fontSizeMin;
                m_lineSpacingDelta = 0;
                m_charWidthAdjDelta = 0;

                m_isTextTruncated = false;

                _havePropertiesChanged = false;
                m_isLayoutDirty = false;
                m_ignoreActiveState = false;

                m_IsAutoSizePointSizeSet = false;
                m_AutoSizeIterationCount = 0;

                while (!m_IsAutoSizePointSizeSet)
                {
                    GenerateTextMesh();
                    m_AutoSizeIterationCount += 1;
                }

                SetTextBounds();
            }
        }

        private TextBackingContainer textBackingArray = new(4);

        protected void ParseInputText()
        {
            k_ParseTextMarker.Begin();

            var input = m_text;

            if (m_TextPreprocessor != null)
            {
                input = m_TextPreprocessor.PreprocessText(input);
                PreprocessedText = input;
            }

            PopulateTextArrays(input);
            k_ParseTextMarker.End();
        }

        private void PopulateTextArrays(string input)
        {
            PopulateTextBackingArray(input);
            PopulateTextProcessingArray();
            SetArraySizes(m_TextProcessingArray);
        }

        private void PopulateTextBackingArray(string sourceText)
        {
            int srcLength = sourceText?.Length ?? 0;

            PopulateTextBackingArray(sourceText, 0, srcLength);
        }

        private void PopulateTextBackingArray(string sourceText, int start, int length)
        {
            int readIndex;
            int writeIndex = 0;

            if (sourceText == null)
            {
                readIndex = 0;
                length = 0;
            }
            else
            {
                readIndex = Mathf.Clamp(start, 0, sourceText.Length);
                length = Mathf.Clamp(length, 0,
                    start + length < sourceText.Length ? length : sourceText.Length - start);
            }

            if (length >= textBackingArray.Capacity)
                textBackingArray.Resize((length));

            int end = readIndex + length;
            for (; readIndex < end; readIndex++)
            {
                textBackingArray[writeIndex] = sourceText[readIndex];
                writeIndex += 1;
            }

            textBackingArray[writeIndex] = 0;
            textBackingArray.Count = writeIndex;
        }

        private void PopulateTextProcessingArray()
        {
            TMP_TextProcessingStack<int>.SetDefault(m_TextStyleStacks, 0);

            int srcLength = textBackingArray.Count;
            int requiredCapacity = srcLength + (textStyle.styleOpeningDefinition?.Length ?? 0);
            if (m_TextProcessingArray.Length < requiredCapacity)
                ResizeInternalArray(ref m_TextProcessingArray, requiredCapacity);

            m_TextStyleStackDepth = 0;
            int writeIndex = 0;

            if (textStyle.hashCode != (int)MarkupTag.NORMAL)
                InsertOpeningStyleTag(m_TextStyle, ref m_TextProcessingArray, ref writeIndex);

            tag_NoParsing = false;

            int readIndex = 0;
            for (; readIndex < srcLength; readIndex++)
            {
                uint c = textBackingArray[readIndex];

                if (c == 0)
                    break;

                if (c == '\\' && readIndex < srcLength - 1)
                {
                    switch (textBackingArray[readIndex + 1])
                    {
                        case 92:
                            if (!m_parseCtrlCharacters) break;

                            readIndex += 1;
                            break;
                        case 110:
                            if (!m_parseCtrlCharacters) break;

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 1, unicode = 10
                            };
                            readIndex += 1;
                            writeIndex += 1;
                            continue;
                        case 114:
                            if (!m_parseCtrlCharacters) break;

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 1, unicode = 13
                            };
                            readIndex += 1;
                            writeIndex += 1;
                            continue;
                        case 116:
                            if (!m_parseCtrlCharacters) break;

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 1, unicode = 9
                            };
                            readIndex += 1;
                            writeIndex += 1;
                            continue;
                        case 118:
                            if (!m_parseCtrlCharacters) break;

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 1, unicode = 11
                            };
                            readIndex += 1;
                            writeIndex += 1;
                            continue;
                        case 117:
                            if (srcLength > readIndex + 5 && IsValidUTF16(textBackingArray, readIndex + 2))
                            {
                                m_TextProcessingArray[writeIndex] = new()
                                {
                                    elementType = TextProcessingElementType.TextCharacterElement,
                                    stringIndex = readIndex, length = 6,
                                    unicode = GetUTF16(textBackingArray, readIndex + 2)
                                };
                                readIndex += 5;
                                writeIndex += 1;
                                continue;
                            }

                            break;
                        case 85:
                            if (srcLength > readIndex + 9 && IsValidUTF32(textBackingArray, readIndex + 2))
                            {
                                m_TextProcessingArray[writeIndex] = new()
                                {
                                    elementType = TextProcessingElementType.TextCharacterElement,
                                    stringIndex = readIndex, length = 10,
                                    unicode = GetUTF32(textBackingArray, readIndex + 2)
                                };
                                readIndex += 9;
                                writeIndex += 1;
                                continue;
                            }

                            break;
                    }
                }

                if (c >= CodePoint.HIGH_SURROGATE_START && c <= CodePoint.HIGH_SURROGATE_END &&
                    srcLength > readIndex + 1 && textBackingArray[readIndex + 1] >= CodePoint.LOW_SURROGATE_START &&
                    textBackingArray[readIndex + 1] <= CodePoint.LOW_SURROGATE_END)
                {
                    m_TextProcessingArray[writeIndex] = new()
                    {
                        elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                        length = 2,
                        unicode = TMP_TextParsingUtilities.ConvertToUTF32(c, textBackingArray[readIndex + 1])
                    };
                    readIndex += 1;
                    writeIndex += 1;
                    continue;
                }

                if (c == '<' && m_isRichText)
                {
                    int hashCode = GetMarkupTagHashCode(textBackingArray, readIndex + 1);

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
                            if (writeIndex == m_TextProcessingArray.Length)
                                ResizeInternalArray(ref m_TextProcessingArray);

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 4, unicode = 10
                            };
                            writeIndex += 1;
                            readIndex += 3;
                            continue;
                        case MarkupTag.CR:
                            if (tag_NoParsing) break;
                            if (writeIndex == m_TextProcessingArray.Length)
                                ResizeInternalArray(ref m_TextProcessingArray);

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 4, unicode = 13
                            };
                            writeIndex += 1;
                            readIndex += 3;
                            continue;
                        case MarkupTag.NBSP:
                            if (tag_NoParsing) break;
                            if (writeIndex == m_TextProcessingArray.Length)
                                ResizeInternalArray(ref m_TextProcessingArray);

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 6, unicode = 0xA0
                            };
                            writeIndex += 1;
                            readIndex += 5;
                            continue;
                        case MarkupTag.ZWSP:
                            if (tag_NoParsing) break;
                            if (writeIndex == m_TextProcessingArray.Length)
                                ResizeInternalArray(ref m_TextProcessingArray);

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 6, unicode = 0x200B
                            };
                            writeIndex += 1;
                            readIndex += 5;
                            continue;
                        case MarkupTag.ZWJ:
                            if (tag_NoParsing) break;
                            if (writeIndex == m_TextProcessingArray.Length)
                                ResizeInternalArray(ref m_TextProcessingArray);

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 5, unicode = 0x200D
                            };
                            writeIndex += 1;
                            readIndex += 4;
                            continue;
                        case MarkupTag.SHY:
                            if (tag_NoParsing) break;
                            if (writeIndex == m_TextProcessingArray.Length)
                                ResizeInternalArray(ref m_TextProcessingArray);

                            m_TextProcessingArray[writeIndex] = new()
                            {
                                elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex,
                                length = 5, unicode = 0xAD
                            };
                            writeIndex += 1;
                            readIndex += 4;
                            continue;
                        case MarkupTag.A:
                            if (textBackingArray.Count > readIndex + 4 && textBackingArray[readIndex + 3] == 'h' &&
                                textBackingArray[readIndex + 4] == 'r')
                                InsertOpeningTextStyle(GetStyle((int)MarkupTag.A), ref m_TextProcessingArray,
                                    ref writeIndex);
                            break;
                        case MarkupTag.STYLE:
                            if (tag_NoParsing) break;

                            int openWriteIndex = writeIndex;
                            if (ReplaceOpeningStyleTag(ref textBackingArray, readIndex, out int srcOffset,
                                    ref m_TextProcessingArray, ref writeIndex))
                            {
                                for (; openWriteIndex < writeIndex; openWriteIndex++)
                                {
                                    m_TextProcessingArray[openWriteIndex].stringIndex = readIndex;
                                    m_TextProcessingArray[openWriteIndex].length = (srcOffset - readIndex) + 1;
                                }

                                readIndex = srcOffset;
                                continue;
                            }

                            break;
                        case MarkupTag.SLASH_A:
                            InsertClosingTextStyle(GetStyle((int)MarkupTag.A), ref m_TextProcessingArray,
                                ref writeIndex);
                            break;
                        case MarkupTag.SLASH_STYLE:
                            if (tag_NoParsing) break;

                            int closeWriteIndex = writeIndex;
                            ReplaceClosingStyleTag(ref m_TextProcessingArray, ref writeIndex);

                            for (; closeWriteIndex < writeIndex; closeWriteIndex++)
                            {
                                m_TextProcessingArray[closeWriteIndex].stringIndex = readIndex;
                                m_TextProcessingArray[closeWriteIndex].length = 8;
                            }

                            readIndex += 7;
                            continue;
                    }
                }

                if (writeIndex == m_TextProcessingArray.Length) ResizeInternalArray(ref m_TextProcessingArray);

                m_TextProcessingArray[writeIndex] = new()
                {
                    elementType = TextProcessingElementType.TextCharacterElement, stringIndex = readIndex, length = 1,
                    unicode = c
                };

                writeIndex += 1;
            }

            m_TextStyleStackDepth = 0;

            if (textStyle.hashCode != (int)MarkupTag.NORMAL)
                InsertClosingStyleTag(ref m_TextProcessingArray, ref writeIndex);

            if (writeIndex == m_TextProcessingArray.Length) ResizeInternalArray(ref m_TextProcessingArray);

            m_TextProcessingArray[writeIndex].unicode = 0;
            m_InternalTextProcessingArraySize = writeIndex;
        }

        protected void GenerateTextMesh()
        {
            generateTextMarker.Begin();

            if (m_fontAsset == null || m_fontAsset.characterLookupTable == null)
            {
                Debug.LogWarning(
                    "Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + GetInstanceID());
                m_IsAutoSizePointSizeSet = true;
                generateTextMarker.End();
                return;
            }

            if (m_textInfo != null)
                m_textInfo.Clear();

            if (m_TextProcessingArray == null || m_TextProcessingArray.Length == 0 ||
                m_TextProcessingArray[0].unicode == 0)
            {
                ClearMesh();

                TMPro_EventManager.ON_TEXT_CHANGED(this);
                m_IsAutoSizePointSizeSet = true;
                generateTextMarker.End();
                return;
            }

            m_currentFontAsset = m_fontAsset;
            m_currentMaterial = m_sharedMaterial;
            m_currentMaterialIndex = 0;
            m_materialReferenceStack.SetDefault(new(m_currentMaterialIndex, m_currentFontAsset, m_currentMaterial,
                m_padding));

            int totalCharacterCount = m_totalCharacterCount;

            float baseScale = (m_fontSize / m_fontAsset.m_FaceInfo.pointSize * m_fontAsset.m_FaceInfo.scale *
                               (m_isOrthographic ? 1 : 0.1f));
            float currentElementScale = baseScale;
            float currentEmScale = m_fontSize * 0.01f * (m_isOrthographic ? 1 : 0.1f);
            m_fontScaleMultiplier = 1;

            m_currentFontSize = m_fontSize;
            m_sizeStack.SetDefault(m_currentFontSize);
            float fontSizeDelta = 0;

            uint charCode = 0;

            m_FontStyleInternal = m_fontStyle;
            m_FontWeightInternal = (m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold
                ? FontWeight.Bold
                : m_fontWeight;
            m_FontWeightStack.SetDefault(m_FontWeightInternal);
            m_fontStyleStack.Clear();

            m_lineJustification = m_HorizontalAlignment;
            m_lineJustificationStack.SetDefault(m_lineJustification);

            float padding = 0;

            m_baselineOffset = 0;
            m_baselineOffsetStack.Clear();

            bool beginUnderline = false;
            Vector3 underlineStart = Vector3.zero;
            Vector3 underlineEnd = Vector3.zero;

            bool beginStrikethrough = false;
            Vector3 strikethroughStart = Vector3.zero;
            Vector3 strikethroughEnd = Vector3.zero;

            bool beginHighlight = false;
            Vector3 highlightStart = Vector3.zero;
            Vector3 highlightEnd = Vector3.zero;

            m_fontColor32 = m_fontColor;
            m_htmlColor = m_fontColor32;
            m_underlineColor = m_htmlColor;
            m_strikethroughColor = m_htmlColor;

            m_colorStack.SetDefault(m_htmlColor);
            m_underlineColorStack.SetDefault(m_htmlColor);
            m_strikethroughColorStack.SetDefault(m_htmlColor);
            m_HighlightStateStack.SetDefault(new(m_htmlColor, TMP_Offset.zero));

            m_colorGradientPreset = null;
            m_colorGradientStack.SetDefault(null);

            m_ItalicAngle = m_currentFontAsset.italicStyle;
            m_ItalicAngleStack.SetDefault(m_ItalicAngle);

            m_actionStack.Clear();

            m_FXScale = Vector3.one;
            m_FXRotation = Quaternion.identity;

            m_lineOffset = 0;
            m_lineHeight = TMP_Math.FLOAT_UNSET;
            float lineGap = m_currentFontAsset.m_FaceInfo.lineHeight -
                            (m_currentFontAsset.m_FaceInfo.ascentLine - m_currentFontAsset.m_FaceInfo.descentLine);

            m_cSpacing = 0;
            m_monoSpacing = 0;
            m_xAdvance = 0;

            tag_LineIndent = 0;
            tag_Indent = 0;
            m_indentStack.SetDefault(0);
            tag_NoParsing = false;

            m_characterCount = 0;

            m_firstCharacterOfLine = m_firstVisibleCharacter;
            m_lastCharacterOfLine = 0;
            m_firstVisibleCharacterOfLine = 0;
            m_lastVisibleCharacterOfLine = 0;
            m_maxLineAscender = largeNegativeFloat;
            m_maxLineDescender = largePositiveFloat;
            m_lineNumber = 0;
            m_startOfLineAscender = 0;
            m_startOfLineDescender = 0;
            m_lineVisibleCharacterCount = 0;
            m_lineVisibleSpaceCount = 0;
            bool isStartOfNewLine = true;
            m_IsDrivenLineSpacing = false;
            m_firstOverflowCharacterIndex = -1;
            m_LastBaseGlyphIndex = int.MinValue;

            bool kerning = m_ActiveFontFeatures.Contains(OTL_FeatureTag.kern);
            bool markToBase = m_ActiveFontFeatures.Contains(OTL_FeatureTag.mark);
            bool markToMark = m_ActiveFontFeatures.Contains(OTL_FeatureTag.mkmk);

            Vector4 margins = m_margin;
            float marginWidth = m_marginWidth > 0 ? m_marginWidth : 0;
            float marginHeight = m_marginHeight > 0 ? m_marginHeight : 0;
            m_marginLeft = 0;
            m_marginRight = 0;
            m_width = -1;
            float widthOfTextArea = marginWidth + 0.0001f;

            m_meshExtents.min = largePositiveVector2;
            m_meshExtents.max = largeNegativeVector2;

            m_textInfo.ClearLineInfo();

            m_maxCapHeight = 0;
            m_maxTextAscender = 0;
            m_ElementDescender = 0;
            float maxVisibleDescender = 0;
            bool isMaxVisibleDescenderSet = false;

            bool isFirstWordOfLine = true;
            m_isNonBreakingSpace = false;
            bool ignoreNonBreakingSpace = false;
            int lastSoftLineBreak = 0;

            CharacterSubstitution characterToSubstitute = new(-1, 0);
            bool isSoftHyphenIgnored = false;

            SaveWordWrappingState(ref m_SavedWordWrapState, -1, -1);
            SaveWordWrappingState(ref m_SavedLineState, -1, -1);
            SaveWordWrappingState(ref m_SavedEllipsisState, -1, -1);
            SaveWordWrappingState(ref m_SavedLastValidState, -1, -1);
            SaveWordWrappingState(ref m_SavedSoftLineBreakState, -1, -1);

            m_EllipsisInsertionCandidateStack.Clear();

            int restoreCount = 0;

            generateTextPhaseIMarker.Begin();

            for (int i = 0; i < m_TextProcessingArray.Length && m_TextProcessingArray[i].unicode != 0; i++)
            {
                charCode = m_TextProcessingArray[i].unicode;

                if (restoreCount > 5)
                {
                    Debug.LogError("Line breaking recursion max threshold hit... Character [" + charCode + "] index: " +
                                   i);
                    characterToSubstitute.index = m_characterCount;
                    characterToSubstitute.unicode = 0x03;
                }

                if (charCode == 0x1A)
                    continue;

                #region Parse Rich Text Tag

                ref var chInfo = ref m_textInfo.characterInfo[m_characterCount];
                if (m_isRichText && charCode == '<')
                {
                    parseMarkupTextMarker.Begin();

                    m_isTextLayoutPhase = true;

                    if (ValidateHtmlTag(m_TextProcessingArray, i + 1, out var endTagIndex))
                    {
                        i = endTagIndex;
                        parseMarkupTextMarker.End();
                        continue;
                    }

                    parseMarkupTextMarker.End();
                }
                else
                {
                    m_currentMaterialIndex = chInfo.materialReferenceIndex;
                    m_currentFontAsset = chInfo.fontAsset;
                }

                #endregion End Parse Rich Text Tag

                bool isUsingAltTypeface = chInfo.isUsingAlternateTypeface;

                m_isTextLayoutPhase = false;

                #region Character Substitutions

                bool isInjectedCharacter = false;

                if (characterToSubstitute.index == m_characterCount)
                {
                    charCode = characterToSubstitute.unicode;
                    isInjectedCharacter = true;

                    switch (charCode)
                    {
                        case 0x03:
                            chInfo.textElement = m_currentFontAsset.characterLookupTable[0x03];
                            m_isTextTruncated = true;
                            break;
                        case 0x2D:
                            break;
                        case 0x2026:
                            chInfo.textElement = m_Ellipsis.character;
                            chInfo.fontAsset = m_Ellipsis.fontAsset;
                            chInfo.material = m_Ellipsis.material;
                            chInfo.materialReferenceIndex = m_Ellipsis.materialIndex;

                            m_materialReferences[m_Underline.materialIndex].referenceCount += 1;

                            m_isTextTruncated = true;

                            characterToSubstitute.index = m_characterCount + 1;
                            characterToSubstitute.unicode = 0x03;
                            break;
                    }
                }

                #endregion


                #region Linked Text

                if (m_characterCount < m_firstVisibleCharacter && charCode != 0x03)
                {
                    chInfo.isVisible = false;
                    chInfo.character = (char)0x200B;
                    chInfo.lineNumber = 0;
                    m_characterCount += 1;
                    continue;
                }

                #endregion


                #region Handling of LowerCase, UpperCase and SmallCaps Font Styles

                float smallCapsMultiplier = 1.0f;

                if ((m_FontStyleInternal & FontStyles.UpperCase) == FontStyles.UpperCase)
                {
                    if (char.IsLower((char)charCode))
                        charCode = char.ToUpper((char)charCode);
                }
                else if ((m_FontStyleInternal & FontStyles.LowerCase) == FontStyles.LowerCase)
                {
                    if (char.IsUpper((char)charCode))
                        charCode = char.ToLower((char)charCode);
                }
                else if ((m_FontStyleInternal & FontStyles.SmallCaps) == FontStyles.SmallCaps)
                {
                    if (char.IsLower((char)charCode))
                    {
                        smallCapsMultiplier = 0.8f;
                        charCode = char.ToUpper((char)charCode);
                    }
                }

                #endregion


                #region Look up Character Data

                characterLookupMarker.Begin();

                float baselineOffset = 0;
                float elementAscentLine = 0;
                float elementDescentLine = 0;

                m_cached_TextElement = chInfo.textElement;
                if (m_cached_TextElement == null)
                {
                    characterLookupMarker.End();
                    continue;
                }

                m_currentFontAsset = chInfo.fontAsset;
                m_currentMaterial = chInfo.material;
                m_currentMaterialIndex = chInfo.materialReferenceIndex;

                float adjustedScale;
                if (isInjectedCharacter && m_TextProcessingArray[i].unicode == 0x0A &&
                    m_characterCount != m_firstCharacterOfLine)
                    adjustedScale = m_textInfo.characterInfo[m_characterCount - 1].pointSize * smallCapsMultiplier /
                                    m_currentFontAsset.m_FaceInfo.pointSize * m_currentFontAsset.m_FaceInfo.scale *
                                    (m_isOrthographic ? 1 : 0.1f);
                else
                    adjustedScale = m_currentFontSize * smallCapsMultiplier / m_currentFontAsset.m_FaceInfo.pointSize *
                                    m_currentFontAsset.m_FaceInfo.scale * (m_isOrthographic ? 1 : 0.1f);

                if (isInjectedCharacter && charCode == 0x2026)
                {
                    elementAscentLine = 0;
                    elementDescentLine = 0;
                }
                else
                {
                    elementAscentLine = m_currentFontAsset.m_FaceInfo.ascentLine;
                    elementDescentLine = m_currentFontAsset.m_FaceInfo.descentLine;
                }

                currentElementScale = adjustedScale * m_fontScaleMultiplier * m_cached_TextElement.m_Scale *
                                      m_cached_TextElement.m_Glyph.scale;
                baselineOffset = m_currentFontAsset.m_FaceInfo.baseline * adjustedScale * m_fontScaleMultiplier *
                                 m_currentFontAsset.m_FaceInfo.scale;

                chInfo.scale = currentElementScale;

                padding = m_currentMaterialIndex == 0 ? m_padding : subTextObjects[m_currentMaterialIndex].padding;

                characterLookupMarker.End();

                #endregion


                #region Handle Soft Hyphen

                float currentElementUnmodifiedScale = currentElementScale;
                if (charCode == 0xAD || charCode == 0x03)
                    currentElementScale = 0;

                #endregion


                chInfo.character = (char)charCode;
                chInfo.pointSize = m_currentFontSize;
                chInfo.underlineColor = m_underlineColor;
                chInfo.strikethroughColor = m_strikethroughColor;
                chInfo.highlightState = m_HighlightState;
                chInfo.style = m_FontStyleInternal;

                Glyph altGlyph = chInfo.alternativeGlyph;
                GlyphMetrics currentGlyphMetrics =
                    altGlyph == null ? m_cached_TextElement.m_Glyph.metrics : altGlyph.metrics;

                bool isWhiteSpace = charCode <= 0xFFFF && char.IsWhiteSpace((char)charCode);

                #region Handle Kerning

                GlyphValueRecord glyphAdjustments = new();
                float characterSpacingAdjustment = m_characterSpacing;
                if (kerning)
                {
                    handleGposFeaturesMarker.Begin();

                    GlyphPairAdjustmentRecord adjustmentPair;
                    uint baseGlyphIndex = m_cached_TextElement.m_GlyphIndex;

                    if (m_characterCount < totalCharacterCount - 1)
                    {
                        uint nextGlyphIndex = m_textInfo.characterInfo[m_characterCount + 1].textElement.m_GlyphIndex;
                        uint key = nextGlyphIndex << 16 | baseGlyphIndex;

                        if (m_currentFontAsset.m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.TryGetValue(key,
                                out adjustmentPair))
                        {
                            glyphAdjustments = adjustmentPair.firstAdjustmentRecord.glyphValueRecord;
                            characterSpacingAdjustment =
                                (adjustmentPair.featureLookupFlags & UnityEngine.TextCore.LowLevel
                                    .FontFeatureLookupFlags.IgnoreSpacingAdjustments) ==
                                UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments
                                    ? 0
                                    : characterSpacingAdjustment;
                        }
                    }

                    if (m_characterCount >= 1)
                    {
                        uint previousGlyphIndex =
                            m_textInfo.characterInfo[m_characterCount - 1].textElement.m_GlyphIndex;
                        uint key = baseGlyphIndex << 16 | previousGlyphIndex;

                        if (m_currentFontAsset.m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.TryGetValue(key,
                                out adjustmentPair))
                        {
                            glyphAdjustments += adjustmentPair.secondAdjustmentRecord.glyphValueRecord;
                            characterSpacingAdjustment =
                                (adjustmentPair.featureLookupFlags & UnityEngine.TextCore.LowLevel
                                    .FontFeatureLookupFlags.IgnoreSpacingAdjustments) ==
                                UnityEngine.TextCore.LowLevel.FontFeatureLookupFlags.IgnoreSpacingAdjustments
                                    ? 0
                                    : characterSpacingAdjustment;
                        }
                    }

                    handleGposFeaturesMarker.End();
                }

                chInfo.adjustedHorizontalAdvance = glyphAdjustments.xAdvance;

                #endregion


                #region Handle Diacritical Marks

                bool isBaseGlyph = TMP_TextParsingUtilities.IsBaseGlyph(charCode);

                if (isBaseGlyph)
                    m_LastBaseGlyphIndex = m_characterCount;

                if (m_characterCount > 0 && !isBaseGlyph)
                {
                    if (markToBase && m_LastBaseGlyphIndex != int.MinValue &&
                        m_LastBaseGlyphIndex == m_characterCount - 1)
                    {
                        Glyph baseGlyph = m_textInfo.characterInfo[m_LastBaseGlyphIndex].textElement.glyph;
                        uint baseGlyphIndex = baseGlyph.index;
                        uint markGlyphIndex = m_cached_TextElement.glyphIndex;
                        uint key = markGlyphIndex << 16 | baseGlyphIndex;

                        if (m_currentFontAsset.fontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.TryGetValue(key,
                                out MarkToBaseAdjustmentRecord glyphAdjustmentRecord))
                        {
                            float advanceOffset = (m_textInfo.characterInfo[m_LastBaseGlyphIndex].origin - m_xAdvance) /
                                                  currentElementScale;

                            glyphAdjustments.xPlacement = advanceOffset +
                                                          glyphAdjustmentRecord.baseGlyphAnchorPoint.xCoordinate -
                                                          glyphAdjustmentRecord.markPositionAdjustment
                                                              .xPositionAdjustment;
                            glyphAdjustments.yPlacement = glyphAdjustmentRecord.baseGlyphAnchorPoint.yCoordinate -
                                                          glyphAdjustmentRecord.markPositionAdjustment
                                                              .yPositionAdjustment;

                            characterSpacingAdjustment = 0;
                        }
                    }
                    else
                    {
                        bool wasLookupApplied = false;

                        if (markToMark)
                        {
                            for (int characterLookupIndex = m_characterCount - 1;
                                 characterLookupIndex >= 0 && characterLookupIndex != m_LastBaseGlyphIndex;
                                 characterLookupIndex--)
                            {
                                Glyph baseMarkGlyph = m_textInfo.characterInfo[characterLookupIndex].textElement.glyph;
                                uint baseGlyphIndex = baseMarkGlyph.index;
                                uint combiningMarkGlyphIndex = m_cached_TextElement.glyphIndex;
                                uint key = combiningMarkGlyphIndex << 16 | baseGlyphIndex;

                                if (m_currentFontAsset.fontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.TryGetValue(
                                        key, out MarkToMarkAdjustmentRecord glyphAdjustmentRecord))
                                {
                                    float baseMarkOrigin =
                                        (m_textInfo.characterInfo[characterLookupIndex].origin - m_xAdvance) /
                                        currentElementScale;
                                    float currentBaseline = baselineOffset - m_lineOffset + m_baselineOffset;
                                    float baseMarkBaseline =
                                        (m_textInfo.characterInfo[characterLookupIndex].baseLine - currentBaseline) /
                                        currentElementScale;

                                    glyphAdjustments.xPlacement = baseMarkOrigin +
                                                                  glyphAdjustmentRecord.baseMarkGlyphAnchorPoint
                                                                      .xCoordinate -
                                                                  glyphAdjustmentRecord.combiningMarkPositionAdjustment
                                                                      .xPositionAdjustment;
                                    glyphAdjustments.yPlacement = baseMarkBaseline +
                                                                  glyphAdjustmentRecord.baseMarkGlyphAnchorPoint
                                                                      .yCoordinate -
                                                                  glyphAdjustmentRecord.combiningMarkPositionAdjustment
                                                                      .yPositionAdjustment;

                                    characterSpacingAdjustment = 0;
                                    wasLookupApplied = true;
                                    break;
                                }
                            }
                        }

                        if (markToBase && m_LastBaseGlyphIndex != int.MinValue && !wasLookupApplied)
                        {
                            Glyph baseGlyph = m_textInfo.characterInfo[m_LastBaseGlyphIndex].textElement.glyph;
                            uint baseGlyphIndex = baseGlyph.index;
                            uint markGlyphIndex = m_cached_TextElement.glyphIndex;
                            uint key = markGlyphIndex << 16 | baseGlyphIndex;

                            if (m_currentFontAsset.fontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.TryGetValue(key,
                                    out MarkToBaseAdjustmentRecord glyphAdjustmentRecord))
                            {
                                float advanceOffset =
                                    (m_textInfo.characterInfo[m_LastBaseGlyphIndex].origin - m_xAdvance) /
                                    currentElementScale;

                                glyphAdjustments.xPlacement = advanceOffset +
                                    glyphAdjustmentRecord.baseGlyphAnchorPoint.xCoordinate - glyphAdjustmentRecord
                                        .markPositionAdjustment.xPositionAdjustment;
                                glyphAdjustments.yPlacement = glyphAdjustmentRecord.baseGlyphAnchorPoint.yCoordinate -
                                                              glyphAdjustmentRecord.markPositionAdjustment
                                                                  .yPositionAdjustment;

                                characterSpacingAdjustment = 0;
                            }
                        }
                    }
                }

                elementAscentLine += glyphAdjustments.yPlacement;
                elementDescentLine += glyphAdjustments.yPlacement;

                #endregion

                float xAdvanceBeforeChar = m_xAdvance;


                #region Handle Mono Spacing

                float monoAdvance = 0;
                if (m_monoSpacing != 0)
                {
                    if (m_duoSpace && (charCode == '.' || charCode == ':' || charCode == ','))
                        monoAdvance =
                            (m_monoSpacing / 4 -
                             (currentGlyphMetrics.width / 2 + currentGlyphMetrics.horizontalBearingX) *
                             currentElementScale) * (1 - m_charWidthAdjDelta);
                    else
                        monoAdvance =
                            (m_monoSpacing / 2 -
                             (currentGlyphMetrics.width / 2 + currentGlyphMetrics.horizontalBearingX) *
                             currentElementScale) * (1 - m_charWidthAdjDelta);

                    m_xAdvance += monoAdvance;
                }

                #endregion


                #region Handle Style Padding

                float boldSpacingAdjustment;
                float stylePadding;
                if (!isUsingAltTypeface && ((m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold))
                {
                    if (m_currentMaterial != null && m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
                    {
                        float gradientScale = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
                        stylePadding = m_currentFontAsset.boldStyle / 4.0f * gradientScale *
                                       m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                        if (stylePadding + padding > gradientScale)
                            padding = gradientScale - stylePadding;
                    }
                    else
                        stylePadding = 0;

                    boldSpacingAdjustment = m_currentFontAsset.boldSpacing;
                }
                else
                {
                    if (m_currentMaterial != null && m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale) &&
                        m_currentMaterial.HasProperty(ShaderUtilities.ID_ScaleRatio_A))
                    {
                        float gradientScale = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
                        stylePadding = m_currentFontAsset.normalStyle / 4.0f * gradientScale *
                                       m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                        if (stylePadding + padding > gradientScale)
                            padding = gradientScale - stylePadding;
                    }
                    else
                        stylePadding = 0;

                    boldSpacingAdjustment = 0;
                }

                #endregion Handle Style Padding


                #region Calculate Vertices Position

                calculateVerticesPositionMarker.Begin();
                Vector3 topLeft;
                topLeft.x = m_xAdvance +
                            ((currentGlyphMetrics.horizontalBearingX * m_FXScale.x - padding - stylePadding +
                              glyphAdjustments.xPlacement) * currentElementScale * (1 - m_charWidthAdjDelta));
                topLeft.y = baselineOffset +
                    (currentGlyphMetrics.horizontalBearingY + padding + glyphAdjustments.yPlacement) *
                    currentElementScale - m_lineOffset + m_baselineOffset;
                topLeft.z = 0;

                Vector3 bottomLeft;
                bottomLeft.x = topLeft.x;
                bottomLeft.y = topLeft.y - ((currentGlyphMetrics.height + padding * 2) * currentElementScale);
                bottomLeft.z = 0;

                Vector3 topRight;
                topRight.x = bottomLeft.x +
                             ((currentGlyphMetrics.width * m_FXScale.x + padding * 2 + stylePadding * 2) *
                              currentElementScale * (1 - m_charWidthAdjDelta));
                topRight.y = topLeft.y;
                topRight.z = 0;

                Vector3 bottomRight;
                bottomRight.x = topRight.x;
                bottomRight.y = bottomLeft.y;
                bottomRight.z = 0;

                calculateVerticesPositionMarker.End();

                #endregion


                #region Handle Italic & Shearing

                if (!isUsingAltTypeface && ((m_FontStyleInternal & FontStyles.Italic) == FontStyles.Italic))
                {
                    float shearValue = m_ItalicAngle * 0.01f;
                    float midPoint =
                        ((m_currentFontAsset.m_FaceInfo.capLine -
                          (m_currentFontAsset.m_FaceInfo.baseline + m_baselineOffset)) / 2) * m_fontScaleMultiplier *
                        m_currentFontAsset.m_FaceInfo.scale;
                    Vector3 topShear =
                        new(
                            shearValue * ((currentGlyphMetrics.horizontalBearingY + padding + stylePadding - midPoint) *
                                          currentElementScale), 0, 0);
                    Vector3 bottomShear =
                        new(
                            shearValue *
                            (((currentGlyphMetrics.horizontalBearingY - currentGlyphMetrics.height - padding -
                               stylePadding - midPoint)) * currentElementScale), 0, 0);

                    topLeft += topShear;
                    bottomLeft += bottomShear;
                    topRight += topShear;
                    bottomRight += bottomShear;
                }

                #endregion Handle Italics & Shearing


                #region Handle Character FX Rotation

                if (m_FXRotation != Quaternion.identity)
                {
                    Matrix4x4 rotationMatrix = Matrix4x4.Rotate(m_FXRotation);
                    Vector3 positionOffset = (topRight + bottomLeft) / 2;

                    topLeft = rotationMatrix.MultiplyPoint3x4(topLeft - positionOffset) + positionOffset;
                    bottomLeft = rotationMatrix.MultiplyPoint3x4(bottomLeft - positionOffset) + positionOffset;
                    topRight = rotationMatrix.MultiplyPoint3x4(topRight - positionOffset) + positionOffset;
                    bottomRight = rotationMatrix.MultiplyPoint3x4(bottomRight - positionOffset) + positionOffset;
                }

                #endregion


                chInfo.bottomLeft = bottomLeft;
                chInfo.topLeft = topLeft;
                chInfo.topRight = topRight;
                chInfo.bottomRight = bottomRight;

                chInfo.origin = m_xAdvance + glyphAdjustments.xPlacement * currentElementScale;
                chInfo.baseLine = (baselineOffset - m_lineOffset + m_baselineOffset) +
                                  glyphAdjustments.yPlacement * currentElementScale;
                chInfo.aspectRatio = (topRight.x - bottomLeft.x) / (topLeft.y - bottomLeft.y);

                #region Compute Ascender & Descender values

                computeTextMetricsMarker.Begin();
                float elementAscender =
                    elementAscentLine * currentElementScale / smallCapsMultiplier + m_baselineOffset;
                float elementDescender =
                    elementDescentLine * currentElementScale / smallCapsMultiplier + m_baselineOffset;

                float adjustedAscender = elementAscender;
                float adjustedDescender = elementDescender;

                bool isFirstCharacterOfLine = m_characterCount == m_firstCharacterOfLine;
                if (isFirstCharacterOfLine || !isWhiteSpace)
                {
                    if (m_baselineOffset != 0)
                    {
                        adjustedAscender = Mathf.Max((elementAscender - m_baselineOffset) / m_fontScaleMultiplier,
                            adjustedAscender);
                        adjustedDescender = Mathf.Min((elementDescender - m_baselineOffset) / m_fontScaleMultiplier,
                            adjustedDescender);
                    }

                    m_maxLineAscender = Mathf.Max(adjustedAscender, m_maxLineAscender);
                    m_maxLineDescender = Mathf.Min(adjustedDescender, m_maxLineDescender);
                }

                if (isFirstCharacterOfLine || !isWhiteSpace)
                {
                    chInfo.adjustedAscender = adjustedAscender;
                    chInfo.adjustedDescender = adjustedDescender;

                    m_ElementAscender = chInfo.ascender = elementAscender - m_lineOffset;
                    m_ElementDescender = chInfo.descender = elementDescender - m_lineOffset;
                }
                else
                {
                    chInfo.adjustedAscender = m_maxLineAscender;
                    chInfo.adjustedDescender = m_maxLineDescender;

                    m_ElementAscender = chInfo.ascender = m_maxLineAscender - m_lineOffset;
                    m_ElementDescender = chInfo.descender = m_maxLineDescender - m_lineOffset;
                }

                if (m_lineNumber == 0)
                {
                    if (isFirstCharacterOfLine || !isWhiteSpace)
                    {
                        m_maxTextAscender = m_maxLineAscender;
                        m_maxCapHeight = Mathf.Max(m_maxCapHeight,
                            m_currentFontAsset.m_FaceInfo.capLine * currentElementScale / smallCapsMultiplier);
                    }
                }

                computeTextMetricsMarker.End();

                #endregion


                chInfo.isVisible = false;

                bool isJustifiedOrFlush =
                    (m_lineJustification & HorizontalAlignmentOptions.Flush) == HorizontalAlignmentOptions.Flush ||
                    (m_lineJustification & HorizontalAlignmentOptions.Justified) ==
                    HorizontalAlignmentOptions.Justified;

                #region Handle Visible Characters

                ref var lineInfo = ref m_textInfo.lineInfo[m_lineNumber];
                if (charCode == 9
                    || ((m_TextWrappingMode == TextWrappingModes.PreserveWhitespace
                         || m_TextWrappingMode == TextWrappingModes.PreserveWhitespaceNoWrap)
                        && (isWhiteSpace || charCode == 0x200B))
                    || (!isWhiteSpace && charCode != 0x200B && charCode != 0xAD && charCode != 0x03)
                    || (charCode == 0xAD && !isSoftHyphenIgnored))
                {
                    handleVisibleCharacterMarker.Begin();

                    chInfo.isVisible = true;

                    float marginLeft = m_marginLeft;
                    float marginRight = m_marginRight;

                    if (isInjectedCharacter)
                    {
                        marginLeft = lineInfo.marginLeft;
                        marginRight = lineInfo.marginRight;
                    }

                    widthOfTextArea = m_width != -1
                        ? Mathf.Min(marginWidth + 0.0001f - marginLeft - marginRight, m_width)
                        : marginWidth + 0.0001f - marginLeft - marginRight;

                    float textWidth = Mathf.Abs(m_xAdvance) + (currentGlyphMetrics.horizontalAdvance) *
                        (1 - m_charWidthAdjDelta) *
                        (charCode == 0xAD ? currentElementUnmodifiedScale : currentElementScale);
                    float textHeight = m_maxTextAscender - (m_maxLineDescender - m_lineOffset) +
                                       (m_lineOffset > 0 && !m_IsDrivenLineSpacing
                                           ? m_maxLineAscender - m_startOfLineAscender
                                           : 0);

                    int testedCharacterCount = m_characterCount;

                    #region Current Line Vertical Bounds Check

                    if (textHeight > marginHeight + 0.0001f)
                    {
                        handleVerticalLineBreakingMarker.Begin();

                        if (m_firstOverflowCharacterIndex == -1)
                            m_firstOverflowCharacterIndex = m_characterCount;

                        if (m_enableAutoSizing)
                        {
                            #region Line Spacing Adjustments

                            if (m_lineSpacingDelta > m_lineSpacingMax && m_lineOffset > 0 &&
                                m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                            {
                                float adjustmentDelta = (marginHeight - textHeight) / m_lineNumber;

                                m_lineSpacingDelta = Mathf.Max(m_lineSpacingDelta + adjustmentDelta / baseScale,
                                    m_lineSpacingMax);

                                handleVerticalLineBreakingMarker.End();
                                handleVisibleCharacterMarker.End();
                                generateTextPhaseIMarker.End();
                                generateTextMarker.End();
                                return;
                            }

                            #endregion


                            #region Text Auto-Sizing (Text greater than vertical bounds)

                            if (m_fontSize > m_fontSizeMin && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                            {
                                m_maxFontSize = m_fontSize;

                                float sizeDelta = Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                                m_fontSize -= sizeDelta;
                                m_fontSize = Mathf.Max((int)(m_fontSize * 20 + 0.5f) / 20f, m_fontSizeMin);

                                handleVerticalLineBreakingMarker.End();
                                handleVisibleCharacterMarker.End();
                                generateTextPhaseIMarker.End();
                                generateTextMarker.End();
                                return;
                            }

                            #endregion Text Auto-Sizing
                        }

                        switch (m_overflowMode)
                        {
                            case TextOverflowModes.Overflow:
                                break;

                            case TextOverflowModes.Truncate:
                                i = RestoreWordWrappingState(ref m_SavedLastValidState);

                                characterToSubstitute.index = testedCharacterCount;
                                characterToSubstitute.unicode = 0x03;
                                handleVerticalLineBreakingMarker.End();
                                handleVisibleCharacterMarker.End();
                                continue;

                            case TextOverflowModes.Ellipsis:
                                if (m_EllipsisInsertionCandidateStack.Count == 0)
                                {
                                    i = -1;
                                    m_characterCount = 0;
                                    characterToSubstitute.index = 0;
                                    characterToSubstitute.unicode = 0x03;
                                    m_firstCharacterOfLine = 0;
                                    handleVerticalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    continue;
                                }

                                var ellipsisState = m_EllipsisInsertionCandidateStack.Pop();
                                i = RestoreWordWrappingState(ref ellipsisState);

                                i -= 1;
                                m_characterCount -= 1;
                                characterToSubstitute.index = m_characterCount;
                                characterToSubstitute.unicode = 0x2026;

                                restoreCount += 1;
                                handleVerticalLineBreakingMarker.End();
                                handleVisibleCharacterMarker.End();
                                continue;
                        }

                        handleVerticalLineBreakingMarker.End();
                    }

                    #endregion


                    #region Current Line Horizontal Bounds Check

                    if (isBaseGlyph && textWidth > widthOfTextArea * (isJustifiedOrFlush ? 1.05f : 1.0f))
                    {
                        handleHorizontalLineBreakingMarker.Begin();

                        if (m_TextWrappingMode != TextWrappingModes.NoWrap &&
                            m_TextWrappingMode != TextWrappingModes.PreserveWhitespaceNoWrap &&
                            m_characterCount != m_firstCharacterOfLine)
                        {
                            i = RestoreWordWrappingState(ref m_SavedWordWrapState);

                            float lineOffsetDelta = 0;
                            if (m_lineHeight == TMP_Math.FLOAT_UNSET)
                            {
                                float ascender = chInfo.adjustedAscender;
                                lineOffsetDelta =
                                    (m_lineOffset > 0 && !m_IsDrivenLineSpacing
                                        ? m_maxLineAscender - m_startOfLineAscender
                                        : 0) - m_maxLineDescender + ascender +
                                    (lineGap + m_lineSpacingDelta) * baseScale + m_lineSpacing * currentEmScale;
                            }
                            else
                            {
                                lineOffsetDelta = m_lineHeight + m_lineSpacing * currentEmScale;
                                m_IsDrivenLineSpacing = true;
                            }

                            float newTextHeight = m_maxTextAscender + lineOffsetDelta + m_lineOffset -
                                                  chInfo.adjustedDescender;

                            #region Handle Soft Hyphenation

                            if (m_textInfo.characterInfo[m_characterCount - 1].character == 0xAD &&
                                !isSoftHyphenIgnored)
                            {
                                if (m_overflowMode == TextOverflowModes.Overflow ||
                                    newTextHeight < marginHeight + 0.0001f)
                                {
                                    characterToSubstitute.index = m_characterCount - 1;
                                    characterToSubstitute.unicode = 0x2D;

                                    i -= 1;
                                    m_characterCount -= 1;
                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    continue;
                                }
                            }

                            isSoftHyphenIgnored = false;

                            if (chInfo.character == 0xAD)
                            {
                                isSoftHyphenIgnored = true;
                                handleHorizontalLineBreakingMarker.End();
                                handleVisibleCharacterMarker.End();
                                continue;
                            }

                            #endregion

                            if (m_enableAutoSizing && isFirstWordOfLine)
                            {
                                #region Character Width Adjustments

                                if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100 &&
                                    m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                                {
                                    float adjustedTextWidth = textWidth;

                                    if (m_charWidthAdjDelta > 0)
                                        adjustedTextWidth /= 1f - m_charWidthAdjDelta;

                                    float adjustmentDelta = textWidth - (widthOfTextArea - 0.0001f) *
                                        (isJustifiedOrFlush ? 1.05f : 1.0f);
                                    m_charWidthAdjDelta += adjustmentDelta / adjustedTextWidth;
                                    m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100);

                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    generateTextPhaseIMarker.End();
                                    generateTextMarker.End();
                                    return;
                                }

                                #endregion

                                #region Text Auto-Sizing (Text greater than vertical bounds)

                                if (m_fontSize > m_fontSizeMin &&
                                    m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                                {
                                    m_maxFontSize = m_fontSize;

                                    float sizeDelta = Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                                    m_fontSize -= sizeDelta;
                                    m_fontSize = Mathf.Max((int)(m_fontSize * 20 + 0.5f) / 20f, m_fontSizeMin);

                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    generateTextPhaseIMarker.End();
                                    generateTextMarker.End();
                                    return;
                                }

                                #endregion Text Auto-Sizing
                            }


                            int savedSoftLineBreakingSpace = m_SavedSoftLineBreakState.previous_WordBreak;
                            if (isFirstWordOfLine && savedSoftLineBreakingSpace != -1)
                            {
                                if (savedSoftLineBreakingSpace != lastSoftLineBreak)
                                {
                                    i = RestoreWordWrappingState(ref m_SavedSoftLineBreakState);
                                    lastSoftLineBreak = savedSoftLineBreakingSpace;

                                    if (m_textInfo.characterInfo[m_characterCount - 1].character == 0xAD)
                                    {
                                        characterToSubstitute.index = m_characterCount - 1;
                                        characterToSubstitute.unicode = 0x2D;

                                        i -= 1;
                                        m_characterCount -= 1;
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        continue;
                                    }
                                }
                            }

                            if (newTextHeight > marginHeight + 0.0001f)
                            {
                                handleVerticalLineBreakingMarker.Begin();

                                if (m_firstOverflowCharacterIndex == -1)
                                    m_firstOverflowCharacterIndex = m_characterCount;

                                if (m_enableAutoSizing)
                                {
                                    #region Line Spacing Adjustments

                                    if (m_lineSpacingDelta > m_lineSpacingMax &&
                                        m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                                    {
                                        float adjustmentDelta = (marginHeight - newTextHeight) / (m_lineNumber + 1);

                                        m_lineSpacingDelta = Mathf.Max(m_lineSpacingDelta + adjustmentDelta / baseScale,
                                            m_lineSpacingMax);

                                        handleVerticalLineBreakingMarker.End();
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        generateTextPhaseIMarker.End();
                                        generateTextMarker.End();
                                        return;
                                    }

                                    #endregion

                                    #region Character Width Adjustments

                                    if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100 &&
                                        m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                                    {
                                        float adjustedTextWidth = textWidth;

                                        if (m_charWidthAdjDelta > 0)
                                            adjustedTextWidth /= 1f - m_charWidthAdjDelta;

                                        float adjustmentDelta = textWidth - (widthOfTextArea - 0.0001f) *
                                            (isJustifiedOrFlush ? 1.05f : 1.0f);
                                        m_charWidthAdjDelta += adjustmentDelta / adjustedTextWidth;
                                        m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100);

                                        handleVerticalLineBreakingMarker.End();
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        generateTextPhaseIMarker.End();
                                        generateTextMarker.End();
                                        return;
                                    }

                                    #endregion

                                    #region Text Auto-Sizing (Text greater than vertical bounds)

                                    if (m_fontSize > m_fontSizeMin &&
                                        m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                                    {
                                        m_maxFontSize = m_fontSize;

                                        float sizeDelta = Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                                        m_fontSize -= sizeDelta;
                                        m_fontSize = Mathf.Max((int)(m_fontSize * 20 + 0.5f) / 20f, m_fontSizeMin);

                                        handleVerticalLineBreakingMarker.End();
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        generateTextPhaseIMarker.End();
                                        generateTextMarker.End();
                                        return;
                                    }

                                    #endregion Text Auto-Sizing
                                }

                                switch (m_overflowMode)
                                {
                                    case TextOverflowModes.Overflow:
                                        InsertNewLine(i, baseScale, currentElementScale, currentEmScale,
                                            boldSpacingAdjustment, characterSpacingAdjustment, widthOfTextArea, lineGap,
                                            ref isMaxVisibleDescenderSet, ref maxVisibleDescender);
                                        isStartOfNewLine = true;
                                        isFirstWordOfLine = true;
                                        handleVerticalLineBreakingMarker.End();
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        continue;

                                    case TextOverflowModes.Truncate:
                                        i = RestoreWordWrappingState(ref m_SavedLastValidState);

                                        characterToSubstitute.index = testedCharacterCount;
                                        characterToSubstitute.unicode = 0x03;
                                        handleVerticalLineBreakingMarker.End();
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        continue;

                                    case TextOverflowModes.Ellipsis:
                                        if (m_EllipsisInsertionCandidateStack.Count == 0)
                                        {
                                            i = -1;
                                            m_characterCount = 0;
                                            characterToSubstitute.index = 0;
                                            characterToSubstitute.unicode = 0x03;
                                            m_firstCharacterOfLine = 0;
                                            handleVerticalLineBreakingMarker.End();
                                            handleHorizontalLineBreakingMarker.End();
                                            handleVisibleCharacterMarker.End();
                                            continue;
                                        }

                                        var ellipsisState = m_EllipsisInsertionCandidateStack.Pop();
                                        i = RestoreWordWrappingState(ref ellipsisState);

                                        i -= 1;
                                        m_characterCount -= 1;
                                        characterToSubstitute.index = m_characterCount;
                                        characterToSubstitute.unicode = 0x2026;

                                        restoreCount += 1;
                                        handleVerticalLineBreakingMarker.End();
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        continue;
                                }
                            }
                            else
                            {
                                InsertNewLine(i, baseScale, currentElementScale, currentEmScale, boldSpacingAdjustment,
                                    characterSpacingAdjustment, widthOfTextArea, lineGap, ref isMaxVisibleDescenderSet,
                                    ref maxVisibleDescender);
                                isStartOfNewLine = true;
                                isFirstWordOfLine = true;
                                handleHorizontalLineBreakingMarker.End();
                                handleVisibleCharacterMarker.End();
                                continue;
                            }
                        }
                        else
                        {
                            if (m_enableAutoSizing && m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
                            {
                                #region Character Width Adjustments

                                if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100)
                                {
                                    float adjustedTextWidth = textWidth;

                                    if (m_charWidthAdjDelta > 0)
                                        adjustedTextWidth /= 1f - m_charWidthAdjDelta;

                                    float adjustmentDelta = textWidth - (widthOfTextArea - 0.0001f) *
                                        (isJustifiedOrFlush ? 1.05f : 1.0f);
                                    m_charWidthAdjDelta += adjustmentDelta / adjustedTextWidth;
                                    m_charWidthAdjDelta = Mathf.Min(m_charWidthAdjDelta, m_charWidthMaxAdj / 100);

                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    generateTextPhaseIMarker.End();
                                    generateTextMarker.End();
                                    return;
                                }

                                #endregion

                                #region Text Exceeds Horizontal Bounds - Reducing Point Size

                                if (m_fontSize > m_fontSizeMin)
                                {
                                    m_maxFontSize = m_fontSize;

                                    float sizeDelta = Mathf.Max((m_fontSize - m_minFontSize) / 2, 0.05f);
                                    m_fontSize -= sizeDelta;
                                    m_fontSize = Mathf.Max((int)(m_fontSize * 20 + 0.5f) / 20f, m_fontSizeMin);

                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    generateTextPhaseIMarker.End();
                                    generateTextMarker.End();
                                    return;
                                }

                                #endregion
                            }

                            switch (m_overflowMode)
                            {
                                case TextOverflowModes.Overflow:
                                    break;

                                case TextOverflowModes.Truncate:
                                    i = RestoreWordWrappingState(ref m_SavedWordWrapState);

                                    characterToSubstitute.index = testedCharacterCount;
                                    characterToSubstitute.unicode = 0x03;
                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    continue;

                                case TextOverflowModes.Ellipsis:
                                    if (m_EllipsisInsertionCandidateStack.Count == 0)
                                    {
                                        i = -1;
                                        m_characterCount = 0;
                                        characterToSubstitute.index = 0;
                                        characterToSubstitute.unicode = 0x03;
                                        m_firstCharacterOfLine = 0;
                                        handleHorizontalLineBreakingMarker.End();
                                        handleVisibleCharacterMarker.End();
                                        continue;
                                    }

                                    var ellipsisState = m_EllipsisInsertionCandidateStack.Pop();
                                    i = RestoreWordWrappingState(ref ellipsisState);

                                    i -= 1;
                                    m_characterCount -= 1;
                                    characterToSubstitute.index = m_characterCount;
                                    characterToSubstitute.unicode = 0x2026;

                                    restoreCount += 1;
                                    handleHorizontalLineBreakingMarker.End();
                                    handleVisibleCharacterMarker.End();
                                    continue;
                            }
                        }

                        handleHorizontalLineBreakingMarker.End();
                    }

                    #endregion


                    if (isWhiteSpace)
                    {
                        chInfo.isVisible = false;
                        m_lastVisibleCharacterOfLine = m_characterCount;
                        m_lineVisibleSpaceCount = lineInfo.spaceCount += 1;
                        lineInfo.marginLeft = marginLeft;
                        lineInfo.marginRight = marginRight;
                        m_textInfo.spaceCount += 1;

                        if (charCode == 0xA0)
                            lineInfo.controlCharacterCount += 1;
                    }
                    else if (charCode == 0xAD)
                    {
                        chInfo.isVisible = false;
                    }
                    else
                    {
                        Color32 vertexColor;
                        if (m_overrideHtmlColors)
                            vertexColor = m_fontColor32;
                        else
                            vertexColor = m_htmlColor;

                        saveGlyphVertexDataMarker.Begin();
                        SaveGlyphVertexInfo(padding, stylePadding, vertexColor);
                        saveGlyphVertexDataMarker.End();

                        if (isStartOfNewLine)
                        {
                            isStartOfNewLine = false;
                            m_firstVisibleCharacterOfLine = m_characterCount;
                        }

                        m_lineVisibleCharacterCount += 1;
                        m_lastVisibleCharacterOfLine = m_characterCount;
                        lineInfo.marginLeft = marginLeft;
                        lineInfo.marginRight = marginRight;
                    }

                    handleVisibleCharacterMarker.End();
                }
                else
                {
                    handleWhiteSpacesMarker.Begin();

                    if ((charCode == 10 || charCode == 11 || charCode == 0xA0 || charCode == 0x2007 ||
                         charCode == 0x2028 || charCode == 0x2029 || char.IsSeparator((char)charCode)) &&
                        charCode != 0xAD && charCode != 0x200B && charCode != 0x2060)
                    {
                        lineInfo.spaceCount += 1;
                        m_textInfo.spaceCount += 1;
                    }

                    if (charCode == 0xA0)
                        lineInfo.controlCharacterCount += 1;

                    handleWhiteSpacesMarker.End();
                }

                #endregion Handle Visible Characters


                #region Track Potential Insertion Location for Ellipsis

                if (m_overflowMode == TextOverflowModes.Ellipsis && (!isInjectedCharacter || charCode == 0x2D))
                {
                    float fontScale = m_currentFontSize / m_Ellipsis.fontAsset.m_FaceInfo.pointSize *
                                      m_Ellipsis.fontAsset.m_FaceInfo.scale * (m_isOrthographic ? 1 : 0.1f);
                    float scale = fontScale * m_fontScaleMultiplier * m_Ellipsis.character.m_Scale *
                                  m_Ellipsis.character.m_Glyph.scale;
                    float marginLeft = m_marginLeft;
                    float marginRight = m_marginRight;

                    if (charCode == 0x0A && m_characterCount != m_firstCharacterOfLine)
                    {
                        fontScale = m_textInfo.characterInfo[m_characterCount - 1].pointSize /
                                    m_Ellipsis.fontAsset.m_FaceInfo.pointSize * m_Ellipsis.fontAsset.m_FaceInfo.scale *
                                    (m_isOrthographic ? 1 : 0.1f);
                        scale = fontScale * m_fontScaleMultiplier * m_Ellipsis.character.m_Scale *
                                m_Ellipsis.character.m_Glyph.scale;
                        marginLeft = lineInfo.marginLeft;
                        marginRight = lineInfo.marginRight;
                    }

                    float textHeight = m_maxTextAscender - (m_maxLineDescender - m_lineOffset) +
                                       (m_lineOffset > 0 && !m_IsDrivenLineSpacing
                                           ? m_maxLineAscender - m_startOfLineAscender
                                           : 0);
                    float textWidth = Mathf.Abs(m_xAdvance) + (m_Ellipsis.character.m_Glyph.metrics.horizontalAdvance) *
                        (1 - m_charWidthAdjDelta) * scale;
                    float widthOfTextAreaForEllipsis = m_width != -1
                        ? Mathf.Min(marginWidth + 0.0001f - marginLeft - marginRight, m_width)
                        : marginWidth + 0.0001f - marginLeft - marginRight;

                    if (textWidth < widthOfTextAreaForEllipsis * (isJustifiedOrFlush ? 1.05f : 1.0f) &&
                        textHeight < marginHeight + 0.0001f)
                    {
                        SaveWordWrappingState(ref m_SavedEllipsisState, i, m_characterCount);
                        m_EllipsisInsertionCandidateStack.Push(m_SavedEllipsisState);
                    }
                }

                #endregion


                #region Store Character Data

                chInfo.lineNumber = m_lineNumber;

                if (charCode != 10 && charCode != 11 && charCode != 13 && !isInjectedCharacter ||
                    lineInfo.characterCount == 1)
                    lineInfo.alignment = m_lineJustification;

                #endregion Store Character Data


                #region XAdvance, Tabulation & Stops

                computeCharacterAdvanceMarker.Begin();
                if (charCode == 9)
                {
                    float tabSize = m_currentFontAsset.m_FaceInfo.tabWidth * m_currentFontAsset.tabSize *
                                    currentElementScale;
                    float tabs = Mathf.Ceil(m_xAdvance / tabSize) * tabSize;
                    m_xAdvance = tabs > m_xAdvance ? tabs : m_xAdvance + tabSize;
                }
                else if (m_monoSpacing != 0)
                {
                    float monoAdjustment;
                    if (m_duoSpace && (charCode == '.' || charCode == ':' || charCode == ','))
                        monoAdjustment = m_monoSpacing / 2 - monoAdvance;
                    else
                        monoAdjustment = m_monoSpacing - monoAdvance;

                    m_xAdvance +=
                        (monoAdjustment +
                         ((m_currentFontAsset.normalSpacingOffset + characterSpacingAdjustment) * currentEmScale) +
                         m_cSpacing) * (1 - m_charWidthAdjDelta);

                    if (isWhiteSpace || charCode == 0x200B)
                        m_xAdvance += m_wordSpacing * currentEmScale;
                }
                else
                {
                    m_xAdvance +=
                        ((currentGlyphMetrics.horizontalAdvance * m_FXScale.x + glyphAdjustments.xAdvance) *
                         currentElementScale +
                         (m_currentFontAsset.normalSpacingOffset + characterSpacingAdjustment +
                          boldSpacingAdjustment) * currentEmScale + m_cSpacing) * (1 - m_charWidthAdjDelta);

                    if (isWhiteSpace || charCode == 0x200B)
                        m_xAdvance += m_wordSpacing * currentEmScale;
                }

                chInfo.xAdvance = m_xAdvance;
                computeCharacterAdvanceMarker.End();

                #endregion Tabulation & Stops

                float glyphAdvance = m_xAdvance - xAdvanceBeforeChar;

                if (glyphAdvance < 0f)
                {
                    glyphAdvance = -glyphAdvance;
                }

                if (charCode == 10 || charCode == 11 || charCode == 13 || charCode == 0x2028 || charCode == 0x2029)
                {
                    glyphAdvance = 0f;
                }

                chInfo.glyphAdvance = glyphAdvance;

                #region Carriage Return

                if (charCode == 13)
                {
                    handleCarriageReturnMarker.Begin();
                    m_xAdvance = 0 + tag_Indent;
                    handleCarriageReturnMarker.End();
                }

                #endregion Carriage Return


                #region Check for Line Feed and Last Character

                if (charCode == 10 || charCode == 11 || charCode == 0x03 || charCode == 0x2028 || charCode == 0x2029 ||
                    (charCode == 0x2D && isInjectedCharacter) || m_characterCount == totalCharacterCount - 1)
                {
                    handleLineTerminationMarker.Begin();

                    float baselineAdjustmentDelta = m_maxLineAscender - m_startOfLineAscender;
                    if (m_lineOffset > 0 && Math.Abs(baselineAdjustmentDelta) > 0.01f && !m_IsDrivenLineSpacing)
                    {
                        AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, baselineAdjustmentDelta);
                        m_ElementDescender -= baselineAdjustmentDelta;
                        m_lineOffset += baselineAdjustmentDelta;

                        if (m_SavedEllipsisState.lineNumber == m_lineNumber)
                        {
                            m_SavedEllipsisState = m_EllipsisInsertionCandidateStack.Pop();
                            m_SavedEllipsisState.startOfLineAscender += baselineAdjustmentDelta;
                            m_SavedEllipsisState.lineOffset += baselineAdjustmentDelta;
                            m_EllipsisInsertionCandidateStack.Push(m_SavedEllipsisState);
                        }
                    }

                    float lineAscender = m_maxLineAscender - m_lineOffset;
                    float lineDescender = m_maxLineDescender - m_lineOffset;

                    m_ElementDescender = m_ElementDescender < lineDescender ? m_ElementDescender : lineDescender;
                    if (!isMaxVisibleDescenderSet)
                        maxVisibleDescender = m_ElementDescender;

                    if (m_useMaxVisibleDescender &&
                        (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
                        isMaxVisibleDescenderSet = true;

                    lineInfo.firstCharacterIndex = m_firstCharacterOfLine;
                    lineInfo.firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine =
                        m_firstCharacterOfLine > m_firstVisibleCharacterOfLine
                            ? m_firstCharacterOfLine
                            : m_firstVisibleCharacterOfLine;
                    lineInfo.lastCharacterIndex = m_lastCharacterOfLine = m_characterCount;
                    lineInfo.lastVisibleCharacterIndex = m_lastVisibleCharacterOfLine =
                        m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine
                            ? m_firstVisibleCharacterOfLine
                            : m_lastVisibleCharacterOfLine;

                    lineInfo.characterCount = lineInfo.lastCharacterIndex - lineInfo.firstCharacterIndex + 1;
                    lineInfo.visibleCharacterCount = m_lineVisibleCharacterCount;
                    lineInfo.visibleSpaceCount =
                        (lineInfo.lastVisibleCharacterIndex + 1 - lineInfo.firstCharacterIndex) -
                        m_lineVisibleCharacterCount;
                    lineInfo.lineExtents.min = new(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x,
                        lineDescender);
                    lineInfo.lineExtents.max = new(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x,
                        lineAscender);
                    lineInfo.length = lineInfo.lineExtents.max.x - (padding * currentElementScale);
                    lineInfo.width = widthOfTextArea;

                    if (lineInfo.characterCount == 1)
                        lineInfo.alignment = m_lineJustification;

                    float maxAdvanceOffset =
                        ((m_currentFontAsset.normalSpacingOffset + characterSpacingAdjustment + boldSpacingAdjustment) *
                            currentEmScale + m_cSpacing) * (1 - m_charWidthAdjDelta);
                    if (m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].isVisible)
                        lineInfo.maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance +
                                              (-maxAdvanceOffset);
                    else
                        lineInfo.maxAdvance = m_textInfo.characterInfo[m_lastCharacterOfLine].xAdvance +
                                              (-maxAdvanceOffset);

                    lineInfo.baseline = 0 - m_lineOffset;
                    lineInfo.ascender = lineAscender;
                    lineInfo.descender = lineDescender;
                    lineInfo.lineHeight = lineAscender - lineDescender + lineGap * baseScale;

                    if (charCode == 10 || charCode == 11 || (charCode == 0x2D && isInjectedCharacter) ||
                        charCode == 0x2028 || charCode == 0x2029)
                    {
                        SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount);

                        m_lineNumber += 1;
                        isStartOfNewLine = true;
                        ignoreNonBreakingSpace = false;
                        isFirstWordOfLine = true;

                        m_firstCharacterOfLine = m_characterCount + 1;
                        m_lineVisibleCharacterCount = 0;
                        m_lineVisibleSpaceCount = 0;

                        if (m_lineNumber >= m_textInfo.lineInfo.Length)
                            ResizeLineExtents(m_lineNumber);

                        float lastVisibleAscender = chInfo.adjustedAscender;

                        if (m_lineHeight == TMP_Math.FLOAT_UNSET)
                        {
                            float lineOffsetDelta = 0 - m_maxLineDescender + lastVisibleAscender +
                                                    (lineGap + m_lineSpacingDelta) * baseScale +
                                                    (m_lineSpacing + (charCode == 10 || charCode == 0x2029
                                                        ? m_paragraphSpacing
                                                        : 0)) * currentEmScale;
                            m_lineOffset += lineOffsetDelta;
                            m_IsDrivenLineSpacing = false;
                        }
                        else
                        {
                            m_lineOffset += m_lineHeight +
                                            (m_lineSpacing + (charCode == 10 || charCode == 0x2029
                                                ? m_paragraphSpacing
                                                : 0)) * currentEmScale;
                            m_IsDrivenLineSpacing = true;
                        }

                        m_maxLineAscender = largeNegativeFloat;
                        m_maxLineDescender = largePositiveFloat;
                        m_startOfLineAscender = lastVisibleAscender;

                        m_xAdvance = 0 + tag_LineIndent + tag_Indent;

                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
                        SaveWordWrappingState(ref m_SavedLastValidState, i, m_characterCount);

                        m_characterCount += 1;

                        handleLineTerminationMarker.End();

                        continue;
                    }

                    if (charCode == 0x03)
                        i = m_TextProcessingArray.Length;

                    handleLineTerminationMarker.End();
                }

                #endregion Check for Linefeed or Last Character


                #region Track Text Extents

                saveTextExtentMarker.Begin();
                if (chInfo.isVisible)
                {
                    m_meshExtents.min.x = Mathf.Min(m_meshExtents.min.x, chInfo.bottomLeft.x);
                    m_meshExtents.min.y = Mathf.Min(m_meshExtents.min.y, chInfo.bottomLeft.y);

                    m_meshExtents.max.x = Mathf.Max(m_meshExtents.max.x, chInfo.topRight.x);
                    m_meshExtents.max.y = Mathf.Max(m_meshExtents.max.y, chInfo.topRight.y);
                }

                saveTextExtentMarker.End();

                #endregion Track Text Extents


                #region Save Word Wrapping State

                if ((m_TextWrappingMode != TextWrappingModes.NoWrap &&
                     m_TextWrappingMode != TextWrappingModes.PreserveWhitespaceNoWrap) ||
                    m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis)
                {
                    saveProcessingStatesMarker.Begin();

                    bool shouldSaveHardLineBreak = false;
                    bool shouldSaveSoftLineBreak = false;

                    if ((isWhiteSpace || charCode == 0x200B || charCode == 0x2D || charCode == 0xAD) &&
                        (!m_isNonBreakingSpace || ignoreNonBreakingSpace) && charCode != 0xA0 && charCode != 0x2007 &&
                        charCode != 0x2011 && charCode != 0x202F && charCode != 0x2060)
                    {
                        if (!(charCode == 0x2D && m_characterCount > 0 &&
                              char.IsWhiteSpace(m_textInfo.characterInfo[m_characterCount - 1].character) &&
                              m_textInfo.characterInfo[m_characterCount - 1].lineNumber == m_lineNumber))
                        {
                            isFirstWordOfLine = false;
                            shouldSaveHardLineBreak = true;

                            m_SavedSoftLineBreakState.previous_WordBreak = -1;
                        }
                    }
                    else if (!m_isNonBreakingSpace &&
                             (TMP_TextParsingUtilities.IsHangul(charCode) &&
                              !TMP_Settings.useModernHangulLineBreakingRules ||
                              TMP_TextParsingUtilities.IsCJK(charCode)))
                    {
                        bool isCurrentLeadingCharacter =
                            TMP_Settings.linebreakingRules.leadingCharacters.Contains(charCode);
                        bool isNextFollowingCharacter = m_characterCount < totalCharacterCount - 1 &&
                                                        TMP_Settings.linebreakingRules.followingCharacters.Contains(
                                                            m_textInfo.characterInfo[m_characterCount + 1].character);

                        if (!isCurrentLeadingCharacter)
                        {
                            if (!isNextFollowingCharacter)
                            {
                                isFirstWordOfLine = false;
                                shouldSaveHardLineBreak = true;
                            }

                            if (isFirstWordOfLine)
                            {
                                if (isWhiteSpace)
                                    shouldSaveSoftLineBreak = true;

                                shouldSaveHardLineBreak = true;
                            }
                        }
                        else
                        {
                            if (isFirstWordOfLine && isFirstCharacterOfLine)
                            {
                                if (isWhiteSpace)
                                    shouldSaveSoftLineBreak = true;

                                shouldSaveHardLineBreak = true;
                            }
                        }
                    }
                    else if (!m_isNonBreakingSpace && m_characterCount + 1 < totalCharacterCount &&
                             TMP_TextParsingUtilities.IsCJK(m_textInfo.characterInfo[m_characterCount + 1].character))
                    {
                        shouldSaveHardLineBreak = true;
                    }
                    else if (isFirstWordOfLine)
                    {
                        if (isWhiteSpace && charCode != 0xA0 || (charCode == 0xAD && !isSoftHyphenIgnored))
                            shouldSaveSoftLineBreak = true;

                        shouldSaveHardLineBreak = true;
                    }

                    if (shouldSaveHardLineBreak)
                        SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);

                    if (shouldSaveSoftLineBreak)
                        SaveWordWrappingState(ref m_SavedSoftLineBreakState, i, m_characterCount);

                    saveProcessingStatesMarker.End();
                }

                #endregion Save Word Wrapping State

                SaveWordWrappingState(ref m_SavedLastValidState, i, m_characterCount);

                m_characterCount += 1;
            }

            #region Check Auto-Sizing (Upper Font Size Bounds)

            fontSizeDelta = m_maxFontSize - m_minFontSize;
            if (m_enableAutoSizing && fontSizeDelta > 0.051f && m_fontSize < m_fontSizeMax &&
                m_AutoSizeIterationCount < m_AutoSizeMaxIterationCount)
            {
                if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100)
                    m_charWidthAdjDelta = 0;

                m_minFontSize = m_fontSize;

                float sizeDelta = Mathf.Max((m_maxFontSize - m_fontSize) / 2, 0.05f);
                m_fontSize += sizeDelta;
                m_fontSize = Mathf.Min((int)(m_fontSize * 20 + 0.5f) / 20f, m_fontSizeMax);

                generateTextPhaseIMarker.End();
                generateTextMarker.End();
                return;
            }

            #endregion End Auto-sizing Check

            m_IsAutoSizePointSizeSet = true;

            if (m_AutoSizeIterationCount >= m_AutoSizeMaxIterationCount)
                Debug.Log(
                    "Auto Size Iteration Count: " + m_AutoSizeIterationCount + ". Final Point Size: " + m_fontSize);

            if (m_characterCount == 0 || (m_characterCount == 1 && charCode == 0x03))
            {
                ClearMesh();

                TMPro_EventManager.ON_TEXT_CHANGED(this);
                generateTextPhaseIMarker.End();
                generateTextMarker.End();
                return;
            }

            generateTextPhaseIMarker.End();

            generateTextPhaseIiMarker.Begin();
            int lastVertIndex = m_materialReferences[m_Underline.materialIndex].referenceCount * 4;

            m_textInfo.meshInfo[0].Clear(false);

            #region Text Vertical Alignment

            Vector3 anchorOffset = Vector3.zero;
            Vector3[] corners = rectTransformCorners;

            switch (m_VerticalAlignment)
            {
                case VerticalAlignmentOptions.Top:
                    anchorOffset = corners[1] + new Vector3(0 + margins.x, 0 - m_maxTextAscender - margins.y, 0);
                    break;

                case VerticalAlignmentOptions.Middle:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x,
                        0 - (m_maxTextAscender + margins.y + maxVisibleDescender - margins.w) / 2, 0);
                    break;

                case VerticalAlignmentOptions.Bottom:
                    anchorOffset = corners[0] + new Vector3(0 + margins.x, 0 - maxVisibleDescender + margins.w, 0);
                    break;

                case VerticalAlignmentOptions.Baseline:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x, 0, 0);
                    break;

                case VerticalAlignmentOptions.Geometry:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x,
                        0 - (m_meshExtents.max.y + margins.y + m_meshExtents.min.y - margins.w) / 2, 0);
                    break;

                case VerticalAlignmentOptions.Capline:
                    anchorOffset = (corners[0] + corners[1]) / 2 + new Vector3(0 + margins.x,
                        0 - (m_maxCapHeight - margins.y - margins.w) / 2, 0);
                    break;
            }

            #endregion

            Vector3 justificationOffset = Vector3.zero;
            Vector3 offset = Vector3.zero;

            int wordCount = 0;
            int lineCount = 0;
            int lastLine = 0;
            bool isFirstSeperator = false;

            bool isStartOfWord = false;
            int wordFirstChar = 0;
            int wordLastChar = 0;

            bool isCameraAssigned = cachedCanvas.worldCamera == null ? false : true;
            float lossyScale = transform.lossyScale.y;
            RenderMode canvasRenderMode = cachedCanvas.renderMode;
            float canvasScaleFactor = cachedCanvas.scaleFactor;

            Color32 underlineColor = Color.white;
            Color32 strikethroughColor = Color.white;
            HighlightState highlightState = new(new(255, 255, 0, 64), TMP_Offset.zero);
            float xScale = 0;
            float xScaleMax = 0;
            float underlineStartScale = 0;
            float underlineEndScale = 0;
            float underlineMaxScale = 0;
            float underlineBaseLine = largePositiveFloat;

            float strikethroughPointSize = 0;
            float strikethroughScale = 0;
            float strikethroughBaseline = 0;
            int lastLineNumber = -1;

            TMP_CharacterInfo[] characterInfos = m_textInfo.characterInfo;

            #region Handle Line Justification & UV Mapping & Character Visibility & More

            for (int i = 0; i < m_characterCount; i++)
            {
                ref var chInfo = ref characterInfos[i];
                ref var bl = ref chInfo.vertex_BL;
                ref var tl = ref chInfo.vertex_TL;
                ref var tr = ref chInfo.vertex_TR;
                ref var br = ref chInfo.vertex_BR;
                TMP_FontAsset currentFontAsset = chInfo.fontAsset;

                char unicode = chInfo.character;
                bool isWhiteSpace = char.IsWhiteSpace(unicode);

                int currentLine = chInfo.lineNumber;
                ref var currLineInfo = ref m_textInfo.lineInfo[currentLine];
                TMP_LineInfo lineInfo = currLineInfo;
                lineCount = currentLine + 1;

                #region Handle Line Justification

                if (lastLineNumber != currentLine)
                {
                    lastLineNumber = currentLine;
                    HorizontalAlignmentOptions lineAlignment = lineInfo.alignment;

                    switch (lineAlignment)
                    {
                        case HorizontalAlignmentOptions.Left:
                            justificationOffset = new(0 + lineInfo.marginLeft, 0, 0);
                            break;

                        case HorizontalAlignmentOptions.Center:
                            justificationOffset =
                                new(lineInfo.marginLeft + lineInfo.width / 2 - lineInfo.maxAdvance / 2, 0, 0);
                            break;

                        case HorizontalAlignmentOptions.Geometry:
                            justificationOffset =
                                new(
                                    lineInfo.marginLeft + lineInfo.width / 2 -
                                    (lineInfo.lineExtents.min.x + lineInfo.lineExtents.max.x) / 2, 0, 0);
                            break;

                        case HorizontalAlignmentOptions.Right:
                            justificationOffset = new(lineInfo.marginLeft + lineInfo.width - lineInfo.maxAdvance, 0, 0);
                            break;

                        case HorizontalAlignmentOptions.Justified:
                        case HorizontalAlignmentOptions.Flush:
                            if (i > lineInfo.lastVisibleCharacterIndex || unicode == 0x0A || unicode == 0xAD ||
                                unicode == 0x200B || unicode == 0x2060 || unicode == 0x03) break;

                            char lastCharOfCurrentLine = characterInfos[lineInfo.lastCharacterIndex].character;

                            bool isFlush = (lineAlignment & HorizontalAlignmentOptions.Flush) ==
                                           HorizontalAlignmentOptions.Flush;

                            if (char.IsControl(lastCharOfCurrentLine) == false && currentLine < m_lineNumber ||
                                isFlush || lineInfo.maxAdvance > lineInfo.width)
                            {
                                if (currentLine != lastLine || i == 0 || i == m_firstVisibleCharacter)
                                {
                                    justificationOffset = new(lineInfo.marginLeft, 0, 0);

                                    if (char.IsSeparator(unicode))
                                        isFirstSeperator = true;
                                    else
                                        isFirstSeperator = false;
                                }
                                else
                                {
                                    float gap = lineInfo.width - lineInfo.maxAdvance;
                                    int visibleCount = lineInfo.visibleCharacterCount - 1 +
                                                       lineInfo.controlCharacterCount;
                                    int spaces = lineInfo.visibleSpaceCount - lineInfo.controlCharacterCount;

                                    if (isFirstSeperator)
                                    {
                                        spaces -= 1;
                                        visibleCount += 1;
                                    }

                                    float ratio = spaces > 0 ? m_wordWrappingRatios : 1;

                                    if (spaces < 1) spaces = 1;

                                    if (unicode != 0xA0 && (unicode == 9 || char.IsSeparator(unicode)))
                                    {
                                        justificationOffset += new Vector3(gap * (1 - ratio) / spaces, 0, 0);
                                    }
                                    else
                                    {
                                        justificationOffset += new Vector3(gap * ratio / visibleCount, 0, 0);
                                    }
                                }
                            }
                            else
                            {
                                justificationOffset = new(lineInfo.marginLeft, 0, 0);
                            }

                            break;
                    }
                }

                #endregion End Text Justification

                offset = anchorOffset + justificationOffset;

                #region Handling of UV2 mapping & Scale packing

                bool isCharacterVisible = chInfo.isVisible;
                if (isCharacterVisible)
                {
                    Extents lineExtents = lineInfo.lineExtents;
                    float uvOffset = (m_uvLineOffset * currentLine) % 1;

                    #region Handle UV Mapping Options

                    switch (m_horizontalMapping)
                    {
                        case TextureMappingOptions.Character:
                            bl.uv2.x = 0;
                            tl.uv2.x = 0;
                            tr.uv2.x = 1;
                            br.uv2.x = 1;
                            break;

                        case TextureMappingOptions.Line:
                            if (m_textAlignment != TextAlignmentOptions.Justified)
                            {
                                bl.uv2.x =
                                    (bl.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) +
                                    uvOffset;
                                tl.uv2.x =
                                    (tl.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) +
                                    uvOffset;
                                tr.uv2.x =
                                    (tr.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) +
                                    uvOffset;
                                br.uv2.x =
                                    (br.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) +
                                    uvOffset;
                                break;
                            }

                            bl.uv2.x = (bl.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            tl.uv2.x = (tl.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            tr.uv2.x = (tr.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            br.uv2.x = (br.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            break;

                        case TextureMappingOptions.Paragraph:
                            bl.uv2.x = (bl.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            tl.uv2.x = (tl.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            tr.uv2.x = (tr.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            br.uv2.x = (br.position.x + justificationOffset.x - m_meshExtents.min.x) /
                                (m_meshExtents.max.x - m_meshExtents.min.x) + uvOffset;
                            break;

                        case TextureMappingOptions.MatchAspect:

                            switch (m_verticalMapping)
                            {
                                case TextureMappingOptions.Character:
                                    bl.uv2.y = 0;
                                    tl.uv2.y = 1;
                                    tr.uv2.y = 0;
                                    br.uv2.y = 1;
                                    break;

                                case TextureMappingOptions.Line:
                                    bl.uv2.y = (bl.position.y - lineExtents.min.y) /
                                        (lineExtents.max.y - lineExtents.min.y) + uvOffset;
                                    tl.uv2.y = (tl.position.y - lineExtents.min.y) /
                                        (lineExtents.max.y - lineExtents.min.y) + uvOffset;
                                    tr.uv2.y = bl.uv2.y;
                                    br.uv2.y = tl.uv2.y;
                                    break;

                                case TextureMappingOptions.Paragraph:
                                    bl.uv2.y = (bl.position.y - m_meshExtents.min.y) /
                                        (m_meshExtents.max.y - m_meshExtents.min.y) + uvOffset;
                                    tl.uv2.y = (tl.position.y - m_meshExtents.min.y) /
                                        (m_meshExtents.max.y - m_meshExtents.min.y) + uvOffset;
                                    tr.uv2.y = bl.uv2.y;
                                    br.uv2.y = tl.uv2.y;
                                    break;

                                case TextureMappingOptions.MatchAspect:
                                    Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
                                    break;
                            }

                            float xDelta = (1 - ((bl.uv2.y + tl.uv2.y) * chInfo.aspectRatio)) / 2;

                            bl.uv2.x = (bl.uv2.y * chInfo.aspectRatio) + xDelta + uvOffset;
                            tl.uv2.x = bl.uv2.x;
                            tr.uv2.x = (tl.uv2.y * chInfo.aspectRatio) + xDelta + uvOffset;
                            br.uv2.x = tr.uv2.x;
                            break;
                    }

                    switch (m_verticalMapping)
                    {
                        case TextureMappingOptions.Character:
                            bl.uv2.y = 0;
                            tl.uv2.y = 1;
                            tr.uv2.y = 1;
                            br.uv2.y = 0;
                            break;

                        case TextureMappingOptions.Line:
                            bl.uv2.y = (bl.position.y - lineInfo.descender) / (lineInfo.ascender - lineInfo.descender);
                            tl.uv2.y = (tl.position.y - lineInfo.descender) / (lineInfo.ascender - lineInfo.descender);
                            tr.uv2.y = tl.uv2.y;
                            br.uv2.y = bl.uv2.y;
                            break;

                        case TextureMappingOptions.Paragraph:
                            bl.uv2.y = (bl.position.y - m_meshExtents.min.y) /
                                       (m_meshExtents.max.y - m_meshExtents.min.y);
                            tl.uv2.y = (tl.position.y - m_meshExtents.min.y) /
                                       (m_meshExtents.max.y - m_meshExtents.min.y);
                            tr.uv2.y = tl.uv2.y;
                            br.uv2.y = bl.uv2.y;
                            break;

                        case TextureMappingOptions.MatchAspect:
                            float yDelta = (1 - ((bl.uv2.x + tr.uv2.x) / chInfo.aspectRatio)) / 2;

                            bl.uv2.y = yDelta + (bl.uv2.x / chInfo.aspectRatio);
                            tl.uv2.y = yDelta + (tr.uv2.x / chInfo.aspectRatio);
                            br.uv2.y = bl.uv2.y;
                            tr.uv2.y = tl.uv2.y;
                            break;
                    }

                    #endregion

                    #region Pack Scale into UV2

                    xScale = chInfo.scale * (1 - m_charWidthAdjDelta);
                    if (!chInfo.isUsingAlternateTypeface && (chInfo.style & FontStyles.Bold) == FontStyles.Bold)
                        xScale *= -1;

                    switch (canvasRenderMode)
                    {
                        case RenderMode.ScreenSpaceOverlay:
                            xScale *= Mathf.Abs(lossyScale) / canvasScaleFactor;
                            break;
                        case RenderMode.ScreenSpaceCamera:
                            xScale *= isCameraAssigned ? Mathf.Abs(lossyScale) : 1;
                            break;
                        case RenderMode.WorldSpace:
                            xScale *= Mathf.Abs(lossyScale);
                            break;
                    }

                    bl.uv.w = xScale;
                    tl.uv.w = xScale;
                    tr.uv.w = xScale;
                    br.uv.w = xScale;

                    #endregion

                    #region Handle maxVisibleCharacters / maxVisibleLines / Page Mode

                    if (i < m_maxVisibleCharacters && wordCount < m_maxVisibleWords && currentLine < m_maxVisibleLines)
                    {
                        bl.position += offset;
                        tl.position += offset;
                        tr.position += offset;
                        br.position += offset;
                    }
                    else
                    {
                        bl.position = Vector3.zero;
                        tl.position = Vector3.zero;
                        tr.position = Vector3.zero;
                        br.position = Vector3.zero;
                        chInfo.isVisible = false;
                    }

                    #endregion


                    FillCharacterVertexBuffers(i);
                }

                #endregion

                chInfo.bottomLeft += offset;
                chInfo.topLeft += offset;
                chInfo.topRight += offset;
                chInfo.bottomRight += offset;

                chInfo.origin += offset.x;
                chInfo.xAdvance += offset.x;

                chInfo.ascender += offset.y;
                chInfo.descender += offset.y;
                chInfo.baseLine += offset.y;

                #region Adjust lineExtents resulting from alignment offset

                if (currentLine != lastLine || i == m_characterCount - 1)
                {
                    if (currentLine != lastLine)
                    {
                        ref var lastLineInfo = ref m_textInfo.lineInfo[lastLine];
                        lastLineInfo.baseline += offset.y;
                        lastLineInfo.ascender += offset.y;
                        lastLineInfo.descender += offset.y;

                        lastLineInfo.maxAdvance += offset.x;

                        lastLineInfo.lineExtents.min =
                            new(m_textInfo.characterInfo[lastLineInfo.firstCharacterIndex].bottomLeft.x,
                                lastLineInfo.descender);
                        lastLineInfo.lineExtents.max =
                            new(m_textInfo.characterInfo[lastLineInfo.lastVisibleCharacterIndex].topRight.x,
                                lastLineInfo.ascender);
                    }

                    if (i == m_characterCount - 1)
                    {
                        currLineInfo.baseline += offset.y;
                        currLineInfo.ascender += offset.y;
                        currLineInfo.descender += offset.y;

                        currLineInfo.maxAdvance += offset.x;

                        currLineInfo.lineExtents.min =
                            new(m_textInfo.characterInfo[currLineInfo.firstCharacterIndex].bottomLeft.x,
                                currLineInfo.descender);
                        currLineInfo.lineExtents.max =
                            new(m_textInfo.characterInfo[currLineInfo.lastVisibleCharacterIndex].topRight.x,
                                currLineInfo.ascender);
                    }
                }

                #endregion


                #region Track Word Count

                if (char.IsLetterOrDigit(unicode) || unicode == 0x2D || unicode == 0xAD || unicode == 0x2010 ||
                    unicode == 0x2011)
                {
                    if (!isStartOfWord)
                    {
                        isStartOfWord = true;
                        wordFirstChar = i;
                    }

                    if (isStartOfWord && i == m_characterCount - 1)
                    {
                        int size = m_textInfo.wordInfo.Length;
                        int index = m_textInfo.wordCount;

                        if (m_textInfo.wordCount + 1 > size)
                            TMP_TextInfo.Resize(ref m_textInfo.wordInfo, size + 1);

                        wordLastChar = i;

                        ref var wordInfo = ref m_textInfo.wordInfo[index];
                        wordInfo.firstCharacterIndex = wordFirstChar;
                        wordInfo.lastCharacterIndex = wordLastChar;
                        wordInfo.characterCount = wordLastChar - wordFirstChar + 1;
                        wordInfo.textComponent = this;

                        wordCount += 1;
                        m_textInfo.wordCount += 1;
                        currLineInfo.wordCount += 1;
                    }
                }
                else if (isStartOfWord || i == 0 && (!char.IsPunctuation(unicode) || isWhiteSpace ||
                                                     unicode == 0x200B || i == m_characterCount - 1))
                {
                    if (i > 0 && i < characterInfos.Length - 1 && i < m_characterCount &&
                        (unicode == 39 || unicode == 8217) && char.IsLetterOrDigit(characterInfos[i - 1].character) &&
                        char.IsLetterOrDigit(characterInfos[i + 1].character))
                    {
                    }
                    else
                    {
                        wordLastChar = i == m_characterCount - 1 && char.IsLetterOrDigit(unicode) ? i : i - 1;
                        isStartOfWord = false;

                        int size = m_textInfo.wordInfo.Length;
                        int index = m_textInfo.wordCount;

                        if (m_textInfo.wordCount + 1 > size)
                            TMP_TextInfo.Resize(ref m_textInfo.wordInfo, size + 1);

                        ref var wordInfo = ref m_textInfo.wordInfo[index];
                        wordInfo.firstCharacterIndex = wordFirstChar;
                        wordInfo.lastCharacterIndex = wordLastChar;
                        wordInfo.characterCount = wordLastChar - wordFirstChar + 1;
                        wordInfo.textComponent = this;

                        wordCount += 1;
                        m_textInfo.wordCount += 1;
                        currLineInfo.wordCount += 1;
                    }
                }

                #endregion


                #region Underline

                bool isUnderline = (m_textInfo.characterInfo[i].style & FontStyles.Underline) == FontStyles.Underline;
                if (isUnderline)
                {
                    bool isUnderlineVisible = !(i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines);

                    if (!isWhiteSpace && unicode != 0x200B)
                    {
                        underlineMaxScale = Mathf.Max(underlineMaxScale, m_textInfo.characterInfo[i].scale);
                        xScaleMax = Mathf.Max(xScaleMax, Mathf.Abs(xScale));
                        underlineBaseLine = Mathf.Min(largePositiveFloat,
                            m_textInfo.characterInfo[i].baseLine + font.m_FaceInfo.underlineOffset * underlineMaxScale);
                    }

                    if (!beginUnderline && isUnderlineVisible && i <= lineInfo.lastVisibleCharacterIndex &&
                        unicode != 10 && unicode != 11 && unicode != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(unicode))
                        {
                        }
                        else
                        {
                            beginUnderline = true;
                            underlineStartScale = m_textInfo.characterInfo[i].scale;
                            if (underlineMaxScale == 0)
                            {
                                underlineMaxScale = underlineStartScale;
                                xScaleMax = xScale;
                            }

                            underlineStart = new(m_textInfo.characterInfo[i].bottomLeft.x, underlineBaseLine, 0);
                            underlineColor = m_textInfo.characterInfo[i].underlineColor;
                        }
                    }

                    if (beginUnderline && m_characterCount == 1)
                    {
                        beginUnderline = false;
                        underlineEnd = new(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i].scale;

                        DrawUnderlineMesh(underlineStart, underlineEnd, ref lastVertIndex, underlineStartScale,
                            underlineEndScale, underlineMaxScale, xScaleMax, underlineColor);
                        underlineMaxScale = 0;
                        xScaleMax = 0;
                        underlineBaseLine = largePositiveFloat;
                    }
                    else if (beginUnderline &&
                             (i == lineInfo.lastCharacterIndex || i >= lineInfo.lastVisibleCharacterIndex))
                    {
                        if (isWhiteSpace || unicode == 0x200B)
                        {
                            int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                            underlineEnd = new(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x,
                                underlineBaseLine, 0);
                            underlineEndScale = m_textInfo.characterInfo[lastVisibleCharacterIndex].scale;
                        }
                        else
                        {
                            underlineEnd = new(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                            underlineEndScale = m_textInfo.characterInfo[i].scale;
                        }

                        beginUnderline = false;
                        DrawUnderlineMesh(underlineStart, underlineEnd, ref lastVertIndex, underlineStartScale,
                            underlineEndScale, underlineMaxScale, xScaleMax, underlineColor);
                        underlineMaxScale = 0;
                        xScaleMax = 0;
                        underlineBaseLine = largePositiveFloat;
                    }
                    else if (beginUnderline && !isUnderlineVisible)
                    {
                        beginUnderline = false;
                        underlineEnd = new(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underlineStart, underlineEnd, ref lastVertIndex, underlineStartScale,
                            underlineEndScale, underlineMaxScale, xScaleMax, underlineColor);
                        underlineMaxScale = 0;
                        xScaleMax = 0;
                        underlineBaseLine = largePositiveFloat;
                    }
                    else if (beginUnderline && i < m_characterCount - 1 &&
                             !underlineColor.Compare(m_textInfo.characterInfo[i + 1].underlineColor))
                    {
                        beginUnderline = false;
                        underlineEnd = new(m_textInfo.characterInfo[i].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i].scale;

                        DrawUnderlineMesh(underlineStart, underlineEnd, ref lastVertIndex, underlineStartScale,
                            underlineEndScale, underlineMaxScale, xScaleMax, underlineColor);
                        underlineMaxScale = 0;
                        xScaleMax = 0;
                        underlineBaseLine = largePositiveFloat;
                    }
                }
                else
                {
                    if (beginUnderline)
                    {
                        beginUnderline = false;
                        underlineEnd = new(m_textInfo.characterInfo[i - 1].topRight.x, underlineBaseLine, 0);
                        underlineEndScale = m_textInfo.characterInfo[i - 1].scale;

                        DrawUnderlineMesh(underlineStart, underlineEnd, ref lastVertIndex, underlineStartScale,
                            underlineEndScale, underlineMaxScale, xScaleMax, underlineColor);
                        underlineMaxScale = 0;
                        xScaleMax = 0;
                        underlineBaseLine = largePositiveFloat;
                    }
                }

                #endregion


                #region Strikethrough

                bool isStrikethrough = (m_textInfo.characterInfo[i].style & FontStyles.Strikethrough) ==
                                       FontStyles.Strikethrough;
                float strikethroughOffset = currentFontAsset.m_FaceInfo.strikethroughOffset;

                if (isStrikethrough)
                {
                    bool isStrikeThroughVisible = !(i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines);

                    if (!beginStrikethrough && isStrikeThroughVisible && i <= lineInfo.lastVisibleCharacterIndex &&
                        unicode != 10 && unicode != 11 && unicode != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(unicode))
                        {
                        }
                        else
                        {
                            beginStrikethrough = true;
                            strikethroughPointSize = m_textInfo.characterInfo[i].pointSize;
                            strikethroughScale = m_textInfo.characterInfo[i].scale;
                            strikethroughStart = new(m_textInfo.characterInfo[i].bottomLeft.x,
                                m_textInfo.characterInfo[i].baseLine + strikethroughOffset * strikethroughScale, 0);
                            strikethroughColor = m_textInfo.characterInfo[i].strikethroughColor;
                            strikethroughBaseline = m_textInfo.characterInfo[i].baseLine;
                        }
                    }

                    if (beginStrikethrough && m_characterCount == 1)
                    {
                        beginStrikethrough = false;
                        strikethroughEnd = new(m_textInfo.characterInfo[i].topRight.x,
                            m_textInfo.characterInfo[i].baseLine + strikethroughOffset * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethroughStart, strikethroughEnd, ref lastVertIndex, strikethroughScale,
                            strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i == lineInfo.lastCharacterIndex)
                    {
                        if (isWhiteSpace || unicode == 0x200B)
                        {
                            int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                            strikethroughEnd = new(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x,
                                m_textInfo.characterInfo[lastVisibleCharacterIndex].baseLine +
                                strikethroughOffset * strikethroughScale, 0);
                        }
                        else
                        {
                            strikethroughEnd = new(m_textInfo.characterInfo[i].topRight.x,
                                m_textInfo.characterInfo[i].baseLine + strikethroughOffset * strikethroughScale, 0);
                        }

                        beginStrikethrough = false;
                        DrawUnderlineMesh(strikethroughStart, strikethroughEnd, ref lastVertIndex, strikethroughScale,
                            strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i < m_characterCount &&
                             (m_textInfo.characterInfo[i + 1].pointSize != strikethroughPointSize ||
                              !TMP_Math.Approximately(m_textInfo.characterInfo[i + 1].baseLine + offset.y,
                                  strikethroughBaseline)))
                    {
                        beginStrikethrough = false;

                        int lastVisibleCharacterIndex = lineInfo.lastVisibleCharacterIndex;
                        if (i > lastVisibleCharacterIndex)
                            strikethroughEnd = new(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x,
                                m_textInfo.characterInfo[lastVisibleCharacterIndex].baseLine +
                                strikethroughOffset * strikethroughScale, 0);
                        else
                            strikethroughEnd = new(m_textInfo.characterInfo[i].topRight.x,
                                m_textInfo.characterInfo[i].baseLine + strikethroughOffset * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethroughStart, strikethroughEnd, ref lastVertIndex, strikethroughScale,
                            strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && i < m_characterCount && currentFontAsset.GetInstanceID() !=
                             characterInfos[i + 1].fontAsset.GetInstanceID())
                    {
                        beginStrikethrough = false;
                        strikethroughEnd = new(m_textInfo.characterInfo[i].topRight.x,
                            m_textInfo.characterInfo[i].baseLine + strikethroughOffset * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethroughStart, strikethroughEnd, ref lastVertIndex, strikethroughScale,
                            strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                    else if (beginStrikethrough && !isStrikeThroughVisible)
                    {
                        beginStrikethrough = false;
                        strikethroughEnd = new(m_textInfo.characterInfo[i - 1].topRight.x,
                            m_textInfo.characterInfo[i - 1].baseLine + strikethroughOffset * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethroughStart, strikethroughEnd, ref lastVertIndex, strikethroughScale,
                            strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                }
                else
                {
                    if (beginStrikethrough)
                    {
                        beginStrikethrough = false;
                        strikethroughEnd = new(m_textInfo.characterInfo[i - 1].topRight.x,
                            m_textInfo.characterInfo[i - 1].baseLine + strikethroughOffset * strikethroughScale, 0);

                        DrawUnderlineMesh(strikethroughStart, strikethroughEnd, ref lastVertIndex, strikethroughScale,
                            strikethroughScale, strikethroughScale, xScale, strikethroughColor);
                    }
                }

                #endregion


                #region Text Highlighting

                bool isHighlight = (m_textInfo.characterInfo[i].style & FontStyles.Highlight) == FontStyles.Highlight;
                if (isHighlight)
                {
                    bool isHighlightVisible = !(i > m_maxVisibleCharacters || currentLine > m_maxVisibleLines);

                    if (!beginHighlight && isHighlightVisible && i <= lineInfo.lastVisibleCharacterIndex &&
                        unicode != 10 && unicode != 11 && unicode != 13)
                    {
                        if (i == lineInfo.lastVisibleCharacterIndex && char.IsSeparator(unicode))
                        {
                        }
                        else
                        {
                            beginHighlight = true;
                            highlightStart = largePositiveVector2;
                            highlightEnd = largeNegativeVector2;
                            highlightState = m_textInfo.characterInfo[i].highlightState;
                        }
                    }

                    if (beginHighlight)
                    {
                        TMP_CharacterInfo currentCharacter = m_textInfo.characterInfo[i];
                        HighlightState currentState = currentCharacter.highlightState;

                        bool isColorTransition = false;

                        if (highlightState != currentState)
                        {
                            if (isWhiteSpace)
                                highlightEnd.x =
                                    (highlightEnd.x - highlightState.padding.right + currentCharacter.origin) / 2;
                            else
                                highlightEnd.x = (highlightEnd.x - highlightState.padding.right +
                                                  currentCharacter.bottomLeft.x) / 2;

                            highlightStart.y = Mathf.Min(highlightStart.y, currentCharacter.descender);
                            highlightEnd.y = Mathf.Max(highlightEnd.y, currentCharacter.ascender);

                            DrawTextHighlight(highlightStart, highlightEnd, ref lastVertIndex, highlightState.color);

                            beginHighlight = true;
                            highlightStart = new Vector2(highlightEnd.x,
                                currentCharacter.descender - currentState.padding.bottom);

                            if (isWhiteSpace)
                                highlightEnd = new Vector2(currentCharacter.xAdvance + currentState.padding.right,
                                    currentCharacter.ascender + currentState.padding.top);
                            else
                                highlightEnd = new Vector2(currentCharacter.topRight.x + currentState.padding.right,
                                    currentCharacter.ascender + currentState.padding.top);

                            highlightState = currentState;

                            isColorTransition = true;
                        }

                        if (!isColorTransition)
                        {
                            if (isWhiteSpace)
                            {
                                highlightStart.x = Mathf.Min(highlightStart.x,
                                    currentCharacter.origin - highlightState.padding.left);
                                highlightEnd.x = Mathf.Max(highlightEnd.x,
                                    currentCharacter.xAdvance + highlightState.padding.right);
                            }
                            else
                            {
                                highlightStart.x = Mathf.Min(highlightStart.x,
                                    currentCharacter.bottomLeft.x - highlightState.padding.left);
                                highlightEnd.x = Mathf.Max(highlightEnd.x,
                                    currentCharacter.topRight.x + highlightState.padding.right);
                            }

                            highlightStart.y = Mathf.Min(highlightStart.y,
                                currentCharacter.descender - highlightState.padding.bottom);
                            highlightEnd.y = Mathf.Max(highlightEnd.y,
                                currentCharacter.ascender + highlightState.padding.top);
                        }
                    }

                    if (beginHighlight && m_characterCount == 1)
                    {
                        beginHighlight = false;

                        DrawTextHighlight(highlightStart, highlightEnd, ref lastVertIndex, highlightState.color);
                    }
                    else if (beginHighlight &&
                             (i == lineInfo.lastCharacterIndex || i >= lineInfo.lastVisibleCharacterIndex))
                    {
                        beginHighlight = false;
                        DrawTextHighlight(highlightStart, highlightEnd, ref lastVertIndex, highlightState.color);
                    }
                    else if (beginHighlight && !isHighlightVisible)
                    {
                        beginHighlight = false;
                        DrawTextHighlight(highlightStart, highlightEnd, ref lastVertIndex, highlightState.color);
                    }
                }
                else
                {
                    if (beginHighlight)
                    {
                        beginHighlight = false;
                        DrawTextHighlight(highlightStart, highlightEnd, ref lastVertIndex, highlightState.color);
                    }
                }

                #endregion

                lastLine = currentLine;
            }

            #endregion

            m_textInfo.meshInfo[m_Underline.materialIndex].vertexCount = lastVertIndex;

            m_textInfo.characterCount = m_characterCount;
            m_textInfo.spriteCount = m_spriteCount;
            m_textInfo.lineCount = lineCount;
            m_textInfo.wordCount = wordCount != 0 && m_characterCount > 0 ? wordCount : 1;

            generateTextPhaseIiMarker.End();

            generateTextPhaseIiiMarker.Begin();
            if (m_renderMode == TextRenderFlags.Render && IsActive())
            {
                OnPreRenderText?.Invoke(m_textInfo);

                if (cachedCanvas.additionalShaderChannels != (AdditionalCanvasShaderChannels)25)
                    cachedCanvas.additionalShaderChannels |= (AdditionalCanvasShaderChannels)25;

                if (m_geometrySortingOrder != VertexSortingOrder.Normal)
                    m_textInfo.meshInfo[0].SortGeometry(VertexSortingOrder.Reverse);

                m_mesh.MarkDynamic();
                m_mesh.vertices = m_textInfo.meshInfo[0].vertices;
                m_mesh.SetUVs(0, m_textInfo.meshInfo[0].uvs0);
                m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
                m_mesh.colors32 = m_textInfo.meshInfo[0].colors32;

                m_mesh.RecalculateBounds();

                CanvasRenderer.SetMesh(m_mesh);

                Color parentBaseColor = cachedCanvasRenderer.GetColor();

                bool isCullTransparentMeshEnabled = cachedCanvasRenderer.cullTransparentMesh;

                for (int i = 1; i < m_textInfo.materialCount; i++)
                {
                    m_textInfo.meshInfo[i].ClearUnusedVertices();

                    if (subTextObjects[i] == null) continue;

                    if (m_geometrySortingOrder != VertexSortingOrder.Normal)
                        m_textInfo.meshInfo[i].SortGeometry(VertexSortingOrder.Reverse);

                    subTextObjects[i].mesh.vertices = m_textInfo.meshInfo[i].vertices;
                    subTextObjects[i].mesh.SetUVs(0, m_textInfo.meshInfo[i].uvs0);
                    subTextObjects[i].mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
                    subTextObjects[i].mesh.colors32 = m_textInfo.meshInfo[i].colors32;

                    subTextObjects[i].mesh.RecalculateBounds();

                    subTextObjects[i].canvasRenderer.SetMesh(subTextObjects[i].mesh);

                    subTextObjects[i].canvasRenderer.SetColor(parentBaseColor);

                    subTextObjects[i].canvasRenderer.cullTransparentMesh = isCullTransparentMeshEnabled;

                    subTextObjects[i].raycastTarget = raycastTarget;
                }
            }

            if (shouldUpdateCulling)
                UpdateCulling();

            TMPro_EventManager.ON_TEXT_CHANGED(this);

            generateTextPhaseIiiMarker.End();
            generateTextMarker.End();
        }

        internal bool ValidateHtmlTag(TextProcessingElement[] chars, int startIndex, out int endIndex)
        {
            int tagCharCount = 0;
            byte attributeFlag = 0;

            int attributeIndex = 0;
            ClearMarkupTagAttributes();
            TagValueType tagValueType = TagValueType.None;
            TagUnitType tagUnitType = TagUnitType.Pixels;

            endIndex = startIndex;
            bool isTagSet = false;
            bool isValidHtmlTag = false;

            for (int i = startIndex;
                 i < chars.Length && chars[i].unicode != 0 && tagCharCount < m_htmlTag.Length &&
                 chars[i].unicode != '<';
                 i++)
            {
                uint unicode = chars[i].unicode;

                if (unicode == '>')
                {
                    isValidHtmlTag = true;
                    endIndex = i;
                    m_htmlTag[tagCharCount] = (char)0;
                    break;
                }

                m_htmlTag[tagCharCount] = (char)unicode;
                tagCharCount += 1;

                if (attributeFlag == 1)
                {
                    if (tagValueType == TagValueType.None)
                    {
                        if (unicode == '+' || unicode == '-' || unicode == '.' || (unicode >= '0' && unicode <= '9'))
                        {
                            tagUnitType = TagUnitType.Pixels;
                            tagValueType = m_xmlAttribute[attributeIndex].valueType = TagValueType.NumericalValue;
                            m_xmlAttribute[attributeIndex].valueStartIndex = tagCharCount - 1;
                            m_xmlAttribute[attributeIndex].valueLength += 1;
                        }
                        else if (unicode == '#')
                        {
                            tagUnitType = TagUnitType.Pixels;
                            tagValueType = m_xmlAttribute[attributeIndex].valueType = TagValueType.ColorValue;
                            m_xmlAttribute[attributeIndex].valueStartIndex = tagCharCount - 1;
                            m_xmlAttribute[attributeIndex].valueLength += 1;
                        }
                        else if (unicode == '"')
                        {
                            tagUnitType = TagUnitType.Pixels;
                            tagValueType = m_xmlAttribute[attributeIndex].valueType = TagValueType.StringValue;
                            m_xmlAttribute[attributeIndex].valueStartIndex = tagCharCount;
                        }
                        else
                        {
                            tagUnitType = TagUnitType.Pixels;
                            tagValueType = m_xmlAttribute[attributeIndex].valueType = TagValueType.StringValue;
                            m_xmlAttribute[attributeIndex].valueStartIndex = tagCharCount - 1;

                            m_xmlAttribute[attributeIndex].valueHashCode =
                                (m_xmlAttribute[attributeIndex].valueHashCode << 5) +
                                m_xmlAttribute[attributeIndex].valueHashCode ^
                                TMP_TextUtilities.ToUpperFast((char)unicode);

                            m_xmlAttribute[attributeIndex].valueLength += 1;
                        }
                    }
                    else
                    {
                        if (tagValueType == TagValueType.NumericalValue)
                        {
                            if (unicode == 'p' || unicode == 'e' || unicode == '%' || unicode == ' ')
                            {
                                attributeFlag = 2;
                                tagValueType = TagValueType.None;

                                switch (unicode)
                                {
                                    case 'e':
                                        m_xmlAttribute[attributeIndex].unitType = tagUnitType = TagUnitType.FontUnits;
                                        break;
                                    case '%':
                                        m_xmlAttribute[attributeIndex].unitType = tagUnitType = TagUnitType.Percentage;
                                        break;
                                    default:
                                        m_xmlAttribute[attributeIndex].unitType = tagUnitType = TagUnitType.Pixels;
                                        break;
                                }

                                attributeIndex += 1;
                                m_xmlAttribute[attributeIndex].nameHashCode = 0;
                                m_xmlAttribute[attributeIndex].valueHashCode = 0;
                                m_xmlAttribute[attributeIndex].valueType = TagValueType.None;
                                m_xmlAttribute[attributeIndex].unitType = TagUnitType.Pixels;
                                m_xmlAttribute[attributeIndex].valueStartIndex = 0;
                                m_xmlAttribute[attributeIndex].valueLength = 0;
                            }
                            else
                            {
                                m_xmlAttribute[attributeIndex].valueLength += 1;
                            }
                        }
                        else if (tagValueType == TagValueType.ColorValue)
                        {
                            if (unicode != ' ')
                            {
                                m_xmlAttribute[attributeIndex].valueLength += 1;
                            }
                            else
                            {
                                attributeFlag = 2;
                                tagValueType = TagValueType.None;
                                tagUnitType = TagUnitType.Pixels;
                                attributeIndex += 1;
                                m_xmlAttribute[attributeIndex].nameHashCode = 0;
                                m_xmlAttribute[attributeIndex].valueType = TagValueType.None;
                                m_xmlAttribute[attributeIndex].unitType = TagUnitType.Pixels;
                                m_xmlAttribute[attributeIndex].valueHashCode = 0;
                                m_xmlAttribute[attributeIndex].valueStartIndex = 0;
                                m_xmlAttribute[attributeIndex].valueLength = 0;
                            }
                        }
                        else if (tagValueType == TagValueType.StringValue)
                        {
                            if (unicode != '"')
                            {
                                m_xmlAttribute[attributeIndex].valueHashCode =
                                    (m_xmlAttribute[attributeIndex].valueHashCode << 5) +
                                    m_xmlAttribute[attributeIndex].valueHashCode ^
                                    TMP_TextUtilities.ToUpperFast((char)unicode);
                                m_xmlAttribute[attributeIndex].valueLength += 1;
                            }
                            else
                            {
                                attributeFlag = 2;
                                tagValueType = TagValueType.None;
                                tagUnitType = TagUnitType.Pixels;
                                attributeIndex += 1;
                                m_xmlAttribute[attributeIndex].nameHashCode = 0;
                                m_xmlAttribute[attributeIndex].valueType = TagValueType.None;
                                m_xmlAttribute[attributeIndex].unitType = TagUnitType.Pixels;
                                m_xmlAttribute[attributeIndex].valueHashCode = 0;
                                m_xmlAttribute[attributeIndex].valueStartIndex = 0;
                                m_xmlAttribute[attributeIndex].valueLength = 0;
                            }
                        }
                    }
                }


                if (unicode == '=') attributeFlag = 1;

                if (attributeFlag == 0 && unicode == ' ')
                {
                    if (isTagSet) return false;

                    isTagSet = true;
                    attributeFlag = 2;

                    tagValueType = TagValueType.None;
                    tagUnitType = TagUnitType.Pixels;
                    attributeIndex += 1;
                    m_xmlAttribute[attributeIndex].nameHashCode = 0;
                    m_xmlAttribute[attributeIndex].valueType = TagValueType.None;
                    m_xmlAttribute[attributeIndex].unitType = TagUnitType.Pixels;
                    m_xmlAttribute[attributeIndex].valueHashCode = 0;
                    m_xmlAttribute[attributeIndex].valueStartIndex = 0;
                    m_xmlAttribute[attributeIndex].valueLength = 0;
                }

                if (attributeFlag == 0)
                    m_xmlAttribute[attributeIndex].nameHashCode =
                        (m_xmlAttribute[attributeIndex].nameHashCode << 5) +
                        m_xmlAttribute[attributeIndex].nameHashCode ^ TMP_TextUtilities.ToUpperFast((char)unicode);

                if (attributeFlag == 2 && unicode == ' ')
                    attributeFlag = 0;
            }

            if (!isValidHtmlTag)
            {
                return false;
            }

            #region Rich Text Tag Processing

            if (tag_NoParsing && (m_xmlAttribute[0].nameHashCode != (int)MarkupTag.SLASH_NO_PARSE))
                return false;

            if (m_xmlAttribute[0].nameHashCode == (int)MarkupTag.SLASH_NO_PARSE)
            {
                tag_NoParsing = false;
                return true;
            }

            if (m_htmlTag[0] == 35 && tagCharCount == 4)
            {
                m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                m_colorStack.Add(m_htmlColor);
                return true;
            }

            if (m_htmlTag[0] == 35 && tagCharCount == 5)
            {
                m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                m_colorStack.Add(m_htmlColor);
                return true;
            }

            if (m_htmlTag[0] == 35 && tagCharCount == 7)
            {
                m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                m_colorStack.Add(m_htmlColor);
                return true;
            }

            if (m_htmlTag[0] == 35 && tagCharCount == 9)
            {
                m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                m_colorStack.Add(m_htmlColor);
                return true;
            }

            float value = 0;
            float fontScale;

            switch ((MarkupTag)m_xmlAttribute[0].nameHashCode)
            {
                case MarkupTag.BOLD:
                    m_FontStyleInternal |= FontStyles.Bold;
                    m_fontStyleStack.Add(FontStyles.Bold);

                    m_FontWeightInternal = FontWeight.Bold;
                    return true;
                case MarkupTag.SLASH_BOLD:
                    if ((m_fontStyle & FontStyles.Bold) != FontStyles.Bold)
                    {
                        if (m_fontStyleStack.Remove(FontStyles.Bold) == 0)
                        {
                            m_FontStyleInternal &= ~FontStyles.Bold;
                            m_FontWeightInternal = m_FontWeightStack.Peek();
                        }
                    }

                    return true;
                case MarkupTag.ITALIC:
                    m_FontStyleInternal |= FontStyles.Italic;
                    m_fontStyleStack.Add(FontStyles.Italic);

                    if (m_xmlAttribute[1].nameHashCode == (int)MarkupTag.ANGLE)
                    {
                        m_ItalicAngle = (int)ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex,
                            m_xmlAttribute[1].valueLength);

                        if (m_ItalicAngle < -180 || m_ItalicAngle > 180) return false;
                    }
                    else
                        m_ItalicAngle = m_currentFontAsset.italicStyle;

                    m_ItalicAngleStack.Add(m_ItalicAngle);

                    return true;
                case MarkupTag.SLASH_ITALIC:
                    if ((m_fontStyle & FontStyles.Italic) != FontStyles.Italic)
                    {
                        m_ItalicAngle = m_ItalicAngleStack.Remove();

                        if (m_fontStyleStack.Remove(FontStyles.Italic) == 0)
                            m_FontStyleInternal &= ~FontStyles.Italic;
                    }

                    return true;
                case MarkupTag.STRIKETHROUGH:
                    m_FontStyleInternal |= FontStyles.Strikethrough;
                    m_fontStyleStack.Add(FontStyles.Strikethrough);

                    if (m_xmlAttribute[1].nameHashCode == (int)MarkupTag.COLOR)
                    {
                        m_strikethroughColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[1].valueStartIndex,
                            m_xmlAttribute[1].valueLength);
                        m_strikethroughColor.a = m_htmlColor.a < m_strikethroughColor.a
                            ? (byte)(m_htmlColor.a)
                            : (byte)(m_strikethroughColor.a);
                    }
                    else
                        m_strikethroughColor = m_htmlColor;

                    m_strikethroughColorStack.Add(m_strikethroughColor);

                    return true;
                case MarkupTag.SLASH_STRIKETHROUGH:
                    if ((m_fontStyle & FontStyles.Strikethrough) != FontStyles.Strikethrough)
                    {
                        if (m_fontStyleStack.Remove(FontStyles.Strikethrough) == 0)
                            m_FontStyleInternal &= ~FontStyles.Strikethrough;
                    }

                    m_strikethroughColor = m_strikethroughColorStack.Remove();
                    return true;
                case MarkupTag.UNDERLINE:
                    m_FontStyleInternal |= FontStyles.Underline;
                    m_fontStyleStack.Add(FontStyles.Underline);

                    if (m_xmlAttribute[1].nameHashCode == (int)MarkupTag.COLOR)
                    {
                        m_underlineColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[1].valueStartIndex,
                            m_xmlAttribute[1].valueLength);
                        m_underlineColor.a =
                            m_htmlColor.a < m_underlineColor.a ? (m_htmlColor.a) : (m_underlineColor.a);
                    }
                    else
                        m_underlineColor = m_htmlColor;

                    m_underlineColorStack.Add(m_underlineColor);

                    return true;
                case MarkupTag.SLASH_UNDERLINE:
                    if ((m_fontStyle & FontStyles.Underline) != FontStyles.Underline)
                    {
                        if (m_fontStyleStack.Remove(FontStyles.Underline) == 0)
                            m_FontStyleInternal &= ~FontStyles.Underline;
                    }

                    m_underlineColor = m_underlineColorStack.Remove();
                    return true;
                case MarkupTag.MARK:
                    m_FontStyleInternal |= FontStyles.Highlight;
                    m_fontStyleStack.Add(FontStyles.Highlight);

                    Color32 highlightColor = new(255, 255, 0, 64);
                    TMP_Offset highlightPadding = TMP_Offset.zero;

                    for (int i = 0; i < m_xmlAttribute.Length && m_xmlAttribute[i].nameHashCode != 0; i++)
                    {
                        switch ((MarkupTag)m_xmlAttribute[i].nameHashCode)
                        {
                            case MarkupTag.MARK:
                                if (m_xmlAttribute[i].valueType == TagValueType.ColorValue)
                                    highlightColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[0].valueStartIndex,
                                        m_xmlAttribute[0].valueLength);
                                break;

                            case MarkupTag.COLOR:
                                highlightColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[i].valueStartIndex,
                                    m_xmlAttribute[i].valueLength);
                                break;

                            case MarkupTag.PADDING:
                                int paramCount = GetAttributeParameters(m_htmlTag, m_xmlAttribute[i].valueStartIndex,
                                    m_xmlAttribute[i].valueLength, ref m_attributeParameterValues);
                                if (paramCount != 4) return false;

                                highlightPadding = new(m_attributeParameterValues[0], m_attributeParameterValues[1],
                                    m_attributeParameterValues[2], m_attributeParameterValues[3]);
                                highlightPadding *= m_fontSize * 0.01f * (m_isOrthographic ? 1 : 0.1f);
                                break;
                        }
                    }

                    highlightColor.a = m_htmlColor.a < highlightColor.a
                        ? (byte)(m_htmlColor.a)
                        : (byte)(highlightColor.a);

                    m_HighlightState = new(highlightColor, highlightPadding);
                    m_HighlightStateStack.Push(m_HighlightState);

                    return true;
                case MarkupTag.SLASH_MARK:
                    if ((m_fontStyle & FontStyles.Highlight) != FontStyles.Highlight)
                    {
                        m_HighlightStateStack.Remove();
                        m_HighlightState = m_HighlightStateStack.current;

                        if (m_fontStyleStack.Remove(FontStyles.Highlight) == 0)
                            m_FontStyleInternal &= ~FontStyles.Highlight;
                    }

                    return true;
                case MarkupTag.SUBSCRIPT:
                    m_fontScaleMultiplier *= m_currentFontAsset.faceInfo.subscriptSize > 0
                        ? m_currentFontAsset.faceInfo.subscriptSize
                        : 1;
                    m_baselineOffsetStack.Push(m_baselineOffset);
                    m_materialReferenceStack.Push(m_materialReferences[m_currentMaterialIndex]);
                    fontScale = (m_currentFontSize / m_currentFontAsset.faceInfo.pointSize *
                                 m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1 : 0.1f));
                    m_baselineOffset += m_currentFontAsset.faceInfo.subscriptOffset * fontScale * m_fontScaleMultiplier;

                    m_fontStyleStack.Add(FontStyles.Subscript);
                    m_FontStyleInternal |= FontStyles.Subscript;
                    return true;
                case MarkupTag.SLASH_SUBSCRIPT:
                    if ((m_FontStyleInternal & FontStyles.Subscript) == FontStyles.Subscript)
                    {
                        var previousFontAsset = m_materialReferenceStack.Pop().fontAsset;
                        if (m_fontScaleMultiplier < 1)
                        {
                            m_baselineOffset = m_baselineOffsetStack.Pop();
                            m_fontScaleMultiplier /= previousFontAsset.faceInfo.subscriptSize > 0
                                ? previousFontAsset.faceInfo.subscriptSize
                                : 1;
                        }

                        if (m_fontStyleStack.Remove(FontStyles.Subscript) == 0)
                            m_FontStyleInternal &= ~FontStyles.Subscript;
                    }

                    return true;
                case MarkupTag.SUPERSCRIPT:
                    m_fontScaleMultiplier *= m_currentFontAsset.faceInfo.superscriptSize > 0
                        ? m_currentFontAsset.faceInfo.superscriptSize
                        : 1;
                    m_baselineOffsetStack.Push(m_baselineOffset);
                    m_materialReferenceStack.Push(m_materialReferences[m_currentMaterialIndex]);
                    fontScale = (m_currentFontSize / m_currentFontAsset.faceInfo.pointSize *
                                 m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1 : 0.1f));
                    m_baselineOffset += m_currentFontAsset.faceInfo.superscriptOffset * fontScale *
                                        m_fontScaleMultiplier;

                    m_fontStyleStack.Add(FontStyles.Superscript);
                    m_FontStyleInternal |= FontStyles.Superscript;
                    return true;
                case MarkupTag.SLASH_SUPERSCRIPT:
                    if ((m_FontStyleInternal & FontStyles.Superscript) == FontStyles.Superscript)
                    {
                        var previousFontAsset = m_materialReferenceStack.Pop().fontAsset;
                        if (m_fontScaleMultiplier < 1)
                        {
                            m_baselineOffset = m_baselineOffsetStack.Pop();
                            m_fontScaleMultiplier /= previousFontAsset.faceInfo.superscriptSize > 0
                                ? previousFontAsset.faceInfo.superscriptSize
                                : 1;
                        }

                        if (m_fontStyleStack.Remove(FontStyles.Superscript) == 0)
                            m_FontStyleInternal &= ~FontStyles.Superscript;
                    }

                    return true;
                case MarkupTag.FONT_WEIGHT:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch ((int)value)
                    {
                        case 100:
                            m_FontWeightInternal = FontWeight.Thin;
                            break;
                        case 200:
                            m_FontWeightInternal = FontWeight.ExtraLight;
                            break;
                        case 300:
                            m_FontWeightInternal = FontWeight.Light;
                            break;
                        case 400:
                            m_FontWeightInternal = FontWeight.Regular;
                            break;
                        case 500:
                            m_FontWeightInternal = FontWeight.Medium;
                            break;
                        case 600:
                            m_FontWeightInternal = FontWeight.SemiBold;
                            break;
                        case 700:
                            m_FontWeightInternal = FontWeight.Bold;
                            break;
                        case 800:
                            m_FontWeightInternal = FontWeight.Heavy;
                            break;
                        case 900:
                            m_FontWeightInternal = FontWeight.Black;
                            break;
                    }

                    m_FontWeightStack.Add(m_FontWeightInternal);

                    return true;
                case MarkupTag.SLASH_FONT_WEIGHT:
                    m_FontWeightStack.Remove();

                    if (m_FontStyleInternal == FontStyles.Bold)
                        m_FontWeightInternal = FontWeight.Bold;
                    else
                        m_FontWeightInternal = m_FontWeightStack.Peek();

                    return true;
                case MarkupTag.POSITION:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_xAdvance = value * (m_isOrthographic ? 1.0f : 0.1f);
                            return true;
                        case TagUnitType.FontUnits:
                            m_xAdvance = value * m_currentFontSize * (m_isOrthographic ? 1.0f : 0.1f);
                            return true;
                        case TagUnitType.Percentage:
                            m_xAdvance = m_marginWidth * value / 100;
                            return true;
                    }

                    return false;
                case MarkupTag.VERTICAL_OFFSET:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_baselineOffset = value * (m_isOrthographic ? 1 : 0.1f);
                            return true;
                        case TagUnitType.FontUnits:
                            m_baselineOffset = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            return true;
                        case TagUnitType.Percentage:
                            return false;
                    }

                    return false;
                case MarkupTag.SLASH_VERTICAL_OFFSET:
                    m_baselineOffset = 0;
                    return true;
                case MarkupTag.NO_BREAK:
                    m_isNonBreakingSpace = true;
                    return true;
                case MarkupTag.SLASH_NO_BREAK:
                    m_isNonBreakingSpace = false;
                    return true;
                case MarkupTag.SIZE:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            if (m_htmlTag[5] == 43)
                            {
                                m_currentFontSize = m_fontSize + value;
                                m_sizeStack.Add(m_currentFontSize);
                                return true;
                            }

                            if (m_htmlTag[5] == 45)
                            {
                                m_currentFontSize = m_fontSize + value;
                                m_sizeStack.Add(m_currentFontSize);
                                return true;
                            }

                            m_currentFontSize = value;
                            m_sizeStack.Add(m_currentFontSize);
                            return true;
                        case TagUnitType.FontUnits:
                            m_currentFontSize = m_fontSize * value;
                            m_sizeStack.Add(m_currentFontSize);
                            return true;
                        case TagUnitType.Percentage:
                            m_currentFontSize = m_fontSize * value / 100;
                            m_sizeStack.Add(m_currentFontSize);
                            return true;
                    }

                    return false;
                case MarkupTag.SLASH_SIZE:
                    m_currentFontSize = m_sizeStack.Remove();
                    return true;
                case MarkupTag.FONT:
                    int fontHashCode = m_xmlAttribute[0].valueHashCode;
                    int materialAttributeHashCode = m_xmlAttribute[1].nameHashCode;
                    int materialHashCode = m_xmlAttribute[1].valueHashCode;

                    if (fontHashCode == (int)MarkupTag.DEFAULT)
                    {
                        m_currentFontAsset = m_materialReferences[0].fontAsset;
                        m_currentMaterial = m_materialReferences[0].material;
                        m_currentMaterialIndex = 0;

                        m_materialReferenceStack.Add(m_materialReferences[0]);

                        return true;
                    }

                    Material tempMaterial;

                    MaterialReferenceManager.TryGetFontAsset(fontHashCode, out var tempFont);

                    if (tempFont == null)
                    {
                        tempFont = OnFontAssetRequest?.Invoke(fontHashCode,
                            new(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));

                        if (tempFont == null)
                        {
                            tempFont = Resources.Load<TMP_FontAsset>(TMP_Settings.defaultFontAssetPath +
                                                                     new string(m_htmlTag,
                                                                         m_xmlAttribute[0].valueStartIndex,
                                                                         m_xmlAttribute[0].valueLength));
                        }

                        if (tempFont == null)
                            return false;

                        MaterialReferenceManager.AddFontAsset(tempFont);
                    }

                    if (materialAttributeHashCode == 0 && materialHashCode == 0)
                    {
                        m_currentMaterial = tempFont.material;

                        m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFont,
                            ref m_materialReferences, m_materialReferenceIndexLookup);

                        m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
                    }
                    else if (materialAttributeHashCode == (int)MarkupTag.MATERIAL)
                    {
                        if (MaterialReferenceManager.TryGetMaterial(materialHashCode, out tempMaterial))
                        {
                            m_currentMaterial = tempMaterial;

                            m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFont,
                                ref m_materialReferences, m_materialReferenceIndexLookup);

                            m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
                        }
                        else
                        {
                            tempMaterial = Resources.Load<Material>(TMP_Settings.defaultFontAssetPath +
                                                                    new string(m_htmlTag,
                                                                        m_xmlAttribute[1].valueStartIndex,
                                                                        m_xmlAttribute[1].valueLength));

                            if (tempMaterial == null)
                                return false;

                            MaterialReferenceManager.AddFontMaterial(materialHashCode, tempMaterial);

                            m_currentMaterial = tempMaterial;

                            m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, tempFont,
                                ref m_materialReferences, m_materialReferenceIndexLookup);

                            m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
                        }
                    }
                    else
                        return false;

                    m_currentFontAsset = tempFont;

                    return true;
                case MarkupTag.SLASH_FONT:
                {
                    MaterialReference materialReference = m_materialReferenceStack.Remove();

                    m_currentFontAsset = materialReference.fontAsset;
                    m_currentMaterial = materialReference.material;
                    m_currentMaterialIndex = materialReference.index;

                    return true;
                }
                case MarkupTag.MATERIAL:
                    materialHashCode = m_xmlAttribute[0].valueHashCode;

                    if (materialHashCode == (int)MarkupTag.DEFAULT)
                    {
                        m_currentMaterial = m_materialReferences[0].material;
                        m_currentMaterialIndex = 0;

                        m_materialReferenceStack.Add(m_materialReferences[0]);

                        return true;
                    }


                    if (MaterialReferenceManager.TryGetMaterial(materialHashCode, out tempMaterial))
                    {
                        m_currentMaterial = tempMaterial;

                        m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial,
                            m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);

                        m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
                    }
                    else
                    {
                        tempMaterial = Resources.Load<Material>(TMP_Settings.defaultFontAssetPath +
                                                                new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex,
                                                                    m_xmlAttribute[0].valueLength));

                        if (tempMaterial == null)
                            return false;

                        MaterialReferenceManager.AddFontMaterial(materialHashCode, tempMaterial);

                        m_currentMaterial = tempMaterial;

                        m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial,
                            m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);

                        m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
                    }

                    return true;
                case MarkupTag.SLASH_MATERIAL:
                {
                    MaterialReference materialReference = m_materialReferenceStack.Remove();

                    m_currentMaterial = materialReference.material;
                    m_currentMaterialIndex = materialReference.index;

                    return true;
                }
                case MarkupTag.SPACE:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_xAdvance += value * (m_isOrthographic ? 1 : 0.1f);
                            return true;
                        case TagUnitType.FontUnits:
                            m_xAdvance += value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            return true;
                        case TagUnitType.Percentage:
                            return false;
                    }

                    return false;
                case MarkupTag.ALPHA:
                    if (m_xmlAttribute[0].valueLength != 3) return false;

                    m_htmlColor.a = (byte)(HexToInt(m_htmlTag[7]) * 16 + HexToInt(m_htmlTag[8]));
                    return true;

                case MarkupTag.A:
                    if (m_isTextLayoutPhase)
                    {
                        if (m_xmlAttribute[1].nameHashCode == (int)MarkupTag.HREF)
                        {
                            int index = m_textInfo.linkCount;

                            if (index + 1 > m_textInfo.linkInfo.Length)
                                TMP_TextInfo.Resize(ref m_textInfo.linkInfo, index + 1);

                            m_textInfo.linkInfo[index].textComponent = this;
                            m_textInfo.linkInfo[index].hashCode = (int)MarkupTag.HREF;
                            m_textInfo.linkInfo[index].linkTextfirstCharacterIndex = m_characterCount;
                            m_textInfo.linkInfo[index].SetLinkID(m_htmlTag, m_xmlAttribute[1].valueStartIndex,
                                m_xmlAttribute[1].valueLength);
                        }
                    }

                    return true;
                case MarkupTag.SLASH_A:
                    if (m_isTextLayoutPhase)
                    {
                        int index = m_textInfo.linkCount;

                        m_textInfo.linkInfo[index].linkTextLength =
                            m_characterCount - m_textInfo.linkInfo[index].linkTextfirstCharacterIndex;

                        m_textInfo.linkCount += 1;
                    }

                    return true;
                case MarkupTag.LINK:
                    if (m_isTextLayoutPhase)
                    {
                        int index = m_textInfo.linkCount;

                        if (index + 1 > m_textInfo.linkInfo.Length)
                            TMP_TextInfo.Resize(ref m_textInfo.linkInfo, index + 1);

                        m_textInfo.linkInfo[index].textComponent = this;
                        m_textInfo.linkInfo[index].hashCode = m_xmlAttribute[0].valueHashCode;
                        m_textInfo.linkInfo[index].linkTextfirstCharacterIndex = m_characterCount;

                        m_textInfo.linkInfo[index].linkIdFirstCharacterIndex =
                            startIndex + m_xmlAttribute[0].valueStartIndex;
                        m_textInfo.linkInfo[index].linkIdLength = m_xmlAttribute[0].valueLength;
                        m_textInfo.linkInfo[index].SetLinkID(m_htmlTag, m_xmlAttribute[0].valueStartIndex,
                            m_xmlAttribute[0].valueLength);
                    }

                    return true;
                case MarkupTag.SLASH_LINK:
                    if (m_isTextLayoutPhase)
                    {
                        if (m_textInfo.linkCount < m_textInfo.linkInfo.Length)
                        {
                            m_textInfo.linkInfo[m_textInfo.linkCount].linkTextLength = m_characterCount -
                                m_textInfo.linkInfo[m_textInfo.linkCount].linkTextfirstCharacterIndex;

                            m_textInfo.linkCount += 1;
                        }
                    }

                    return true;
                case MarkupTag.ALIGN:
                    switch ((MarkupTag)m_xmlAttribute[0].valueHashCode)
                    {
                        case MarkupTag.LEFT:
                            m_lineJustification = HorizontalAlignmentOptions.Left;
                            m_lineJustificationStack.Add(m_lineJustification);
                            return true;
                        case MarkupTag.RIGHT:
                            m_lineJustification = HorizontalAlignmentOptions.Right;
                            m_lineJustificationStack.Add(m_lineJustification);
                            return true;
                        case MarkupTag.CENTER:
                            m_lineJustification = HorizontalAlignmentOptions.Center;
                            m_lineJustificationStack.Add(m_lineJustification);
                            return true;
                        case MarkupTag.JUSTIFIED:
                            m_lineJustification = HorizontalAlignmentOptions.Justified;
                            m_lineJustificationStack.Add(m_lineJustification);
                            return true;
                        case MarkupTag.FLUSH:
                            m_lineJustification = HorizontalAlignmentOptions.Flush;
                            m_lineJustificationStack.Add(m_lineJustification);
                            return true;
                    }

                    return false;
                case MarkupTag.SLASH_ALIGN:
                    m_lineJustification = m_lineJustificationStack.Remove();
                    return true;
                case MarkupTag.WIDTH:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_width = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            return false;
                        case TagUnitType.Percentage:
                            m_width = m_marginWidth * value / 100;
                            break;
                    }

                    return true;
                case MarkupTag.SLASH_WIDTH:
                    m_width = -1;
                    return true;

                case MarkupTag.COLOR:
                    if (m_htmlTag[6] == 35 && tagCharCount == 10)
                    {
                        m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                        m_colorStack.Add(m_htmlColor);
                        return true;
                    }

                    if (m_htmlTag[6] == 35 && tagCharCount == 11)
                    {
                        m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                        m_colorStack.Add(m_htmlColor);
                        return true;
                    }

                    if (m_htmlTag[6] == 35 && tagCharCount == 13)
                    {
                        m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                        m_colorStack.Add(m_htmlColor);
                        return true;
                    }

                    if (m_htmlTag[6] == 35 && tagCharCount == 15)
                    {
                        m_htmlColor = HexCharsToColor(m_htmlTag, tagCharCount);
                        m_colorStack.Add(m_htmlColor);
                        return true;
                    }

                    switch (m_xmlAttribute[0].valueHashCode)
                    {
                        case (int)MarkupTag.RED:
                            m_htmlColor = Color.red;
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.LIGHTBLUE:
                            m_htmlColor = new(173, 216, 230, 255);
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.BLUE:
                            m_htmlColor = Color.blue;
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.GREY:
                            m_htmlColor = new(128, 128, 128, 255);
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.BLACK:
                            m_htmlColor = Color.black;
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.GREEN:
                            m_htmlColor = Color.green;
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.WHITE:
                            m_htmlColor = Color.white;
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.ORANGE:
                            m_htmlColor = new(255, 128, 0, 255);
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.PURPLE:
                            m_htmlColor = new(160, 32, 240, 255);
                            m_colorStack.Add(m_htmlColor);
                            return true;
                        case (int)MarkupTag.YELLOW:
                            m_htmlColor = Color.yellow;
                            m_colorStack.Add(m_htmlColor);
                            return true;
                    }

                    return false;

                case MarkupTag.GRADIENT:
                    int gradientPresetHashCode = m_xmlAttribute[0].valueHashCode;

                    if (MaterialReferenceManager.TryGetColorGradientPreset(gradientPresetHashCode,
                            out var tempColorGradientPreset))
                    {
                        m_colorGradientPreset = tempColorGradientPreset;
                    }
                    else
                    {
                        if (tempColorGradientPreset == null)
                        {
                            tempColorGradientPreset = Resources.Load<TMP_ColorGradient>(
                                TMP_Settings.defaultColorGradientPresetsPath + new string(m_htmlTag,
                                    m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
                        }

                        if (tempColorGradientPreset == null)
                            return false;

                        MaterialReferenceManager.AddColorGradientPreset(gradientPresetHashCode,
                            tempColorGradientPreset);
                        m_colorGradientPreset = tempColorGradientPreset;
                    }

                    m_colorGradientPresetIsTinted = false;

                    for (int i = 1; i < m_xmlAttribute.Length && m_xmlAttribute[i].nameHashCode != 0; i++)
                    {
                        int nameHashCode = m_xmlAttribute[i].nameHashCode;

                        switch ((MarkupTag)nameHashCode)
                        {
                            case MarkupTag.TINT:
                                m_colorGradientPresetIsTinted = ConvertToFloat(m_htmlTag,
                                    m_xmlAttribute[i].valueStartIndex, m_xmlAttribute[i].valueLength) != 0;
                                break;
                        }
                    }

                    m_colorGradientStack.Add(m_colorGradientPreset);

                    return true;

                case MarkupTag.SLASH_GRADIENT:
                    m_colorGradientPreset = m_colorGradientStack.Remove();
                    return true;

                case MarkupTag.CHARACTER_SPACE:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_cSpacing = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            m_cSpacing = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            return false;
                    }

                    return true;
                case MarkupTag.SLASH_CHARACTER_SPACE:
                    if (!m_isTextLayoutPhase) return true;

                    if (m_characterCount > 0)
                    {
                        m_xAdvance -= m_cSpacing;
                        m_textInfo.characterInfo[m_characterCount - 1].xAdvance = m_xAdvance;
                    }

                    m_cSpacing = 0;
                    return true;
                case MarkupTag.MONOSPACE:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (m_xmlAttribute[0].unitType)
                    {
                        case TagUnitType.Pixels:
                            m_monoSpacing = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            m_monoSpacing = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            return false;
                    }

                    if (m_xmlAttribute[1].nameHashCode == (int)MarkupTag.DUOSPACE)
                        m_duoSpace = ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex,
                            m_xmlAttribute[1].valueLength) != 0;

                    return true;
                case MarkupTag.SLASH_MONOSPACE:
                    m_monoSpacing = 0;
                    m_duoSpace = false;
                    return true;
                case MarkupTag.CLASS:
                    return false;
                case MarkupTag.SLASH_COLOR:
                    m_htmlColor = m_colorStack.Remove();
                    return true;
                case MarkupTag.INDENT:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            tag_Indent = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            tag_Indent = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            tag_Indent = m_marginWidth * value / 100;
                            break;
                    }

                    m_indentStack.Add(tag_Indent);

                    m_xAdvance = tag_Indent;
                    return true;
                case MarkupTag.SLASH_INDENT:
                    tag_Indent = m_indentStack.Remove();
                    return true;
                case MarkupTag.LINE_INDENT:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            tag_LineIndent = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            tag_LineIndent = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            tag_LineIndent = m_marginWidth * value / 100;
                            break;
                    }

                    m_xAdvance += tag_LineIndent;
                    return true;
                case MarkupTag.SLASH_LINE_INDENT:
                    tag_LineIndent = 0;
                    return true;
                case MarkupTag.LOWERCASE:
                    m_FontStyleInternal |= FontStyles.LowerCase;
                    m_fontStyleStack.Add(FontStyles.LowerCase);
                    return true;
                case MarkupTag.SLASH_LOWERCASE:
                    if ((m_fontStyle & FontStyles.LowerCase) != FontStyles.LowerCase)
                    {
                        if (m_fontStyleStack.Remove(FontStyles.LowerCase) == 0)
                            m_FontStyleInternal &= ~FontStyles.LowerCase;
                    }

                    return true;
                case MarkupTag.ALLCAPS:
                case MarkupTag.UPPERCASE:
                    m_FontStyleInternal |= FontStyles.UpperCase;
                    m_fontStyleStack.Add(FontStyles.UpperCase);
                    return true;
                case MarkupTag.SLASH_ALLCAPS:
                case MarkupTag.SLASH_UPPERCASE:
                    if ((m_fontStyle & FontStyles.UpperCase) != FontStyles.UpperCase)
                    {
                        if (m_fontStyleStack.Remove(FontStyles.UpperCase) == 0)
                            m_FontStyleInternal &= ~FontStyles.UpperCase;
                    }

                    return true;
                case MarkupTag.SMALLCAPS:
                    m_FontStyleInternal |= FontStyles.SmallCaps;
                    m_fontStyleStack.Add(FontStyles.SmallCaps);
                    return true;
                case MarkupTag.SLASH_SMALLCAPS:
                    if ((m_fontStyle & FontStyles.SmallCaps) != FontStyles.SmallCaps)
                    {
                        if (m_fontStyleStack.Remove(FontStyles.SmallCaps) == 0)
                            m_FontStyleInternal &= ~FontStyles.SmallCaps;
                    }

                    return true;
                case MarkupTag.MARGIN:
                    switch (m_xmlAttribute[0].valueType)
                    {
                        case TagValueType.NumericalValue:
                            value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex,
                                m_xmlAttribute[0].valueLength);

                            if (value == Int16.MinValue) return false;

                            switch (tagUnitType)
                            {
                                case TagUnitType.Pixels:
                                    m_marginLeft = value * (m_isOrthographic ? 1 : 0.1f);
                                    break;
                                case TagUnitType.FontUnits:
                                    m_marginLeft = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                                    break;
                                case TagUnitType.Percentage:
                                    m_marginLeft = (m_marginWidth - (m_width != -1 ? m_width : 0)) * value / 100;
                                    break;
                            }

                            m_marginLeft = m_marginLeft >= 0 ? m_marginLeft : 0;
                            m_marginRight = m_marginLeft;
                            return true;

                        case TagValueType.None:
                            for (int i = 1; i < m_xmlAttribute.Length && m_xmlAttribute[i].nameHashCode != 0; i++)
                            {
                                int nameHashCode = m_xmlAttribute[i].nameHashCode;

                                switch ((MarkupTag)nameHashCode)
                                {
                                    case MarkupTag.LEFT:
                                        value = ConvertToFloat(m_htmlTag, m_xmlAttribute[i].valueStartIndex,
                                            m_xmlAttribute[i].valueLength);

                                        if (value == Int16.MinValue) return false;

                                        switch (m_xmlAttribute[i].unitType)
                                        {
                                            case TagUnitType.Pixels:
                                                m_marginLeft = value * (m_isOrthographic ? 1 : 0.1f);
                                                break;
                                            case TagUnitType.FontUnits:
                                                m_marginLeft = value * (m_isOrthographic ? 1 : 0.1f) *
                                                               m_currentFontSize;
                                                break;
                                            case TagUnitType.Percentage:
                                                m_marginLeft = (m_marginWidth - (m_width != -1 ? m_width : 0)) * value /
                                                               100;
                                                break;
                                        }

                                        m_marginLeft = m_marginLeft >= 0 ? m_marginLeft : 0;
                                        break;

                                    case MarkupTag.RIGHT:
                                        value = ConvertToFloat(m_htmlTag, m_xmlAttribute[i].valueStartIndex,
                                            m_xmlAttribute[i].valueLength);

                                        if (value == Int16.MinValue) return false;

                                        switch (m_xmlAttribute[i].unitType)
                                        {
                                            case TagUnitType.Pixels:
                                                m_marginRight = value * (m_isOrthographic ? 1 : 0.1f);
                                                break;
                                            case TagUnitType.FontUnits:
                                                m_marginRight = value * (m_isOrthographic ? 1 : 0.1f) *
                                                                m_currentFontSize;
                                                break;
                                            case TagUnitType.Percentage:
                                                m_marginRight = (m_marginWidth - (m_width != -1 ? m_width : 0)) *
                                                    value / 100;
                                                break;
                                        }

                                        m_marginRight = m_marginRight >= 0 ? m_marginRight : 0;
                                        break;
                                }
                            }

                            return true;
                    }

                    return false;
                case MarkupTag.SLASH_MARGIN:
                    m_marginLeft = 0;
                    m_marginRight = 0;
                    return true;
                case MarkupTag.MARGIN_LEFT:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_marginLeft = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            m_marginLeft = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            m_marginLeft = (m_marginWidth - (m_width != -1 ? m_width : 0)) * value / 100;
                            break;
                    }

                    m_marginLeft = m_marginLeft >= 0 ? m_marginLeft : 0;
                    return true;
                case MarkupTag.MARGIN_RIGHT:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_marginRight = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            m_marginRight = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            m_marginRight = (m_marginWidth - (m_width != -1 ? m_width : 0)) * value / 100;
                            break;
                    }

                    m_marginRight = m_marginRight >= 0 ? m_marginRight : 0;
                    return true;
                case MarkupTag.LINE_HEIGHT:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    switch (tagUnitType)
                    {
                        case TagUnitType.Pixels:
                            m_lineHeight = value * (m_isOrthographic ? 1 : 0.1f);
                            break;
                        case TagUnitType.FontUnits:
                            m_lineHeight = value * (m_isOrthographic ? 1 : 0.1f) * m_currentFontSize;
                            break;
                        case TagUnitType.Percentage:
                            fontScale = (m_currentFontSize / m_currentFontAsset.faceInfo.pointSize *
                                         m_currentFontAsset.faceInfo.scale * (m_isOrthographic ? 1 : 0.1f));
                            m_lineHeight = m_fontAsset.faceInfo.lineHeight * value / 100 * fontScale;
                            break;
                    }

                    return true;
                case MarkupTag.SLASH_LINE_HEIGHT:
                    m_lineHeight = TMP_Math.FLOAT_UNSET;
                    return true;
                case MarkupTag.NO_PARSE:
                    tag_NoParsing = true;
                    return true;
                case MarkupTag.ACTION:
                    int actionID = m_xmlAttribute[0].valueHashCode;

                    if (m_isTextLayoutPhase)
                    {
                        m_actionStack.Add(actionID);

                        Debug.Log("Action ID: [" + actionID + "] First character index: " + m_characterCount);
                    }

                    return true;
                case MarkupTag.SLASH_ACTION:
                    if (m_isTextLayoutPhase)
                    {
                        Debug.Log("Action ID: [" + m_actionStack.CurrentItem() + "] Last character index: " +
                                  (m_characterCount - 1));
                    }

                    m_actionStack.Remove();
                    return true;
                case MarkupTag.SCALE:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    m_FXScale = new(value, 1, 1);

                    return true;
                case MarkupTag.SLASH_SCALE:
                    m_FXScale = Vector3.one;
                    return true;
                case MarkupTag.ROTATE:
                    value = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);

                    if (value == Int16.MinValue) return false;

                    m_FXRotation = Quaternion.Euler(0, 0, value);

                    return true;
                case MarkupTag.SLASH_ROTATE:
                    m_FXRotation = Quaternion.identity;
                    return true;
                case MarkupTag.TABLE:

                    return false;
                case MarkupTag.SLASH_TABLE:
                    return false;
                case MarkupTag.TR:
                    return false;
                case MarkupTag.SLASH_TR:
                    return false;
                case MarkupTag.TH:
                    return false;
                case MarkupTag.SLASH_TH:
                    return false;
                case MarkupTag.TD:

                    return false;
                case MarkupTag.SLASH_TD:
                    return false;
            }

            #endregion

            return false;
        }

        internal int SetArraySizes(TextProcessingElement[] textProcessingArray)
        {
            setArraySizesMarker.Begin();

            int spriteCount = 0;

            m_totalCharacterCount = 0;
            m_isUsingBold = false;
            m_isTextLayoutPhase = false;
            tag_NoParsing = false;
            m_FontStyleInternal = m_fontStyle;
            m_fontStyleStack.Clear();

            m_FontWeightInternal = (m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold
                ? FontWeight.Bold
                : m_fontWeight;
            m_FontWeightStack.SetDefault(m_FontWeightInternal);

            m_currentFontAsset = m_fontAsset;
            m_currentMaterial = m_sharedMaterial;
            m_currentMaterialIndex = 0;

            m_materialReferenceStack.SetDefault(new(m_currentMaterialIndex, m_currentFontAsset, m_currentMaterial,
                m_padding));

            m_materialReferenceIndexLookup.Clear();
            MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, ref m_materialReferences,
                m_materialReferenceIndexLookup);

            if (m_textInfo == null)
                m_textInfo = new(m_InternalTextProcessingArraySize);
            else if (m_textInfo.characterInfo.Length < m_InternalTextProcessingArraySize)
                TMP_TextInfo.Resize(ref m_textInfo.characterInfo, m_InternalTextProcessingArraySize, false);

            #region Setup Ellipsis Special Character

            if (m_overflowMode == TextOverflowModes.Ellipsis)
            {
                GetEllipsisSpecialCharacter(m_currentFontAsset);

                if (m_Ellipsis.character != null)
                {
                    if (m_Ellipsis.fontAsset.GetInstanceID() != m_currentFontAsset.GetInstanceID())
                    {
                        if (TMP_Settings.matchMaterialPreset && m_currentMaterial.GetInstanceID() !=
                            m_Ellipsis.fontAsset.material.GetInstanceID())
                            m_Ellipsis.material =
                                TMP_MaterialManager.GetFallbackMaterial(m_currentMaterial,
                                    m_Ellipsis.fontAsset.material);
                        else
                            m_Ellipsis.material = m_Ellipsis.fontAsset.material;

                        m_Ellipsis.materialIndex = MaterialReference.AddMaterialReference(m_Ellipsis.material,
                            m_Ellipsis.fontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
                        m_materialReferences[m_Ellipsis.materialIndex].referenceCount = 0;
                    }
                }
                else
                {
                    m_overflowMode = TextOverflowModes.Truncate;

                    if (!TMP_Settings.warningsDisabled)
                        Debug.LogWarning(
                            "The character used for Ellipsis is not available in font asset [" +
                            m_currentFontAsset.name +
                            "] or any potential fallbacks. Switching Text Overflow mode to Truncate.", this);
                }
            }

            #endregion

            bool ligature = m_ActiveFontFeatures.Contains(OTL_FeatureTag.liga);

            for (int i = 0; i < textProcessingArray.Length && textProcessingArray[i].unicode != 0; i++)
            {
                if (m_textInfo.characterInfo == null || m_totalCharacterCount >= m_textInfo.characterInfo.Length)
                    TMP_TextInfo.Resize(ref m_textInfo.characterInfo, m_totalCharacterCount + 1, true);

                uint unicode = textProcessingArray[i].unicode;

                #region PARSE XML TAGS

                if (m_isRichText && unicode == 60)
                {
                    int prev_MaterialIndex = m_currentMaterialIndex;

                    if (ValidateHtmlTag(textProcessingArray, i + 1, out var endTagIndex))
                    {
                        int tagStartIndex = textProcessingArray[i].stringIndex;
                        i = endTagIndex;

                        if ((m_FontStyleInternal & FontStyles.Bold) == FontStyles.Bold)
                            m_isUsingBold = true;

                        continue;
                    }
                }

                #endregion

                bool isUsingAlternativeTypeface = false;
                bool isUsingFallbackOrAlternativeTypeface = false;

                TMP_FontAsset prev_fontAsset = m_currentFontAsset;
                Material prev_material = m_currentMaterial;
                int prev_materialIndex = m_currentMaterialIndex;

                #region Handling of LowerCase, UpperCase and SmallCaps Font Styles

                if ((m_FontStyleInternal & FontStyles.UpperCase) == FontStyles.UpperCase)
                {
                    if (char.IsLower((char)unicode))
                        unicode = char.ToUpper((char)unicode);
                }
                else if ((m_FontStyleInternal & FontStyles.LowerCase) == FontStyles.LowerCase)
                {
                    if (char.IsUpper((char)unicode))
                        unicode = char.ToLower((char)unicode);
                }
                else if ((m_FontStyleInternal & FontStyles.SmallCaps) == FontStyles.SmallCaps)
                {
                    if (char.IsLower((char)unicode))
                        unicode = char.ToUpper((char)unicode);
                }

                #endregion

                #region LOOKUP GLYPH

                TMP_TextElement character = null;

                uint nextCharacter = i + 1 < textProcessingArray.Length ? textProcessingArray[i + 1].unicode : 0;

                if (character == null)
                    character = GetTextElement(unicode, m_currentFontAsset, m_FontStyleInternal, m_FontWeightInternal,
                        out isUsingAlternativeTypeface);

                #region MISSING CHARACTER HANDLING

                if (character == null)
                {
                    DoMissingGlyphCallback((int)unicode, textProcessingArray[i].stringIndex, m_currentFontAsset);

                    uint srcGlyph = unicode;

                    unicode = textProcessingArray[i].unicode = (uint)TMP_Settings.missingGlyphCharacter == 0
                        ? 9633
                        : (uint)TMP_Settings.missingGlyphCharacter;

                    character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, m_currentFontAsset, true,
                        FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);

                    if (character == null)
                    {
                        if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
                            character = TMP_FontAssetUtilities.GetCharacterFromFontAssets(unicode, m_currentFontAsset,
                                TMP_Settings.fallbackFontAssets, true, FontStyles.Normal, FontWeight.Regular,
                                out isUsingAlternativeTypeface);
                    }

                    if (character == null)
                    {
                        if (TMP_Settings.defaultFontAsset != null)
                            character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode,
                                TMP_Settings.defaultFontAsset, true, FontStyles.Normal, FontWeight.Regular,
                                out isUsingAlternativeTypeface);
                    }

                    if (character == null)
                    {
                        unicode = textProcessingArray[i].unicode = 32;
                        character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, m_currentFontAsset, true,
                            FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);
                    }

                    if (character == null)
                    {
                        unicode = textProcessingArray[i].unicode = 0x03;
                        character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(unicode, m_currentFontAsset, true,
                            FontStyles.Normal, FontWeight.Regular, out isUsingAlternativeTypeface);
                    }

                    if (!TMP_Settings.warningsDisabled)
                    {
                        string formattedWarning = srcGlyph > 0xFFFF
                            ? string.Format(
                                "The character with Unicode value \\U{0:X8} was not found in the [{1}] font asset or any potential fallbacks. It was replaced by Unicode character \\u{2:X4} in text object [{3}].",
                                srcGlyph, m_fontAsset.name, character.unicode, name)
                            : string.Format(
                                "The character with Unicode value \\u{0:X4} was not found in the [{1}] font asset or any potential fallbacks. It was replaced by Unicode character \\u{2:X4} in text object [{3}].",
                                srcGlyph, m_fontAsset.name, character.unicode, name);

                        Debug.LogWarning(formattedWarning, this);
                    }
                }

                #endregion

                ref var chInfo = ref m_textInfo.characterInfo[m_totalCharacterCount];
                chInfo.alternativeGlyph = null;
                if (character.textAsset.instanceID != m_currentFontAsset.instanceID)
                {
                    isUsingFallbackOrAlternativeTypeface = true;
                    m_currentFontAsset = character.textAsset as TMP_FontAsset;
                }

                #region VARIATION SELECTOR

                if (nextCharacter >= 0xFE00 && nextCharacter <= 0xFE0F ||
                    nextCharacter >= 0xE0100 && nextCharacter <= 0xE01EF)
                {
                    uint variantGlyphIndex = m_currentFontAsset.GetGlyphVariantIndex((uint)unicode, nextCharacter);

                    if (variantGlyphIndex != 0)
                    {
                        if (m_currentFontAsset.TryAddGlyphInternal(variantGlyphIndex, out Glyph glyph))
                        {
                            chInfo.alternativeGlyph = glyph;
                        }
                    }

                    textProcessingArray[i + 1].unicode = 0x1A;
                    i += 1;
                }

                #endregion

                #region LIGATURES

                if (ligature &&
                    m_currentFontAsset.fontFeatureTable.m_LigatureSubstitutionRecordLookup.TryGetValue(
                        character.glyphIndex, out List<LigatureSubstitutionRecord> records))
                {
                    if (records == null)
                        break;

                    for (int j = 0; j < records.Count; j++)
                    {
                        LigatureSubstitutionRecord record = records[j];

                        int componentCount = record.componentGlyphIDs.Length;
                        uint ligatureGlyphID = record.ligatureGlyphID;

                        for (int k = 1; k < componentCount; k++)
                        {
                            uint componentUnicode = (uint)textProcessingArray[i + k].unicode;

                            uint glyphIndex = m_currentFontAsset.GetGlyphIndex(componentUnicode);

                            if (glyphIndex == record.componentGlyphIDs[k])
                                continue;

                            ligatureGlyphID = 0;
                            break;
                        }

                        if (ligatureGlyphID != 0)
                        {
                            if (m_currentFontAsset.TryAddGlyphInternal(ligatureGlyphID, out Glyph glyph))
                            {
                                chInfo.alternativeGlyph = glyph;

                                for (int c = 0; c < componentCount; c++)
                                {
                                    if (c == 0)
                                    {
                                        textProcessingArray[i + c].length = componentCount;
                                        continue;
                                    }

                                    textProcessingArray[i + c].unicode = 0x1A;
                                }

                                i += componentCount - 1;
                                break;
                            }
                        }
                    }
                }

                #endregion

                #endregion

                chInfo.textElement = character;
                chInfo.isUsingAlternateTypeface = isUsingAlternativeTypeface;
                chInfo.character = (char)unicode;
                chInfo.index = textProcessingArray[i].stringIndex;
                chInfo.stringLength = textProcessingArray[i].length;
                chInfo.fontAsset = m_currentFontAsset;

                if (isUsingFallbackOrAlternativeTypeface && m_currentFontAsset.instanceID != m_fontAsset.instanceID)
                {
                    if (TMP_Settings.matchMaterialPreset)
                        m_currentMaterial =
                            TMP_MaterialManager.GetFallbackMaterial(m_currentMaterial, m_currentFontAsset.material);
                    else
                        m_currentMaterial = m_currentFontAsset.material;

                    m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial,
                        m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
                }

                if (character != null && character.glyph.atlasIndex > 0)
                {
                    m_currentMaterial = TMP_MaterialManager.GetFallbackMaterial(m_currentFontAsset, m_currentMaterial,
                        character.glyph.atlasIndex);

                    m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial,
                        m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);

                    isUsingFallbackOrAlternativeTypeface = true;
                }

                if (!char.IsWhiteSpace((char)unicode) && unicode != 0x200B)
                {
                    if (m_materialReferences[m_currentMaterialIndex].referenceCount < 16383)
                        m_materialReferences[m_currentMaterialIndex].referenceCount += 1;
                    else if (isUsingFallbackOrAlternativeTypeface)
                    {
                        if (materialIndexPairs.TryGetValue(m_currentMaterialIndex,
                                out int prev_fallbackMaterialIndex) &&
                            m_materialReferences[prev_fallbackMaterialIndex].referenceCount < 16383)
                        {
                            m_currentMaterialIndex = prev_fallbackMaterialIndex;
                        }
                        else
                        {
                            int fallbackMaterialIndex = MaterialReference.AddMaterialReference(new(m_currentMaterial),
                                m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
                            materialIndexPairs[m_currentMaterialIndex] = fallbackMaterialIndex;
                            m_currentMaterialIndex = fallbackMaterialIndex;
                        }

                        m_materialReferences[m_currentMaterialIndex].referenceCount += 1;
                    }
                    else
                    {
                        m_currentMaterialIndex = MaterialReference.AddMaterialReference(new(m_currentMaterial),
                            m_currentFontAsset, ref m_materialReferences, m_materialReferenceIndexLookup);
                        m_materialReferences[m_currentMaterialIndex].referenceCount += 1;
                    }
                }

                chInfo.material = m_currentMaterial;
                chInfo.materialReferenceIndex = m_currentMaterialIndex;
                m_materialReferences[m_currentMaterialIndex].isFallbackMaterial = isUsingFallbackOrAlternativeTypeface;

                if (isUsingFallbackOrAlternativeTypeface)
                {
                    m_materialReferences[m_currentMaterialIndex].fallbackMaterial = prev_material;
                    m_currentFontAsset = prev_fontAsset;
                    m_currentMaterial = prev_material;
                    m_currentMaterialIndex = prev_materialIndex;
                }

                m_totalCharacterCount += 1;
            }

            m_textInfo.spriteCount = spriteCount;
            int materialCount = m_textInfo.materialCount = m_materialReferenceIndexLookup.Count;

            if (materialCount > m_textInfo.meshInfo.Length)
                TMP_TextInfo.Resize(ref m_textInfo.meshInfo, materialCount, false);

            if (materialCount > subTextObjects.Length)
                TMP_TextInfo.Resize(ref subTextObjects, Mathf.NextPowerOfTwo(materialCount + 1));

            if (m_VertexBufferAutoSizeReduction && m_textInfo.characterInfo.Length - m_totalCharacterCount > 256)
                TMP_TextInfo.Resize(ref m_textInfo.characterInfo, Mathf.Max(m_totalCharacterCount + 1, 256), true);


            for (int i = 0; i < materialCount; i++)
            {
                if (i > 0)
                {
                    if (subTextObjects[i] == null)
                    {
                        subTextObjects[i] = TMP_SubMeshUI.AddSubTextObject(this, m_materialReferences[i]);

                        m_textInfo.meshInfo[i].vertices = null;
                    }

                    if (m_rectTransform.pivot != subTextObjects[i].rectTransform.pivot)
                        subTextObjects[i].rectTransform.pivot = m_rectTransform.pivot;

                    if (subTextObjects[i].sharedMaterial == null || subTextObjects[i].sharedMaterial.GetInstanceID() !=
                        m_materialReferences[i].material.GetInstanceID())
                    {
                        subTextObjects[i].sharedMaterial = m_materialReferences[i].material;
                        subTextObjects[i].fontAsset = m_materialReferences[i].fontAsset;
                    }

                    if (m_materialReferences[i].isFallbackMaterial)
                    {
                        subTextObjects[i].fallbackMaterial = m_materialReferences[i].material;
                        subTextObjects[i].fallbackSourceMaterial = m_materialReferences[i].fallbackMaterial;
                    }
                }

                int referenceCount = m_materialReferences[i].referenceCount;

                if (m_textInfo.meshInfo[i].vertices == null ||
                    m_textInfo.meshInfo[i].vertices.Length < referenceCount * 4)
                {
                    if (m_textInfo.meshInfo[i].vertices == null)
                    {
                        if (i == 0)
                            m_textInfo.meshInfo[i] = new(m_mesh, referenceCount + 1);
                        else
                            m_textInfo.meshInfo[i] = new(subTextObjects[i].mesh, referenceCount + 1);
                    }
                    else
                        m_textInfo.meshInfo[i].ResizeMeshInfo(referenceCount > 1024
                            ? referenceCount + 256
                            : Mathf.NextPowerOfTwo(referenceCount + 1));
                }
                else if (m_VertexBufferAutoSizeReduction && referenceCount > 0 &&
                         m_textInfo.meshInfo[i].vertices.Length / 4 - referenceCount > 256)
                {
                    m_textInfo.meshInfo[i].ResizeMeshInfo(referenceCount > 1024
                        ? referenceCount + 256
                        : Mathf.NextPowerOfTwo(referenceCount + 1));
                }

                m_textInfo.meshInfo[i].material = m_materialReferences[i].material;
            }

            for (int i = materialCount; i < subTextObjects.Length && subTextObjects[i] != null; i++)
            {
                if (i < m_textInfo.meshInfo.Length)
                {
                    subTextObjects[i].canvasRenderer.SetMesh(null);
                }
            }

            setArraySizesMarker.End();
            return m_totalCharacterCount;
        }
    }
}