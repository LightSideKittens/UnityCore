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
                Config[questIds]![questId]!.Remove();
                QuestConfig.Delete(QuestConfig.Type.Data, GetQuestPath(questId));
                QuestConfig.DeletePath(QuestConfig.Type.View, questId);
            }
        }

        [Serializable]
        public class DeleteAfterFinish : DeleteHandler
        {
            [ValueDropdown("Keys")] 
            [SerializeField]
            private string timeMarkKey;

            [TimeSpan] public long time;

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