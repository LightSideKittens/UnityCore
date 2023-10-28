using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class IdGroup : SerializedScriptableObject
{
    [OdinSerialize] [HideReferenceObjectPicker] 
    [ValueDropdown("AllIds", IsUniqueList = true)]
    private HashSet<Id> ids = new();

    public HashSet<Id> Ids => ids;
    public bool Contains(Id id) => ids.Contains(id);
    
#if UNITY_EDITOR
    private IEnumerable<Id> AllIds
    {
        get
        {
            var path = this.GetFolderPath();
            var allIds = AssetDatabaseUtils.LoadAllAssets<Id>(paths: path);
            
            foreach (var id in allIds)
            {
                yield return id;
            }
        }
    }
#endif
}