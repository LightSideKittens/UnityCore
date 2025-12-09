using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;


namespace TMPro
{
    [Serializable]
    public class TMP_FontFeatureTable
    {
        public List<LigatureSubstitutionRecord> ligatureRecords
        {
            get { return m_LigatureSubstitutionRecords; }
            set { m_LigatureSubstitutionRecords = value; }
        }

        public List<GlyphPairAdjustmentRecord> glyphPairAdjustmentRecords
        {
            get { return m_GlyphPairAdjustmentRecords; }
            set { m_GlyphPairAdjustmentRecords = value; }
        }

        public List<MarkToBaseAdjustmentRecord> MarkToBaseAdjustmentRecords
        {
            get { return m_MarkToBaseAdjustmentRecords; }
            set { m_MarkToBaseAdjustmentRecords = value; }
        }

        public List<MarkToMarkAdjustmentRecord> MarkToMarkAdjustmentRecords
        {
            get { return m_MarkToMarkAdjustmentRecords; }
            set { m_MarkToMarkAdjustmentRecords = value; }
        }

        [SerializeField] internal List<LigatureSubstitutionRecord> m_LigatureSubstitutionRecords;

        [SerializeField] internal List<GlyphPairAdjustmentRecord> m_GlyphPairAdjustmentRecords;

        [SerializeField] internal List<MarkToBaseAdjustmentRecord> m_MarkToBaseAdjustmentRecords;

        [SerializeField] internal List<MarkToMarkAdjustmentRecord> m_MarkToMarkAdjustmentRecords;


        internal Dictionary<uint, List<LigatureSubstitutionRecord>> m_LigatureSubstitutionRecordLookup;

        internal Dictionary<uint, GlyphPairAdjustmentRecord> m_GlyphPairAdjustmentRecordLookup;

        internal Dictionary<uint, MarkToBaseAdjustmentRecord> m_MarkToBaseAdjustmentRecordLookup;

        internal Dictionary<uint, MarkToMarkAdjustmentRecord> m_MarkToMarkAdjustmentRecordLookup;

        public TMP_FontFeatureTable()
        {
            m_LigatureSubstitutionRecords = new();
            m_LigatureSubstitutionRecordLookup = new();

            m_GlyphPairAdjustmentRecords = new();
            m_GlyphPairAdjustmentRecordLookup = new();

            m_MarkToBaseAdjustmentRecords = new();
            m_MarkToBaseAdjustmentRecordLookup = new();

            m_MarkToMarkAdjustmentRecords = new();
            m_MarkToMarkAdjustmentRecordLookup = new();
        }

        public void SortGlyphPairAdjustmentRecords()
        {
            if (m_GlyphPairAdjustmentRecords.Count > 0)
                m_GlyphPairAdjustmentRecords = m_GlyphPairAdjustmentRecords
                    .OrderBy(s => s.firstAdjustmentRecord.glyphIndex).ThenBy(s => s.secondAdjustmentRecord.glyphIndex)
                    .ToList();
        }

        public void SortMarkToBaseAdjustmentRecords()
        {
            if (m_MarkToBaseAdjustmentRecords.Count > 0)
                m_MarkToBaseAdjustmentRecords = m_MarkToBaseAdjustmentRecords.OrderBy(s => s.baseGlyphID)
                    .ThenBy(s => s.markGlyphID).ToList();
        }

        public void SortMarkToMarkAdjustmentRecords()
        {
            if (m_MarkToMarkAdjustmentRecords.Count > 0)
                m_MarkToMarkAdjustmentRecords = m_MarkToMarkAdjustmentRecords.OrderBy(s => s.baseMarkGlyphID)
                    .ThenBy(s => s.combiningMarkGlyphID).ToList();
        }
    }
}