using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class Id : ScriptableObject
    {
        private static readonly Dictionary<string, Id> cachedIds;
        
        static Id()
        {
            cachedIds = new Dictionary<string, Id>();
#if UNITY_EDITOR
            World.Destroyed += cachedIds.Clear;
#endif
        }
        
        public static implicit operator string(Id id) => id.name;
        public static implicit operator Id(string name)
        {
            if (string.IsNullOrEmpty(name)) name = string.Empty;
            if (cachedIds.TryGetValue(name, out var id)) return id;
            
            id = CreateInstance<Id>();
            id.name = name;
            cachedIds.Add(name, id);

            return id;
        }

        public override string ToString()
        {
            if (this == null)
            {
                return "Null Id";
            }
            return name;
        }

        public override bool Equals(object other)
        {
#if UNITY_EDITOR
            if (World.IsPlaying)
            {
#endif
                if (other is Id id)
                {
                    return id == name;
                }

                if (other is string str)
                {
                    return str == name;
                }

                return false;
#if UNITY_EDITOR
            }
#endif
            
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
#if UNITY_EDITOR
            if (World.IsPlaying)
#endif
                return name.GetHashCode();
            
            
            return base.GetHashCode();
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