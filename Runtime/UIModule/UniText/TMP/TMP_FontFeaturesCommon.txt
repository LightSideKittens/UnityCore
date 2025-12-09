using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;


namespace TMPro
{
    [Flags]
    public enum FontFeatureLookupFlags
    {
        None = 0x0,
        IgnoreLigatures = 0x004,
        IgnoreSpacingAdjustments = 0x100,
    }

    [Serializable]
    public struct TMP_GlyphValueRecord
    {
        public float xPlacement
        {
            get { return m_XPlacement; }
            set { m_XPlacement = value; }
        }

        public float yPlacement
        {
            get { return m_YPlacement; }
            set { m_YPlacement = value; }
        }

        public float xAdvance
        {
            get { return m_XAdvance; }
            set { m_XAdvance = value; }
        }

        public float yAdvance
        {
            get { return m_YAdvance; }
            set { m_YAdvance = value; }
        }

        [SerializeField] internal float m_XPlacement;

        [SerializeField] internal float m_YPlacement;

        [SerializeField] internal float m_XAdvance;

        [SerializeField] internal float m_YAdvance;


        public TMP_GlyphValueRecord(float xPlacement, float yPlacement, float xAdvance, float yAdvance)
        {
            m_XPlacement = xPlacement;
            m_YPlacement = yPlacement;
            m_XAdvance = xAdvance;
            m_YAdvance = yAdvance;
        }

        internal TMP_GlyphValueRecord(GlyphValueRecord_Legacy valueRecord)
        {
            m_XPlacement = valueRecord.xPlacement;
            m_YPlacement = valueRecord.yPlacement;
            m_XAdvance = valueRecord.xAdvance;
            m_YAdvance = valueRecord.yAdvance;
        }

        internal TMP_GlyphValueRecord(GlyphValueRecord valueRecord)
        {
            m_XPlacement = valueRecord.xPlacement;
            m_YPlacement = valueRecord.yPlacement;
            m_XAdvance = valueRecord.xAdvance;
            m_YAdvance = valueRecord.yAdvance;
        }

        public static TMP_GlyphValueRecord operator +(TMP_GlyphValueRecord a, TMP_GlyphValueRecord b)
        {
            TMP_GlyphValueRecord c;
            c.m_XPlacement = a.xPlacement + b.xPlacement;
            c.m_YPlacement = a.yPlacement + b.yPlacement;
            c.m_XAdvance = a.xAdvance + b.xAdvance;
            c.m_YAdvance = a.yAdvance + b.yAdvance;

            return c;
        }
    }

    [Serializable]
    public struct TMP_GlyphAdjustmentRecord
    {
        public uint glyphIndex
        {
            get { return m_GlyphIndex; }
            set { m_GlyphIndex = value; }
        }

        public TMP_GlyphValueRecord glyphValueRecord
        {
            get { return m_GlyphValueRecord; }
            set { m_GlyphValueRecord = value; }
        }

        [SerializeField] internal uint m_GlyphIndex;

        [SerializeField] internal TMP_GlyphValueRecord m_GlyphValueRecord;


        public TMP_GlyphAdjustmentRecord(uint glyphIndex, TMP_GlyphValueRecord glyphValueRecord)
        {
            m_GlyphIndex = glyphIndex;
            m_GlyphValueRecord = glyphValueRecord;
        }

        internal TMP_GlyphAdjustmentRecord(GlyphAdjustmentRecord adjustmentRecord)
        {
            m_GlyphIndex = adjustmentRecord.glyphIndex;
            m_GlyphValueRecord = new(adjustmentRecord.glyphValueRecord);
        }
    }

    [Serializable]
    public class TMP_GlyphPairAdjustmentRecord
    {
        public TMP_GlyphAdjustmentRecord firstAdjustmentRecord
        {
            get { return m_FirstAdjustmentRecord; }
            set { m_FirstAdjustmentRecord = value; }
        }

        public TMP_GlyphAdjustmentRecord secondAdjustmentRecord
        {
            get { return m_SecondAdjustmentRecord; }
            set { m_SecondAdjustmentRecord = value; }
        }

        public FontFeatureLookupFlags featureLookupFlags
        {
            get { return m_FeatureLookupFlags; }
            set { m_FeatureLookupFlags = value; }
        }

        [SerializeField] internal TMP_GlyphAdjustmentRecord m_FirstAdjustmentRecord;

        [SerializeField] internal TMP_GlyphAdjustmentRecord m_SecondAdjustmentRecord;

        [SerializeField] internal FontFeatureLookupFlags m_FeatureLookupFlags;


        public TMP_GlyphPairAdjustmentRecord(TMP_GlyphAdjustmentRecord firstAdjustmentRecord,
            TMP_GlyphAdjustmentRecord secondAdjustmentRecord)
        {
            m_FirstAdjustmentRecord = firstAdjustmentRecord;
            m_SecondAdjustmentRecord = secondAdjustmentRecord;
            m_FeatureLookupFlags = FontFeatureLookupFlags.None;
        }


        internal TMP_GlyphPairAdjustmentRecord(GlyphPairAdjustmentRecord glyphPairAdjustmentRecord)
        {
            m_FirstAdjustmentRecord = new(glyphPairAdjustmentRecord.firstAdjustmentRecord);
            m_SecondAdjustmentRecord = new(glyphPairAdjustmentRecord.secondAdjustmentRecord);
            m_FeatureLookupFlags = FontFeatureLookupFlags.None;
        }
    }

    public struct GlyphPairKey
    {
        public uint firstGlyphIndex;
        public uint secondGlyphIndex;
        public uint key;

        public GlyphPairKey(uint firstGlyphIndex, uint secondGlyphIndex)
        {
            this.firstGlyphIndex = firstGlyphIndex;
            this.secondGlyphIndex = secondGlyphIndex;
            key = secondGlyphIndex << 16 | firstGlyphIndex;
        }

        internal GlyphPairKey(TMP_GlyphPairAdjustmentRecord record)
        {
            firstGlyphIndex = record.firstAdjustmentRecord.glyphIndex;
            secondGlyphIndex = record.secondAdjustmentRecord.glyphIndex;
            key = secondGlyphIndex << 16 | firstGlyphIndex;
        }
    }
}