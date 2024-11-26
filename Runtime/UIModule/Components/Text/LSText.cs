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
    public class LSText : TextMeshProUGUI
    {
        [ValueDropdown("LocalizationKeys")]
        [OnValueChanged("OnLocalizationKeyChanged")]
        [SerializeField] private string localizationKey;
        
        [SerializeField] private SharedTableData tableData;
        
        public SharedTableData TableData
        {
            get => tableData;
            set
            {
                if (tableData != value)
                {
                    tableData = value;
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
                    table = AssetDatabaseUtils.LoadAny<StringTable>($"{tableData.TableCollectionName}_{LocalizationSettings.ProjectLocale.Identifier.Code}");
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
            get => localizationKey;
            set
            {
                if (value != localizationKey)
                {
                    localizationKey = value;
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
            localizedText = localizationKey.Translate(Table);
            base.text = localizedText;
            m_text = lastText;
        }
        
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

        public bool IsLocalized => !string.IsNullOrEmpty(localizationKey);
        
        public override string text
        {
            get => IsLocalized ? localizedText : base.text;
            set
            {
                localizationKey = string.Empty;
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

        private void OnLocalizationKeyChanged()
        {
            UpdateLocalizedText();
        }
        
        private TableReference lastTableRef;
        private void UpdateTable()
        {
            if (!IsLocalized) return;
            
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            lastTableRef = tableData.TableCollectionName;
            LocalizationSettings.StringDatabase.GetTableAsync(lastTableRef).OnComplete(t => Table = t);
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        SerializedProperty padding;
        protected PropertyTree propertyTree;
        private InspectorProperty localizationKey;
        private InspectorProperty tableData;
        
        private bool showImageProperties;

        protected void DrawTextPropertiesAsFoldout()
        {
            showImageProperties = EditorGUILayout.Foldout(showImageProperties, "Text Properties");
            if (showImageProperties)
            {
                TextOnInspector();
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            padding = serializedObject.FindProperty("m_RaycastPadding");
            propertyTree = PropertyTree.Create(serializedObject);
            localizationKey = propertyTree.RootProperty.Children["localizationKey"];
            tableData = propertyTree.RootProperty.Children["tableData"];
            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
        }
        
        public override void OnInspectorGUI()
        {
            TextOnInspector();
        }

        private void TextOnInspector()
        {
            propertyTree.BeginDraw(true);
            localizationKey.Draw();
            tableData.Draw();
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