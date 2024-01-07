using System.Collections.Generic;
using Attributes;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
#endif
using UnityEngine;

namespace LSCore.LevelSystem
{
    [CreateAssetMenu(fileName = nameof(LevelConfig), menuName = "Battle/" + nameof(LevelConfig), order = 0)]
    public class LevelConfig : SerializedScriptableObject
    {
        [field: SerializeField, ReadOnly] public LevelsContainer Container { get; set; }
        public Id Id => Container.Id;

        [HideReferenceObjectPicker]
        [OdinSerialize] public GameProps Props { get; private set; } = new();

        [OdinSerialize]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(HideAddButton = true, OnTitleBarGUI = "OtherPropsGui")]
        public List<GroupGameProps> OtherProps { get; private set; } = new();
        
#if UNITY_EDITOR
        public override string ToString() => name;

        private void OtherPropsGui()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
            {
                var allDestinationsProps = new GroupGameProps();
                OtherProps.Add(allDestinationsProps);
            }
        }

        public static LevelConfig currentInspected;

        [OnInspectorInit]
        private void OnInit()
        {
            if (Container == null)
            {
                Container = AssetDatabaseUtils.LoadAny<LevelsContainer>(paths: this.GetFolderPath());
            }
            
            OnGui();
        }

        [OnInspectorGUI] 
        private void OnGui() => currentInspected = this;
#endif
    }
}