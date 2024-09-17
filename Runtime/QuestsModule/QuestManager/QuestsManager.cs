using System.Collections.Generic;
using UnityEngine;

namespace LSCore.QuestModule
{
    public partial class QuestsManager : ScriptableObject
    {
        public const string createdAt = nameof(createdAt);
        public const string startedAt = nameof(startedAt);
        public const string finishedAt = nameof(finishedAt);

        public static IEnumerable<string> TimeMarkKeys
        {
            get
            {
                yield return createdAt;
                yield return startedAt;
                yield return finishedAt;
            }
        }

        [GenerateGuid] public string id;
        public List<Quest> quests;
        
        [SerializeReference] public CreateHandler create;
        [SerializeReference] public StartHandler start;
        [SerializeReference] public FinishHandler finish;
        [SerializeReference] public DeleteHandler delete;

        public void Init()
        {
            create?.Init(id, quests);
            start?.Init(id, quests);
            finish?.Init(id, quests);
            delete?.Init(id, quests);
        }
    }
}