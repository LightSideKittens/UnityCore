using System;
using System.Collections.Generic;
using LightSide;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

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

    [RequireComponent(typeof(UniText))]
    public class LocalizationText : MonoBehaviour
    {
        [OnValueChanged("OnLocalizationKeyChanged", true)]
        [SerializeField]
        [BoxGroup]
        internal LocalizationData localizationData;

        private UniText text;
        private UniText Target => text ? text : text = GetComponent<UniText>();

        private StringTable table;
        private string localizedText;
        private Action releaseAction;
        private bool isTableLoading;
        private bool isAwake;

        public bool IsLocalized => localizationData.IsValid;

        public void SetLocalizationData(LocalizationData data)
        {
            TableData = data.tableData;
            Localize(data.id, data.Arguments);
            if (!isTableLoading && table == null) UpdateTable();
        }

        /// <summary>Sets literal (non-localized) text and clears the current localization key.</summary>
        public void SetRawText(string value)
        {
            localizationData.id = 0;
            Target.Text = value;
        }

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
            localizationData.rawArguments = args;
            localizationData.id = id;
            UpdateLocalizedText();
        }

        public void Localize(string key, params object[] args)
        {
            localizationData.rawArguments = args;
            localizationData.Key = key;
            UpdateLocalizedText();
        }

        public void LocalizeArguments(params object[] args) //TODO: Optimize. We don't need to translate whole text when only arguments were changed
        {
            localizationData.rawArguments = args;
            UpdateLocalizedText();
        }

        private void UpdateLocalizedText()
        {
            if (!IsLocalized) return;
            if (Table == null) return;
            localizedText = localizationData.id.Translate(Table, localizationData.Arguments);
            Target.Text = localizedText;
        }

        private void Awake()
        {
            isAwake = true;
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            UpdateTable();

#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                UpdateLocalizedText();
            }
#endif
        }

        private void OnDestroy()
        {
            isAwake = false;
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
            releaseAction?.Invoke();
        }

#if UNITY_EDITOR
        private void OnLocalizationKeyChanged()
        {
            UpdateLocalizedText();
        }

        private void OnValidate()
        {
            if (isAwake)
            {
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
}
