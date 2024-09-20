using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public abstract class DeleteHandler : Handler
        {
            protected void Delete(string questId)
            {
                Config[questIds]?[questId]?.Parent?.Remove();
                QuestConfig.Delete(QuestConfig.Type.Data, GetQuestPath(questId));
                QuestConfig.DeletePath(QuestConfig.Type.View, questId);
            }
        }

        [Serializable]
        public class DeleteAfterTimeMark : DeleteHandler
        {
   
            [ValueDropdown("Keys")] 
            [SerializeField]
            [Required]
            private string timeMarkKey;
            
            [TimeSpan(0, 5, 0)] 
            public long time;

            private IEnumerable<string> Keys => TimeMarkKeys;
            
            protected void Delete(string questId, TimeSpan timeForDelete)
            {
                DoForQuestAfterTime(questId, timeMarkKey, timeForDelete, Delete);
            }
            
            protected override void OnInit()
            {
                DoForEachAfterTime(time, Delete);
            }
        }
    }
}