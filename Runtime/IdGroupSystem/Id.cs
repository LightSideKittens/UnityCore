using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class Id : ScriptableObject
    {
        public static implicit operator string(Id id) => id.name;

        public static implicit operator Id(string name)
        {
            var id = new Id();
            id.name = name;
            return id;
        }
        
        
        public override string ToString() => name;
        
        public override bool Equals(object obj)
        {
            if (obj is Id id)
            {
                return Equals(id);
            }

            return false;
        }

        private bool Equals(Id other)
        {
            try
            {
                return name == other.name;
            }
            catch
            {
                return base.Equals(other);
            }
        }

        public override int GetHashCode()
        {
            try
            {
                return name.GetHashCode();
            }
            catch
            {
                return base.GetHashCode();
            }
        }
        

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