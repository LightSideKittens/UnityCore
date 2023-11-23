#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace LSCore.LevelSystem
{
    public class PropTypesByIdGroup : ValuesByIdGroup<LevelIdGroup, HashSet<Type>>
    {
        private static readonly SingleObject<PropTypesByIdGroup> singleObject = new();
        public static PropTypesByIdGroup Instance => singleObject.Get();
        
        public static HashSet<Type> GetAllObjectsById(Id id)
        {
            var allIdGroups = id.GetAllGroups<LevelIdGroup>();
            var set = new HashSet<Type>();

            foreach (var group in allIdGroups)
            {
                if (Instance.ByKey.TryGetValue(group, out var objects))
                {
                    set.UnionWith(objects);
                }
            }

            return set;
        }

        protected override void OnValueProcessAttributes(List<Attribute> attributes)
        {
            attributes.Add(new ValueDropdownAttribute("@LSCore.LevelSystem.BaseGameProperty.AllPropertyTypes"){IsUniqueList = true});
        }
    }
}
#endif