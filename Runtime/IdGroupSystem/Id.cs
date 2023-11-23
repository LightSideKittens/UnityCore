using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class Id : ScriptableObject
    {
        public static implicit operator string(Id id) => id.name;
        
        public override string ToString() => name;
        
#if UNITY_EDITOR
        public HashSet<TIdGroup> GetAllGroups<TIdGroup>() where TIdGroup : IdGroup
        {
            var set = new HashSet<TIdGroup>();
            var allGroups = AssetDatabaseUtils.LoadAllAssets<TIdGroup>();

            foreach (var group in allGroups)
            {
                if (group.Contains(this))
                {
                    set.Add(group);
                }
            }

            return set;
        }
#endif
    }
}