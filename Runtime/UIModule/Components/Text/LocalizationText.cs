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
    public class ChangeLocale : DoIt
    {
        public Locale locale;

        public override void Do()
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
    
    [Serializable]
    [Unwrap]
    [HideReferenceObjectPicker]
    public class ToStringer<T> : ILocalizationArgument
    {
        public T value;
        public override string ToString() => value.ToString();
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
        public bool IsValid => id > 0;
        public SharedTableData tableData;
        
        public string Key
        {
            get => tableData.GetKey(id);
            set => id = tableData.GetId(value);
        }
        
        [ValueDropdown("LocalizationKeys")] public long id;

        [SerializeReference] private ILocalizationArgument[] arguments;
#if UNITY_EDITOR
        [ReadOnly] [ShowInInspector] [HideLabel] [MultiLineProperty] internal string text;
#endif
        
        [ReadOnly]
        [ShowInInspector]
        [ShowIf("@rawArguments != null")]
        public object[] rawArguments;
        
        public object[] Arguments => rawArguments ?? arguments;
        
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<long>> LocalizationKeys
        {
            get
            {
                yield return new ValueDropdownItem<long>("", 0);
                if (tableData != null)
                {
                    foreach (var entry in tableData.Entries)
                    {
                        yield return new ValueDropdownItem<long>(entry.Key, entry.Id);
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
        [BoxGroup]
        internal LocalizationData localizationData;
        
        public void SetLocalizationData(LocalizationData data)
        {
            TableData = data.tableData;
            Localize(data.id, data.Arguments);
            if (!isTableLoading && table == null) UpdateTable();
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

        public void Localize(long id, params object[] args)
        {
            m_text = string.Empty;
            localizationData.rawArguments = args;
            localizationData.id = id;
            UpdateLocalizedText();
        }
        
        public void Localize(string key, params object[] args)
        {
            m_text = string.Empty;
            localizationData.rawArguments = args;
            localizationData.Key = key;
            UpdateLocalizedText();
        }
        
        public void LocalizeArguments(params object[] args) //TODO: Optimize. We don't need to translate whole text when only arguments were changed
        {
            m_text = string.Empty;
            localizationData.rawArguments = args;
            UpdateLocalizedText();
        }

        private string localizedText;
        
        private void UpdateLocalizedText()
        {
            if(!IsLocalized) return;
            if(Table == null) return;
            var lastText = m_text;
            localizedText = localizationData.id.Translate(Table, localizationData.Arguments);
            base.text = localizedText;
            m_text = lastText;
        }

        public bool IsLocalized => localizationData.IsValid;
        
        public override string text
        {
            get => IsLocalized ? localizedText : base.text;
            set
            {
                localizationData.id = 0;
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
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (didAwake)
            {
                LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
                LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            }
        }
#endif
        private void OnSelectedLocaleChanged(Locale _)
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                UpdateLocalizedText();
                return;
            }
#endif
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
            localizationData = propertyTree.RootProperty.Children["#_DefaultBoxGroup"];
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