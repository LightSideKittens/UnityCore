using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace LSCore
{
    [Serializable]
    [Unwrap]
    public class Int : ILocalizationArgument
    {
        public int value;
        public override string ToString() => value.ToString();
    }

    [Serializable]
    [Unwrap]
    public class String : ILocalizationArgument
    {
        public string value;
        public override string ToString() => value;
    }

    public interface ILocalizationArgument { }
    
    //Example
    /*public class HealthOfUnit : ILocalizationArgument
    {
        public Id id;
        public LevelsManager levelsManager;
        public int  level;
        public override string ToString()
        {
            var unit = levelsManager.GetLevel<Unit>(id, level);
            unit.RegisterComps();
            return unit.GetComp<BaseHealthComp>().Health.ToString();
        }
    }*/

    [Serializable]
    public struct LocalizationData
    {
        public SharedTableData tableData;

        [ValueDropdown("LocalizationKeys")]
        public string key;

        [SerializeReference] private ILocalizationArgument[] arguments;
        public object[] rawArguments;
        
        public object[] Arguments => rawArguments ?? arguments;
        
#if UNITY_EDITOR
        public IEnumerable<string> LocalizationKeys
        {
            get
            {
                yield return "";
                if (tableData != null)
                {
                    foreach (var entry in tableData.Entries)
                    {
                        yield return entry.Key;
                    }
                }
            }
        }
#endif
    }
    
    public class LocalizationText : LSText
    {
        [OnValueChanged("OnLocalizationKeyChanged", true)]
        [SerializeField] private LocalizationData localizationData;
        
        public void SetLocalizationData(LocalizationData localizationData)
        {
            TableData = localizationData.tableData;
            Localize(localizationData.key, localizationData.Arguments);
        }
        
#if UNITY_EDITOR
        private void OnLocalizationKeyChanged()
        {
            UpdateLocalizedTextOrUpdateTable();
        }
#endif
        public SharedTableData TableData
        {
            get => localizationData.tableData;
            set
            {
                if (localizationData.tableData != value)
                {
                    localizationData.tableData = value;
                    UpdateTable();
                }
            }
        }
        
        private Locale currentLocale;
        private StringTable table;
        public StringTable Table
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    table = AssetDatabaseUtils.LoadAny<StringTable>($"{localizationData.tableData.TableCollectionName}_{LocalizationSettings.ProjectLocale.Identifier.Code}");
                }
#endif
                return table;
            }
            set
            {
                if (table != value)
                {
#if UNITY_EDITOR
                    if (World.IsEditMode)
                    {
                        table = value;
                        UpdateLocalizedText();
                        return;
                    }
#endif
                    if (currentLocale != null && currentLocale != LocalizationSettings.SelectedLocale)
                    {
                        LocalizationSettings.StringDatabase.ReleaseTable(lastTableRef, currentLocale);
                    }
                    
                    currentLocale = LocalizationSettings.SelectedLocale;
                    table = value;
                    UpdateLocalizedText();
                }
            }
        }

        public void Localize(string key, params object[] args)
        {
            m_text = string.Empty;
            localizationData.rawArguments = args;
            localizationData.key = key;
            UpdateLocalizedTextOrUpdateTable();
        }

        private string localizedText;
        
        private void UpdateLocalizedText()
        {
            var lastText = m_text;
            localizedText = localizationData.key.Translate(Table, localizationData.Arguments);
            base.text = localizedText;
            m_text = lastText;
        }
        
        private void UpdateLocalizedTextOrUpdateTable()
        {
            if (Table == null)
            {
                UpdateTable();
                return;
            }

            UpdateLocalizedText();
        }

        
        public bool IsLocalized => !string.IsNullOrEmpty(localizationData.key);
        
        public override string text
        {
            get => IsLocalized ? localizedText : base.text;
            set
            {
                localizationData.key = string.Empty;
                base.text = value;
            }
        }

        public override void Rebuild(CanvasUpdate update)
        {
            var lastText = m_text;
            m_text = text;
            base.Rebuild(update);
            m_text = lastText;
        }

        public override float preferredWidth
        {
            get
            {
                var lastText = m_text;
                m_text = text;
                var width = base.preferredWidth;
                m_text = lastText;
                return width;
            }
        }

        public override float preferredHeight
        {
            get
            {
                var lastText = m_text;
                m_text = text;
                var height = base.preferredHeight;
                m_text = lastText;
                return height;
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            UpdateTable();
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                UpdateLocalizedTextOrUpdateTable();
            }
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            if (table != null)
            {
                LocalizationSettings.StringDatabase.ReleaseTable(lastTableRef);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        private void OnSelectedLocaleChanged(Locale _)
        {
            if (IsLocalized)
            {
                UpdateTable();
            }
        }
        
        private TableReference lastTableRef;
        
        private void UpdateTable()
        {
            if (!IsLocalized) return;
            
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            localizationData.tableData.GetStringTableAsync(out lastTableRef).OnComplete(t => Table = t);
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(LocalizationText), true), CanEditMultipleObjects]
    public class LocalizationTextEditor : LSTextEditor
    {
        private InspectorProperty localizationData;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            localizationData = propertyTree.RootProperty.Children["localizationData"];
        }
        
        public override void OnInspectorGUI()
        {
            TextOnInspector();
        }

        private void TextOnInspector()
        {
            propertyTree.BeginDraw(true);
            localizationData.Draw();
            propertyTree.EndDraw();
            base.OnInspectorGUI();
        }
    }
#endif
}