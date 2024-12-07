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
    public class ChangeLocale : LSAction
    {
        public Locale locale;

        public override void Invoke()
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
    
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
#if UNITY_EDITOR
        [ReadOnly] [ShowInInspector] [HideLabel] [MultiLineProperty] internal string text;
#endif
        
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
        [SerializeField]
        internal LocalizationData localizationData;
        
        public void SetLocalizationData(LocalizationData localizationData)
        {
            TableData = localizationData.tableData;
            Localize(localizationData.key, localizationData.Arguments);
        }
        
#if UNITY_EDITOR
        private void OnLocalizationKeyChanged()
        {
            UpdateLocalizedText();
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
        
        private StringTable table;
        public StringTable Table
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    if (localizationData.tableData == null) return null;
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
            UpdateLocalizedText();
        }

        private string localizedText;
        
        private void UpdateLocalizedText()
        {
            if(!IsLocalized) return;
            
            var lastText = m_text;
            localizedText = localizationData.key.Translate(Table, localizationData.Arguments);
            base.text = localizedText;
            m_text = lastText;
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
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            UpdateTable();
            
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                UpdateLocalizedText();
            }
#endif
        }

        protected override void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
            releaseAction?.Invoke();
            base.OnDestroy();
        }

        private void OnSelectedLocaleChanged(Locale _)
        {
            UpdateTable();
        }
        
        private Action releaseAction;
        private bool isTableLoading;
        
        private void UpdateTable()
        {
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            
            if (isTableLoading) return;
            if (!IsLocalized) return;
            
            isTableLoading = true;
            TableReference tableRef = default;
            Locale locale = null;
            
            if (LocalizationSettings.SelectedLocaleAsync.IsDone)
            {
                locale = LocalizationSettings.SelectedLocaleAsync.Result;
            }
            else
            {
                LocalizationSettings.SelectedLocaleAsync.OnComplete(x => locale = x);
            }
            releaseAction?.Invoke();
            releaseAction = () => LocalizationSettings.StringDatabase.ReleaseTable(tableRef, locale);
            localizationData.tableData.GetStringTableAsync(out tableRef).OnComplete(t =>
            {
                isTableLoading = false;
                Table = t;
            });
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(LocalizationText), true), CanEditMultipleObjects]
    public class LocalizationTextEditor : LSTextEditor
    {
        private InspectorProperty localizationData;
        private LocalizationText localizationText;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            localizationText = (LocalizationText)target;
            localizationData = propertyTree.RootProperty.Children["localizationData"];
        }
        
        public override void OnInspectorGUI()
        {
            TextOnInspector();
        }

        private void TextOnInspector()
        {
            propertyTree.BeginDraw(true);
            localizationText.localizationData.text = text.text;
            localizationData.Draw();
            propertyTree.EndDraw();
            base.OnInspectorGUI();
        }
    }
#endif
}