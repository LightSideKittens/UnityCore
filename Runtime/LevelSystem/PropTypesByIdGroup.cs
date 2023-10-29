#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace LSCore.LevelSystem
{
    public class PropTypesByIdGroup : SerializedScriptableObject
    {
        [Serializable]
        private class Data
        {
            [IdGroup]
            public IdGroup group;
            
            [ValueDropdown("Types", IsUniqueList = true)]
            [HideReferenceObjectPicker]
            public HashSet<Type> types = new ();

            private static IEnumerable<Type> Types => BaseGameProperty.AllPropertyTypes;
        }
        
        private static PropTypesByIdGroup instance;
        public static PropTypesByIdGroup Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabaseUtils.LoadAny<PropTypesByIdGroup>();
                }

                instance.Init();
                return instance;
            }
        }

        [OdinSerialize] 
        [HideReferenceObjectPicker]
        private List<Data> typesByGroup = new();
        
        public static Dictionary<IdGroup, HashSet<Type>> Types => Instance.types;
        private Dictionary<IdGroup, HashSet<Type>> types = new();

        public static HashSet<Type> GetAllTypesById(Id id)
        {
            var allIdGroups = id.AllGroups;
            var set = new HashSet<Type>();

            foreach (var group in allIdGroups)
            {
                if (Types.TryGetValue(group, out var types))
                {
                    set.UnionWith(types);
                }
            }

            return set;
        }
        
        
        private void Init()
        {
            types.Clear();
            
            foreach (var data in typesByGroup)
            {
                types.Add(data.group, data.types);
            }
            
        }
    }
}
#endif