using System.Collections.Generic;
using UnityEngine;

public class Id : ScriptableObject
{
    public static implicit operator string(Id id) => id.name;
    public override string ToString() => name;

#if UNITY_EDITOR
    public HashSet<IdGroup> AllGroups
    {
        get
        {
            var set = new HashSet<IdGroup>();
            var allGroups = AssetDatabaseUtils.LoadAllAssets<IdGroup>();

            foreach (var group in allGroups)
            {
                if (group.Contains(this))
                {
                    set.Add(group);
                }
            }

            return set;
        }
    }
#endif
}