using System;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager
    {
        [Serializable]
        public class CreateImmediate : CreateHandler
        {
            [SerializeReference] public QuestsSelector selector;
            [Range(1, 10)] public int count = 1;

            protected override void StartCreating()
            {
                if (Config["wasCreated"] != null) return;
                for (int i = 0; i < count; i++)
                {
                    CreateBySelector(selector);
                }
                Config["wasCreated"] = true;
            }
        }
    }
}