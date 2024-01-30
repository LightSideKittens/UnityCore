#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using static LSCore.SingleObject<LSCore.LevelSystem.PropTypesByIdGroup>;

namespace LSCore.LevelSystem
{
    public class PropTypesByIdGroup : ValueByIdGroup<LevelIdGroup, HashSet<Type>>
    {
        public static HashSet<Type> GetAllTypesById(Id id)
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