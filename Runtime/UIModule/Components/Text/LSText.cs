using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using TMPro.EditorUtilities;
using UnityEditor;
#endif

namespace LSCore
{
    [Serializable]
    public struct LocalizationData
    {
        [ValueDropdown("LocalizationKeys")]
        public string key;
        
        public SharedTableData tableData;
        
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
    
    public class LSText : TextMeshProUGUI
    {
        [LSOnValueChanged("OnLocalizationKeyChanged", true, "key")]
        [SerializeField] private LocalizationData localizationData;

        public void SetLocalizationData(LocalizationData localizationData)
        {
            TableData = localizationData.tableData;
            LocalizationKey = localizationData.key;
        }
        
#if UNITY_EDITOR
        private void OnLocalizationKeyChanged(string key)
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

        public string LocalizationKey
        {
            get => localizationData.key;
            set
            {
                m_text = string.Empty;
                if (value != localizationData.key)
                {
                    localizationData.key = value;
                    if (Table == null)
                    {
                        UpdateTable();
                        return;
                    }
                    UpdateLocalizedText();
                }
            }
        }

        private string localizedText;
        
        private void UpdateLocalizedText()
        {
            var lastText = m_text;
            localizedText = localizationData.key.Translate(Table);
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
            UpdateTable();
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

        public event Action Enabled;
        public event Action Disabled;

        protected override void OnEnable()
        {
            base.OnEnable();
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            Enabled?.Invoke();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
            Disabled?.Invoke();
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
            lastTableRef = localizationData.tableData.TableCollectionName;
            LocalizationSettings.StringDatabase.GetTableAsync(lastTableRef).OnComplete(t => Table = t);
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        SerializedProperty padding;
        protected PropertyTree propertyTree;
        private InspectorProperty localizationData;
        
        private LSText text;

        protected void DrawTextPropertiesAsFoldout()
        {
            EditorUtils.DrawInBoxFoldout("Text Properties", TextOnInspector, text, false);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            text = (LSText)target;
            padding = serializedObject.FindProperty("m_RaycastPadding");
            propertyTree = PropertyTree.Create(serializedObject);
            localizationData = propertyTree.RootProperty.Children["localizationData"];
            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
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

        protected override void OnDisable()
        {
            base.OnDisable();
            SceneView.duringSceneGui -= DrawAnchorsOnSceneView;
        }

        private void OnDestroy()
        {
            propertyTree.Dispose();
        }

        private void DrawAnchorsOnSceneView(SceneView sceneView) => LSRaycastTargetEditor.DrawAnchorsOnSceneView(this, sceneView);

        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();
            
            if (Foldout.extraSettings)
            {
                EditorGUILayout.PropertyField(padding);
            }
        }
    }
#endif
}